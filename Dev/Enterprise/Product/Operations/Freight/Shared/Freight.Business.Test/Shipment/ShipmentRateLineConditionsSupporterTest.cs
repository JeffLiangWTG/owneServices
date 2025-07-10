using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business.Testing
{
	public class ShipmentRateLineConditionsSupporterTest : RateLineConditionsSupporterTest<ShipmentRatingAdapter<CommonShipment>, ShipmentRateLineConditionsSupporter>
	{
		public override void TestHasDangerousGoods()
		{
			Shipment.OuterPackLines.RemoveAndDeleteAll();
			var supporter = new ShipmentRateLineConditionsSupporter(Shipment);

			Assert("Shipment has no dg", !supporter.HasDangerousGoods);

			var packLine = Shipment.OuterPackLines.AddNew();
			packLine.UNDGs.AddNew();

			Assert(supporter.HasDangerousGoods);
		}

		protected override void RunAssertions()
		{
			SetupConsols(Shipment);
			base.RunAssertions();
			RunSetterGetterAssertion("SendingAgent", SetSendingAgentDifferentSetup, x => x.SendingAgent);
			RunSetterGetterAssertion("ReceivingAgent", SetReceivingAgentDifferentSetup, x => x.ReceivingAgent);
			RunSetterGetterAssertion("ControllingAgent", SetControllingAgent, x => x.ControllingAgent);
		}

		#region Sending & Receiving Agent Tests

		OrgHeader c1Sf, c2Sf, c3Sf, c4Sf, c1Rf, c2Rf, c3Rf, c4Rf;
		CommonConsol consol1, consol2, consol3, consol4;

		void SetupConsols(CommonShipment objectToWrap)
		{
			consol2 = objectToWrap.Consols.AddNew();
			consol1 = objectToWrap.Consols.AddNew();
			consol4 = objectToWrap.Consols.AddNew();
			consol3 = objectToWrap.Consols.AddNew();

			consol1.JK_TransportMode = "RAI";
			consol2.JK_TransportMode = "SEA";
			consol3.JK_TransportMode = "SEA";
			consol4.JK_TransportMode = "ROA";

			consol1.JK_RL_NKLoadPort = "AUBNE";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "NLAMS";

			consol3.JK_RL_NKLoadPort = "NLAMS";
			consol3.JK_RL_NKDischargePort = "USNYC";

			consol4.JK_RL_NKLoadPort = "USNYC";
			consol4.JK_RL_NKDischargePort = "USLAX";
		}

		void SetupSendingForwarders()
		{
			c1Sf = Factory.NewWithValidTestData<OrgHeader>();
			c2Sf = Factory.NewWithValidTestData<OrgHeader>();
			c3Sf = Factory.NewWithValidTestData<OrgHeader>();
			c4Sf = Factory.NewWithValidTestData<OrgHeader>();

			consol1.JK_OA_SendingForwarderAddress = c1Sf.MainAddress.PK;
			consol2.JK_OA_SendingForwarderAddress = c2Sf.MainAddress.PK;
			consol3.JK_OA_SendingForwarderAddress = c3Sf.MainAddress.PK;
			consol4.JK_OA_SendingForwarderAddress = c4Sf.MainAddress.PK;
		}

		void SetupReceivinfForwarders()
		{
			c1Rf = Factory.NewWithValidTestData<OrgHeader>();
			c2Rf = Factory.NewWithValidTestData<OrgHeader>();
			c3Rf = Factory.NewWithValidTestData<OrgHeader>();
			c4Rf = Factory.NewWithValidTestData<OrgHeader>();

			consol1.JK_OA_ReceivingForwarderAddress = c1Rf.MainAddress.PK;
			consol2.JK_OA_ReceivingForwarderAddress = c2Rf.MainAddress.PK;
			consol3.JK_OA_ReceivingForwarderAddress = c3Rf.MainAddress.PK;
			consol4.JK_OA_ReceivingForwarderAddress = c4Rf.MainAddress.PK;
		}

		OrgHeader SetSendingAgentDifferentSetup(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			SetupSendingForwarders();
			Shipment.JS_TransportMode = "SEA";
			return c2Sf;
		}

		OrgHeader SetReceivingAgentDifferentSetup(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			SetupReceivinfForwarders();
			Shipment.JS_TransportMode = "SEA";
			return c3Rf;
		}

		protected override OrgHeader SetSendingAgent(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			SetupSendingForwarders();
			Shipment.JS_TransportMode = "COU";
			return c1Sf;
		}

		protected override OrgHeader SetReceivingAgent(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			SetupReceivinfForwarders();
			Shipment.JS_TransportMode = "COU";
			return c4Rf;
		}

		#endregion

		protected override OrgHeader SetDepartureCFS(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.GetDepartureCFSDocAddress.OrganisationPK = org.PK;
			return org;
		}

		protected override OrgHeader SetArrivalCFS(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.GetArrivalCFSDocAddress.OrganisationPK = org.PK;
			return org;
		}

		protected override OrgHeader SetExportBroker(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.JS_OH_ExportBroker = org.PK;
			return org;
		}

		protected override OrgHeader SetImportBroker(ShipmentRatingAdapter<CommonShipment> objectToWrap)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.JS_OH_ImportBroker = org.PK;
			return org;
		}

		protected override OrgHeader SetControllingAgent(ShipmentRatingAdapter<CommonShipment> adapter)
		{
			var address = Shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);
			return address.Organisation;
		}

		protected override ShipmentRatingAdapter<CommonShipment> GetInterfacedObject()
		{
			return Shipment.RatingAdapter as ShipmentRatingAdapter<CommonShipment>;
		}

		CommonShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.NewWithValidTestData<CommonShipment>()); }
		}
		CommonShipment shipment;
	}
}
