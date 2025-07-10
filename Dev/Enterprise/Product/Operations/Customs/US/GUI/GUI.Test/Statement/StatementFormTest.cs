using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(StatementForm))]
	sealed class StatementFormTest : ZFormBasherTest
	{
		public void TestClickSendButton()
		{
			Header.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			Header.B2_EntryFilerCode = "F12";
			Header.B2_StatementNumber = "8804004001";
			Header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			var line = Header.StatementLines.AddNew();
			line.B3_CustomsFeesTotal = 123.45m;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (var form = new StatementFormTestForTesting(Header))
			{
				form.paymentAuthorizationMessage.PerformClick();
				AssertEquals("message has been generated (same as before)", 1, Header.Messages.Count);
				AssertEquals(PaymentStatusList.Codes.PaymentInProgress, Header.B2_PaymentStatus);
			}

			Header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;

			using (var form = new StatementFormTestForTesting(Header))
			{
				form.ShouldContinueToSend = false;
				form.paymentAuthorizationMessage.PerformClick();
				AssertEquals("no message has been generated", 1, Header.Messages.Count);
				AssertEquals("status stays the same", PaymentStatusList.Codes.PaymentFailed, Header.B2_PaymentStatus);
			}
		}

		public void TestSecurityRightsForPaymentAuthorizationMessage()
		{
			Env.Security.USCustomsImportStatementModify.IsAllowed = false;
			Env.Security.USCustomsImportStatementView.IsAllowed = true;
			Env.Security.USCustomsImportStatementSendAuthMsgBroker.IsAllowed = true;
			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = true;

			using (var form = new StatementFormTestForTesting(Header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.paymentAuthorizationMessage.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Statement Payment message Sent"));
			}

			Header.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			Env.Security.USCustomsImportStatementSendAuthMsgBroker.IsAllowed = false;

			using (var form = new StatementFormTestForTesting(Header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.paymentAuthorizationMessage.PerformClick();
				AssertEquals(Env.Security.USCustomsImportStatementSendAuthMsgBroker.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.USCustomsImportStatementSendAuthMsgBroker.IsAllowed = true;

			using (var form = new StatementFormTestForTesting(Header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.paymentAuthorizationMessage.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("payment is in progress for this statement"));
			}

			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = false;
			Header.B2_PaymentParty = PaymentPartyList.Codes.Importer;

			using (var form = new StatementFormTestForTesting(Header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.paymentAuthorizationMessage.PerformClick();
				AssertEquals(Env.Security.USCustomsImportStatementSendAuthMsgImporter.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResetPaymentStatusMenu()
		{
			Header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			Header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			using (var form = new StatementForm(header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.resetPaymentStatusMenu.PerformClick();
				AssertEquals(PaymentStatusList.Codes.PaymentAuthorizationAccepted, Header.B2_PaymentStatus);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.resetPaymentStatusMenu.PerformClick();
				AssertEquals("", Header.B2_PaymentStatus);
				var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CusStatementHeader.PaymentAuthorizationAcceptedReference);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.Events.Authorised.Code);
				var logs = Header.Logs.Find(query);
				foreach (var log in logs)
				{
					AssertEquals(log.IsCancelled, true);
				}
			}
		}

		public void TestStatementDeleteAdd_Click()
		{
			Header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			var line = Header.StatementLines.AddNew();
			line.B3_Status = StatementLineStatusList.Codes.Active;
			using (var form = new StatementForm(header))
			{
				form.statementDeleteAddMessage.PerformClick();
				AssertEquals(typeof(StatementSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestMarkAsDeletedOnStatementForm()
		{
			Env.Security.USCustomsImportStatementModify.IsAllowed = true;
			var line1 = Header.StatementLines.AddNew();
			var line2 = Header.StatementLines.AddNew();
			line1.B3_Status = StatementLineStatusList.Codes.Active;
			line2.B3_Status = StatementLineStatusList.Codes.Deleted;
			//test not allowed for FIN statement
			Header.B2_Status = StatementHeaderStatusList.Codes.Final;
			using (var form = new StatementForm(header))
			{
				form.Show();
				form.StatementLinesGrid.CurrentRowIndex = 0;
				form.UpdateLineStatusMenuItemCaptionAndVisibility(null, null); //calling event handler directly instead of using ContextMenu.Show which blocks test execution
				var markAsDeleted = form.StatementLinesGrid.ContextMenu.MenuItems.FindByText("&Mark Line Status as Deleted");
				AssertNotNull(markAsDeleted);
				markAsDeleted.PerformClick();
				var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(Res.GetString("6CB97CC1-E9EE-410D-910C-AED8E1EFC7B8", "This statement is final, and changing line status is not allowed."), actualMessage);
			}

			//test data is changed from active to deleted and backwards
			Header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			using (var form = new StatementForm(header))
			{
				form.Show();
				form.StatementLinesGrid.CurrentRowIndex = 0;
				form.UpdateLineStatusMenuItemCaptionAndVisibility(null, null); //calling event handler directly instead of using ContextMenu.Show which blocks test execution
				var switchLineStatusItem = form.StatementLinesGrid.ContextMenu.MenuItems.FindByText("&Mark Line Status as Deleted");
				AssertNotNull(switchLineStatusItem);
				AssertEquals(true, switchLineStatusItem.Visible);
				switchLineStatusItem.PerformClick();
				AssertEquals(StatementLineStatusList.Codes.Deleted, line1.B3_Status);
				form.StatementLinesGrid.CurrentRowIndex = 1;
				form.UpdateLineStatusMenuItemCaptionAndVisibility(null, null); //calling event handler directly instead of using ContextMenu.Show which blocks test execution
				switchLineStatusItem = form.StatementLinesGrid.ContextMenu.MenuItems.FindByText("&Mark Line Status as Active");
				AssertNotNull(switchLineStatusItem);
				AssertEquals(true, switchLineStatusItem.Visible);
				switchLineStatusItem.PerformClick();
				AssertEquals(StatementLineStatusList.Codes.Active, line2.B3_Status);
				//test data is not changed without necessary rights
				Env.Security.USCustomsImportStatementModify.IsAllowed = false;
				switchLineStatusItem.PerformClick();
				AssertEquals(Env.Security.USCustomsImportStatementModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(StatementLineStatusList.Codes.Active, line2.B3_Status);
			}

			//test statement header status changed accordingly
			Env.Security.USCustomsImportStatementModify.IsAllowed = true;
			Header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			line1.B3_Status = StatementLineStatusList.Codes.DeletionPending;
			line2.B3_Status = StatementLineStatusList.Codes.Active;
			using (var form = new StatementForm(header))
			{
				form.Show();
				form.StatementLinesGrid.CurrentRowIndex = 0;
				form.UpdateLineStatusMenuItemCaptionAndVisibility(null, null); //calling event handler directly instead of using ContextMenu.Show which blocks test execution					
				var switchLineStatusItem = form.StatementLinesGrid.ContextMenu.MenuItems.FindByText("&Mark Line Status as Active");
				AssertNotNull(switchLineStatusItem);
				switchLineStatusItem.PerformClick();
				AssertEquals(StatementLineStatusList.Codes.Active, line1.B3_Status);
				AssertEquals(StatementHeaderStatusList.Codes.Preliminary, Header.B2_Status);
				form.StatementLinesGrid.CurrentRowIndex = 1;
				form.UpdateLineStatusMenuItemCaptionAndVisibility(null, null); //calling event handler directly instead of using ContextMenu.Show which blocks test execution
				switchLineStatusItem = form.StatementLinesGrid.ContextMenu.MenuItems.FindByText("&Mark Line Status as Deleted");
				AssertNotNull(switchLineStatusItem);
				switchLineStatusItem.PerformClick();
				AssertEquals(StatementLineStatusList.Codes.Deleted, line2.B3_Status);
				AssertEquals(StatementHeaderStatusList.Codes.Preliminary, Header.B2_Status);
				form.StatementLinesGrid.CurrentRowIndex = 0;
				form.UpdateLineStatusMenuItemCaptionAndVisibility(null, null); //calling event handler directly instead of using ContextMenu.Show which blocks test execution
				switchLineStatusItem = form.StatementLinesGrid.ContextMenu.MenuItems.FindByText("&Mark Line Status as Deleted");
				AssertNotNull(switchLineStatusItem);
				switchLineStatusItem.PerformClick();
				AssertEquals(StatementLineStatusList.Codes.Deleted, line1.B3_Status);
				AssertEquals(StatementHeaderStatusList.Codes.Deleted, Header.B2_Status);
				form.StatementLinesGrid.CurrentRowIndex = 0;
				form.UpdateLineStatusMenuItemCaptionAndVisibility(null, null); //calling event handler directly instead of using ContextMenu.Show which blocks test execution
				switchLineStatusItem = form.StatementLinesGrid.ContextMenu.MenuItems.FindByText("&Mark Line Status as Active");
				AssertNotNull(switchLineStatusItem);
				switchLineStatusItem.PerformClick();
				AssertEquals(StatementLineStatusList.Codes.Active, line1.B3_Status);
				AssertEquals(StatementHeaderStatusList.Codes.Preliminary, Header.B2_Status);
			}
		}

		public void TestPaymentAuthorization_ClickWhenMessageError()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EntryFilerCode = "XJ5";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "ENS";
			entry.EntryNumber = "1";
			Factory.Save();
			declaration.ReleaseStatus = "CAN";
			var statementLine = Header.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "1";
			USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = new StatementForm(Header))
			{
				form.paymentAuthorizationMessageApproval.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Release Status: This entry has not been released yet"));
			}
		}

		public void TestPaymentAuthorization_Click()
		{
			using (var form = new StatementForm(Header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.paymentAuthorizationMessageApproval.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("This action is inactive"));
			}

			USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = new StatementForm(Header))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.paymentAuthorizationMessageApproval.PerformClick();
				AssertEquals("Action Authorized and log exists", true, header.ApprovalActionAuthorised);
			}
		}

		public void TestDeletePaymentAuthorization_Click()
		{
			USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = new StatementForm(Header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.deletePaymentAuthorization.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Permission to 'Send Payment Authorization' needs to be granted"));
			}

			USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var message = Header.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			using (var form = new StatementForm(Header))
			{
				Assert(form.deletePaymentAuthorization.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.deletePaymentAuthorization.PerformClick();
				AssertEquals(2, Header.Messages.Count);
				AssertEquals(Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, Header.Messages[1].EM_MessageType);
			}

			Header.Messages.RemoveAndDeleteAllFromTest();
			message = Header.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.Codes.DailyStatement;
			using (var form = new StatementForm(Header))
			{
				Assert(form.deletePaymentAuthorization.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.deletePaymentAuthorization.PerformClick();
				AssertEquals(2, Header.Messages.Count);
				AssertEquals(Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, Header.Messages[1].EM_MessageType);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new StatementForm(Header);
			result.ControllerID = ControllerIDs.Customs.CustomsStatement;
			return result;
		}
		CusStatementHeader header;
		CusStatementHeader Header => header ?? (header = Factory.New<CusStatementHeader>());

		sealed class StatementFormTestForTesting : StatementForm
		{
			public StatementFormTestForTesting(CusStatementHeader statementHeader)
				: base(statementHeader)
			{
			}

			protected override bool ShouldContinueToSendPaymentMessage(StatementPaymentAction action) => ShouldContinueToSend;

			public bool ShouldContinueToSend { get; set; } = true;
		}
	}
}
