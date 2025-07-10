using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class ReleaseImportOrderMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEHubIdIsNotSet_LogError()
		{
			using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ShippingPortsMessagingEHubIDCollection()))
			{
				var target = Factory.NewWithValidTestData<BillOfLading>();
				target.RealContainers.Add(Factory.NewWithValidTestData<AgencyShipmentContainer>());
				var processor = new ReleaseImportOrderMessageProcessor(target);
				var notificationBuffer = new NotificationBuffer();
				processor.Process(notificationBuffer);

				CombineAssertions(() =>
				{
					Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
					AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
					AssertEquals("The Import Release Order message cannot be sent as eHub ID is not set.\r\n", notificationBuffer.AsString);
				});
			}
		}

		public void TestBranchWithoutOrgProxy_LogError()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var target = Factory.NewWithValidTestData<BillOfLading>();
					target.RealContainers.Add(Factory.NewWithValidTestData<AgencyShipmentContainer>());
					var processor = new ReleaseImportOrderMessageProcessor(target);
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
		}

		public void TestPrincipal_LogWarning()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Australia).PK;
			branch.Company.GC_RN_NKCountryCode = "AU";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, dischargePort: "AUSYD");
					var processor = new ReleaseImportOrderMessageProcessor(container.Booking);
					var notificationBuffer = new NotificationBuffer();
					processor.Process(notificationBuffer);

					CombineAssertions(() =>
					{
						Assert("message successfully sent.", !notificationBuffer.HasErrors);
						Assert(notificationBuffer.AsString.Contains("Principal is not configured for sending Import Release Order to AUSYD."));
					});
				}
			}
		}

		public void TestOrgProxyHasMatchingCARCode_LogWarning()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Australia).PK;
			branch.Company.GC_RN_NKCountryCode = "AU";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ);
					var processor = new ReleaseImportOrderMessageProcessor(container.Booking);
					var notificationBuffer = new NotificationBuffer();
					processor.Process(notificationBuffer);

					CombineAssertions(() =>
					{
						Assert("message successfully sent.", !notificationBuffer.HasErrors);
						Assert(notificationBuffer.AsString.Contains("The current branch's organization proxy does not have a CAR code for the country/region of container's port of discharge entered. Containers is skipped."));
					});
				}
			}
		}

		public void TestContainers_LogError()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", ZGuid.Empty, true);
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithErrors("BOOKING_1", "CONTAINER_1");
					var processor = new ReleaseImportOrderMessageProcessor(container.Booking);
					var notificationBuffer = new NotificationBuffer();
					processor.Process(notificationBuffer);

					CombineAssertions(() =>
					{
						Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
						AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
						AssertEquals("BOOKING_1 has errors and/or message errors.\r\nYou will need to correct these before an Import Release Order message can be sent for it.\r\n", notificationBuffer.AsString);
					});
				}
			}
		}

		public void TestAssertUXMLMessage()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true).OfType<PortMessagingPort>().First(x => x.Port == "NZAKL").SenderID = "DIJIAAUTOMAN";

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "AUSYD", "NZAKL", Constants.CountryCodes.NewZealand);
					var processor = new ReleaseImportOrderMessageProcessor(container.Booking);
					var notificationBuffer = new NotificationBuffer();
					processor.Process(notificationBuffer);

					Assert("No error logged.", !notificationBuffer.HasErrors);

					var ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
					AssertEquals("Message created", 1, ediMessages.Length);

					var uxml = XDocument.Parse(ediMessages[0].EM_MessageText);
					var ns = uxml.Root.GetDefaultNamespace();

					var addInfoCollectionDataContext = uxml.Descendants(ns + "AddInfoCollection").First();
					var expectedAddInfoCollectionDataContext = @"<AddInfoCollection xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <AddInfo>
    <Key>SenderID</Key>
    <Value>DIJIAAUTOMAN</Value>
  </AddInfo>
  <AddInfo>
    <Key>OperationalPort_Code</Key>
    <Value>NZAKL</Value>
  </AddInfo>
  <AddInfo>
    <Key>OperationalPort_Name</Key>
    <Value>Auckland</Value>
  </AddInfo>
