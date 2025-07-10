using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(ShipmentCollection))]
	sealed class ShipmentCollectionTest : ActiveBusinessObjectCollectionTestCase<ShipmentCollection>
	{
		public void TestNewShip_IssuerSCAC_DefaultFrom_Shipment()
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "ZYZW";
			for (var i = 0; i < 5; i++)
			{
				var shipment = trip.Shipments.AddNew();
				AssertEquals("shipment.B0_IssuerSCAC default to trip.BH_CarrierSCAC", trip.BH_CarrierSCAC, shipment.B0_IssuerSCAC);
			}
		}

		protected override ShipmentCollection GetCollectionToTest() => new ShipmentCollection(Factory.New<Trip>());
	}
}
