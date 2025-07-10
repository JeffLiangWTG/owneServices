using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteBookingProcessTaskCollection))]
	public class GteBookingProcessTaskCollectionTest : ProcessTaskCollectionTest<GteBookingProcessTaskCollection>
	{
		protected override GteBookingProcessTaskCollection GetCollectionToTestCore()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			return new GteBookingProcessTaskCollection(booking);
		}
	}
}
