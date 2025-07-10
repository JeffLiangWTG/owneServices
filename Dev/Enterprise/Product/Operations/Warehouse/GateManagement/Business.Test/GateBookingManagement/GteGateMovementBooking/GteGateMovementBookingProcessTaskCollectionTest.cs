using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementBookingProcessTaskCollection))]
	public class GteGateMovementBookingProcessTaskCollectionTest : ProcessTaskCollectionTest<GteGateMovementBookingProcessTaskCollection>
	{
		protected override GteGateMovementBookingProcessTaskCollection GetCollectionToTestCore()
		{
			var transportationUnit = Factory.NewWithValidTestData<GteGateMovementBooking>();
			return new GteGateMovementBookingProcessTaskCollection(transportationUnit);
		}
	}
}
