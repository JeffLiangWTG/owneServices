using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(QuotationsModule))]
	public class QuotationsModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (QuotationsModule module = new QuotationsModule())
			{
				AssertEquals(ModuleIDs.Quotations, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (QuotationsModule module = new QuotationsModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Quotation business context should be returned", BusinessContext.Quotation, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestDeleteForm()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			Quote acceptedQuote = GetAcceptedQuote();
			Quote clientAcceptedQuote = GetClientAcceptedQuote();
			Quote activeQuote = GetActiveQuote();

			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			{
				using (IZForm deleteForm = module.ShowDeleteForm_ForTest(acceptedQuote))
				{
					AssertNull(deleteForm);
					AssertEquals("The selected quote has already been accepted and cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				using (IZForm deleteForm = module.ShowDeleteForm_ForTest(activeQuote))
				{
					AssertNotNull(deleteForm);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				using (IZForm deleteForm = module.ShowDeleteForm_ForTest(clientAcceptedQuote))
				{
					AssertNull(deleteForm);
					AssertEquals("The selected quote has already been accepted by the client and cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestImportFromXmlMenuItem()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = new QuotationsModule())
			{
				var found = module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems.Cast<MenuItem>().Any(item => item.Text == "Import From &XML");
				AssertEquals("Should find the Import From XML menu Item", true, found);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		[GuiTest]
		public void TestShowEditForm_Active()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.ShowApprovalDialog += new EventHandler<Quote.ApprovalDialogEventArgs>(delegate(object sender, Quote.ApprovalDialogEventArgs e)
			{ e.Cancel = true; });
			quote.TH_OH = orgHeader.PK;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
			{
				form.Show();
				AssertEquals("Should be NO Messages", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should be Edit form", ODisplayMode.Browse, form.DisplayMode);
				AssertEquals("Should be the same Quotation", quote.PK, ((Quote)form.BusinessEntity).PK);
			}
		}

		[GuiTest]
		public void TestShowEditForm_Accepted()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = orgHeader.PK;
			quote.TH_Accepted = ZDateTime.Today;
			Factory.Save();

			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					AssertEquals("Should be Message", "Question The selected quote has already been accepted and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("Should be NO forms", form);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					form.Show();
					AssertEquals("Should be Message", "Question The selected quote has already been accepted and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should be View form", ODisplayMode.ReadOnly, form.DisplayMode);
					AssertEquals("Should be the same Quotation", quote.PK, ((Quote)form.BusinessEntity).PK);
				}
			}
		}

		[GuiTest]
		public void TestShowEditForm_Cancelled()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = orgHeader.PK;
			quote.TH_IsCancelled = true;
			Factory.Save();

			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					AssertEquals("Should be Message", "Question The selected quote has already been canceled and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("Should be NO forms", form);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					form.Show();
					AssertEquals("Should be Message", "Question The selected quote has already been canceled and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should be View form", ODisplayMode.ReadOnly, form.DisplayMode);
					AssertEquals("Should be the same Quotation", quote.PK, ((Quote)form.BusinessEntity).PK);
				}
			}
		}

		[GuiTest]
		public void TestShowEditForm_Expired()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = orgHeader.PK;
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			quote.TH_QuoteEndDate = ZDate.Today.AddDays(-3);
			Factory.Save();

			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					AssertEquals("Should be Message", "Question The selected quote has already expired and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("Should be NO forms", form);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					form.Show();
					AssertEquals("Should be Message", "Question The selected quote has already expired and cannot be edited.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should be View form", ODisplayMode.ReadOnly, form.DisplayMode);
					AssertEquals("Should be the same Quotation", quote.PK, ((Quote)form.BusinessEntity).PK);
				}
			}
		}

		[GuiTest]
		public void TestShowEditForm_Finalised()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = orgHeader.PK;
			quote.TH_IsLocked = true;
			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			quote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			Factory.Save();

			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					AssertEquals("Should be Message", "Question The selected quote has already been printed in Final mode and cannot be edited.  Would you like to create an Amendment / copy instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("Should be NO forms", form);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					form.Show();
					AssertEquals("Should be Message", "Question The selected quote has already been printed in Final mode and cannot be edited.  Would you like to create an Amendment / copy instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should be Edit form", ODisplayMode.Edit, form.DisplayMode);
					AssertNotEquals("Should be different Quotation", quote.PK, ((Quote)form.BusinessEntity).PK);
				}

				Quote newQuote1 = (Quote)quote.CopyIncludingChildren();
				newQuote1.TH_OH = orgHeader.PK;
				newQuote1.TH_QuoteNumber = quote.GetNewQuoteNumberForAmendment();

				Quote newQuote2 = (Quote)quote.CopyIncludingChildren();
				newQuote2.TH_OH = orgHeader.PK;
				newQuote2.TH_QuoteNumber = quote.GetNewQuoteNumberForAmendment();

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					string expected = "Question " +
						"The selected quote has already been printed in Final mode and cannot be edited.\r\n" +
						"However, an un-printed Amended quote exists (Quote Number: BXCPI2VTM38SN4162V/E).\r\n\r\n" +
						"Would you like to edit this one instead?" +
						"";
					AssertEquals("Should be Message", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("Should be NO forms", form);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
				{
					form.Show();
					string expected = "Question " +
						"The selected quote has already been printed in Final mode and cannot be edited.\r\n" +
						"However, an un-printed Amended quote exists (Quote Number: BXCPI2VTM38SN4162V/E).\r\n\r\n" +
						"Would you like to edit this one instead?" +
						"";
					AssertEquals("Should be Message", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should be Edit form", ODisplayMode.Browse, form.DisplayMode);
					AssertEquals("Should be Amended Quotation", newQuote2.PK, ((Quote)form.BusinessEntity).PK);
				}
			}
		}

		[GuiTest]
		public void TestShowEditForm_Approved()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = orgHeader.PK;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			using (ZForm form = (ZForm)module.ShowEditForm_ForTest(quote))
			{
				AssertEquals("Should be NO Messages", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				form.Show();
				AssertEquals("Should be Edit form", ODisplayMode.Browse, form.DisplayMode);
				AssertEquals("Should be the same Quotation", quote.PK, ((Quote)form.BusinessEntity).PK);
			}
		}

		[GuiTest]
		public void TestShowAcceptForm()
		{
			Quote acceptedQuote = GetAcceptedQuote();
			Quote activeQuote = GetActiveQuote();
			Quote finalisedQuote = GetFinalisedQuote();
			Quote cancelledQuote = GetCancelledQuote();
			Quote expiredQuote = GetExpiredQuote();
			Quote approvedQuote = GetApprovedQuote();
			Quote clientAcceptedQuote = GetClientAcceptedQuote();

			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			using (EmbeddedModulePopup popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				module.PerformSearch_ForTest();

				MenuItem menu = MenuAssertion.AssertHasMenu(module.ContextMenu_ForTest, "Accept");

				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Accepted, acceptedQuote, AssertAcceptAccepted, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Active, activeQuote, AssertAcceptActive, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Finalized, finalisedQuote, AssertAcceptActive, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Cancelled, cancelledQuote, AssertAcceptCancelled, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Expired, expiredQuote, AssertAcceptExpired, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Approved, approvedQuote, AssertAcceptActive, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.ClientAccepted, clientAcceptedQuote, AssertAcceptActive, module, menu);
			}
		}

		[GuiTest]
		public void TestShowCancelForm()
		{
			Quote acceptedQuote = GetAcceptedQuote();
			Quote activeQuote = GetActiveQuote();
			Quote finalisedQuote = GetFinalisedQuote();
			Quote cancelledQuote = GetCancelledQuote();
			Quote expiredQuote = GetExpiredQuote();
			Quote approvedQuote = GetApprovedQuote();
			Quote clientAcceptedQuote = GetClientAcceptedQuote();

			using (QuotationsModule module = (QuotationsModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations))
			using (EmbeddedModulePopup popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				module.PerformSearch_ForTest();

				MenuItem menu = MenuAssertion.AssertHasMenu(module.ContextMenu_ForTest, "Cancel");

				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Accepted, acceptedQuote, AssertCancelAccepted, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Active, activeQuote, AssertCancelActive, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Finalized, finalisedQuote, AssertCancelActive, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Cancelled, cancelledQuote, AssertCancelCancelled, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Expired, expiredQuote, AssertCancelExpired, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.Approved, approvedQuote, AssertCancelActive, module, menu);
				AssertQuoteStatusBehaviour(Quote.QuoteStatusOptions.ClientAccepted, clientAcceptedQuote, AssertCancelActive, module, menu);
			}
		}

		#region Implementation
		void AssertQuoteStatusBehaviour(ZString statusOption, Quote typeOfQuote, AssertAction assertAction, QuotationsModule module, MenuItem menu)
		{
			AssertEquals("Should be the same Status", typeOfQuote.QuoteStatus, statusOption);

			int index = module.GridCollection.IndexOf(typeOfQuote);
			Assert("The Quote should exists", index >= 0);

			module.Grid_ForTest.ListManager.Position = index;
			assertAction(statusOption, menu, module);
		}

		delegate void AssertAction(ZString status, MenuItem menu, QuotationsModule module);

		[GuiTest]
		void AssertAcceptActive(ZString status, MenuItem menu, QuotationsModule module)
		{
			module.LastUsedControllerForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be NO Messages", status), "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNotNull(string.Format("{0}: Should be controller", status), module.LastUsedControllerForTest);

			using (IZForm form = module.LastUsedControllerForTest.LastShownForm)
			{
				AssertType(typeof(ActiveRatesForm), form);
				AssertEquals("Should be Accept form", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		void AssertAcceptAccepted(ZString status, MenuItem menu, QuotationsModule module)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be Message", status), "Question The selected quote has already been accepted.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		void AssertAcceptCancelled(ZString status, MenuItem menu, QuotationsModule module)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be Message", status), "Question The selected quote has already been canceled and cannot be accepted.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		void AssertAcceptExpired(ZString status, MenuItem menu, QuotationsModule module)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be Message", status), "Question The selected quote has already expired and cannot be accepted.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		[GuiTest]
		void AssertCancelActive(ZString status, MenuItem menu, QuotationsModule module)
		{
			module.LastUsedControllerForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be NO Messages", status), "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNotNull(string.Format("{0}: Should be controller", status), module.LastUsedControllerForTest);

			using (IZForm form = module.LastUsedControllerForTest.LastShownForm)
			{
				AssertType(typeof(QuotationForm), form);
				AssertEquals("Should be Cancel form", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		void AssertCancelAccepted(ZString status, MenuItem menu, QuotationsModule module)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be Message", status), "Question The selected quote has already been accepted and cannot be canceled.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		void AssertCancelCancelled(ZString status, MenuItem menu, QuotationsModule module)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be Message", status), "Question The selected quote has already been canceled.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		void AssertCancelExpired(ZString status, MenuItem menu, QuotationsModule module)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.PerformClick();
			AssertEquals(string.Format("{0}: Should be Message", status), "Question The selected quote has already expired and cannot be canceled.  Would you like to view it instead?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		Quote GetClientAcceptedQuote()
		{
			Quote quote = GetNewQuote();
			quote.TH_ClientAccepted = ZDateTime.Today;

			Factory.Save();

			return quote;
		}

		Quote GetAcceptedQuote()
		{
			Quote quote = GetNewQuote();
			quote.TH_Accepted = ZDateTime.Today;

			Factory.Save();

			return quote;
		}

		Quote GetActiveQuote()
		{
			bool oldValue = DataRegistryRating.Instance.QuoteRequireInternalApproval.Value;
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Quote quote = GetNewQuote();
			quote.ShowApprovalDialog += new EventHandler<Quote.ApprovalDialogEventArgs>(delegate(object sender, Quote.ApprovalDialogEventArgs e)
			{ e.Cancel = true; });

			Factory.Save();
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);

			return quote;
		}

		Quote GetFinalisedQuote()
		{
			Quote quote = GetNewQuote();
			quote.TH_IsLocked = true;
			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			quote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			Factory.Save();

			return quote;
		}

		Quote GetCancelledQuote()
		{
			Quote quote = GetNewQuote();
			quote.TH_IsCancelled = true;

			Factory.Save();

			return quote;
		}

		Quote GetExpiredQuote()
		{
			var quote = GetNewQuote();
			quote.TH_QuoteEndDate = ZDate.Today.AddDays(-3);
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			Factory.Save();

			return quote;
		}

		Quote GetApprovedQuote()
		{
			bool oldValue = DataRegistryRating.Instance.QuoteRequireInternalApproval.Value;
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Quote quote = GetNewQuote();

			Factory.Save();
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);

			return quote;
		}

		Quote GetNewQuote()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = orgHeader.PK;

			return quote;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Quotations;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			var quote = GetNewQuote();
			quote.TH_IsCancelled = true; // this will enable CanDelete so the delete test works.

			return quote;
		}

		// ShowEditForm has been completely overridden and replaces the threadsafe functionality from the base class.
		// This is dangerous and is recommended to be changed.
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Edit => true;

		#endregion
	}
}
