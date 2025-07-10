using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentCollectionTest : TestCaseWithFactory
	{
		public void TestIndexer()
		{
			ShipmentCollection shipments = new ShipmentCollection(Factory);
			CommonShipment shipment1 = shipments.AddNew();
			AssertEquals(shipment1, shipments[0]);

			CommonShipment shipment2 = shipments.AddNew();
			AssertEquals(shipment2, shipments[1]);
		}
	}
}
