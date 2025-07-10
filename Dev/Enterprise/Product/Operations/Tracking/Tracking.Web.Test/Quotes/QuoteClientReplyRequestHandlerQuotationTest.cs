using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(QuoteClientReplyRequestHandler))]
	sealed class QuoteClientReplyRequestHandlerQuotationTest : QuoteClientReplyRequestHandlerTest
	{
		protected override Quote CreateQuote() => Factory.NewWithValidTestData<Quote>();

		protected override string QuoteBusinessObjectName => "Quotation";

		protected override string ExpectedRequestFurtherDiscussionWithNoClientReplyAllowed => "Request for discussion could NOT be sent for {0} {1} at this time.";

		protected override string ExpectedAcceptQuotationWithNoClientReplyAllowed => "{0} {1} could NOT be accepted at this time.";
	}
}
