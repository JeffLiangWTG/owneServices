using System.Web;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Moq;
using NUnit.Framework;
using static Enterprise.Tracking.Web.QuoteClientReplyRequestHandler;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(QuoteClientReplyRequestHandler))]
	abstract class QuoteClientReplyRequestHandlerTest : DataRequestHandlerTestCase<QuoteClientReplyRequestHelper>
	{
		public void TestMimeType()
		{
			var handler = new QuoteClientReplyRequestHandler();
			AssertEquals("text/plain", handler.ContentType);
		}

		public void TestClientReply_GivenAcceptQuotation_ThenShouldResponseOK()
		{
			var quote = CreateQuoteAllowingClientReply();
			TestClientReply
			(
				quote,
				ClientReply.AcceptQuotation,
				isClientReplyAllowed: true,
				expectedResponse: $"{QuoteBusinessObjectName} {quote.TH_QuoteNumber} is accepted. Thank you."
			);
		}

		public void TestClientReply_GivenAcceptQuotatioWithNoClientReplyAllowed_ThenShouldResponseError()
		{
			var quote = CreateQuoteDisallowingClientResponse();
			TestClientReply
			(
				quote,
				ClientReply.AcceptQuotation,
				isClientReplyAllowed: false,
				expectedResponse: string.Format(ExpectedAcceptQuotationWithNoClientReplyAllowed, QuoteBusinessObjectName, quote.TH_QuoteNumber)
			);
		}

		public void TestClientReply_GivenRequestFurtherDiscussion_ThenShouldResponseOK()
		{
			var quote = CreateQuoteAllowingClientReply();
			TestClientReply
			(
				quote,
				ClientReply.RequestFurtherDiscussion,
				isClientReplyAllowed: true,
				expectedResponse: $"Request for discussion is sent for {QuoteBusinessObjectName} {quote.TH_QuoteNumber}. Thank you."
			);
		}

		public void TestClientReply_GivenRequestFurtherDiscussionWithNoClientReplyAllowed_ThenShouldResponseError()
		{
			var quote = CreateQuoteAllowingClientReply();
			TestClientReply
			(
				quote,
				ClientReply.RequestFurtherDiscussion,
				isClientReplyAllowed: false,
				expectedResponse: string.Format(ExpectedRequestFurtherDiscussionWithNoClientReplyAllowed, QuoteBusinessObjectName, quote.TH_QuoteNumber)
			);
		}

		void TestClientReply(Quote quote, ClientReply clientReply, bool isClientReplyAllowed, string expectedResponse)
		{
			var trackingQuoteClientReplyController = new Mock<ITrackingQuoteClientReplyController>();
			trackingQuoteClientReplyController.Setup(x => x.IsClientReplyAllowed(It.IsAny<Quote>(), It.IsAny<TrackingSiteUser>())).Returns(isClientReplyAllowed);

			var quoteClientReplyRequestHandler = new QuoteClientReplyRequestHandler();
			quoteClientReplyRequestHandler.QueryString.Add(DataRequestHelper.DataKey, quote.PK.ToString());
			quoteClientReplyRequestHandler.QueryString.Add(nameof(ClientReply), clientReply.ToString());
			quoteClientReplyRequestHandler.SetControllerForTest(trackingQuoteClientReplyController.Object);

			var response = quoteClientReplyRequestHandler.GetBinaryData();

			AssertEquals("Response IsRequestBeingRedirected", false, HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertNullOrEmpty(string.Empty, HttpContext.Current.Response.RedirectLocation);
			AssertEquals("Response", expectedResponse, response.ToAscii());
			trackingQuoteClientReplyController.VerifyAll();
		}

		#region Implementation 

		public override void TestWithBinaryData()
		{
			AssertNotNull("I do not have an attachment to download");
		}

		public override void TestGetBinaryDataWithLock()
		{
			AssertNotNull("Nothing to lock in this class");
		}

		Quote CreateQuoteDisallowingClientResponse()
		{
			var quote = CreateQuote();
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-1);
			quote.TH_QuoteEndDate = quote.TH_QuoteDate.AddMonths(2);
			quote.ShowApprovalDialog += (sender, e) => { e.Cancel = false; };
			quote.InternalApproveQuote();
			quote.TH_IsLocked = false;

			Factory.Save();

			return quote;
		}

		Quote CreateQuoteAllowingClientReply()
		{
			var quote = CreateQuote();
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-1);
			quote.TH_QuoteEndDate = quote.TH_QuoteDate.AddMonths(2);
			quote.ShowApprovalDialog += (sender, e) => { e.Cancel = false; };
			quote.InternalApproveQuote();
			quote.TH_IsLocked = true;

			Factory.Save();

			return quote;
		}

		// This method is needed to keep the base class tests happy.
		protected override DataRequestHandler<QuoteClientReplyRequestHelper> GetNewRequestHandler()
		{
			var quote = CreateQuoteAllowingClientReply();

			var handler = new QuoteClientReplyRequestHandler();
			handler.QueryString.Add(DataRequestHelper.DataKey, quote.PK.ToString());
			handler.QueryString.Add(nameof(ClientReply), nameof(ClientReply.AcceptQuotation));

			return handler;
		}

		protected abstract Quote CreateQuote();

		protected abstract string QuoteBusinessObjectName { get; }

		protected abstract string ExpectedRequestFurtherDiscussionWithNoClientReplyAllowed { get; }

		protected abstract string ExpectedAcceptQuotationWithNoClientReplyAllowed { get; }

		#endregion
	}
}