</AddInfoCollection>";
					AssertMultilineASCIIEquals(expectedAddInfoCollectionDataContext, addInfoCollectionDataContext.ToString());

					var lloydsImo = uxml.Descendants(ns + "Shipment").First().Element(ns + "LloydsIMO").Value;
					AssertEquals("VesselLloydsIMO", "8811924", lloydsImo);
				}
			}
		}

		public void TestMessageIsAlreadySent()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithMessageSent("BOOKING", "CONTAINER", PrincipalNZ);
					var processor = new ReleaseImportOrderMessageProcessor(container.Booking);
					var notificationBuffer = new NotificationBuffer();
					processor.Process(notificationBuffer);

					CombineAssertions(() =>
					{
						Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
						AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
						AssertEquals("CONTAINER is still waiting on a response.\r\n", notificationBuffer.AsString);
					});
				}
			}
		}

		#region Implementation

		ShippingPortsMessagingEHubIDCollection GetShippingPortsMessagingEHubIDCollection()
		{
			var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();

			var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDCollection.AddNew();
			shippingPortsMessagingEHubID.Port = ZString.Empty;
			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.eHub;
			shippingPortsMessagingEHubID.RecipientID = "McLaren";

			return shippingPortsMessagingEHubIDCollection;
		}

		BillOfLadingContainer GetContainerWithMessageSent(string uniqueConsignRef, string containerNumber, OrgHeader principal)
		{
			var container = GetContainerWithoutErrors(uniqueConsignRef, containerNumber, principal);
			container.Logs.AddNew(Events.MessageSent, Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.ImportReleaseOrder));

			return container;
		}

		BillOfLadingContainer GetContainerWithoutErrors(string uniqueConsignRef, string containerNumber, OrgHeader principal, string loadPort = "USLAX", string dischargePort = "NZAKL", string countryCode = Constants.CountryCodes.NewZealand)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", countryCode);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = loadPort;
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
			voyage.JV_VoyageFlight = "EK134";
			voyage.JV_OH_Line = carrier.PK;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_Calc_ArrivalCTOAddressOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			destination.JB_E_ARV = 5.DaysAgo();

			voyage.GenerateSailings();

			var booking = Factory.NewWithValidTestData<BillOfLading>();
			booking.JS_HouseBill = uniqueConsignRef;
			booking.JS_UniqueConsignRef = uniqueConsignRef;
			booking.JS_JX = voyage.Sailings[0].PK;
			booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			booking.JS_OH_DeliveryAgent = principal.PK;
			booking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			booking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			booking.Transports[0].JW_ETD = 10.DaysAgo();
			booking.JS_GoodsDescription = "McLaren";

			var container = booking.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_GrossWeight = 100.0;

			var line = booking.OuterPackLines.AddNew();
			line.JL_JC = container.PK;
			line.JL_DetailedDescription = "McLaren";

			Factory.Save();
			return container;
		}

		BillOfLadingContainer GetContainerWithErrors(string uniqueConsignRef, string containerNumber)
		{
			var booking = Factory.NewWithValidTestData<BillOfLading>();
			booking.JS_HouseBill = uniqueConsignRef;
			booking.JS_UniqueConsignRef = uniqueConsignRef;
			booking.JS_NKDischargePort = "NZAKL";

			var container = booking.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;

			return container;
		}

		GlbBranch NewZeelandBranch
		{
			get
			{
				return newZeelandBranch ?? (newZeelandBranch = CreateNewZeelandBranch());
			}
		}
		GlbBranch newZeelandBranch;

		GlbBranch CreateNewZeelandBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand).PK;
			branch.Company.GC_RN_NKCountryCode = "NZ";

			Factory.Save();

			return branch;
		}

		PortMessagingPortCollection AddNewPortMessagingPort(PortMessagingPortCollection importReleaseOrderPorts, string port, ZGuid principalPK, bool enabled, string senderID = "Sender")
		{
			var portMessagingPort = importReleaseOrderPorts.AddNew();
			portMessagingPort.Port = port;
			portMessagingPort.PrincipalPK = principalPK;
			portMessagingPort.SenderID = $"{senderID}_{importReleaseOrderPorts.Count}";
			portMessagingPort.Enabled = enabled;

			return importReleaseOrderPorts;
		}

		OrgHeader PrincipalNZ
		{
			get
			{
				return principalNZ ?? (principalNZ = CreateNewPrincipal(Constants.CountryCodes.NewZealand));
			}
		}
		OrgHeader principalNZ;

		OrgHeader CreateNewPrincipal(ZString countryCode)
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", countryCode);
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();

			return principal;
		}

		static ZQuery GetEDIMessageFilter()
		{
			var filter = new ZQuery();
			filter.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc;

			return filter;
		}

		#endregion
	}
}
