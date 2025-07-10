using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test.DocumentScanning
{
	internal class QuotedBookingEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			JobHeader job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var viewQuotedBooking = Factory.Load<ViewQuotedBooking>(quotedBooking.PK);
			var loader = new QuotedBookingEDocsViaUniversalXmlSupport();
			AssertEquals(viewQuotedBooking.VB_JS, loader.LoadBusinessObjectFromCode(Factory, viewQuotedBooking.VB_QuoteNumber)?.PK);
		}

		public void TestTryLoadQuotedBookingNotInDb()
		{
			var loader = new QuotedBookingEDocsViaUniversalXmlSupport();
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "00009999"));
		}
	}
}
