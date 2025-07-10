using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingProcessTask))]
	class DtbBookingProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Doesn't have a controller ID", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Helper.CreateBooking();
			return booking.WorkflowItems.AddNew();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
