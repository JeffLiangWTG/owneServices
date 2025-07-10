using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI.MessagingProcess;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessagingProcess;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI.MessagingProcess.Testing
{
	sealed class ZACustomsMessagingGuiTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var declaration = CreateDeclaration(true);

			using (var mainForm = new ZForm())
			{
				var zaGUI = new ZACustomsMessagingGui(declaration, mainForm);

				CombineAssertions(() =>
				{
					AssertType<ZACustomsMessagingProvider>("Provider", zaGUI.CustomsMessagingSupporter.Provider);
					AssertSame("Form", mainForm, zaGUI.topLevelBusinessObjectForm);
				});
			}
		}

		public void TestGetSendDialog()
		{
			var declaration = CreateDeclaration(true);

			using (var mainForm = new ZForm())
			{
				var zaGUI = new ZACustomsMessagingGui(declaration, mainForm);

				CombineAssertions(() =>
				{
					var guiInterface = zaGUI as ISupportSendDialog;
					var dlg = guiInterface.GetSendDialog(new ActionResult());
					var provider = zaGUI.CustomsMessagingSupporter.Provider as ZACustomsMessagingProvider;

					AssertNotNull("Dialog created", dlg);
					AssertType<JobDeclarationMessageSendingObjectParent>("DataSource Type", dlg.DataSource);

					AssertSame("JobDecSendingObjParent is Data source", provider.DeclarationWrapper, dlg.DataSource);
					AssertEquals("Form Type", typeof(MessageSendingForm), dlg.TypeOfForm);
				});
			}
		}

		public void TestSendToCustoms_WithDeferment()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 2 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			using (var mainForm = new ZForm())
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save?
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation errors
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // SendDialog & Deferment Dialog
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Result

				ZACustomsMessagingGui.SendToCustoms(declaration, mainForm);

				AssertNotNull("Check form was displayed", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Deferment Dialog was shown", typeof(DeferredSubmissionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertContains("Final notification", "1 Message(s) queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendToCustoms_WithDefermentCancelled()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 2 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			using (var mainForm = new ZForm())
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save?
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation errors

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((frm) =>
				{
					if (frm is MessageSendingForm)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // SendDialog
					}
					else if (frm is DeferredSubmissionForm)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel; // Deferment
					}
				});

				ZACustomsMessagingGui.SendToCustoms(declaration, mainForm);

				AssertNotNull("Check form was displayed", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Deferment Dialog was shown", typeof(DeferredSubmissionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertContains("Cancel notification", "User canceled, deferment aborted", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendToCustoms_DefermentDisabled()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = false, DaysBeforeETA = 0 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			using (var mainForm = new ZForm())
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save?
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation errors
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // SendDialog
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Result

				ZACustomsMessagingGui.SendToCustoms(declaration, mainForm);

				AssertNotNull("Check form was displayed", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Message Sending Form was shown", typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertContains("Final notification", "1 Message(s) queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConfigureProcess()
		{
			var declaration = CreateDeclaration(false);
			using (var mainForm = new ZForm())
			{
				var gui = new ZACustomsMessagingGui(declaration, mainForm);
				var sendProcess = new SendMessagesProcess();
				var sendChain = sendProcess.SendProcessChain;

				var showSend = sendChain.FindAction(SendMessagesProcess.SendMessageActions.ShowSendDialog);

				CombineAssertions(() =>
				{
					AssertContains("Pre-req", "{ ShowSendDialog: success: { PreSendValidation:", showSend.GetChainAsString());
					gui.ConfigureProcess(sendChain);
					AssertContains("ZA Defer Dialog Step added", "{ ShowSendDialog: success: { ZAShowDefermentDialog: success: { PreSendValidation:", showSend.GetChainAsString());
				});
			}
		}

		public void TestConfigureProcess_FullDeferment()
		{
			var declaration = CreateDeclaration(false);
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var mainForm = new ZForm())
			{
				var gui = new ZACustomsMessagingGui(declaration, mainForm);
				var sendProcess = new SendMessagesProcess(gui.CustomsMessagingSupporter.GetSendMessagesBusinessActionProvider(), gui.GetSendMessagesGuiActionProvider());
				var sendChain = sendProcess.SendProcessChain;

				AssertContains("Deferment steps", "{ ShowSendDialog: success: { ZAShowDefermentDialog: success: { ZADefermentUpdate: success: { PreSendValidation:", sendChain.GetChainAsString());
			}
		}

		public void TestICustomsMessagingGui()
		{
			var declaration = CreateDeclaration(false);
			using (var mainForm = new ZForm())
			{
				var gui = new ZACustomsMessagingGui(declaration, mainForm) as ICustomsMessagingGui;

				CombineAssertions(() =>
				{
					AssertNotNull("Supporter", gui.MessagingSupporter);
					AssertSame("Form", mainForm, gui.TopLevelBusinessObjectForm);
				});
			}
		}

		JobDeclaration CreateDeclaration(bool addEntry)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			if (addEntry)
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var line = entry.MergedLines.AddNew();
				invoiceLine.JI_CL = line.PK;
			}

			return declaration;
		}
	}
}
