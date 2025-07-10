using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.QuotedBookings.Business.Testing
{
	sealed class QuotedBookingVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		#region TestGetParentInAnotherFactory_BookingWithQuote

		public void TestGetParentInAnotherFactory_BookingWithQuote()
		{
			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			Factory.Save();

			var supporter = new QuotedBookingVisualizableDocumentSupporter();

			var factory = new BusinessObjectFactory();

			if (supporter.GetBusinessObjectInAnotherFactory(factory, bookingWithQuote) is QuotedBooking quotedBooking)
			{
				AssertNotEquals("Created different instance of QuotedBooking", bookingWithQuote, quotedBooking);
				AssertEquals("Created QuotedBooking in another factory", factory, quotedBooking.Factory);
				AssertEquals("Created QuotedBooking booking PK matches original", bookingWithQuote.Booking.PK, quotedBooking.Booking.PK);
				AssertEquals("Created QuotedBooking quote ok matches original", bookingWithQuote.Quote.PK, quotedBooking.Quote.PK);
			}
			else
			{
				Fail("GetParentInAnotherFactory should return QuotedBooking");
			}
		}

		#endregion

		#region TestGetParentInAnotherFactory_QuickBooking

		public void TestGetParentInAnotherFactory_QuickBooking()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			Factory.Save();

			var supporter = new QuotedBookingVisualizableDocumentSupporter();

			var factory = new BusinessObjectFactory();

			if (supporter.GetBusinessObjectInAnotherFactory(factory, booking) is QuotedBooking quotedBooking)
			{
				AssertNotEquals("Created different instance of QuotedBooking", booking, quotedBooking);
				AssertEquals("Created QuotedBooking in another factory", factory, quotedBooking.Factory);
				AssertEquals("Created QuotedBooking booking PK matches original", booking.Booking.PK, quotedBooking.Booking.PK);
				AssertNull("Created QuotedBooking quote is null", quotedBooking.Quote);
			}
			else
			{
				Fail("GetParentInAnotherFactory should return QuotedBooking");
			}
		}

		#endregion

		#region TestGetParentInAnotherFactory_SpotQuote

		public void TestGetParentInAnotherFactory_SpotQuote()
		{
			var quote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			Factory.Save();

			var supporter = new QuotedBookingVisualizableDocumentSupporter();

			var factory = new BusinessObjectFactory();

			if (supporter.GetBusinessObjectInAnotherFactory(factory, quote) is QuotedBooking quotedBooking)
			{
				AssertNotEquals("Created different instance of QuotedBooking", quote, quotedBooking);
				AssertEquals("Created QuotedBooking in another factory", factory, quotedBooking.Factory);
				AssertNull("Created QuotedBooking booking is null", quotedBooking.Booking);
				AssertEquals("Created QuotedBooking quote ok matches original", quote.Quote.PK, quotedBooking.Quote.PK);
			}
			else
			{
				Fail("GetParentInAnotherFactory should return QuotedBooking");
			}
		}

		#endregion

		#region TestGetLibraries

		public void TestGetLibraries()
		{
			var supporter = new QuotedBookingVisualizableDocumentSupporter();
			var libraries = supporter.GetLibraries(DataContext.HouseBill).ToArray();

			AssertEquals("1 library expected for HouseBill data context", 1, libraries.Length);
			Assert("", libraries.First() is IHouseBillMacroLibrary);

			Assert("no libraries for other data context", !supporter.GetLibraries(DataContext.BookingRequest).ToArray().Any());
		}

		#endregion

		#region TestGetDocDataObject

		public void TestGetDocDataObject()
		{
			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			Factory.Save();

			var supporter = new QuotedBookingVisualizableDocumentSupporter();
			var docDataObject = supporter.GetDocDataObject(bookingWithQuote, DataContext.HouseBill, null);

			AssertNotNull("Created wrapper for HouseBill data context", docDataObject);
		}

		#endregion
	}
}
