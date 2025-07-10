using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Quotations.Testing
{
	sealed class TrackingQuoteClientReplyControllerQuotationsTest : TrackingQuoteClientReplyControllerTest
	{
		protected override Quote CreateQuoteCore() => Factory.NewWithValidTestData<Quote>();

		#region Client Reply

		[TestDate(2030, 10, 10)]
		public void TestClientReply_Accepted()
		{
			var client = CreateClient("TESTORG1", "OrgHeader1");
			var quote = CreateQuoteAllowingClientReply(client, "0001");
			Factory.Save();
			CreateEmailRecipients(quote);

			var controller = new TrackingQuoteClientReplyController();
			AssertClientReply(controller, quote, expectedIsClientReplyAllowed: true, expectedQuoteStatus: Quote.QuoteStatusOptions.Finalized, expectedLog: AutoEvents.QuotationClientAccepted.Code, expectedLogFound: false, "Precondition");

			var quoteEmailReplyBuilder = new QuoteEmailReplyBuilder(quote);
			controller.ClientResponseIsAcceptedQuotation(quoteEmailReplyBuilder, Contact1);
			AssertClientReply(controller, quote, expectedIsClientReplyAllowed: false, expectedQuoteStatus: Quote.QuoteStatusOptions.ClientAccepted, expectedLog: AutoEvents.QuotationClientAccepted.Code, expectedLogFound: true, "Once it is accepted by the customer, we can't accept again");
			AssertEquals("TH_ClientAccepted", ZDate.Today, quote.TH_ClientAccepted);
			AssertEmail
			(
				expectedEmailCount: 1,
				expectedSubject: "Quotation 0001/A is replied by Client OrgHeader1",
				expectedEmailRecipients: "first.signatory@example.com; second.signatory@example.com; sales.representative@example.com",
				expectedEmailBody: new[]
				{
					"Quotation 0001/A is replied by Contact1 of TESTORG1 on 10-Oct-30 00:00.",
					"<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=Quotation&BusinessEntityPK=",
					">View Quotation</a>",
					@"<p>Client reply: Quotation accepted</p>
<br/>
<p>Quotation Start Date: 10-Sep-30</p>
<p>Quotation Expiry Date: 10-Nov-30</p>
<p>Quotation Created Time: 10-Oct-30 00:00</p>
<p>Quotation Finalized Time: 10-Oct-30 00:00</p>
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

			var controller = new TrackingQuoteClientReplyController();
			AssertClientReply(controller, quote, expectedIsClientReplyAllowed: true, expectedQuoteStatus: Quote.QuoteStatusOptions.Finalized, expectedLog: AutoEvents.QuotationClientRequestedDiscussion.Code, expectedLogFound: false, "Precondition");

			var quoteEmailReplyBuilder = new QuoteEmailReplyBuilder(quote);
			controller.ClientResponseIsRequestFurtherDiscussion(quoteEmailReplyBuilder, Contact1);

			AssertClientReply(controller, quote, expectedIsClientReplyAllowed: true, expectedQuoteStatus: Quote.QuoteStatusOptions.Finalized, expectedLog: AutoEvents.QuotationClientRequestedDiscussion.Code, expectedLogFound: true, "Can still be accepted if 'Request further discussion' was picked");
			AssertEquals("TH_ClientAccepted", ZDateTime.Empty, quote.TH_ClientAccepted);

			AssertEmail
			(
				expectedEmailCount: 1,
				expectedSubject: "Quotation 0001/A is replied by Client OrgHeader1",
				expectedEmailRecipients: "first.signatory@example.com; second.signatory@example.com; sales.representative@example.com",
				expectedEmailBody: new[]
				{
					"Quotation 0001/A is replied by Contact1 of TESTORG1 on 10-Oct-30 00:00.",
					"<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=Quotation&BusinessEntityPK=",
					">View Quotation</a>",
					@"<p>Client reply: Request further discussion</p>
<br/>
<p>Quotation Start Date: 10-Sep-30</p>
<p>Quotation Expiry Date: 10-Nov-30</p>
<p>Quotation Created Time: 10-Oct-30 00:00</p>
<p>Quotation Finalized Time: 10-Oct-30 00:00</p>
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
			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_EmailAddress = "sales.representative@example.com";

			Factory.Save();

			quote.TH_GS_NKFirstSignatory = firstSignatory.GS_Code;
			quote.TH_GS_NKSecondSignatory = secondSignatory.GS_Code;
			var salesRepAssignment = quote.HeaderStaffAssignments.AddNew();
			salesRepAssignment.O8_Department = "ALL";
			salesRepAssignment.O8_Role = "SAL";
			salesRepAssignment.O8_GS_NKPersonResponsible = salesRep.GS_Code;

			Factory.Save();
		}

		#endregion
	}
}
