using CargoWise.EntityFramework.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class ShipmentDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCollections()
		{
			IShipmentDeclaration parentBizO = Factory.New<TrackingShipment>();

			AssertNotNull("Ports", parentBizO.ShipmentDeclarationLookups.Ports);
			AssertNotNull("Currencies", parentBizO.ShipmentDeclarationLookups.Currencies);
			AssertNotNull("ServiceLevels", parentBizO.ShipmentDeclarationLookups.ServiceLevels);
		}
	}
}
