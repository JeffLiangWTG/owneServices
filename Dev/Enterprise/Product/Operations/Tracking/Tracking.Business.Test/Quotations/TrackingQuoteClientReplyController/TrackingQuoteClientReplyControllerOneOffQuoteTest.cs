using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Quotations.Testing
{
	sealed class TrackingQuoteClientReplyControllerOneOffQuoteTest : TrackingQuoteClientReplyControllerTest
	{
		protected override Quote CreateQuoteCore()
		{
			var oneOffQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			return oneOffQuote.Quote;
		}

		#region Client Reply

		[TestDate(2030, 10, 10)]
		public void TestClientReply_Accepted_NoRecipients()
		{
			var client = CreateClient("TESTORG1", "OrgHeader1");
			var quote = CreateQuoteAllowingClientReply(client, "0001");
			Factory.Save();

			var controller = new TrackingQuoteClientReplyController();
			AssertClientReply(controller, quote, expectedIsClientReplyAllowed: true, expectedQuoteStatus: Quote.QuoteStatusOptions.Finalized, expectedLog: AutoEvents.QuotationClientAccepted.Code, expectedLogFound: false, "Precondition");

			var quoteEmailReplyBuilder = new QuoteEmailReplyBuilder(quote);
			controller.ClientResponseIsAcceptedQuotation(quoteEmailReplyBuilder, Contact1);
			AssertClientReply(controller, quote, expectedIsClientReplyAllowed: false, expectedQuoteStatus: Quote.QuoteStatusOptions.ClientAccepted, expectedLog: AutoEvents.QuotationClientAccepted.Code, expectedLogFound: true, "Once it is accepted by the customer, we can't accept again");
			AssertEquals("TH_ClientAccepted", ZDate.Today, quote.TH_ClientAccepted);
			AssertEmail(expectedEmailCount: 0);
		}

		[TestDate(2030, 10, 10)]
		public void TestClientReply_Accepted()
		{
			var client = CreateClient("TESTORG1", "OrgHeader1");
			var quote = CreateQuoteAllowingClientReply(client, "0001");
			Factory.Save();
			CreateEmailRecipients(quote);

			var newFactory = new BusinessObjectFactory();
			var loadedQuote = newFactory.Load<Quote>(quote.PK);
			var controller = new TrackingQuoteClientReplyController();
			AssertClientReply(controller, loadedQuote, expectedIsClientReplyAllowed: true, expectedQuoteStatus: Quote.QuoteStatusOptions.Finalized, expectedLog: AutoEvents.QuotationClientAccepted.Code, expectedLogFound: false, "Precondition");

			var quoteEmailReplyBuilder = new QuoteEmailReplyBuilder(loadedQuote);
			controller.ClientResponseIsAcceptedQuotation(quoteEmailReplyBuilder, Contact1);

			AssertClientReply(controller, loadedQuote, expectedIsClientReplyAllowed: false, expectedQuoteStatus: Quote.QuoteStatusOptions.ClientAccepted, expectedLog: AutoEvents.QuotationClientAccepted.Code, expectedLogFound: true, "Once it is accepted by the customer, we can't accept again");
			AssertEquals("TH_ClientAccepted", ZDate.Today, loadedQuote.TH_ClientAccepted);
			AssertEmail
			(
				expectedEmailCount: 1,
				expectedSubject: "One Off Quote 0001/A is replied by Client OrgHeader1",
				expectedEmailRecipients: "operation.representative1@example.com; sales.representative1@example.com; sales.representative_staff1@example.com",
				expectedEmailBody: new[]
				{
					"One Off Quote 0001/A is replied by Contact1 of TESTORG1 on 10-Oct-30 00:00.",
					"<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=OneOffQuotes&BusinessEntityPK=",
					">View One Off Quote</a>",
					@"<p>Client reply: One Off Quote accepted</p>
<br/>
<p>One Off Quote Start Date: 10-Sep-30</p>
<p>Expiry Date: 10-Nov-30</p>
<p>Created Time: 10-Oct-30 00:00</p>
<p>Finalized Time: 10-Oct-30 00:00</p>
</body>
</html>"
				}
			);
		}

		[TestDate(2030, 10, 10)]
		public void TestClientReply_RequestedDiscussion()
		{
			var client = CreateClient("TESTORG1", "OrgHeader1");
			var quote = CreateQuoteAllowingClientReply(client, "0001");
			Factory.Save();

			CreateEmailRecipients(quote);

			var newFactory = new BusinessObjectFactory();
			var loadedQuote = newFactory.Load<Quote>(quote.PK);
			var controller = new TrackingQuoteClientReplyController();
			AssertClientReply(controller, quote, expectedIsClientReplyAllowed: true, expectedQuoteStatus: Quote.QuoteStatusOptions.Finalized, expectedLog: AutoEvents.QuotationClientRequestedDiscussion.Code, expectedLogFound: false, "Precondition");

			var quoteEmailReplyBuilder = new QuoteEmailReplyBuilder(loadedQuote);
			controller.ClientResponseIsRequestFurtherDiscussion(quoteEmailReplyBuilder, Contact1);

			AssertClientReply(controller, loadedQuote, expectedIsClientReplyAllowed: true, expectedQuoteStatus: Quote.QuoteStatusOptions.Finalized, expectedLog: AutoEvents.QuotationClientRequestedDiscussion.Code, expectedLogFound: true, "Can still be accepted if 'Request further discussion' was picked");
			AssertEquals("TH_ClientAccepted", ZDateTime.Empty, loadedQuote.TH_ClientAccepted);

			AssertEmail
			(
				expectedEmailCount: 1,
				expectedSubject: "One Off Quote 0001/A is replied by Client OrgHeader1",
				expectedEmailRecipients: "operation.representative1@example.com; sales.representative1@example.com; sales.representative_staff1@example.com",
				expectedEmailBody: new[]
				{
					"One Off Quote 0001/A is replied by Contact1 of TESTORG1 on 10-Oct-30 00:00.",
					"<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=OneOffQuotes&BusinessEntityPK=",
					">View One Off Quote</a>",
					@"<p>Client reply: Request further discussion</p>
<br/>
<p>One Off Quote Start Date: 10-Sep-30</p>
<p>Expiry Date: 10-Nov-30</p>
<p>Created Time: 10-Oct-30 00:00</p>
<p>Finalized Time: 10-Oct-30 00:00</p>
</body>
</html>"
				}
			);
		}

		void CreateEmailRecipients(Quote quote)
		{
			var firstSignatory = Factory.NewWithValidTestData<GlbStaff>();
			firstSignatory.GS_EmailAddress = "first.signatory@example.com";
			var secondSignatory = Factory.NewWithValidTestData<GlbStaff>();
			secondSignatory.GS_EmailAddress = "second.signatory@example.com";
			var salesRepStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			salesRepStaff1.GS_EmailAddress = "sales.representative_staff1@example.com";

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_EmailAddress = "sales.representative1@example.com";
			var operationRep = Factory.NewWithValidTestData<GlbStaff>();
			operationRep.GS_EmailAddress = "operation.representative1@example.com";

			((QuotedBooking)quote.ParentQuotedBooking).TryLoadOrCreateJob();
			quote.ParentQuotedBooking.Job.JH_GS_NKRepSales = salesRep.GS_Code;
			quote.ParentQuotedBooking.Job.JH_GS_NKRepOps = operationRep.GS_Code;

			Factory.Save();

			quote.TH_GS_NKFirstSignatory = firstSignatory.GS_Code;
			quote.TH_GS_NKSecondSignatory = secondSignatory.GS_Code;
			var salesRepAssignment = quote.HeaderStaffAssignments.AddNew();
			salesRepAssignment.O8_Department = "ALL";
			salesRepAssignment.O8_Role = "SAL";
			salesRepAssignment.O8_GS_NKPersonResponsible = salesRepStaff1.GS_Code;

			Factory.Save();
		}

		#endregion
	}
}
