using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business.Quotations.Testing
{
	abstract class TrackingQuoteClientReplyControllerTest : TestCaseWithFactory
	{
		#region Client Reply - checking availability

		public void TestClientReply_CanViewQuotations()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());
			var controller = new TrackingQuoteClientReplyController();

			CombineAssertions("Has WebQuotes security right", () =>
			{
				AssertEquals("CanViewQuotations", true, helper.TestSiteUser.CanViewQuotations);
				AssertEquals("IsClientReplyAllowed", true, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
			});

			CombineAssertions("Has No WebQuotes security right", () =>
			{
				ChangeSecurityRight(helper.TestSiteUser, WebSecurityRightsList.WebQuotes, false);
				AssertEquals("CanViewQuotations", false, helper.TestSiteUser.CanViewQuotations);
				AssertEquals("IsClientReplyAllowed", false, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
			});
		}

		static void ChangeSecurityRight(TrackingSiteUser siteUser, WebSecurityRight right, bool isGranted)
		{
			foreach (OrgSecurityContacts userRight in siteUser.LoggedInUser.SecurityRightsForBindingOnly)
			{
				if (userRight.Security.OX_SecurityItemName == right.Code)
				{
					userRight.OZ_Granted = isGranted;
					break;
				}
			}
			siteUser.OnSecurityRightsChangedForTest();
			AssertEquals(isGranted, siteUser.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(right));
		}

		public void TestClientReply_WhenNoLoggedInUser()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(false, controller.IsClientReplyAllowed(quote, null));
		}

		public void TestClientReply_WhenQuoteStateIsFinalised_CanBe_AcceptedOrNotAccepted()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(true, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
		}

		public void TestClientReply_WhenQuoteStateIsApprovedAndNotPrintedFinal_CannotBe_AcceptedOrNotAccepted()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());
			quote.TH_IsLocked = false; // not in finalised state

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(false, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
		}

		public void TestClientReply_WhenQuoteStateIsExpired_CannotBe_AcceptedOrNotAccepted()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());
			quote.TH_QuoteEndDate = ZDate.Today.AddDays(-4); // quote ended 4 days ago

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(false, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
		}

		public void TestClientReply_WhenQuoteStateIsCancelled_CannotBe_AcceptedOrNotAccepted()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());
			quote.TH_IsCancelled = true;

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(false, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
		}

		public void TestClientReply_WhenQuoteStateIsAccepted_CannotBe_AcceptedOrNoteAccepted()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());
			quote.TH_Accepted = ZDate.Today;

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(false, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
		}

		public void TestClientReply_WhenQuoteStateIsClientAccepted_CannotBe_AcceptedOrNotAccepted()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());
			quote.TH_ClientAccepted = ZDate.Today;

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(false, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
		}

		public void TestClientReply_WhenQuoteStateIsBothAcceptedAndClientAccepted_CannotBe_AcceptedOrNotAccepted()
		{
			var quote = CreateQuoteAllowingClientReply(CreateClient());
			quote.TH_ClientAccepted = ZDate.Today.AddDays(-1);
			quote.TH_Accepted = ZDate.Today;

			var controller = new TrackingQuoteClientReplyController();
			AssertEquals(false, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
		}

		#endregion

		#region Implementation

		protected OrgContact Contact1
		{
			get
			{
				if (contact1 == null)
				{
					contact1 = Factory.NewWithValidTestData<OrgContact>();
					contact1.OC_ContactName = "Contact1";
				}
				return contact1;
			}
		}
		OrgContact contact1;

		protected void AssertClientReply(TrackingQuoteClientReplyController controller, Quote quote, bool expectedIsClientReplyAllowed, ZString expectedQuoteStatus, string expectedLog, bool expectedLogFound, string message)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("IsClientReplyAllowed", expectedIsClientReplyAllowed, controller.IsClientReplyAllowed(quote, helper.TestSiteUser));
				AssertEquals("QuoteStatus", expectedQuoteStatus, quote.QuoteStatus);
				AssertEquals("Logs", expectedLogFound, quote.Logs.HasLogWith(log => log.SL_SE_NKEvent == expectedLog));
			});
		}

		protected void AssertEmail(int expectedEmailCount, string expectedSubject = null, string expectedEmailRecipients = null, string[] expectedEmailBody = null)
		{
			AssertEquals("Emails count", expectedEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);

			if (expectedEmailCount > 0)
			{
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email subject", expectedSubject, email.Subject);
				AssertEquals("Recipients", expectedEmailRecipients, email.Recipients.RecipientsAsDelimitedString());

				AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);

				foreach (var currentExpectedEmailBody in expectedEmailBody)
				{
					AssertContains("Email body", currentExpectedEmailBody, email.Body);
				}
			}
		}

		protected OrgHeader CreateClient(string orgCode = "TESTORG1", string fullName = "OrgHeader1")
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = orgCode;
			client.OH_FullName = fullName;
			return client;
		}

		protected Quote CreateQuoteAllowingClientReply(OrgHeader client, string quoteNumber = "0001")
		{
			var quote = CreateQuoteDisallowingClientReply(client, quoteNumber);

			quote.TH_IsLocked = true;
			quote.Logs.AddNew(AutoEvents.QuotationFinalisedPrinted);

			Factory.Save();

			return quote;
		}

		Quote CreateQuoteDisallowingClientReply(OrgHeader client, string quoteNumber = "0001")
		{
			var quote = CreateQuoteCore();
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-1);
			quote.TH_QuoteEndDate = quote.TH_QuoteDate.AddMonths(2);
			quote.TH_OH = client.PK;
			quote.TH_QuoteNumber = quoteNumber;

			quote.ShowApprovalDialog += (sender, e) => { e.Cancel = false; };
			quote.InternalApproveQuote();

			Factory.Save();

			return quote;
		}

		protected abstract Quote CreateQuoteCore();

		protected override void SetUp()
		{
			base.SetUp();

			helper = new TestHelper(Factory);
			Factory.Save();
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
		}

		protected TestHelper helper;

		#endregion
	}
}
