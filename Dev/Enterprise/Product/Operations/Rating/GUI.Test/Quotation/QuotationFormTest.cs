using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class QuotationFormTest : RatingTestCase
	{
		#region Signatories

		public void TestSignaturesWithOldQuote()
		{
			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_GS_NKFirstSignatory = ZString.Empty;
			Factory.Save();

			using (QuotationForm form = new QuotationForm(testQuote))
			{
				form.Show();
				AssertEquals(GlbStaff.CurrentUser.GS_Code, testQuote.TH_GS_NKFirstSignatory);
			}
		}

		#endregion

		#region Enable Filters

		public void TestFiltersShouldBeEnabledInViewMode()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new QuotationForm(testQuote))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				var filterControl = (IReadOnlyToggleControl)form.rateEntryFilterStripControl;
				AssertEquals(false, filterControl.ReadOnly);
			}
		}

		#endregion

		#region Approve Quote

		[GuiTest]
		public void TestApprove()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			using (QuotationForm form = new QuotationForm(quote))
			{
				form.Show();

				AssertEquals("Should be NOT approved", 0, quote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QuotationInternallyApproved.Code)).Length);

				form.ApproveQuoteButton.PerformClick();
				AssertEquals("Should be approved", 1, quote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QuotationInternallyApproved.Code)).Length);
			}
		}

		[GuiTest]
		public void TestShowApprovalDialog()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			using (QuotationForm form = new QuotationForm(quote))
			{
				form.Show();

				string expected = "Question " +
					"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.\r\n\r\n" +
					"Do you wish to Approve this quote?" +
					"";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApprove));
				AssertEquals("Dialog: WishToApprove", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Question " +
					"This Quotation is not internally approved.\r\n" +
					"It must be internally Approved first before it can be Accepted.\r\n\r\n" +
					"Do you wish to Approve this quote first?" +
					"";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApproveWhenAccepting));
				AssertEquals("Dialog: WishToApproveWhenAccepting", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Question " +
					"This Quotation is not internally approved.\r\n" +
					"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.\r\n\r\n" +
					"Do you wish to Approve this quote for printing in Final mode? Otherwise this quote will be printed in Draft mode." +
					"";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApproveWhenFinalizing));
				AssertEquals("Dialog: WishToApproveWhenFinalizing", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Question " +
					"This Quotation is not internally approved.\r\n" +
					"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.\r\n\r\n" +
					"Do you wish to Approve this quote?" +
					"";
				quote.OnShowApprovalDialog(new Quote.ApprovalDialogEventArgs(Quote.ApprovalDialog.WishToApproveWhenSaving));
				AssertEquals("Dialog: WishToApproveWhenSaving", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestShowApprovalMessage()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			using (QuotationForm form = new QuotationForm(quote))
			{
				form.Show();

				string expected = "Information " +
					"This Quotation is already approved." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.AlreadyApprovedMessage);
				AssertEquals("Message: AlreadyApprovedMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Information " +
					"The login details entered do not have security rights to approve Quotations." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.HaveNoRightsMessage);
				AssertEquals("Message: HaveNoRightsMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Information " +
					"The login details entered are incorrect or password is expired." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.LoginFailedMessage);
				AssertEquals("Message: LoginFailedMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				expected = "Error " +
					"A local client or overseas agent is required to approve this Quotation." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.MissingLocalClientMessage);
				AssertEquals("Message: MissingLocalClientMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());

				expected = "Error " +
					"An overseas agent is required to approve this Quotation." +
					"";
				quote.OnShowApprovalMessage(Quote.ApprovalMessage.MissingOverseasAgentMessage);
				AssertEquals("Message: MissingOverseasAgentMessage", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[GuiTest]
		public void TestQuoteApprovalSecurityNotGranted()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			using (QuotationForm form = new QuotationForm(quote))
			{
				form.Show();

				string expected = "Question " +
					"You do not have the appropriate security rights to mark this Quotation as Approved.\r\n" +
					"If you do not mark this quotation as approved you cannot use it for Auto Rating Purposes and it can only be printed in Draft Mode.\r\n\r\n" +
					"You can either continue, or have a user with higher security rights enter their credentials.\r\n\r\n" +
					"Do you wish to have a user with higher rights enter their credentials?" +
					"";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				quote.OnQuoteApprovalSecurityNotGranted(new Quote.QuoteApprovalSecurityEventArgs());
				AssertEquals("Dialog: WishToSwitchSecurity", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				quote.OnQuoteApprovalSecurityNotGranted(new Quote.QuoteApprovalSecurityEventArgs());
				AssertEquals("Message: HaveNoRightsMessage", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		#endregion

		#region TestPrintQuoteWithUseDocumentDoesNotThrowException

		[ExpectNoExceptions]
		public void TestPrintQuoteWithUseDocumentDoesNotThrowException()
		{
			var org = Helper.NewOrgHeader();
			org.OH_IsConsignee = true;

			var quote = Helper.NewQuote(org);
			quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "STD", "20GP");

			using (var quotationForm = new QuotationForm(quote))
			{
				quotationForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var command = DocumentCommand.GetDocumentCommand(Factory, quote, Core.Constants.MenuNameConstantsForPrinting.QuotationPack);

				var pivot = Factory.New<StmMenuMenuPivotBase>();

				var parentMenu = Factory.New<StmMenuItemBase>();
				parentMenu.SU_MenuName = "Parent";

				var childMenu = Factory.New<StmMenuItemBase>();
				childMenu.SU_MenuName = "Child";
				childMenu.SU_BusinessContext = nameof(BusinessContext.Quotation);

				pivot.SF_SU_Inward = parentMenu.PK;
				pivot.SF_SU_Outward = childMenu.PK;
				pivot.SF_OverriddenBusinessContext = ZString.Empty;

				command.ChildMenus.Add(pivot);
				Factory.Save();

				quotationForm.PrintQuoteButton.PerformClick();
			}
		}

		#endregion

		[GuiTest]
		public void TestSetupFormForQuoteCancellation()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_QuoteNumber = "01230450";

			using (QuotationForm form = new QuotationForm(quote))
			{
				form.Show();
				form.SetupFormForQuoteCancellation();

				AssertEquals("Form Text", "Cancel Quote 123045", form.Text);
				AssertEquals("SaveButton should be hidden", false, form.PostingButtonsUserControl.SaveButton.Visible);
				AssertEquals("SaveAndCloseButton should be renamed", "Confirm", form.PostingButtonsUserControl.SaveAndCloseButton.Text);
			}
		}

		[GuiTest]
		public void TestQuoteTabControl_CustomFieldTab()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_QuoteNumber = "01230450";

			using (QuotationForm form = new QuotationForm(quote))
			{
				form.Show();

				var customFieldControl = (ZTabPage)form.QuotationTabControl1.TopLevelTabControl.AllTabPages.Where(control => control.Name.Equals("CustomFieldsTabPage")).First();
				AssertNotNull(customFieldControl);
				Assert(customFieldControl.TabVisible);
			}
		}

		#region TestPrintAllTradeLanes

		public void TestPrintAllQuoteTradeLanes()
		{
			var quote = Helper.NewQuote(Consignee);
			var fclEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "FRT", 100);
			fclEntry.TI_RC = Helper.Containers["20GP"].PK;

			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "NZAKL", "ODOC", 10);

			var expiredRateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, "FCL", "UAODS", "AUSYD", "DDOC", 20);
			expiredRateEntry.TI_RateStartDate = ZDateTime.Now.AddDays(-15).Date;
			expiredRateEntry.TI_RateEndDate = ZDateTime.Now.AddDays(-5).Date;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedQuote = factory2.Load<Quote>(quote.PK);
			bool noTradeLanesToPrintHasBeenCalled = false;
			reloadedQuote.NoTradeLanesToPrint += (s, e) => { noTradeLanesToPrintHasBeenCalled = true; };

			using (var quotationForm = new QuotationForm(reloadedQuote))
			{
				quotationForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				quotationForm.PrintQuoteButton.PerformClick();
				Assert("No Trade Lanes To Print should not have been called", !noTradeLanesToPrintHasBeenCalled);
			}

			var factory3 = new BusinessObjectFactory();
			var reloadedQuote2 = factory2.Load<Quote>(quote.PK);

			var pages = new PricingPageCollection(reloadedQuote2);
			pages.LoadStandard();
			AssertEquals("3 pages for 3 rate entries", 3, pages.Count);
		}

		#endregion
	}

	#region Form Basher

	[TestedType(typeof(QuotationForm))]
	public class QuotationFormBasherTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (ZForm testForm = (ZForm)GetFormToBash())
			{
				AssertEquals("Quotation 00000999/A", testForm.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			Helper.IsMarkAsNeedingValidationSuspended = true;

			Quote testQuote = Helper.NewFullyPopulatedQuote();
			Costing cost = Helper.NewFullyPopulatedCosting();
			CompanyTariff tariff = Helper.NewFullyPopulatedCompanyTariff();

			Helper.IsMarkAsNeedingValidationSuspended = false;

			IDisposable quoteValidationSuspender = testQuote.SuspendMarkingAsNeedingValidation();
			IDisposable costValidaitonSuspender = cost.SuspendMarkingAsNeedingValidation();
			IDisposable tariffValidationSuspender = tariff.SuspendMarkingAsNeedingValidation();

			Factory.Save();

			quoteValidationSuspender.Dispose();
			costValidaitonSuspender.Dispose();
			tariffValidationSuspender.Dispose();

			QuotationForm result = new QuotationForm(testQuote)
			{
				ControllerID = ControllerIDs.Quotations
			};
			return result;
		}

		#region Implementation

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}

	#endregion
}
