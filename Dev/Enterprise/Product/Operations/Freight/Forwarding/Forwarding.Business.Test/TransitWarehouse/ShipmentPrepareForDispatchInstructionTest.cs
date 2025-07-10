using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ShipmentPrepareForDispatchInstruction))]
	sealed class ShipmentPrepareForDispatchInstructionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentPrepareForDispatchInstruction(Factory.NewWithValidTestData<ForwardingShipment>(), Factory.NewWithValidTestData<OrgAddress>());
		}

		public void TestPrepareDispatchStatus_Sent()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var header = address.Header;
			header.OH_Code = "yey";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Logs.AddNew(AutoEvents.ServiceRequested, $"|FAC=CFS|WHS=yey");

			var shipmentForPrepareDispatch = new ShipmentPrepareForDispatchInstruction(shipment, address);
			AssertEquals("shipment status is SENT", "Sent", shipmentForPrepareDispatch.PrepareDispatchStatus);
		}

		public void TestPrepareDispatchStatus_NotSent()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var header = address.Header;
			header.OH_Code = "yey";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var shipmentForPrepareDispatch = new ShipmentPrepareForDispatchInstruction(shipment, address);
			AssertEquals("shipment status is Not Sent", "Not Sent", shipmentForPrepareDispatch.PrepareDispatchStatus);
		}

		public void TestIsValidToSend_Valid()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			packline.JL_OA_LastKnownTransitWarehouseAddress = address.PK;
			packline.JL_LastKnownTransitWarehouseStatus = "RCV";

			var shipmentForPrepareDispatch = new ShipmentPrepareForDispatchInstruction(shipment, address);
			AssertEquals("Shipment is valid to Send", true, shipmentForPrepareDispatch.IsValidToSend);
		}

		public void TestIsValidToSend_Invalid()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			packline.JL_OA_LastKnownTransitWarehouseAddress = address.PK;
			packline.JL_LastKnownTransitWarehouseStatus = "DSP";

			var shipmentForPrepareDispatch = new ShipmentPrepareForDispatchInstruction(shipment, address);
			AssertEquals("Shipment is valid to Send", false, shipmentForPrepareDispatch.IsValidToSend);
		} 
	}
}
