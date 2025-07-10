using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentFieldStateChangeTest : TestCaseWithFactory
	{
		public void TestNestedInitialization()
		{
			var shipmentFieldChange = new ShipmentFieldStateChange();
			var nested = new VeryNested();
			var count = 20;
			var disposable = nested.CreateNestedShipments(Factory, ref count);

			foreach (var shipment in nested.Shipments)
			{
				AssertEquals(false, shipmentFieldChange.IsObjectInitialized(shipment));
			}

			disposable.Dispose();

			foreach (var shipment in nested.Shipments)
			{
				AssertEquals(true, shipmentFieldChange.IsObjectInitialized(shipment));
			}
		}
	}
}
