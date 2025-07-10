using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingQuotedBooking))]
	sealed class TrackingQuotedBooking_TrackingQuotedBookingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			currentTestQuotedBooking = new TrackingQuotedBooking(quote.PK, booking.PK, true, Factory);
			return currentTestQuotedBooking;
		}

		protected override void TearDown()
		{
			DisposeCurrentTestQuotedBookingJobHeaderMutexes();
			base.TearDown();
		}

		void DisposeCurrentTestQuotedBookingJobHeaderMutexes()
		{
			if (currentTestQuotedBooking != null)
			{
				if (currentTestQuotedBooking.Job != null)
				{
					currentTestQuotedBooking.Job.Dispose();
				}

				if (currentTestQuotedBooking.Booking != null && currentTestQuotedBooking.Booking.Job != null)
				{
					currentTestQuotedBooking.Booking.Job.Dispose();
				}
			}
		}

		TrackingQuotedBooking currentTestQuotedBooking;
	}
}
