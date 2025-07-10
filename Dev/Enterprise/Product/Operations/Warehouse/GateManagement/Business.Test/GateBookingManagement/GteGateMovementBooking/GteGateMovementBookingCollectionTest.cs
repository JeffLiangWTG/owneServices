using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementBookingCollection))]
	public class GteGateMovementBookingCollectionTest : ActiveBusinessObjectCollectionTestCase<GteGateMovementBookingCollection>
	{
		protected override GteGateMovementBookingCollection GetCollectionToTest()
		{
			return new GteGateMovementBookingCollection(Factory.New<GteBooking>());
		}
	}
}
