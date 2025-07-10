using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class DiscrepancyFormTest : TestCaseWithFactory
	{
		public void TestButtonLabels()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			using (var form = new DiscrepancyForm(quotedBooking, false))
			{
				var useQuotedPriceButton = form.Controls.Find("UseQuotedPriceButton", true).First() as ZButton;
				var reRateButton = form.Controls.Find("ReRateButton", true).First() as ZButton;
				AssertEquals("Use Quoted Price", useQuotedPriceButton.CaptionResourceString.Caption);
				AssertEquals("Re-Rate", reRateButton.CaptionResourceString.Caption);
			}

			using (var form = new DiscrepancyForm(quotedBooking, true))
			{
				var useQuotedPriceButton = form.Controls.Find("UseQuotedPriceButton", true).First() as ZButton;
				var reRateButton = form.Controls.Find("ReRateButton", true).First() as ZButton;
				AssertEquals("Use Quoted Price && Save", useQuotedPriceButton.CaptionResourceString.Caption);
				AssertEquals("Re-Rate && Save", reRateButton.CaptionResourceString.Caption);
			}
		}
	}
}
