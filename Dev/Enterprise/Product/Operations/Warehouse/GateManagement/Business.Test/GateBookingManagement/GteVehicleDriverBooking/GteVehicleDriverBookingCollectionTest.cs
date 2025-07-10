using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleDriverBookingCollection))]
	public class GteVehicleDriverBookingCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GteVehicleDriverBookingCollection(Factory.New<GteBooking>());
		}
	}
}

