using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyContainerTest : CommonContainerTest2
	{
		#region PackUnpackShipmentsChildEditableRegistration

		public void TestChildEditableRegistrationForPackUnpackShipments()
		{
			var container = (TallyContainer)GetNewContainer();
			var shipments = container.PackUnpackShipments;
			Assert(!container.IsRegisteredEditableChildObject(shipments));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.TallyContainer);
			container = (TallyContainer)GetNewContainer();
			shipments = container.PackUnpackShipments;
			Assert(container.IsRegisteredEditableChildObject(shipments));
		}

		#endregion

		[TestDate(2006, 9, 22, 10, 0, 0)]
		public void TestNilOutturn()
		{
			Container.JC_IsSealOk = false;
			PackUnpackShipment shipment1 = Container.PackUnpackShipments.AddNew();

			shipment1.JS_HouseBill = "HOUSE1";

			AssertEquals(1, shipment1.OuterPackLines.Count);
			PackLine line = shipment1.OuterPackLines[0];
			line.JL_PackageCount = 5;
			line.JL_Pillaged = 7;
			line.JL_Damaged = 3;

			linkMock.SetupProperty(m => m.ReceiptDate, new ZDateTime(2006, 9, 22, 10, 0, 0));
			Container.NilOutturn();

			AssertEquals(true, Container.JC_IsSealOk);
			AssertEquals(new ZDateTime(2006, 9, 22, 10, 0, 0), Container.JC_LCLUnpack);

			AssertEquals(5, line.JL_Outturn);
			AssertEquals(0, line.JL_Damaged);
			AssertEquals(0, line.JL_Pillaged);
		}

		public void TestValidateTotalsAgainstPackLines()
		{
			AssertEquals("Yes totals validation", true, Container.PackUnpackShipments.ValidateTotalsAgainstPackLines);
		}

		public void TestJC_OH_CFSClientInfoDefaultsToReadonly()
		{
			Assert("Should be readonly", Container.JC_OH_CFSClientInfo.ReadOnly);
		}

		public void TestAllowSurplusPacks()
		{
			AssertEquals("Yes allow surplus on new shipments", true, Container.PackUnpackShipments.AllowSurplusPacks);
		}

		public override void TestValidateJC_DeliveryMode()
		{
			Assert("Delivery Mode Does not apply in CFS/Tally", true);
		}

		public override void TestValidateJC_ContainerNum()
		{
			Assert("Duplicate number validation Does not apply in CFS", true);
		}

		public void TestSavingUnpackDoesNotResetLCLAvailableDate()
		{
			TallyContainerWithStatusOverride container = Factory.New(typeof(TallyContainerWithStatusOverride)) as TallyContainerWithStatusOverride;
			container.TestPackUnpackStatus = PackUnpackStatusHelper.PackUnpackStatus.Unpack;
			container.JC_LCLUnpack = new ZDateTime(2001, 1, 1);
			container.JC_LCLAvailable = new ZDateTime(2001, 2, 22);
			AssertEquals("LCL Available before saving", new ZDateTime(2001, 2, 22), container.JC_LCLAvailable);
			Factory.Save();
			AssertEquals("LCL Available after saving", new ZDateTime(2001, 2, 22), container.JC_LCLAvailable);
		}

		public void TestGetContactOrganisation()
		{
			var container = Factory.New<TallyContainerWithStatusOverride>();
			var client = Factory.LoadTop1<OrgHeader>(new ZQuery());
			container.JC_OH_CFSClient = client.PK;
			var contact = container.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Organisation for FIS type", client.PK, contact.OrgHeader.PK);
		}

		public void TestTallyHasChangesWhenShipmentChanges()
		{
			PackUnpackLoadListConsol loadList = Factory.New<PackUnpackLoadListConsol>();
			loadList.AutomaticallyUpdatePackLineContainers = true;
			loadList.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			loadList.Containers.Add(Container);

			PackUnpackShipment shipment = loadList.Shipments.AddNew();
			shipment.JS_RL_NKDestination = HomePort;
			shipment.JS_RL_NKOrigin = OverseasPort2;
			PackLine pack = shipment.OuterPackLines.AddNew();
			Container.PackLines.Add(pack);

			Factory.Save();

			Assert(!shipment.HasChanges);
			Assert(!Container.HasChanges);

			shipment.JS_GoodsDescription = "hey";
			Assert(shipment.HasChanges);
			Assert(Container.HasChanges);
		}

		public void TestJobServices()
		{
			AssertNotNull("Container should contain a job services collection", Container.Services);

			TallyService fumigation = Container.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TallyContainer loadedContainer = newFactory.Load<TallyContainer>(Container.PK);
			AssertEquals("Job services of container were not loaded correctly", 1, loadedContainer.Services.Count);

			AssertEquals("Precondition - LoadedContainer.Services.ReadOnly should be false.", false, Container.Services.ReadOnly);
			Container.SetReadOnlyIncludingChildren(true);
			AssertEquals("LoadedContainer.Services should be true (JobServices is registered editable).", true, Container.Services.ReadOnly);
		}

		#region TestLCLUnpackDate
		[ExpectNoExceptions]
		public void TestLCLUnpackDate()
		{
			PackUnpackShipment shipment1 = Container.PackUnpackShipments.AddNew();
			PackUnpackShipment shipment2 = Container.PackUnpackShipments.AddNew();

			var shipment1LinkMock = new Mock<IOutturn>();
			var shipment2LinkMock = new Mock<IOutturn>();

			linkMock.Setup(m => m.GetOutturnFor(shipment1)).Returns(shipment1LinkMock.Object);
			shipment1LinkMock.Setup(m => m.IsDeleted).Returns(ZBool.False);
			linkMock.Setup(m => m.GetOutturnFor(shipment2)).Returns(shipment2LinkMock.Object);
			shipment2LinkMock.Setup(m => m.IsDeleted).Returns(ZBool.False);

			shipment1LinkMock.SetupProperty(m => m.UnpackDate, ZDateTime.BrettsBirthday);
			shipment2LinkMock.SetupProperty(m => m.UnpackDate, ZDateTime.BrettsBirthday);

			Container.JC_LCLUnpack = ZDateTime.BrettsBirthday;

			shipment1LinkMock.VerifyAll();
			shipment2LinkMock.VerifyAll();
		}

		#endregion

		[ExpectNoExceptions]
		public void TestSaving_UnpackDateIsModifiedAndForwardingShipmentHasProcessTaskForIRPEvent_ShouldNotThrowException()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.TallyContainer);

			var factory1 = new BusinessObjectFactory();
			var consol = (CommonConsol)factory1.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_UnpackDepotAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var forwardingShipment = consol.Shipments.AddNew();
			var packLine = forwardingShipment.OuterPackLines.AddNew();
			var container = consol.Containers.AddNew();
			container.PackLines.Add(packLine);

			var workflowProvider = (IWorkflowProvider)forwardingShipment;

			var trigger = workflowProvider.WorkflowItems.AddNew();
			trigger.IsMilestone = true;
			trigger.P9_Description = "McLaren";
			trigger.TriggerConditions.TriggerEventCode = "IRP";

			factory1.Save();

			var cfsShipment = Factory.Load<PackUnpackShipment>(forwardingShipment.PK);
			var tallyContainer = Factory.Load<TallyContainer>(container.PK);

			tallyContainer.JC_LCLUnpack = ZDateTime.Now;
			Factory.Save();
		}

		#region Canada Specific Tests

		public void TestCanadaNumbers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				AssertEquals("CanadaCCNNumber should be empty", ZString.Empty, Container.CanadaCCNNumber);
				AssertEquals("CanadaPCNNumber should be empty", ZString.Empty, Container.CanadaPCNNumber);

				var loadList = (CFSLoadListConsol)GetNewConsol();
				loadList.Containers.Add(Container);
				Factory.Save();

				AssertEquals("CanadaCCNNumber should be empty", ZString.Empty, Container.CanadaCCNNumber);
				AssertEquals("CanadaPCNNumber should be empty", ZString.Empty, Container.CanadaPCNNumber);

				CusEntryNumber num1 = loadList.Numbers.AddNew();
				num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num1.CE_EntryNum = "1";

				CusEntryNumber num2 = loadList.Numbers.AddNew();
				num2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				num2.CE_EntryNum = "2";

				Factory.Save();

				AssertEquals("CanadaCCNNumber", "1", Container.CanadaCCNNumber);
				AssertEquals("CanadaPCNNumber", "2", Container.CanadaPCNNumber);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				AssertEquals("CanadaCCNNumber should be empty", ZString.Empty, Container.CanadaCCNNumber);
				AssertEquals("CanadaPCNNumber should be empty", ZString.Empty, Container.CanadaPCNNumber);
			}
		}

		#endregion

		#region Implementation

		protected override bool CouldLoadRelatedDeclaration
		{
			get { return false; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			Container = (TallyContainer)GetNewContainer();
			linkMock = new Mock<IOutturnLink>();
			((IOutturnLinkable)Container).SetOutturnLink(linkMock.Object);
		}

		Mock<IOutturnLink> linkMock;
		protected TallyContainer Container;

		class TallyContainerWithStatusOverride : TallyContainer
		{
			public TallyContainerWithStatusOverride(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override PackUnpackStatusHelper.PackUnpackStatus PackOrUnpackStatus
			{
				get { return TestPackUnpackStatus; }
			}

			public PackUnpackStatusHelper.PackUnpackStatus TestPackUnpackStatus;
		}

		protected override CommonContainer GetNewContainer()
		{
			return Factory.New<TallyContainer>();
		}

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<PackUnpackLoadListConsol>();
		}

		#endregion
	}
}
