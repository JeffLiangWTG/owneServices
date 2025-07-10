using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.Module.Testing
{
	public abstract class BookingStatusChangeActionMethodApplicatorTest : DtbBookingOperationalActionMethodApplicatorTest
	{
		public void TestAction()
		{
			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "TM00001";
			booking1.KM_Status = ExpectedInvalidStatusToChangeFrom;

			var firstBookingNumber = 1;
			foreach (var expectedStatusToChangeFrom in ExpectedStatusesToChangeFrom)
			{
				firstBookingNumber++;
				var booking2 = Helper.CreateBooking();
				booking2.KM_JobID = "TM0000" + firstBookingNumber;
				booking2.KM_Status = expectedStatusToChangeFrom;

				Factory.Save();

				var bookings = new[] { booking1, booking2 };
				var statusToChangeTo = booking1.Lookups.BindToLists.Statuses.GetDescriptionFromCode(ExpectedStatusToChangeTo);
				var statusesToChangeFrom = string.Join(", ", ExpectedStatusesToChangeFrom.Select(s => bookings[0].Lookups.BindToLists.Statuses.GetDescriptionFromCode(s)));

				AssertApplyApplicator(bookings, booking1.StatusDescription, statusToChangeTo, statusesToChangeFrom, booking2.KM_JobID);
			}
		}

		protected void AssertApplyApplicator(DtbBooking[] bookings, ZString statusDecription, ZString statusToChangeTo, string statusesToChangeFrom, string jobID)
		{
			ApplyApplicator(bookings,
string.Format(@"WARNING: [HL TM00001] - The Booking cannot be made {0} because the status is {1}. Only Bookings with a status of {2} can be processed.
INFO: [HL {3}] - The action '{4}' was successfully processed.
".Trim(), statusToChangeTo, statusDecription, statusesToChangeFrom, jobID, Applicator.Name));
		}

		protected abstract IEnumerable<ZString> ExpectedStatusesToChangeFrom { get; }
		protected abstract ZString ExpectedInvalidStatusToChangeFrom { get; }
		protected abstract ZString ExpectedStatusToChangeTo { get; }
	}
}
