using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(RemoveBookingHeldStatusActionMethodApplicator))]
	public class RemoveBookingHeldStatusActionMethodApplicatorTest : DtbBookingOperationalActionMethodApplicatorTest
	{
		public void TestAction()
		{
			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "TM00001";
			booking1.KM_Status = TransportStatuses.Codes.Held;

			var booking2 = Helper.CreateBooking();
			booking2.KM_JobID = "TM00002";
			booking2.KM_Status = TransportStatuses.Codes.Available;

			Factory.Save();

			var bookings = new DtbBooking[] { booking1, booking2 };
			ApplyApplicator(bookings,
@"INFO: [HL TM00001] - The action 'Remove Held Status' was successfully processed.
WARNING: [HL TM00002] - The booking status must be 'Held' to make this change.
".Trim());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RemoveBookingHeldStatusActionMethodApplicator();
		}
	}
}
