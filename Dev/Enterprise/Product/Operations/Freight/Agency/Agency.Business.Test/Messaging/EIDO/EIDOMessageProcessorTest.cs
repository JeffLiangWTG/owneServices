using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class EIDOMessageProcessorTest : TestCaseWithFactory
	{
		[EIDOMessagingConfiguration(Enabled = false)]
		public void TestRegistryNotSet_LogError()
		{
			var target = Factory.NewWithValidTestData<BillOfLading>();
			target.RealContainers.Add(Factory.NewWithValidTestData<AgencyShipmentContainer>());
			var processor = new EIDOMessageProcessor(target);
			var notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);

			CombineAssertions(() =>
			{
				Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
				AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
				AssertEquals("E-IDO messaging has not been enabled.\r\nYou can enable it in the 'Liner & Agency -> E-IDO Messaging' registry option.\r\n", notificationBuffer.AsString);
			});
		}

		[EIDOMessagingConfiguration]
		public void TestBranchWithoutOrgProxy_LogError()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var target = Factory.NewWithValidTestData<BillOfLading>();
				target.RealContainers.Add(Factory.NewWithValidTestData<AgencyShipmentContainer>());
				var processor = new EIDOMessageProcessor(target);
				var notificationBuffer = new NotificationBuffer();
				processor.Process(notificationBuffer);

				CombineAssertions(() =>
				{
					Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
					AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
					AssertEquals("The current branch does not have an org. proxy.\r\n", notificationBuffer.AsString);
				});
			}
		}

		[EIDOMessagingConfiguration]
		public void TestCountry_LogWarning()
		{
			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			var container = NewContainerTarget("AUBNE", "SGSIN");
			var processor = new EIDOMessageProcessor(container.Booking);
			var notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);

			CombineAssertions(() =>
			{
				Assert("message successfully sent.", !notificationBuffer.HasErrors);
				Assert(notificationBuffer.AsString.Contains("S00000100 is released in another country/region and is not eligible for E-IDO messaging, skipping."));
			});
		}

		[EIDOMessagingConfiguration]
		public void TestBillOfLading_LogError()
		{
			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			var container = NewContainerTarget("SGSIN", "AUBNE");
			container.Booking.JS_GoodsDescription = "";
			var processor = new EIDOMessageProcessor(container.Booking);
			var notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);

			CombineAssertions(() =>
			{
				Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
				AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
				AssertEquals("S00000100 has errors and/or message errors.\r\nYou will need to correct these before an E-IDO message can be sent for it.\r\n", notificationBuffer.AsString);
			});
		}

		[EIDOMessagingConfiguration(Principals = new string[] { "XPrincipal" })]
		public void TestPrincipal_LogError()
		{
			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			var container = NewContainerTarget("SGSIN", "AUBNE");
			var processor = new EIDOMessageProcessor(container.Booking);
			var notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);

			CombineAssertions(() =>
			{
				Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
				AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
				AssertEquals("E-IDO messaging has not been enabled for the principal Principal.\r\nYou can enable it in the 'Liner & Agency -> E-IDO Messaging' registry option.\r\n", notificationBuffer.AsString);
			});
		}

		[EIDOMessagingConfiguration]
		public void TestRunSuccess()
		{
			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			var container = NewContainerTarget("SGSIN", "AUBNE");
			var processor = new EIDOMessageProcessor(container.Booking);
			var notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);

			CombineAssertions(() =>
			{
				Assert("message successfully sent.", !notificationBuffer.HasErrors);
				AssertEquals("Should have added a message.", 1, container.Messages.Count);
				AssertEquals("The new message should be queued", EDIMessage.Status.Queued, container.Messages[0].EM_Status);
				AssertEquals("The new message should have the correct application code", EDIMessage.ApplicationCodes.EIDO, container.Messages[0].EM_ApplicationCode);
				Assert("Should add release number.", !container.JC_ContainerImportDORelease.IsEmpty);
			});
		}

		[EIDOMessagingConfiguration]
		public void TestMessageIsAlreadySent()
		{
			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");

			var shipment = NewShipmentTarget("SGSIN", "AUBNE");
			var container1 = NewContainerTarget(shipment, "TEST4100013");
			var container2 = NewContainerTarget(shipment, "TEST4100029");

			AddMessage(container2, EDIMessage.Status.Queued);

			var processor = new EIDOMessageProcessor(shipment);
			var notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);

			CombineAssertions(() =>
			{
				Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
				AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
				AssertEquals("TEST4100029 is still waiting on a response.\r\n", notificationBuffer.AsString);
			});
		}

		#region Implementation
		BillOfLadingContainer NewContainerTarget(string load, string discharge)
		{
			var shipment = NewShipmentTarget(load, discharge);
			var container = NewContainerTarget(shipment, "FAKE4100011");

			shipment.RunPreSaveValidation();

			AssertNoErrors("precondition:", shipment);
			AssertNoMessageErrors("precondition:", shipment);

			return container;
		}

		BillOfLading NewShipmentTarget(string load, string discharge)
		{
			var now = ZDateTime.Now;

			var principal = FindOrCreatePrincipal(Factory, "Principal");
			BaseAgencyTest.SetAcosCode(principal, "principal");

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			var cto = Factory.NewWithValidTestData<OrgHeader>();
			BaseAgencyTest.SetAcosCode(cto, "CTO");

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_IsShippingProvider = true;
			BaseAgencyTest.SetAcosCode(shippingLine, "SL");

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
			voyage.JV_VoyageFlight = "x42";
			voyage.JV_OH_Line = shippingLine.PK;
			voyage.Countries.GetCountry("AU", true).J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = now.AddDays(1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = now.AddDays(2);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.Destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;

			var shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_UniqueConsignRef = "S00000100";
			shipment.JS_RL_NKOrigin = load;
			shipment.JS_RL_NKDestination = discharge;
			shipment.JS_JX = sailing.PK;
			shipment.JS_HouseBill = "SINBNE0234";
			shipment.JS_GoodsDescription = "Short description";
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			return shipment;
		}

		BillOfLadingContainer NewContainerTarget(BillOfLading shipment, string containerNumber)
		{
			var emptyReturn = Factory.NewWithValidTestData<OrgHeader>();
			BaseAgencyTest.SetAcosCode(emptyReturn, "CY");

			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_OA_ArrivalContainerYardAddress = emptyReturn.MainAddress.PK;
			container.JC_ContainerImportDORelease = "PIN";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			packline.JL_MarksAndNumbers = "Marks and Numbers.";
			return container;
		}

		void AddMessage(BillOfLadingContainer container, string status)
		{
			var data = EIDOShipmentMessagingData.NewOriginal(container);
			var builder = EIDOMessageBuilderFactory.GetNewBuilder();
			var message = EIDOMessage.New(container, data.MessageFunction, builder.GenerateMessageText(data));
			message.EM_Status = status;
		}

		static OrgHeader FindOrCreatePrincipal(BusinessObjectFactory factory, string code)
		{
			var result = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code);
			if (result == null)
			{
				result = factory.NewWithValidTestData<OrgHeader>();
				result.OH_Code = code;
				result.OH_IsShippingProvider = true;
				result.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			}

			return result;
		}

		#endregion
	}
}
