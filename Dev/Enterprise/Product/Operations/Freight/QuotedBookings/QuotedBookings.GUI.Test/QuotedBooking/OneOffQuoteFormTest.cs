using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	class OneOffQuoteFormTest : BaseFreightTest
	{
		#region ReadOnly

		public void TestReadOnly_ActiveQuote() => AssertReadOnly(CreateActiveOneOffQuote(), expectedReadOnly: false);

		public void TestReadOnly_ApprovedQuote() => AssertReadOnly(CreateApprovedOneOffQuote(), expectedReadOnly: false);

		public void TestReadOnly_ConsolidatedQuote() => AssertReadOnly(CreateConsolidatedOneOffQuote(), expectedReadOnly: true);

		public void TestReadOnly_AcceptedQuote() => AssertReadOnly(CreateAcceptedOneOffQuote(acceptedDateTime: ZDateTime.Today), expectedReadOnly: false);

		public void TestReadOnly_ClientAcceptedQuote() => AssertReadOnly(CreateClientAcceptedOneOffQuote(clientAcceptedDateTime: ZDateTime.Today), expectedReadOnly: true);

		public void TestOnlyQuotationStatisticFieldIsEditable_WhenOneOffQuoteIsConsolidated()
		{
			var oneOffQuote = CreateConsolidatedOneOffQuote();
			using (var form = new QuotedBookingForm(oneOffQuote))
			{
				Assert("ReadOnly", oneOffQuote.ReadOnly);
				Assert("Not ReadOnly", !oneOffQuote.OneOffQuoteStatistics.ReadOnly);
				Assert("Button in Document Selection Page is disable", !form.PrintQuoteButton.Enabled);
				Assert("Button in Document Selection Page is disable", !form.NVOCCModeCheckBox.Enabled);
				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				actionsMenuItem.ShowPopupMenu();
				foreach (MenuItem menuItem in actionsMenuItem.MenuItems)
				{
					if (!ZFormMenuStrategy.IsDefaultActionMenuItemName(menuItem.Name) &&
						!ZFormMenuStrategy.IsAlwaysEnabledActionMenuItemName(menuItem.Name))
					{
						Assert($"Action menu items {menuItem.Name} except default are disabled", !menuItem.Enabled);
					}
				}
			}

			using (var plugin = new InvoicingPluginToFreight(oneOffQuote))
			{
				plugin.GetType().GetProperty("DisplayMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, ODisplayMode.Edit, null);
				var supporter = oneOffQuote.InvoicingSupporter;
				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();
				var revenueDescription = supporter.ConsumerType.RevenueChargeDescription(oneOffQuote);
				AssertNull(topLevelMenu.MenuItems.FindByText($"Autorate Costs and {revenueDescription}"));
				AssertNull(topLevelMenu.MenuItems.FindByText("Autorate Costs"));
				AssertNull(topLevelMenu.MenuItems.FindByText($"Autorate {revenueDescription}"));
				AssertNull(topLevelMenu.MenuItems.FindByText("Autorate Costs (Non-Consol Level Charge Only)"));
				AssertNull(topLevelMenu.MenuItems.FindByText($"Autorate Costs (Non-Consol Level Charge Only) and {revenueDescription}"));
				AssertNull(topLevelMenu.MenuItems.FindByText("Group Companies Charges"));
			}
		}

		public void TestOnlyQuotationStatisticFieldIsEditable_WhenOneOffQuoteIsPrintedAsFinal()
		{
			var oneOffQuote = CreateOneOffQuoteFinalPrint();
			using (var form = new QuotedBookingForm(oneOffQuote))
			{
				Assert("ReadOnly", oneOffQuote.ReadOnly);
				Assert("Not ReadOnly", !oneOffQuote.OneOffQuoteStatistics.ReadOnly);
			}
		}

		void AssertReadOnly(QuotedBooking quotedBooking, bool expectedReadOnly)
		{
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				AssertEquals("ReadOnly", expectedReadOnly, quotedBooking.ReadOnly);
			}
		}

		#endregion

		#region Deactivate

		public void TestDeactivate_ActiveQuote() => AssertDeactivate(CreateActiveOneOffQuote(), expectedMessage: null);

		public void TestDeactivate_ApprovedQuote() => AssertDeactivate(CreateApprovedOneOffQuote(), expectedMessage: null);

		public void TestDeactivate_AcceptedQuote() => AssertDeactivate(CreateAcceptedOneOffQuote(acceptedDateTime: ZDateTime.Today), expectedMessage: null);

		public void TestDeactivate_ClientAcceptedQuote() => AssertDeactivate(CreateClientAcceptedOneOffQuote(clientAcceptedDateTime: ZDateTime.Today), expectedMessage: "The selected one off quote has already been accepted by the client and cannot be deactivated.");

		void AssertDeactivate(QuotedBooking quotedBooking, string expectedMessage)
		{
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				makeInactive.PerformClick();

				AssertEquals("UserNotification", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		#endregion

		public void TestNewOneOffQuoteFormCaption()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OneOffQuotes);
			using (var form = (QuotedBookingForm)controller.ShowNewForm())
			{
				form.Show();
				Application.DoEvents();

				Assert(!form.FormHeading.StartsWith("Edit"));
				AssertEquals("New", form.FormVerb);

				AssertEquals(true, form.ConvertQuoteToQuotedBookingButton.Visible);
				AssertEquals(true, form.ConvertQuoteToQuotedBookingButton.Enabled);

				AssertEquals(true, form.ApproveOneOffButton.Visible);
				AssertEquals(true, form.ApproveOneOffButton.Enabled);
			}
		}

		#region Implementation

		QuotedBooking CreateApprovedOneOffQuote()
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			Factory.Save();

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Approved, oneOffQuote.Quote.QuoteStatus);
			return oneOffQuote;
		}

		QuotedBooking CreateActiveOneOffQuote()
		{
			bool oldValue = DataRegistryRating.Instance.QuoteRequireInternalApproval.Value;
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			var quote = oneOffQuote.Quote;
			quote.ShowApprovalDialog += new EventHandler<Quote.ApprovalDialogEventArgs>(delegate(object sender, Quote.ApprovalDialogEventArgs e)
			{
				e.Cancel = true;
			});

			Factory.Save();
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Active, oneOffQuote.Quote.QuoteStatus);

			return oneOffQuote;
		}

		QuotedBooking CreateAcceptedOneOffQuote(ZDateTime acceptedDateTime)
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			oneOffQuote.Quote.TH_Accepted = acceptedDateTime;
			Factory.Save();

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Accepted, oneOffQuote.Quote.QuoteStatus);
			return oneOffQuote;
		}

		QuotedBooking CreateClientAcceptedOneOffQuote(ZDateTime clientAcceptedDateTime)
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			oneOffQuote.Quote.TH_ClientAccepted = clientAcceptedDateTime;
			Factory.Save();

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.ClientAccepted, oneOffQuote.Quote.QuoteStatus);
			return oneOffQuote;
		}

		QuotedBooking CreateOneOffQuoteFinalPrint()
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			oneOffQuote.Quote.TH_IsLocked = true;
			Factory.Save();

			return oneOffQuote;
		}

		QuotedBooking CreateConsolidatedOneOffQuote(bool doSave = true)
		{
			var oneOffQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = Factory.NewWithValidTestData<ForwardingShipment>();
			var quoteAndBookingIsConsolidated = QuotedBooking.New(oneOffQuote.PK, booking.PK, Factory);

			if (doSave)
			{
				Factory.Save();
			}

			return quoteAndBookingIsConsolidated;
		}

		QuotedBooking CreateAndModifyOneOffQuoteWithoutSaving()
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			Factory.Save();
			oneOffQuote.Quote.TH_ClientAccepted = ZDateTime.Now;

			return oneOffQuote;
		}

		#endregion

		#region Copy

		public void TestCopyAsNewOneOffQuote_FinalPrintedQuote() => TestCopy(CreateOneOffQuoteFinalPrint(), "Copy as New One Off Quote", AssertNewQuoteOpened);

		public void TestCopyAsNewOneOffQuote_ConsolidatedQuote() => TestCopy(CreateConsolidatedOneOffQuote(), "Copy as New One Off Quote", AssertNewQuoteOpened);

		public void TestCopyAsAmendment_FinalPrintedQuote() => TestCopy(CreateOneOffQuoteFinalPrint(), "Copy as Amendment", AssertNewQuoteOpened);

		public void TestCopyAsAmendment_ConsolidatedQuote() => TestCopy(CreateConsolidatedOneOffQuote(), "Copy as Amendment", AssertQuoteCannotBeAmended);

		public void TestCopyAsNewOneOffQuote_NewUnsavedQuote() => TestCopy(CreateConsolidatedOneOffQuote(doSave: false), "Copy as New One Off Quote", AssertQuoteMustBeSavedFirst);

		public void TestCopyAsAmendment_NewUnsavedQuote() => TestCopy(CreateConsolidatedOneOffQuote(doSave: false), "Copy as Amendment", AssertQuoteMustBeSavedFirst);

		public void TestCopyAsNewOneOffQuote_ModifiedUnsavedQuote() => TestCopy(CreateAndModifyOneOffQuoteWithoutSaving(), "Copy as New One Off Quote", AssertQuoteMustBeSavedFirst);

		public void TestCopyAsAmendment_ModifiedUnsavedQuote() => TestCopy(CreateAndModifyOneOffQuoteWithoutSaving(), "Copy as Amendment", AssertQuoteMustBeSavedFirst);

		void TestCopy(QuotedBooking oneOffQuote, string copyMenuItemName, Action assert)
		{
			using (var form = new QuotedBookingForm(oneOffQuote))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var copyMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText(copyMenuItemName);
				AssertNotNull("menuItem", copyMenuItem);

				copyMenuItem.PerformClick();

				assert.Invoke();
			}

			AssertEquals(ZBool.False, oneOffQuote.OneOffQuoteIsAmended);
		}

		static void AssertNewQuoteOpened()
		{
			using (var quotedBookingForm = Application.OpenForms.OfType<QuotedBookingForm>().SingleOrDefault())
			{
				AssertNotNull("Should show the QuotedBookingForm", quotedBookingForm);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		static void AssertQuoteCannotBeAmended()
			=> AssertEquals("The quote cannot be copied as an amendment since it has already been used.", UnitTestUserNotification.Instance.LastMessage.Text);

		static void AssertQuoteMustBeSavedFirst()
			=> AssertEquals("The quote must be saved first before it can be copied.", UnitTestUserNotification.Instance.LastMessage.Text);

		#endregion

		public void TestOnQuoteSaved_ShouldSetQuotedBookingReadOnly_WhenQuoteIsLocked()
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			using (var form = new QuotedBookingForm(oneOffQuote))
			{
				form.SetDataBinding(oneOffQuote, nameof(oneOffQuote.Quote));
				oneOffQuote.Quote.TH_IsLocked = true;
				Factory.Save();

				AssertEquals(expected: true, oneOffQuote.ReadOnly);
			}
		}

		public void TestOnQuoteSaved_ShouldNotSetQuotedBookingReadOnly_WhenQuoteIsNotLocked()
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			using (var form = new QuotedBookingForm(oneOffQuote))
			{
				form.SetDataBinding(oneOffQuote, nameof(oneOffQuote.Quote));
				oneOffQuote.Quote.TH_IsLocked = false;
				Factory.Save();

				AssertEquals(expected: false, oneOffQuote.ReadOnly);
			}
		}
	}
}
