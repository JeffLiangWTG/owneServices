using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public class WebPartyProviderTest : TestCaseWithFactory
	{
		public void TestGetUpdatableMilestoneEventCodes_NonSupportedRecord()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var actionResult = WebPartyProvider.GetWebParties(dummy);
			AssertNotNull(actionResult);
			AssertEquals(0, actionResult.WebPartyTypes.Count);
		}

		public void TestGetUpdatableMilestoneEventCodes_EmptyShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var actionResult = WebPartyProvider.GetWebParties(shipment);
			AssertNotNull(actionResult);
			AssertEquals(0, actionResult.WebPartyTypes.Count);
		}

		public void TestGetUpdatableMilestoneEventCodes_ShipmentWithWebParties()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = orgConsignor.PK;

			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = orgConsignee.PK;

			var orgJobLocalCharges = Factory.NewWithValidTestData<OrgHeader>();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = orgJobLocalCharges.PK;

			var consol = Factory.New<ForwardingConsol>();
			var orgConsolSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsolReceivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_UniqueConsignRef = "CONSOL";
			consol.JK_OA_SendingForwarderAddress = orgConsolSendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgConsolReceivingForwarder.MainAddress.PK;
			shipment.Consols.Add(consol);

			var orgDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_DeliveryAgent = orgDeliveryAgent.PK;

			var orgImportBroker = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_ImportBroker = orgImportBroker.PK;

			var orgExportBroker = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_ExportBroker = orgExportBroker.PK;
			Factory.Save();

			var actionResult = WebPartyProvider.GetWebParties(shipment);
			AssertNotNull(actionResult);
			AssertEquals(8, actionResult.WebPartyTypes.Count);
			AssertEquals(orgConsignor.PK, actionResult[WebPartyType.Shipper][0].PK);
			AssertEquals(orgConsignee.PK, actionResult[WebPartyType.Consignee][0].PK);
			AssertEquals(orgJobLocalCharges.PK, actionResult[WebPartyType.LocalClient][0].PK);
			AssertEquals(orgConsolSendingForwarder.PK, actionResult[WebPartyType.SendingAgent][0].PK);
			AssertEquals(orgConsolReceivingForwarder.PK, actionResult[WebPartyType.ReceivingAgent][0].PK);
			AssertEquals(orgDeliveryAgent.PK, actionResult[WebPartyType.DeliveryAgent][0].PK);
			AssertEquals(orgImportBroker.PK, actionResult[WebPartyType.ImportBroker][0].PK);
			AssertEquals(orgExportBroker.PK, actionResult[WebPartyType.ExportBroker][0].PK);
		}
	}
}
