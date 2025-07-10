using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class StatementMessagesHandlerTest : TestCaseWithFactory
	{
		[TestDate(2015, 07, 31)]
		public void TestPerformSentDeleteAddMessageForDeclaration()
		{
			JobDeclaration declaration = GetTestDeclaration();
			var actions = new StatementDeleteAndSendingActionCollection(declaration);
			var mock = new Mock<StatementMessagesHandler>();
			mock
				.Protected()
				.Setup<bool>("GetShouldContinueToSendAfterShowingMessageSendingActions", actions)
				.Returns(true);
			MessageSendingNotificationCollection notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(declaration);
			mock.Object.SendDeleteAddMessage(actions, notifications, declaration.Factory);
			AssertEquals("message has been generated", 1, declaration.Messages.Count);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.StatementUpdate, declaration.Messages[0].EM_MessageType);
		}

		public void TestSecurityRightForMessaging()
		{
			Env.Security.USCustomsImportStatementMessaging.IsAllowed = false;
			var declaration = GetTestDeclaration();
			var actions = new StatementDeleteAndSendingActionCollection(declaration);
			var mock = new Mock<StatementMessagesHandler>();
			mock
				.Protected()
				.Setup<bool>("GetShouldContinueToSendAfterShowingMessageSendingActions", actions)
				.Returns(true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(declaration);
			mock.Object.SendDeleteAddMessage(actions, notifications, declaration.Factory);
			AssertEquals(Env.Security.USCustomsImportStatementMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPerformSentDeleteAddMessageForRecon()
		{
			var reconActions = new StatementDeleteAndSendingActionCollection(ReconDeclaration);
			var mock = new Mock<StatementMessagesHandler>();
			mock
				.Protected()
				.Setup<bool>("GetShouldContinueToSendAfterShowingMessageSendingActions", reconActions)
				.Returns(true);
			MessageSendingNotificationCollection notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(ReconDeclaration);
			mock.Object.SendDeleteAddMessage(reconActions, notifications, ReconDeclaration.Factory);
			AssertEquals("message has been generated", 1, ReconDeclaration.Messages.Count);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.StatementUpdate, ReconDeclaration.Messages[0].EM_MessageType);
		}

		public void TestSecurityRightForMessagingFromRecon()
		{
			Env.Security.USCustomsImportStatementMessaging.IsAllowed = false;
			var reconActions = new StatementDeleteAndSendingActionCollection(ReconDeclaration);
			var mock = new Mock<StatementMessagesHandler>();
			mock
				.Protected()
				.Setup<bool>("GetShouldContinueToSendAfterShowingMessageSendingActions", reconActions)
				.Returns(true);
			var notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(ReconDeclaration);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			mock.Object.SendDeleteAddMessage(reconActions, notifications, Factory);
			AssertEquals(Env.Security.USCustomsImportStatementMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[TestDate(2015, 07, 31)]
		public void TestPerformSentDeleteAddMessageForStatement()
		{
			Header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			CusStatementLine line = Header.StatementLines.AddNew();
			line.B3_Status = StatementLineStatusList.Codes.Active;
			StatementDeleteAndSendingActionCollection actions = new StatementDeleteAndSendingActionCollection(Header);
			actions[0].US_SendMessage = true;
			var mock = new Mock<StatementMessagesHandler>();
			MessageSendingNotificationCollection notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(Header);
			mock
				.Protected()
				.Setup<bool>("GetShouldContinueToSendAfterShowingMessageSendingActions", actions)
				.Returns(true);
			mock.Object.SendDeleteAddMessage(actions, notifications, Header.Factory);
			AssertEquals("message has been generated", 1, Header.Messages.Count);
			AssertEquals(StatementLineStatusList.Codes.DeletionPending, Header.StatementLines[0].B3_Status);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.StatementUpdate, Header.Messages[0].EM_MessageType);
		}

		public void TestSecurityRightForMessagingFromStatement()
		{
			Env.Security.USCustomsImportStatementMessaging.IsAllowed = false;
			var actions = new StatementDeleteAndSendingActionCollection(Header);
			var mock = new Mock<StatementMessagesHandler>();
			mock
				.Protected()
				.Setup<bool>("GetShouldContinueToSendAfterShowingMessageSendingActions", actions)
				.Returns(true);
			var notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(Header);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			mock.Object.SendDeleteAddMessage(actions, notifications, Factory);
			AssertEquals(Env.Security.USCustomsImportStatementMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");
					reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
					reconDeclaration.OriginalEntries.AddNew();
					reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
					JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
					invoice.US_CH_ReconEntry = reconDeclaration.OriginalEntries[0].CH_PK;
					reconDeclaration.InvoiceLines.AddNew();
					ReconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
					Factory.Save();
				}

				return reconDeclaration;
			}
		}

		CusStatementHeader header;
		CusStatementHeader Header
		{
			get
			{
				return header ?? (header = Factory.New<CusStatementHeader>());
			}
		}

		JobDeclaration GetTestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.Factory.Save();
			return declaration;
		}
	}
}
