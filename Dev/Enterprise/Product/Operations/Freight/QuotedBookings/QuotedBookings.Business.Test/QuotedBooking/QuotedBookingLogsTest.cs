using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingLogs))]
	public class QuotedBookingLogsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var logs = new QuotedBookingLogs(quotedBooking);
			AssertEquals(quotedBooking, logs.Parent);
		}

		public void TestQuotedBookingLogs_SpotQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			quotedBooking.Quote.Logs.AddNew(Events.Authorised);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, ZGuid.Empty, factory);
			var logs = new QuotedBookingLogs(quotedBooking);
			AssertContainsExactElementsInAnyOrder("should not contain quote logs", System.Array.Empty<StmALog>(), logs.GetAllLogs());
		}

		public void TestQuotedBookingLogs_Booking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.Logs.AddNew(Events.Authorised);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(ZGuid.Empty, quotedBooking.Booking.PK, factory);
			var logs = new QuotedBookingLogs(quotedBooking);
			AssertContainsExactElementsInAnyOrder("should not contain booking logs", System.Array.Empty<StmALog>(), logs.GetAllLogs());
		}

		public void TestQuotedBookingLogs_QuotedBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Quote.Logs.AddNew(Events.Authorised);
			quotedBooking.Booking.Logs.AddNew(Events.Authorised);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, quotedBooking.Booking.PK, factory);
			var logs = new QuotedBookingLogs(quotedBooking);
			AssertContainsExactElementsInAnyOrder("should not contain quote and booking logs", System.Array.Empty<StmALog>(), logs.GetAllLogs());
		}

		public void TestQuotedBookingLogs_AddNewEventLog_ForUnconvertedQuotedBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			quotedBooking.Logs.AddNew(Events.CustomisableEvent69);
			AssertEquals("nothing was reported", 0, ErrorReporter.TotalErrorCount);

			quotedBooking.Logs.AddNew(Events.StatusChange);
			AssertEquals("nothing was reported", 0, ErrorReporter.TotalErrorCount);

			quotedBooking.Logs.AddNew(Events.Arrival);
			AssertEquals("nothing was reported", 0, ErrorReporter.TotalErrorCount);

			quotedBooking.Logs.AddNew(Events.DocumentImported);
			AssertEquals("nothing was reported", 0, ErrorReporter.TotalErrorCount);

			quotedBooking.Logs.AddNew(Events.DocumentAllocated);
			AssertEquals("nothing was reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestQuotedBookingLogs_AddNewEventLog_ForConvertedQuotedBooking_DocumentAllocated()
			=> AssertQuotedBookingLogs_AddNewEventLog_ForConvertedQuotedBooking(Events.DocumentAllocated, true);

		public void TestQuotedBookingLogs_AddNewEventLog_ForConvertedQuotedBooking_DocumentImported()
			=> AssertQuotedBookingLogs_AddNewEventLog_ForConvertedQuotedBooking(Events.DocumentImported, false);

		public void TestQuotedBookingLogs_AddNewEventLog_ForConvertedQuotedBooking_CustomisableEvent00()
			=> AssertQuotedBookingLogs_AddNewEventLog_ForConvertedQuotedBooking(Events.CustomisableEvent00, false);

		void AssertQuotedBookingLogs_AddNewEventLog_ForConvertedQuotedBooking(Event cw1Event, bool expectToReport)
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_IsForwardRegistered = ZBool.True;
			Factory.Save();

			quotedBooking.Logs.AddNew(cw1Event);

			if (expectToReport)
			{
				Assert("ErrorReporter was created for log",
					ErrorReporter.LastMessageReported.Contains(quotedBooking.Booking.JS_UniqueConsignRef)
						&& ErrorReporter.LastMessageReported.Contains(cw1Event.Code));

				ErrorReporter.Clear();
			}
			else
			{
				AssertEquals("nothing was reported", 0, ErrorReporter.TotalErrorCount);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new QuotedBookingLogs(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory));
		}
	}
}
