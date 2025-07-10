using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Rating.Business.Quote;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(QuotationsController))]
	public class QuotationsControllerTest : RatingControllerTest<QuotationsController, Quote>
	{
		#region Overrides

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Quotations;
		}

		protected override SecurityCheckpoint DefaultCheckPointForView
		{
			get { return Env.Security.QuotationView; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForEdit
		{
			get { return Env.Security.QuotationEdit; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForDelete
		{
			get { return Env.Security.QuotationDelete; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForCopy
		{
			get { return Env.Security.QuotationCopy; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForNew
		{
			get { return Env.Security.QuotationNew; }
		}

		protected override RatingHeader GetRatingHeader()
		{
			return Helper.NewQuote(Helper.NewOrgHeader());
		}

		protected override RatingHeader GetGlobalRatingHeader()
		{
			return null;
		}

		protected override CRMSecurity DefaultCRMSecurity
		{
			get
			{
				return Env.Security.QuotationCRMSecurity;
			}
		}

		#endregion

		#region Showing Forms

		public void TestAcceptQuote_CreatedRateEntryHasCreationSource()
		{
			var today = ZDate.Today;

			var client = Helper.NewOrgHeader();
			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var initialRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "USLAX", "SGSIN", "FRT", 10m);
			initialRateEntry.TI_RateStartDate = today.AddDays(1);
			initialRateEntry.TI_RateEndDate = today.AddDays(2);

			AssertEquals("Precondition: TI_CreationSource should be Manual", RateEntryCreator.Sources.Manual, initialRateEntry.TI_CreationSource);

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(10);
			var quoteEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);

			Factory.Save();

			var controller = new QuotationsController();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = controller.ShowAcceptForm(quote))
			{
				var updatedQuote = controller.Factory.Load<Quote>(quote.PK);
				var updatedClientRate = controller.Factory.Load<ClientRate>(clientRate.PK);
				controller.Factory.Save();
			}

			clientRate = Factory.Load<ClientRate>(clientRate.PK);

			AssertEquals("TI_CreationSource should not have changed", RateEntryCreator.Sources.Manual, initialRateEntry.TI_CreationSource);

			var acceptedRateEntry = clientRate.ChildRateEntries.Where(r => r != initialRateEntry).OfType<RateEntry>().Single();

			AssertEquals("TI_CreationSource of acceptedRateEntry should be FromQuotation", RateEntryCreator.Sources.FromQuotation, acceptedRateEntry.TI_CreationSource);
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_NoDuplicateClientRate_DifferentOriginDestination()
		{
			var today = ZDate.Today;

			var client = Helper.NewOrgHeader();
			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "USLAX", "SGSIN", "FRT", 10m);
			rateEntry.TI_RateStartDate = today.AddDays(1);
			rateEntry.TI_RateEndDate = today.AddDays(2);

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(10);
			var quoteEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);

			Factory.Save();

			AssertAcceptQuoteNoDuplicateOrOverlapping(quote, clientRate);
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_NoOverlappingClientRate()
		{
			var today = ZDate.Today;

			var client = Helper.NewOrgHeader();
			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 10m);
			rateEntry.TI_RateStartDate = today.AddDays(-2);
			rateEntry.TI_RateEndDate = today.AddDays(-1);

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(10);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);

			Factory.Save();

			AssertAcceptQuoteNoDuplicateOrOverlapping(quote, clientRate);
		}

		void AssertAcceptQuoteNoDuplicateOrOverlapping(Quote quote, ClientRate clientRate)
		{
			AssertEquals("Precondition: Quote status", QuoteStatusOptions.Approved, quote.QuoteStatus);

			var controller = new QuotationsController();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = controller.ShowAcceptForm(quote))
			{
				var updatedQuote = controller.Factory.Load<Quote>(quote.PK);
				var updatedClientRate = controller.Factory.Load<ClientRate>(clientRate.PK);
				CombineAssertions("GIVEN no overlapping ClientRate WHEN accepting quote THEN quote should be accepted", () =>
				{
					AssertNullOrEmpty("Overlapping Rates Message", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Quote Status", QuoteStatusOptions.Accepted, updatedQuote.QuoteStatus);
				});
			}
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_ExpireExistingRates_OverlappingClientRateWithStartDateEqualsQuoteStartDate()
		{
			var today = ZDate.Today;

			var client = Helper.NewOrgHeader();
			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 10m);
			clientRateEntry.TI_RateStartDate = today;
			clientRateEntry.TI_RateEndDate = today.AddDays(1);

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(1);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);

			Factory.Save();

			AssertAcceptQuote_ExpireExistingRates_OverlappingClientRate
			(
				quote,
				clientRateEntry,
				expectedClientRateEntryEndDateWhenUserOptedNo: today.AddDays(1),
				expectedClientRateEntryEndDateWhenUserOptedYes: quote.TH_QuoteDate.AddDays(-1)
			);
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_ExpireExistingRates_OverlappingClientRateWithEmptyEndDate()
		{
			var today = ZDate.Today;

			var client = Helper.NewOrgHeader();
			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 10m);
			clientRateEntry.TI_RateStartDate = today;
			clientRateEntry.TI_RateEndDate = ZDate.Empty;

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(1);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);

			Factory.Save();

			AssertAcceptQuote_ExpireExistingRates_OverlappingClientRate
			(
				quote,
				clientRateEntry,
				expectedClientRateEntryEndDateWhenUserOptedNo: ZDate.Empty,
				expectedClientRateEntryEndDateWhenUserOptedYes: quote.TH_QuoteDate.AddDays(-1)
			);
		}

		[TestDate(2023, 01, 01)]
		public void TestOverlappingDates()
		{
			var newStartDate = ZDate.Today;

			#region All Dates not null
			// quote ---|<--------------->|--------------------------
			// rate  ------------------------|<----------------->|---
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(12), newStartDate.AddDays(20), false);

			// quote ---|<--------------->|--------------------------
			// rate  ---------------------|<----------------->|------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(10), newStartDate.AddDays(20), true);

			// quote ---|<--------------->|--------------------------
			// rate  ------------------|<----------------->|---------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(8), newStartDate.AddDays(20), true);

			// quote ---|<--------------------------------->|--------
			// rate  -----------|<----------------->|----------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(20), newStartDate.AddDays(8), newStartDate.AddDays(15), true);

			// quote ---------|<---------------->|-------------------
			// rate  ----|<----------------------------->|-----------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(-5), newStartDate.AddDays(20), true);

			// quote -------------------|<--------------->|----------
			// rate  ----|<----------------->|-----------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(20), newStartDate.AddDays(-5), newStartDate.AddDays(10), true);

			// quote -------------------|<--------------->|----------
			// rate  ----|<------------>|----------------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(-10), newStartDate, true);

			// quote -------------------|<--------------->|----------
			// rate  --|<----------->|-------------------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(-10), newStartDate.AddDays(-2), false);

			#endregion

			#region Existing Rate no Expiry
			// quote ---|<--------------->|--------------------------
			// rate  ------------------------|<----------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(12), null, false);

			// quote ---|<--------------->|--------------------------
			// rate  --------------|<--------------------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(8), null, true);

			// quote ---|<--------------->|--------------------------
			// rate  ---------------------|<-------------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(10), null, true);

			// quote ------------|<--------------->|-----------------
			// rate  --|<--------------------------------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate.AddDays(-5), null, true);

			// quote ------------|<--------------->|-----------------
			// rate  ------------|<----------------------------------
			AssertOverlappingDates(newStartDate, newStartDate.AddDays(10), newStartDate, null, true);

			#endregion

			#region New Quote no Expiry
			// quote ------------------------|<----------------------
			// rate  ---|<--------------->|--------------------------
			AssertOverlappingDates(newStartDate, null, newStartDate.AddDays(-10), newStartDate.AddDays(-2), false);

			// quote --------------|<--------------------------------
			// rate  ---|<--------------->|--------------------------
			AssertOverlappingDates(newStartDate, null, newStartDate.AddDays(-10), newStartDate.AddDays(2), true);

			// quote ---------------------|<-------------------------
			// rate  ---|<--------------->|--------------------------
			AssertOverlappingDates(newStartDate, null, newStartDate.AddDays(-10), newStartDate, true);

			// quote --|<--------------------------------------------
			// rate  ------------|<--------------->|-----------------
			AssertOverlappingDates(newStartDate, null, newStartDate.AddDays(2), newStartDate.AddDays(10), true);

			// quote ------------|<----------------------------------
			// rate  ------------|<--------------->|-----------------
			AssertOverlappingDates(newStartDate, null, newStartDate, newStartDate.AddDays(10), true);

			#endregion

			#region Existing and New no Expiry

			// quote ------------|<----------------------------------
			// rate  ------------|<----------------------------------
			AssertOverlappingDates(newStartDate, null, newStartDate, null, true);

			// quote --------|<--------------------------------------
			// rate  ------------|<----------------------------------
			AssertOverlappingDates(newStartDate, null, newStartDate.AddDays(3), null, true);

			// quote ------------|<----------------------------------
			// rate  --------|<--------------------------------------
			AssertOverlappingDates(newStartDate, null, newStartDate.AddDays(-3), null, true);

			#endregion
		}

		void AssertOverlappingDates(ZDate newStartDate, ZDate? newEndDate, ZDate existingStartDate, ZDate? existingEndDate, bool expectedValue)
		{
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 10m);
			clientRateEntry.TI_RateStartDate = existingStartDate;
			clientRateEntry.TI_RateEndDate = existingEndDate ?? ZDate.Empty;

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = newStartDate;
			quote.TH_QuoteEndDate = newEndDate ?? ZDate.Empty;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);

			AssertEquals("Overlapping dates", expectedValue, quote.HasOverlappingClientRate());
		}

		void AssertAcceptQuote_ExpireExistingRates_OverlappingClientRate(Quote quote, RateEntry clientRateEntry, ZDate expectedClientRateEntryEndDateWhenUserOptedNo, ZDate expectedClientRateEntryEndDateWhenUserOptedYes)
		{
			AssertEquals("Precondition: Quote status", QuoteStatusOptions.Approved, quote.QuoteStatus);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var controller = new QuotationsController();
			using (var form = controller.ShowAcceptForm(quote))
			{
				var updatedQuote = controller.Factory.Load<Quote>(quote.PK);
				var updatedClientRate = controller.Factory.Load<ClientRate>(clientRateEntry.Parent.PK);
				CombineAssertions("GIVEN overlapping ClientRate WHEN user opted for No THEN quote should not be accepted", () =>
				{
					AssertEquals("Overlapping Rates Message", string.Format(OverlappingRatesMessage, quote.TH_ClientCode), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Quote Status", QuoteStatusOptions.Approved, quote.QuoteStatus);
					AssertContainsExactElementsInAnyOrder
					(
						"Overlapping ClientRate.EndDate",
						new[] { expectedClientRateEntryEndDateWhenUserOptedNo },
						updatedClientRate.AllEntries.Where(x => x.PK == clientRateEntry.PK).Select(x => x.TI_RateEndDate)
					);
				});
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = controller.ShowAcceptForm(quote))
			{
				var updatedQuote = controller.Factory.Load<Quote>(quote.PK);
				var updatedClientRate = controller.Factory.Load<ClientRate>(clientRateEntry.Parent.PK);
				CombineAssertions("GIVEN overlapping ClientRate WHEN user opted for Yes THEN quote should be accepted", () =>
				{
					AssertEquals("Overlapping Rates Message", string.Format(OverlappingRatesMessage, quote.TH_ClientCode), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Quote Status", QuoteStatusOptions.Accepted, updatedQuote.QuoteStatus);
					AssertContainsExactElementsInAnyOrder
					(
						"Overlapping ClientRate.EndDate",
						new[] { expectedClientRateEntryEndDateWhenUserOptedYes },
						updatedClientRate.AllEntries.Where(x => x.PK == clientRateEntry.PK).Select(x => x.TI_RateEndDate)
					);
					Assert("Start dates should be equal to or earlier than end dates", updatedClientRate.AllEntries.All(x => x.TI_RateStartDate <= x.TI_RateEndDate));
				});
			}
		}

		const string OverlappingRatesMessage = @"During this operation, overlapping rates were found in the Client Rates of '{0}'.

The overlapping rates will be expired one day before the Start Date of the Quotation.

Do you wish to continue?";

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_ExpireExistingRates_OverlappingGlobalClientRateShouldNotBeChanged()
		{
			var startDate = ZDate.Today;
			var endDate = startDate.AddDays(5);

			var client = Helper.NewOrgHeader();
			// Global charge code for global rate
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Factory.Save();

			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalClientRateEntry = globalClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 10m);
			globalClientRateEntry.TI_RateStartDate = startDate;
			globalClientRateEntry.TI_RateEndDate = endDate;

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 10m);
			clientRateEntry.TI_RateStartDate = startDate;
			clientRateEntry.TI_RateEndDate = endDate;

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = startDate;
			quote.TH_QuoteEndDate = endDate;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var controller = new QuotationsController();
			using (controller.ShowAcceptForm(quote))
			{
				var globalRateEntriesInNewFactory = controller.Factory.Load<RateEntry>(globalClientRateEntry.PK);
				AssertEquals("Global rate entry start date should not be changed", startDate, globalRateEntriesInNewFactory.TI_RateStartDate);
				AssertEquals("Global rate entry endDate date should not be changed", endDate, globalRateEntriesInNewFactory.TI_RateEndDate);
			}
		}

		public void TestShowAcceptFormWithQuotationHasToBeApprovedBeforeAccepting()
		{
			Env.Security.QuotationAccept.IsAllowed = true;
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var ratingHeader = GetRatingHeader();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var controller = new QuotationsController();
				controller.ShowAcceptForm(ratingHeader);

				string approveBeforeAcceptMessage = (NoResString)"The Quotation has to be Approved before Accepting.";
				AssertEquals(approveBeforeAcceptMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowAcceptFormCannotBeReloaded()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_QuoteDate = ZDate.Today.AddDays(-1);
			quote.TH_OH = orgHeader.PK;
			Factory.Save();
			AssertEquals("Precondition: Quote status", QuoteStatusOptions.Approved, quote.QuoteStatus);

			var controller = new QuotationsController();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (var acceptForm = controller.ShowAcceptForm(quote) as IClientRateForm)
			{
				AssertNotNull("ShowAcceptForm should return a form", acceptForm);

				ZFormUtilities.ReloadCurrentForm((ZForm)acceptForm);

				AssertEquals(
					"This form type cannot be reloaded.",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
			}
		}

		public void TestShowEditFormWithClientAcceptedQuotation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			quote.TH_OH = orgHeader.PK;
			quote.TH_QuoteEndDate = ZDate.Today.AddDays(5);
			quote.TH_ClientAccepted = ZDate.Today.AddDays(-5);
			Factory.Save();

			var controller = new QuotationsController();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = (ZForm)controller.ShowEditForm(quote))
			{
				AssertEquals("Should be Message", "Question The selected quote has already been accepted by the client and cannot be edited.  Would you like to create an Amendment / copy instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNull("Should be NO forms", form);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (ZForm form = (ZForm)controller.ShowEditForm(quote))
			{
				form.Show();
				AssertEquals("Should be Message", "Question The selected quote has already been accepted by the client and cannot be edited.  Would you like to create an Amendment / copy instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should be Edit form", ODisplayMode.Edit, form.DisplayMode);
				AssertContains("Title should have an amendment quote number in it", quote.GetNewQuoteNumberForAmendment(), form.FormCaption);
			}
		}

		public void TestShowEditFormWithExpiredQuotation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var controller = new QuotationsController();

			quote.TH_OH = orgHeader.PK;
			quote.TH_QuoteEndDate = ZDate.Today.AddDays(-2);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = (ZForm)controller.ShowEditForm(quote))
			{
				AssertEquals("Should be Message", "Question The selected quote has already expired and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNull("Should be NO forms", form);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (ZForm form = (ZForm)controller.ShowEditForm(quote))
			{
				form.Show();
				AssertEquals("Should be Message", "Question The selected quote has already expired and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should be View form", ODisplayMode.ReadOnly, form.DisplayMode);
				AssertEquals("Should be the same Quotation", quote.PK, ((Quote)form.BusinessEntity).PK);
			}
		}

		#region TestShowForm_QuoteMustMatchCurrentCompanyLogin

		public void TestShowViewForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		public void TestShowCopyForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowTemplateCopyForm(bizObj));
		}

		public void TestShowDeleteForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowDeleteForm(bizObj));
		}

		void TestShowForm_QuoteMustMatchCurrentCompanyLogin(Func<QuotationsController, BusinessObject, IZForm> showFormDelegate)
		{
			var differentCompany = Factory.NewWithValidTestData<GlbCompany>();
			differentCompany.GC_Code = "GC2";
			differentCompany.GC_RN_NKCountryCode = "AU";
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			Factory.Save();

			Env.Security.QuotationNew.IsAllowed = true;
			Env.Security.QuotationView.IsAllowed = true;
			Env.Security.QuotationEdit.IsAllowed = true;
			Env.Security.QuotationDelete.IsAllowed = true;
			Env.Security.QuotationCopy.IsAllowed = true;

			Env.Security.QuotationShowAllQuotes.IsAllowed = true;
			Env.Security.QuotationShowBranchQuotes.IsAllowed = true;

			quote.TH_GC = GlbCompany.CurrentCompany.PK;
			ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

			quote.TH_GC = differentCompany.PK;
			ShowFormAndAssert(showFormDelegate, quote, false,
@"The Quotation is for login users in Australia (GC2).
Please login to the relevant company to view the quotation.");
		}

		#endregion

		#region TestShowForm_ChecksShowQuotationCheckpoints

		public void TestShowViewForm_ChecksShowQuotationCheckpoints()
		{
			TestShowForm_ChecksShowQuotationCheckpoints((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditForm_ChecksShowQuotationCheckpoints()
		{
			TestShowForm_ChecksShowQuotationCheckpoints((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		public void TestShowCopyForm_ChecksShowQuotationCheckpoints()
		{
			TestShowForm_ChecksShowQuotationCheckpoints((controller, bizObj) => controller.ShowTemplateCopyForm(bizObj));
		}

		public void TestShowDeleteForm_ChecksShowQuotationCheckpoints()
		{
			TestShowForm_ChecksShowQuotationCheckpoints((controller, bizObj) => controller.ShowDeleteForm(bizObj));
		}

		void TestShowForm_ChecksShowQuotationCheckpoints(Func<QuotationsController, BusinessObject, IZForm> showFormDelegate)
		{
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_BranchName = "Current Branch";
			currentBranch.GB_Code = "BRC";
			currentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			var currentStaff = Factory.NewWithValidTestData<GlbStaff>();
			currentStaff.GS_GB_HomeBranch = currentBranch.PK;
			currentStaff.GS_Code = "SX1";
			var differentStaffInSameBranch = Factory.NewWithValidTestData<GlbStaff>();
			differentStaffInSameBranch.GS_GB_HomeBranch = currentBranch.PK;
			differentStaffInSameBranch.GS_Code = "SX2";

			var differentBranchA = Factory.NewWithValidTestData<GlbBranch>();
			differentBranchA.GB_BranchName = "Branch A";
			differentBranchA.GB_Code = "BRA";
			var staffInDifferentBranchA1 = Factory.NewWithValidTestData<GlbStaff>();
			staffInDifferentBranchA1.GS_GB_HomeBranch = differentBranchA.PK;
			staffInDifferentBranchA1.GS_Code = "SA1";
			var staffInDifferentBranchA2 = Factory.NewWithValidTestData<GlbStaff>();
			staffInDifferentBranchA2.GS_GB_HomeBranch = differentBranchA.PK;
			staffInDifferentBranchA2.GS_Code = "SA2";

			var differentBranchB = Factory.NewWithValidTestData<GlbBranch>();
			differentBranchB.GB_BranchName = "Branch B";
			differentBranchB.GB_Code = "BRB";
			var staffInDifferentBranchB = Factory.NewWithValidTestData<GlbStaff>();
			staffInDifferentBranchB.GS_GB_HomeBranch = differentBranchB.PK;
			staffInDifferentBranchB.GS_Code = "SB1";

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(currentStaff.PK.ToGuid(), currentBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Env.Security.QuotationNew.IsAllowed = true;
				Env.Security.QuotationView.IsAllowed = true;
				Env.Security.QuotationCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.QuotationEdit.IsAllowed = true;
				Env.Security.QuotationCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
				Env.Security.QuotationDelete.IsAllowed = true;
				Env.Security.QuotationCopy.IsAllowed = true;
				Env.Security.QuotationCRMSecurity.IgnoreOSMG.IsAllowed = true;
				Env.Security.QuotationCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

				Env.Security.QuotationShowAllQuotes.IsAllowed = false;
				Env.Security.QuotationShowBranchQuotes.IsAllowed = true;
				{
					quote.TH_GS_NKFirstSignatory = currentStaff.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = differentStaffInSameBranch.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = staffInDifferentBranchA1.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations for your login branch.

If you require access to this function please login to the relevant branch (Branch A (BRA)), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowAllQuotes.DisplayTextPathToSecurityRight);

					quote.TH_GS_NKFirstSignatory = ZString.Empty;
					quote.TH_GS_NKSecondSignatory = currentStaff.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = ZString.Empty;
					quote.TH_GS_NKSecondSignatory = differentStaffInSameBranch.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = ZString.Empty;
					quote.TH_GS_NKSecondSignatory = staffInDifferentBranchA1.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations for your login branch.

If you require access to this function please login to the relevant branch (Branch A (BRA)), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowAllQuotes.DisplayTextPathToSecurityRight);

					quote.TH_GS_NKFirstSignatory = currentStaff.GS_Code;
					quote.TH_GS_NKSecondSignatory = staffInDifferentBranchA1.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = staffInDifferentBranchA1.GS_Code;
					quote.TH_GS_NKSecondSignatory = staffInDifferentBranchB.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations for your login branch.

If you require access to this function please login to the relevant branch (Branch A (BRA) or Branch B (BRB)), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowAllQuotes.DisplayTextPathToSecurityRight);

					quote.TH_GS_NKFirstSignatory = staffInDifferentBranchA1.GS_Code;
					quote.TH_GS_NKSecondSignatory = staffInDifferentBranchA2.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations for your login branch.

If you require access to this function please login to the relevant branch (Branch A (BRA)), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowAllQuotes.DisplayTextPathToSecurityRight);
				}

				Env.Security.QuotationShowAllQuotes.IsAllowed = false;
				Env.Security.QuotationShowBranchQuotes.IsAllowed = false;
				{
					quote.TH_GS_NKFirstSignatory = currentStaff.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = differentStaffInSameBranch.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations where you are one of its signatories.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowBranchQuotes.DisplayTextPathToSecurityRight);

					quote.TH_GS_NKFirstSignatory = staffInDifferentBranchA1.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations where you are one of its signatories.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowAllQuotes.DisplayTextPathToSecurityRight);

					quote.TH_GS_NKFirstSignatory = ZString.Empty;
					quote.TH_GS_NKSecondSignatory = currentStaff.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = ZString.Empty;
					quote.TH_GS_NKSecondSignatory = differentStaffInSameBranch.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations where you are one of its signatories.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowBranchQuotes.DisplayTextPathToSecurityRight);

					quote.TH_GS_NKFirstSignatory = ZString.Empty;
					quote.TH_GS_NKSecondSignatory = staffInDifferentBranchA1.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, false,
@"You do not have the appropriate security rights to view Quotation. You are only allowed to view quotations where you are one of its signatories.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.QuotationShowAllQuotes.DisplayTextPathToSecurityRight);
				}

				Env.Security.QuotationShowAllQuotes.IsAllowed = true;
				Env.Security.QuotationShowBranchQuotes.IsAllowed = true;
				{
					quote.TH_GS_NKFirstSignatory = currentStaff.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = staffInDifferentBranchA2.GS_Code;
					quote.TH_GS_NKSecondSignatory = ZString.Empty;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);

					quote.TH_GS_NKFirstSignatory = ZString.Empty;
					quote.TH_GS_NKSecondSignatory = staffInDifferentBranchB.GS_Code;
					ShowFormAndAssert(showFormDelegate, quote, true, ZString.Empty);
				}
			}
		}

		#endregion

		void ShowFormAndAssert(Func<QuotationsController, BusinessObject, IZForm> showFormDelegate, BusinessObject quote, bool expectedIsAllowed, ZString expectedSecurityMessageText)
		{
			var controller = new QuotationsController();
			ShowFormAndAssert(() => showFormDelegate(controller, quote), expectedIsAllowed, expectedSecurityMessageText);
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<Quote>();
			bizObjWithoutAccess.Header.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<Quote>.AssertController(new QuotationsController(), bizObjWithoutAccess, Env.Security.QuotationCRMSecurity);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Env.Security.QuotationShowAllQuotes.IsAllowed = true;
		}

		#endregion
	}
}
