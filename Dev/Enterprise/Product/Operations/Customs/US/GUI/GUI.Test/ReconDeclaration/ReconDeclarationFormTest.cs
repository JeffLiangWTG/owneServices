using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;
using JobMessageTypeList = Enterprise.Customs.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ReconDeclarationForm))]
	sealed class ReconDeclarationFormTest : ZFormBasherTest
	{
		public void TestStatusesErrorsTabHasUserControl()
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var message = mock.Object;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ReconDeclaration.Messages.Add(message);
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				form.MainTabControlExposed.SelectedTab = form.MessagesTabPage;
				AssertEquals("This should be Status/Errors Tab", "Status/Errors", form.MainTabControlExposed.SelectedTab.Controls[0].Controls[2].Text);
				AssertEquals("Status/Errors Tab is user control", typeof(MessagesStatusErrorsUserControl), form.MainTabControlExposed.SelectedTab.Controls[0].Controls[2].Controls[0].GetType());
			}
		}

		public void TestRefundedFeeTabPageVisibility()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				ReconDeclaration.US_EntryFilerCode = "XJ5";
				ReconDeclaration.US_IsAggregate = true;
				form.MainTabControlExposed.SelectedTab = (ZTabPage)form.Controls.Find("EntriesTabPage", true)[0];
				var feeSummaryTabControl = (ZTabControl)form.MainTabControlExposed.SelectedTab.Controls.Find("OriginalEntryFeeSummaryTabControl", true)[0];
				AssertEquals("Only one tab page", 1, feeSummaryTabControl.TabPages.Count);
				form.MainTabControlExposed.SelectedTab = (ZTabPage)form.Controls.Find("MainTabPage", true)[0];
				ReconDeclaration.US_IsAggregate = false;
				form.MainTabControlExposed.SelectedTab = (ZTabPage)form.Controls.Find("EntriesTabPage", true)[0];
				AssertEquals("Only one tab page", 2, feeSummaryTabControl.TabPages.Count);
			}
		}

		public void TestImportLinesForUnAttachedOriginalEntriesOnSaving()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.CustomsEntryHeaders[0].EntryNumber = "11111111";
			Factory.Save();
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				ReconDeclaration.US_EntryFilerCode = "XJ5";
				ReconOriginalEntryHeader originalEntry2 = ReconDeclaration.OriginalEntries.AddNew();
				originalEntry2.CH_OrigEntryReference = "XJ511111111";
				AssertEquals("PreCondition", 0, originalEntry2.Invoice.JobComInvoiceLines.Count);
				AssertEquals("PreCondition", true, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("InvoiceLines for original entry 2 has been imported", 1, originalEntry2.Invoice.JobComInvoiceLines.Count);
			}
		}

		public void TestMPFOnImportLinesForUnAttachedOriginalEntriesOnSaving()
		{
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 288584m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 288584m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.CustomsEntryHeaders[0].EntryNumber = "11111111";
			Factory.Save();
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				ReconDeclaration.US_EntryFilerCode = "XJ5";
				var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
				originalEntry.CH_OrigEntryReference = "XJ511111111";
				AssertEquals("PreCondition", 0, originalEntry.Invoice.JobComInvoiceLines.Count);
				AssertEquals("PreCondition", true, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("InvoiceLines for original entry 1 has been imported", 1, originalEntry.Invoice.JobComInvoiceLines.Count);
				var reconInvoiceLine = originalEntry.Invoice.InvoiceLines[0];
				var mpf = reconInvoiceLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				AssertEquals("MPF", 999.65m, mpf.CY_FeeAmount);
			}
		}

		public void TestPlugInsAdded()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				AssertPlugIn(ControllerIDs.DocDataPlugIn, form);
				AssertPlugIn(ControllerIDs.JobInvoicing, form);
			}
		}

		public void TestImportBulkDeclarations_Click()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				form.ImportBulkDeclarationsMenuItem.PerformClick();
				Form activeForm = ZFormModaliser.ActiveForm;
				AssertEquals("declaration module popup is shown", typeof(CustomsEmbeddedModulePopup), activeForm.GetType());
				EmbeddedModulePopup popup = (EmbeddedModulePopup)activeForm;
				AssertEquals("OK button strategy", typeof(ReconBulkImportPopupOKButtonStrategy), popup.EmbeddedModulePopupOKButtonStrategy.GetType());
			}
		}

		public void TestImportInvoicesDetails_Click()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ImportInvoicesMenuItem.PerformClick();
				AssertEquals("ReconInvoiceLinesRetriever.Execute() has been run", "0 line(s) have been imported.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCalculateDutyFeeMenuItem_Click()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CalculateDutyFeeMenuItem.PerformClick();
				AssertEquals("Calculation finished. Please check 'Recon Charges & Fees' for each original entry.", UnitTestUserNotification.Instance.LastMessage.Text);
				reconDeclaration.US_IsAggregate = true;
				reconDeclaration.US_R_IsNoChangeAgg = true;
				form.CalculateDutyFeeMenuItem.PerformClick();
				AssertEquals(ReconDeclarationForm.NoCalculationForAggregate, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCalculateCustomsValues_Click()
		{
			ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ClassRecon;
			using (var formCL = new ReconDeclarationForm(ReconDeclaration))
			{
				Assert(!formCL.CalculateCustomValuesMenuItem.Visible);
			}

			ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ValueClassRecon;
			using (var formVC = new ReconDeclarationForm(ReconDeclaration))
			{
				Assert(formVC.CalculateCustomValuesMenuItem.Visible);
				Assert(formVC.BrokerageMenuItem.MenuItems.Contains(formVC.CalculateCustomValuesMenuItem));
				formVC.CalculateCustomValuesMenuItem.PerformClick();
				AssertEquals(typeof(ReconCalculateCustomsValueForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestFontTypeForMessageTabSetRight()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				form.MainTabControlExposed.SelectedTab = form.MessagesTabPage;
				AssertEquals(new System.Drawing.Font("Courier New", 8F), form.MessageDetailsTextBox.Font);
			}
		}

		public void TestSentQueryEntrySummaryMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			var reconJob = new ReconDeclaration(declaration);
			reconJob.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = reconJob.Invoices.AddNew();
			reconJob.InvoiceLines.AddNew();
			Factory.Save();
			using (var form = new ReconDeclarationForm(reconJob))
			{
				form.Show();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.QueryEntrySummaryMenuItem.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Recon entry does not exists for this job"));
				var entryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconEntry);
				if (entryHeader == null)
				{
					entryHeader = declaration.CustomsEntryHeaders.AddNew();
					entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
				}

				entryHeader.EntryNumber = "32377722";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.QueryEntrySummaryMenuItem.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The entry has not been lodged at Customs"));
				var entryHeader2 = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
				entryHeader2.Messages.Load();
				AssertEquals("One message has been created", 1, entryHeader2.Messages.Count);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Recon entry Query Message Sent"));
			}
		}

		public void TestNoMessageIsSentIfNoEntries()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				ReconDeclaration.OriginalEntries.RemoveAndDeleteAll();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				form.SendAddMessageMenuItem.PerformClick();
				AssertEquals("No entry to send.\r\nPlease enter at least one entry record under Recon Declaration > Entries.", UnitTestUserNotification.Instance.LastMessage.Text);
				var originalEntry = reconDeclaration.OriginalEntries.AddNew();
				JobComInvoiceHeader invoice = ReconDeclaration.Invoices.AddNew();
				invoice.US_CH_ReconEntry = originalEntry.CH_PK;
				Factory.Save();
				AssertEquals("No message has been created", 0, ReconDeclaration.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				form.SendAddMessageMenuItem.PerformClick();
				AssertEquals("One message has been created", 1, ReconDeclaration.Messages.Count);
				AssertEquals("Reconciliation 1 Add Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityRightForMessaging()
		{
			Env.Security.USReconMessaging.IsAllowed = false;
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				ReconDeclaration.OriginalEntries.RemoveAndDeleteAll();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendAddMessageMenuItem.PerformClick();
				AssertEquals(Env.Security.USReconMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendReplaceMessageMenuItem.PerformClick();
				AssertEquals(Env.Security.USReconMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendDeleteMessageMenuItem.PerformClick();
				AssertEquals(Env.Security.USReconMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendOriginalMessages()
		{
			ReconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //no to message errors
				form.SendAddMessageMenuItem.PerformClick();
				AssertEquals("No message has been created", 0, ReconDeclaration.Messages.Count);
				var newFactory = new BusinessObjectFactory();
				var loadedDec = newFactory.Load<JobDeclaration>(ReconDeclaration.ReconWrappedJobDeclaration.PK);
				AssertEquals(true, loadedDec.LockSendCustomsMessageMutex);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendAddMessageMenuItem.PerformClick();
				AssertEquals("No message has been created", 0, ReconDeclaration.Messages.Count);
				loadedDec.UnlockSendCustomsMessageMutex();
				newFactory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.SendAddMessageMenuItem.PerformClick();
				AssertEquals("One message has been created", 1, ReconDeclaration.Messages.Count);
				AssertEquals("Reconciliation 1 Add Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
				ReconDeclaration.MessageStatus = ReconMessageStatusList.Codes.ClearReconOriginal;
				AssertEquals(false, ReconDeclaration.CanSendOriginal);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendAddMessageMenuItem.PerformClick();
				AssertContains("System cannot send a reconciliation Add message, as the entry has already been added.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Still one message", 1, ReconDeclaration.Messages.Count);
			}
		}

		public void TestSendReplaceMessages()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				ReconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders[0].EntryNumber = ZString.Empty;
				form.SendReplaceMessageMenuItem.PerformClick();
				AssertContains("Entry Filer Code and Reconciliation Entry Number are required when sending a Replace message.", UnitTestUserNotification.Instance.LastMessage.Text);
				ReconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders[0].EntryNumber = "00123456";
				AssertEquals(false, ReconDeclaration.CanSendWithdrawal);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.SendReplaceMessageMenuItem.PerformClick();
				AssertContains("The Replace message you are going to send is likely to be rejected, as the entry has not been lodged at Customs yet. Proceed with sending only if the entry has been successfully lodged from a legacy system.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No message has been created", 0, ReconDeclaration.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				form.SendReplaceMessageMenuItem.PerformClick();
				AssertEquals("One message has been created", 1, ReconDeclaration.Messages.Count);
				ReconDeclaration.MessageStatus = ReconMessageStatusList.Codes.ClearReconOriginal;
				AssertEquals(true, ReconDeclaration.CanSendWithdrawal);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //no to message errors
				form.SendReplaceMessageMenuItem.PerformClick();
				AssertEquals("No message has been added", 1, ReconDeclaration.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				form.SendReplaceMessageMenuItem.PerformClick();
				AssertEquals("One message has been added", 2, ReconDeclaration.Messages.Count);
				AssertEquals("Reconciliation 1 Replace Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
				ReconDeclaration.MessageStatus = ReconMessageStatusList.Codes.ErrorReconReplace;
				AssertEquals(true, ReconDeclaration.CanSendWithdrawal);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				form.SendReplaceMessageMenuItem.PerformClick();
				AssertEquals("Additional replacement message has been created, because Error Recon Replace should allow to send another Recon Replacement message", 3, ReconDeclaration.Messages.Count);
				AssertEquals("Reconciliation 1 Replace Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendWithdrawalMessages()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				AssertEquals(false, ReconDeclaration.CanSendWithdrawal);
				ReconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders[0].EntryNumber = ZString.Empty;
				form.SendDeleteMessageMenuItem.PerformClick();
				AssertContains("Entry Filer Code and Reconciliation Entry Number are required when sending a Delete message.", UnitTestUserNotification.Instance.LastMessage.Text);
				ReconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders[0].EntryNumber = "00123456";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.SendDeleteMessageMenuItem.PerformClick();
				AssertContains("The Delete message you are going to send is likely to be rejected, as the entry has not been lodged at Customs yet. Proceed with sending only if the entry has been successfully lodged from a legacy system.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No message has been created", 0, ReconDeclaration.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				form.SendDeleteMessageMenuItem.PerformClick();
				AssertEquals("One message has been created", 1, ReconDeclaration.Messages.Count);
				ReconDeclaration.MessageStatus = ReconMessageStatusList.Codes.ClearReconOriginal;
				AssertEquals(true, ReconDeclaration.CanSendWithdrawal);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //no to message errors
				form.SendDeleteMessageMenuItem.PerformClick();
				AssertEquals("No message has been added", 1, ReconDeclaration.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				form.SendDeleteMessageMenuItem.PerformClick();
				AssertEquals("One message has been added", 2, ReconDeclaration.Messages.Count);
				AssertEquals("Reconciliation 1 Delete Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendTariffUpdateRequestMessage()
		{
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //no to save
				form.RequestTariffUpdateMenuItem.PerformClick();
				IMessageAttachee msgAttachee = ReconDeclaration;
				AssertEquals("No messages should have been created", 0, msgAttachee.Messages.Count);
				ReconDeclaration.InvoiceLines[0].JI_Tariff = "1111";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.RequestTariffUpdateMenuItem.PerformClick();
				AssertEquals("One message should have been created", 1, msgAttachee.Messages.Count);
				AssertEquals("Confirmed", ReconDeclarationForm.TariffUpdateRequestConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApportionmentAndDutyCalculationAreRefreshedOnSaving()
		{
			var reconDec = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("PreCondition:One ReconOriginalEntry", 1, reconDec.OriginalEntries.Count);
			var reconEntry = reconDec.OriginalEntries[0];
			reconEntry.US_R_DutyRateDate = CargoWise.Types.ZDateTime.Today;
			reconEntry.US_R_CalcOrigDuty = true;
			reconDec.CalculateDutyFeesForChangedEntries();
			AssertEquals("Original Duty is imported", 45m, reconEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
			using (var form = new ReconDeclarationForm(reconDec))
			{
				form.Show();
				reconEntry.Invoice.JobComInvoiceLines[0].JI_LinePrice = 0m;
				AssertEquals("Original Duty", 45m, reconEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
				AssertEquals("Apportionment dirty", true, reconDec.ApportionmentDirty);
				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
				AssertEquals("Apportionment refreshed", false, reconDec.ApportionmentDirty);
				AssertEquals("Duty is recalculated and cleared", 0m, reconEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
			}
		}

		public void TestStatementDeleteAdd_Click()
		{
			ReconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			ReconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.StatementDeleteAddMessageMenuItem.PerformClick();
				AssertEquals(typeof(StatementSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendMessagesPerformanceIssuesWithSaveButtonUpdate()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";
			Factory.Save();
			ReconDeclaration.JE_OH_NotifyParty = org.PK;
			AssertEquals(true, ReconDeclaration.HasChanges);
			using (var form = new ReconDeclarationForm(ReconDeclaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to save
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //yes to message errors
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.SendAddMessageMenuItem.PerformClick();
				AssertEquals("One message has been created", 1, ReconDeclaration.Messages.Count);
				AssertEquals("Reconciliation 1 Add Message sent", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRefreshTariffDetailsMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDecl = new ReconDeclaration(declaration);
			using (var form = new ReconDeclarationForm(reconDecl))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.RefreshTariffDetailsMenuItem.PerformClick();
				AssertEquals("Refreshing Tariffs complete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRefreshNotificationDispositionActionsMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var message = header.Messages.AddNew(typeof(EDIMessage));
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			var reconDecl = new ReconDeclaration(declaration);
			using (var form = new ReconDeclarationForm(reconDecl))
			{
				form.RefreshNotificationDispositionActionsMenuItem.PerformClick();
				AssertEquals("Declaration's notification disposition actions have been updated.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(declaration.JE_AddInfo, "ENSAction=Incomplete");
			}
		}

		public void TestOriginalEntriesGridUS_NAFTAReconIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDecl = new ReconDeclaration(declaration);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				using (var form = new ReconDeclarationForm(reconDecl))
				{
					var grid = (ZGrid)form.Controls.Find("OriginalEntriesGrid", true)[0];
					var gridColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == AddInfo.Schema.US_NAFTAReconIndicator);
					Assert("Should be visible", !gridColumnInfo.IsUnavailable);
				}
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				using (var form = new ReconDeclarationForm(reconDecl))
				{
					var grid = (ZGrid)form.Controls.Find("OriginalEntriesGrid", true)[0];
					var gridColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == AddInfo.Schema.US_NAFTAReconIndicator);
					Assert("Should not be visible", gridColumnInfo.IsUnavailable);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new ReconDeclarationForm(ReconDeclaration);
			result.ControllerID = ControllerIDs.Customs.US.Recon;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					DeclarationTestHelper.SetEntryFilerCode("XJ5");
					var declaration = Factory.New<JobDeclaration>();
					reconDeclaration = new ReconDeclaration(declaration);
					reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.ActiveEntryHeaders[0].EntryNumber = "00123456";
					reconDeclaration.OriginalEntries.AddNew();
					JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
					invoice.US_CH_ReconEntry = reconDeclaration.OriginalEntries[0].CH_PK;
					reconDeclaration.InvoiceLines.AddNew();
					Factory.Save();
				}

				return reconDeclaration;
			}
		}

		void AssertPlugIn(ControllerID id, ReconDeclarationForm form)
		{
			AssertNotNull(id.Name + " is added", form.PlugIns.GetPlugIn(id));
			AssertNoExceptionThrown(() => form.PlugIns.GetPlugIn(id).SelectTabPage());
		}
	}
}
