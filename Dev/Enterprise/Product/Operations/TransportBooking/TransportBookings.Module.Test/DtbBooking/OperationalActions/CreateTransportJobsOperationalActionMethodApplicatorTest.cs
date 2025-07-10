using System;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CreateTransportJobsOperationalActionMethodApplicator))]
	public class CreateTransportJobsOperationalActionMethodApplicatorTest : DtbBookingOperationalActionMethodApplicatorTest
	{
		public void TestCreateTransportJobsOperationalAction()
		{
			var booking1 = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			booking1.KM_JobID = "TM00001";
			var booking2 = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			booking2.KM_JobID = "TM00002";
			Factory.Save();

			var expectedMessage = @"INFO: Successfully created Port Transport T00001000 from Transport Booking TM00001
INFO: Successfully created Port Transport T00001001 from Transport Booking TM00002";

			var bookings = new DtbBooking[] { booking1, booking2 };
			AssertNoExceptionThrown("No Exception expected", () => ApplyApplicator(bookings, expectedMessage));
		}

		public void TestCreateTransportJobsOperationalAction_WhenSomeTargetsHaveChanges()
		{
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			booking.KM_JobID = "TM00001";
			var bookingWithChanges = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			bookingWithChanges.KM_JobID = "TM00002";
			Factory.Save();
			bookingWithChanges.KM_Description = "The description has been changed!";

			Assert("Precondition - Booking with changes is in the DB but has changes.", bookingWithChanges.IsInDatabase && bookingWithChanges.HasChanges);
			Assert("Precondition - Booking without changes is in the DB and has no changes.", booking.IsInDatabase && !booking.HasChanges);

			var expectedMessage = @"ERROR: Please save the Booking before creating a Transport Job.
INFO: Successfully created Port Transport T00001000 from Transport Booking TM00001";

			var bookings = new DtbBooking[] { booking, bookingWithChanges };
			ApplyApplicator(bookings, expectedMessage);
		}

		public void TestCreateTransportJobsOperationalAction_ErrorsOnEmptyTargets()
		{
			var expectedMessage = "ERROR: Selected Transport Bookings need to exist in database.";

			var bookings = Array.Empty<DtbBooking>();
			AssertNoExceptionThrown("No Developer Exception expected", () => ApplyApplicator(bookings, expectedMessage));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreateTransportJobsOperationalActionMethodApplicator("Print Cartage Advice", Factory);
		}
	}
}
