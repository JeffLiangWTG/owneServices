using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationRateLineConditionsSupporterTest : RateLineConditionsSupporterTest<BaseJobDeclarationRatingAdapter<BaseJobDeclaration>, DeclarationRateLineConditionsSupporter>
	{
		protected override void RunAssertions()
		{
			RunSetterGetterAssertion("ReceivingAgent", SetConsolReceivingAgent, supporter => supporter.ReceivingAgent);
			RunSetterGetterAssertion("SendingAgent", SetConsolSendingAgent, supporter => supporter.SendingAgent);
			RunSetterGetterAssertion("ControllingAgent", SetControllingAgent, supporter => supporter.ControllingAgent);
			RunSetterGetterAssertion("DepartureCFS", SetConsolDepartureCFS, supporter => supporter.DepartureCFS);
			RunSetterGetterAssertion("ArrivalCFS", SetConsolArrivalCFS, supporter => supporter.ArrivalCFS);

			base.RunAssertions();
		}

		#region Setters

		public OrgHeader SetConsolReceivingAgent(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ER";
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			objectToWrap.JE_JS = shipment.PK;

			return org;
		}

		public OrgHeader SetConsolSendingAgent(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ER";
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			objectToWrap.JE_JS = shipment.PK;

			return org;
		}

		public OrgHeader SetConsolDepartureCFS(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ER";
			consol.JK_OA_PackDepotAddress = org.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			objectToWrap.JE_JS = shipment.PK;

			return org;
		}

		public OrgHeader SetConsolArrivalCFS(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ER";
			consol.JK_OA_UnpackDepotAddress = org.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			objectToWrap.JE_JS = shipment.PK;

			return org;
		}

		protected override OrgHeader SetArrivalCFS(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			objectToWrap.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;
			return org;
		}

		protected override OrgHeader SetDepartureCFS(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			objectToWrap.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;
			return org;
		}

		protected override OrgHeader SetExportBroker(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			return GlbBranch.CurrentBranch.OrgProxy;
		}

		protected override OrgHeader SetImportBroker(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			return GlbBranch.CurrentBranch.OrgProxy;
		}

		protected override OrgHeader SetReceivingAgent(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			objectToWrap.JE_OH_Forwarder = org.PK;
			return org;
		}

		protected override OrgHeader SetSendingAgent(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			var objectToWrap = adapter.Parent;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			objectToWrap.JE_OH_Forwarder = org.PK;
			return org;
		}

		protected override OrgHeader SetControllingAgent(BaseJobDeclarationRatingAdapter<BaseJobDeclaration> adapter)
		{
			return null;
		}

		#endregion

		public override void TestHasDangerousGoods()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "ER";
			consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var container = declaration.CusContainers.AddNew();
			container.Packages.RemoveAll(x => true);
			var packingGroup = container.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			var package = packingGroup.Packages.AddNew();

			var supporter = new DeclarationRateLineConditionsSupporter(declaration);

			AssertEquals(false, supporter.HasDangerousGoods);

			package.UNDGs.AddNew();

			AssertEquals(true, supporter.HasDangerousGoods);
		}

		public void TestHasDangerousGoodsFallsBackToShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "ER";
			consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var supporter = new DeclarationRateLineConditionsSupporter(declaration);

			AssertEquals("Pre-condition", false, supporter.HasDangerousGoods);

			shipment.OuterPackLines.AddNew().UNDGs.AddNew();

			AssertEquals("If the shipment has dangerous goods, then we expect the declaration to be flagged as having dangerous goods", true, supporter.HasDangerousGoods);
		}

		protected override BaseJobDeclarationRatingAdapter<BaseJobDeclaration> GetInterfacedObject()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			return new BaseJobDeclarationRatingAdapter<BaseJobDeclaration>(jobDeclaration);
		}
	}
}
