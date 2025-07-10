using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(QuoteClientReplyRequestHandler))]
	sealed class QuoteClientReplyRequestHandlerOneOffQuoteTest : QuoteClientReplyRequestHandlerTest
	{
		protected override Quote CreateQuote()
		{
			var oneOffQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			return oneOffQuote.Quote;
		}

		protected override string QuoteBusinessObjectName => "One Off Quote";

		protected override string ExpectedAcceptQuotationWithNoClientReplyAllowed => "You will need to be granted Web Quoting security access to accept One Off Quote through e-mail. Please check with your Administrator.";

		protected override string ExpectedRequestFurtherDiscussionWithNoClientReplyAllowed => "You will need to be granted Web Quoting security access to request for discussion via e-mail. Please check with your Administrator.";
	}
}
