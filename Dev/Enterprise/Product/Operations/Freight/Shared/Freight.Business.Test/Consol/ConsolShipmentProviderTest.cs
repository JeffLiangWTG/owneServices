using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	public class ConsolShipmentProviderTest : TestCaseWithFactory
	{
		public void TestShipmentsForConsolWithShipments()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();
			var provider = new ConsolShipmentProvider(consol);

			AssertContainsExactElementsInAnyOrder("Shipments should be empty for consol without shipments", new BusinessObject[] { shipment1, shipment2, shipment3 }, provider.Shipments.ToArray());
		}

		public void TestShipmentsForConsolWithNoShipments()
		{
			var consol = Factory.New<CommonConsol>();
			var provider = new ConsolShipmentProvider(consol);

			AssertEquals("Shipments should be empty for consol without shipments", 0, provider.Shipments.Count);
		}

		public void TestShipmentsForNonConsolBusinessObject()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var provider = new ConsolShipmentProvider(dummy);

			AssertNull("Shipments should be null for BusinessObject that is not CommonConsol", provider.Shipments);
		}
	}
}
