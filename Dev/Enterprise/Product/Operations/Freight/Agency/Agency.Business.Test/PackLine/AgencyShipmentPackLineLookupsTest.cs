using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyShipmentPackLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainers()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentPackLine packline = shipment.OuterPackLines.AddNew();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertSame(shipment.BookedContainers, packline.Lookups.Containers);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertSame(shipment.RealContainers, packline.Lookups.Containers);
		}

		public void TestContainersWithNullShipment()
		{
			var shipment = Factory.New<AgencyShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.Delete();

			AssertEquals(0, packline.Lookups.Containers.Count);
		}
	}
}
