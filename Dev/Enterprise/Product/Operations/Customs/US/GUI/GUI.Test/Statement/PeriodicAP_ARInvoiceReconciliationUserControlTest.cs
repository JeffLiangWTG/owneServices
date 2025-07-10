using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	sealed class PeriodicAP_ARInvoiceReconciliationUserControlTest : TestCaseWithFactory
	{
		public void TestWhenThereIsNoSecurityRight()
		{
			CusStatementHeader statement = CreateStatement();
			using (PeriodicStatementForm statementForm = new PeriodicStatementForm(statement))
			{
				statementForm.Show();
				statementForm.MainTabControlForTesting.SelectedTab = statementForm.AccReconTabPage;
				Env.Security.USCustomsImportStatementAccIntegrationModify.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				AssertEquals(Env.Security.USCustomsImportStatementAccIntegrationModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.USCustomsImportStatementAccIntegrationModify.IsAllowed = true;
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				//security is allowed now and should not have the error
				AssertNotEquals(Env.Security.USCustomsImportStatementAccIntegrationModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWhenHasChanges()
		{
			CusStatementHeader statement = CreateStatement();
			using (PeriodicStatementForm statementForm = new PeriodicStatementForm(statement))
			{
				statementForm.Show();
				statement.B2_Status = StatementHeaderStatusList.Codes.Final;
				statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
				AssertEquals("PreCondition", true, statement.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.USCustomsImportStatementAccIntegrationModify.IsAllowed = true;
				statementForm.MainTabControlForTesting.SelectedTab = statementForm.AccReconTabPage;
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Cancel);
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				AssertContains(AccIntegrationHandler.SaveFirst, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK);
				statement.PostARInvoices = true;
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				//should have saved and continued integration
				AssertEquals("Saved", false, statement.HasChanges);
				var htmlForm = (HtmlInterpretationForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("Accounting Integration", htmlForm.FormHeading);
			}
		}

		public void TestWhenWarningNotificationExists()
		{
			CusStatementHeader statement = CreateStatement();
			statement.PostAPInvoices = true;
			using (PeriodicStatementForm statementForm = new PeriodicStatementForm(statement))
			{
				statementForm.Show();
				Factory.Save();
				AssertEquals("PreCondition", false, statement.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.USCustomsImportStatementAccIntegrationModify.IsAllowed = true;
				statementForm.MainTabControlForTesting.SelectedTab = statementForm.AccReconTabPage;
				string notification = statement.GetWarningNotificationsBeforePerformingAccIntegration();
				//notification should exist ie. preliminary status or broker is not indicated as payer
				AssertEquals(false, string.IsNullOrEmpty(notification));
				AssertEquals("PreCondition:No error expected", true, string.IsNullOrEmpty(statement.GetErrorNotificationsBeforePerformingAccIntegration()));
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Cancel);
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				AssertContains(notification, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK);
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				//should have continued integration
				var htmlForm = (HtmlInterpretationForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("Accounting Integration", htmlForm.FormHeading);
			}
		}

		public void TestWhenErrorNotificationExists()
		{
			CusStatementHeader statement = CreateStatement();
			SetRegistryItems();
			using (PeriodicStatementForm statementForm = new PeriodicStatementForm(statement))
			{
				statementForm.Show();
				AssertEquals("PreCondition", false, statement.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.USCustomsImportStatementAccIntegrationModify.IsAllowed = true;
				statementForm.MainTabControlForTesting.SelectedTab = statementForm.AccReconTabPage;
				string notification = statement.GetErrorNotificationsBeforePerformingAccIntegration();
				//notification should exist ie. no option is selected
				AssertEquals(false, string.IsNullOrEmpty(notification));
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				AssertContains(notification, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				statement.PostAPInvoices = true;
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				AssertNotContains(notification, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWhenResultHasChanges()
		{
			var testHelper = SetRegistryItems();
			CusStatementHeader statement = CreateStatement(testHelper.Importer);
			using (PeriodicStatementForm statementForm = new PeriodicStatementForm(statement))
			{
				statementForm.Show();
				statement.PostAPInvoices = true;
				statement.PostARInvoices = true;
				statement.B2_Status = StatementHeaderStatusList.Codes.Final;
				statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.USCustomsImportStatementAccIntegrationModify.IsAllowed = true;
				statementForm.MainTabControlForTesting.SelectedTab = statementForm.AccReconTabPage;
				string notification = statement.GetWarningNotificationsBeforePerformingAccIntegration();
				Assert("PreCondition", string.IsNullOrEmpty(notification));
				statementForm.periodicAP_ARInvoiceReconUserControl1.AccIntegrationButton.PerformClick();
				AssertContains(AccIntegrationHandler.FinishedWithActionsTakenReviewEntry, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		CusStatementHeader CreateStatement(OrgHeader importer = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (importer != null)
			{
				declaration.JE_OH_Importer = importer.PK;
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			entry.EntryNumber = "111111";
			Factory.Save();
			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_StatementNumber = "123456";
			dailyStatement.B2_IsMonthlyStatement = false;
			dailyStatement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			dailyStatement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			dailyStatement.B2_StatementAmount = 30m;
			var statementLine = dailyStatement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "111111";
			statementLine.B3_CustomsFeesTotal = 30m;
			AssertNotNull(statementLine.Declaration);
			var charge = statementLine.Charges.AddNew();
			charge.B4_ChargeType = "DTY";
			charge.B4_ChargeAmount = 30m;
			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1234P";
			monthlyStatement.B2_IsMonthlyStatement = true;
			monthlyStatement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			monthlyStatement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			monthlyStatement.B2_StatementAmount = 30m;
			dailyStatement.B2_B2_PeriodicStatement = monthlyStatement.PK;
			return monthlyStatement;
		}

		Customs.Business.Testing.InvoicingTestHelper SetRegistryItems()
		{
			var postMaster = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var groupNotificaiton = new AutoBillingGroupNotification();
			groupNotificaiton.SendGroupPK = postMaster.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotificaiton);
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "test@cargowise.com";
			postMaster.Staff.Add(currentUserInCurrentFactory);
			Factory.Save();
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();
			return testHelper;
		}
	}
}
