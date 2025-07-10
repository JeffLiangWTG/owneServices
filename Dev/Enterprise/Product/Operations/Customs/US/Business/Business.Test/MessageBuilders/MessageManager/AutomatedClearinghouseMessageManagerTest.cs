using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutomatedClearinghouseMessageManagerTest : TestCaseWithFactory
	{
		public void TestPaymentMessagePTBlockPayType()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_EntryFilerCode = "F12";
			statement.B2_StatementNumber = "1100900000";

			var line = statement.StatementLines.AddNew();
			line.B3_CustomsFeesTotal = 123.45m;

			var manager = new AutomatedClearinghouseMessageManager();
			var action = new StatementPaymentAction(statement);
			manager.SendAuthorisation(action);
			AssertEquals(1, statement.Messages.Count);
			var message = (MQEDIMessage)statement.Messages[0];
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, message.EM_MessageType);

			var pt = (PDSPT)message.MessageBlock.MessageBlocks.Find(x => x is PDSPT);
			AssertEquals("01", pt.PaymentType);

			statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.B2_EntryFilerCode = "F12";
			statement.B2_StatementNumber = "0912345678";
			action = new StatementPaymentAction(statement);
			manager.SendAuthorisation(action);

			message = (MQEDIMessage)statement.Messages[0];
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, message.EM_MessageType);
			pt = (PDSPT)message.MessageBlock.MessageBlocks.Find(x => x is PDSPT);
			AssertEquals("02", pt.PaymentType);

			statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.B2_EntryFilerCode = "F12";
			statement.B2_StatementNumber = "0912345678";
			action = new StatementPaymentAction(statement);
			manager.SendAuthorisation(action);
		}

		public void TestSendMessageAndSaveWithoutSending()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			CusStatementLine line = statementHeader.StatementLines.AddNew();
			line.B3_Status = StatementLineStatusList.Codes.Active;
			AssertEquals("ActiveLines", true, statementHeader.ActiveLines.Contains(line));

			StatementDeleteAndSendingActionCollection actions = new StatementDeleteAndSendingActionCollection(statementHeader);
			actions.IsCancelled = false;
			actions[0].US_SendMessage = false;

			AssertEquals("SendMessage", false, actions.SendMessage);
			AssertEquals("SaveWithoutSending", false, new AutomatedClearinghouseMessageManager().GenerateStatementDeleteAdd(actions));

			actions[0].US_SendMessage = true;
			AssertEquals("SendMessage", true, actions.SendMessage);
			AssertEquals("SaveWithoutSending", true, new AutomatedClearinghouseMessageManager().GenerateStatementDeleteAdd(actions));
			AssertEquals(false, actions.IsCancelled);

			AssertEquals("Line status changed", StatementLineStatusList.Codes.DeletionPending, line.B3_Status);
			AssertEquals("ActiveLines", false, statementHeader.ActiveLines.Contains(line));
		}

		public void TestGenerateStatementDeleteAdd()
		{
			//from a statement
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			CusStatementLine line = statementHeader.StatementLines.AddNew();
			line.B3_Status = StatementLineStatusList.Codes.Active;

			StatementDeleteAndSendingActionCollection actionsForStatement = new StatementDeleteAndSendingActionCollection(statementHeader);
			actionsForStatement[0].US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;//3
			actionsForStatement[0].US_PreliminaryStatementPrintDate = new CargoWise.Types.ZDateTime(2009, 7, 5);
			actionsForStatement[0].US_ClientBranchDesignation = "02";

			bool messagesGeneratedStatement = new AutomatedClearinghouseMessageManager().GenerateStatementDeleteAdd(actionsForStatement);
			AssertEquals("Messages generated for this statement header", true, messagesGeneratedStatement);

			AssertEquals(1, statementHeader.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, statementHeader.Messages[0].EM_MessageSubType);
			AssertEquals(StatementLineStatusList.Codes.DeletionPending, line.B3_Status);

			var astuhRecord = (ASTUH)((MQEDIMessage)statementHeader.Messages[0]).MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "H");
			AssertEquals(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, astuhRecord.PaymentTypeIndicator.ToString());
			AssertEquals(new CargoWise.Types.ZDateTime(2009, 7, 5), astuhRecord.PreliminaryStatementPrintDate);
			AssertEquals("02", astuhRecord.ClientBranchDesignation);

			//from a declaration
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.US_PaymentType = "2";

			line.B3_Status = StatementLineStatusList.Codes.Active;

			StatementDeleteAndSendingActionCollection actionsForEntry = new StatementDeleteAndSendingActionCollection(declaration);
			actionsForEntry[0].US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;//7
			actionsForEntry[0].US_PreliminaryStatementPrintDate = new CargoWise.Types.ZDateTime(2009, 7, 5);
			actionsForEntry[0].US_ClientBranchDesignation = "02";
			actionsForEntry[0].US_PeriodicStatementMonth = "07";

			bool messagesGenerated = new AutomatedClearinghouseMessageManager().GenerateStatementDeleteAdd(actionsForEntry);
			AssertEquals("Messages generated for this statement header", true, messagesGenerated);

			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(StatementLineStatusList.Codes.DeletionPending, line.B3_Status);

			astuhRecord = (ASTUH)((MQEDIMessage)declaration.Messages[0]).MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "H");
			AssertEquals(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, astuhRecord.PaymentTypeIndicator.ToString());
			AssertEquals(new CargoWise.Types.ZDateTime(2009, 7, 5), astuhRecord.PreliminaryStatementPrintDate);
			AssertEquals("02", astuhRecord.ClientBranchDesignation);
			AssertEquals("07", astuhRecord.PeriodicStatementMonth);
		}

		public void TestSendAuthorisation()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(importer);
			wrapper.ZO_AccountNo = "UN123";

			CusStatementHeader bizObj = Factory.New<CusStatementHeader>();
			bizObj.B2_OH_Importer = importer.PK;
			bizObj.B2_PaymentType = "3";
			bizObj.B2_EntryFilerCode = "F12";
			bizObj.B2_StatementNumber = "0912345678";
			CusStatementLine line = bizObj.StatementLines.AddNew();
			line.B3_CustomsFeesTotal = 123.45m;

			AutomatedClearinghouseMessageManager manager = new AutomatedClearinghouseMessageManager();
			StatementPaymentAction action = new StatementPaymentAction(bizObj);
			manager.SendAuthorisation(action);
			AssertEquals(1, bizObj.Messages.Count);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, bizObj.Messages[0].EM_MessageType);

			CusStatementHeader header = Factory.New<CusStatementHeader>();
			header.B2_OH_Importer = importer.PK;
			header.B2_PaymentType = "7";
			header.B2_EntryFilerCode = "F12";
			header.B2_StatementNumber = "1100P00000";
			action = new StatementPaymentAction(header);
			action.PayerUnitNo = "UN123";

			manager.SendAuthorisation(action);
			AssertEquals(1, header.Messages.Count);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, header.Messages[0].EM_MessageType);

			AssertEquals(PaymentPartyList.Codes.Importer, header.B2_PaymentParty);
			AssertEquals("UN123", header.B2_AccountNo);

			header.B2_IsMonthlyStatement = true;
			action = new StatementPaymentAction(header);
			action.PayerUnitNo = "UN123";

			header.Messages.RemoveAndDeleteAll();
			manager.SendAuthorisation(action);
			AssertEquals(1, header.Messages.Count);

			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, header.Messages[0].EM_MessageType);

			header = Factory.New<CusStatementHeader>();
			header.B2_OH_Importer = importer.PK;
			header.B2_PaymentType = Business.PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			header.B2_EntryFilerCode = "F12";
			header.B2_StatementNumber = "1100900000";

			action = new StatementPaymentAction(header);
			manager.SendAuthorisation(action);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, header.Messages[0].EM_MessageType);

			header = Factory.New<CusStatementHeader>();
			header.B2_OH_Importer = importer.PK;
			header.B2_PaymentType = Business.PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			header.B2_EntryFilerCode = "F12";
			header.B2_StatementNumber = "1100900000";

			action = new StatementPaymentAction(header);
			manager.SendAuthorisation(action);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, header.Messages[0].EM_MessageType);

			header.Messages.RemoveAndDeleteAll();
			var message = header.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;

			action = new StatementPaymentAction(header);
			manager.SendAuthorisation(action);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, header.Messages[1].EM_MessageType);
		}

		public void TestBillNumber()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "2710288B73";//picked up from the real statement
			statement.B2_StatementAmount = 200m;
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			statement.B2_EntryFilerCode = "XJ5";

			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.B3_CustomsFeesTotal = 200m;
			statementLine.B3_EntryFilerCode = "XJ5";

			Factory.Save();

			StatementPaymentAction action = new StatementPaymentAction(statement);
			AutomatedClearinghouseMessageManager manager = new AutomatedClearinghouseMessageManager();
			manager.SendAuthorisation(action);

			MQEDIMessage message = (MQEDIMessage)statement.Messages[0];
			var pt = message.MessageBlock.MessageBlocks.OfType<PDSPT>().FirstOrDefault();

			AssertEquals("2710288B73", pt.StatementNumber);
		}
	}
}
