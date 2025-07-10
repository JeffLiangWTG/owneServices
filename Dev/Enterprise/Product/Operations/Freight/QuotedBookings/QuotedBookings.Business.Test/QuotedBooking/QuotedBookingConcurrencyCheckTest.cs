using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	sealed class QuotedBookingConcurrencyCheckTest : TestCaseWithFactory
	{
		public void TestRegister()
		{
			var factory = new BusinessObjectFactory();
			var booking = QuotedBooking.CreateNewBooking(factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, factory);

			Assert("concurrency check has been registered for booking",
				QuotedBookingConcurrencyCheck.IsRegisteredForBooking(factory, booking.PK));

			Assert("concurrency check has been registered for another booking pk",
				!QuotedBookingConcurrencyCheck.IsRegisteredForBooking(factory, new ZGuid()));
		}

		public void TestConcurrencyCheck_ForQuickBooking()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var bookingInFactory1 = QuotedBooking.CreateNewBooking(factory1);
			var quotedBookingInFactory1 = QuotedBooking.New(ZGuid.Empty, bookingInFactory1.PK, factory1);
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var quotedBookingInFactory2 = QuotedBooking.New(ZGuid.Empty, bookingInFactory1.PK, factory2);
			var bookingInFactory2 = quotedBookingInFactory2.Booking;

			bookingInFactory1.JS_IsForwardRegistered = true;
			factory1.Save();

			bookingInFactory2.WorkflowItems.Triggers.AddNew();

			try
			{
				factory2.Save();
				Fail("Expected to throw ZCannotSaveException");
			}
			catch (ZCannotSaveException ex)
			{
				Assert(ex.Data.Contains("RegistrationDateTime(UTC)"));
				Assert(ex.Data.Contains("ExceptionDateTime(UTC)"));
				AssertQuotedBookingCreationStackTrace(bookingInFactory2.PK, ex, "TestConcurrencyCheck_ForQuickBooking");
			}
		}

		public void TestConcurrencyCheck_ForQuotedBooking()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var quoteInFactory1 = QuotedBooking.CreateNewQuote(factory1, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var bookingInFactory1 = QuotedBooking.CreateNewBooking(factory1);
			var quotedBookingInFactory1 = QuotedBooking.New(quoteInFactory1.PK, bookingInFactory1.PK, factory1);
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var quotedBookingInFactory2 = QuotedBooking.New(quoteInFactory1.PK, bookingInFactory1.PK, factory2);
			var bookingInFactory2 = quotedBookingInFactory2.Booking;

			bookingInFactory1.JS_IsForwardRegistered = true;
			factory1.Save();

			bookingInFactory2.WorkflowItems.Triggers.AddNew();

			try
			{
				factory2.Save();
				Fail("Expected to throw ZCannotSaveException");
			}
			catch (ZCannotSaveException ex)
			{
				Assert(ex.Data.Contains("RegistrationDateTime(UTC)"));
				Assert(ex.Data.Contains("ExceptionDateTime(UTC)"));
				AssertQuotedBookingCreationStackTrace(bookingInFactory2.PK, ex, "TestConcurrencyCheck_ForQuotedBooking");
			}
		}

		void AssertQuotedBookingCreationStackTrace(ZGuid bookingPK, Exception exc, string expectedToContainInStackTrace)
		{
			var key = $"QuotedBookingCreationCallStack [PK:{bookingPK}]";
			var stackTrace = (string)exc.Data[key];

			AssertContains($"Expected to contain creation stacktrace for booking with PK {bookingPK}",
				expectedToContainInStackTrace, stackTrace, ignoreCase: true);
		}
	}
}
