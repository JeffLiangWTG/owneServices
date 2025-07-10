using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Confirmations.Business.Testing
{
	public class QuickPODMultipleShipmentsEventArgsTest : TestCaseWithFactory
	{
		public void TestQuickPODMultipleShipmentsEventArgs()
		{
			ShipmentCollection shipments = new ShipmentCollection(Factory);
			CommonShipment shipment1 = shipments.AddNew();
			CommonShipment shipment2 = shipments.AddNew();
			CommonShipment shipment3 = shipments.AddNew();

			QuickPODMultipleShipmentsEventArgs e = new QuickPODMultipleShipmentsEventArgs("S101", shipments);
			AssertSame("Shipments", shipments, e.Shipments);
			AssertEquals("Selected Shipment should default to 1st", shipment1, e.SelectedShipment);
		}
	}
}
