using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class StatementUpdateMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2015, 07, 25)]
		public void TestEndToEndTest()
		{
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_EntryFilerCode = "XJ5";
			statementHeader.B2_ProcessDate = new ZDateTime(2007, 9, 18);
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statementLine.B3_EntryProcessPort = "1111";

			StatementDeleteTransactionWrapper wrapper = new StatementDeleteTransactionWrapper(statementLine);
			wrapper.PreliminaryStatementPrintDate = new ZDateTime(2007, 9, 19);
			wrapper.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			wrapper.PeriodicStatementMonth = MonthList.Codes._09;
			wrapper.ClientBranchDesignation = "02";

			StatementUpdateMessageBuilder builder = new StatementUpdateMessageBuilder(wrapper);
			MQEDIMessage message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated",
@"B  8888XJ5SU                                               <<MSGNO PLACEHOLDER>>
H1111XJ5  1000112730919070209                                                   
Y  8888XJ5SU",
				message.EM_FormattedMessageText);
			AssertEquals(GlbBranch.CurrentBranch.PK, message.EM_GB);
		}

		[TestDate(2015, 07, 25)]
		public void TestEndToEndWhereBranchDiffersToCurrentlyLoggedInBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "EYL";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "12345";
			declaration.US_SchDEntry = "1111";
			declaration.US_EntryType = "01";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 10);

			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "8888";
			declaration.US_PreparerOfficeCode = "02";

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			statementHeader.B2_ProcessPort = "1111";
			statementHeader.B2_EntryFilerCode = "XJ5";
			statementHeader.B2_ProcessDate = new ZDateTime(2007, 9, 20);
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statementLine.B3_EntryProcessPort = "1111";
			statementLine.B3_EntryNum = "12345";

			var wrapper = new StatementDeleteTransactionWrapper(statementLine);
			wrapper.PreliminaryStatementPrintDate = new ZDateTime(2007, 9, 19);
			wrapper.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			wrapper.PeriodicStatementMonth = MonthList.Codes._09;

			var builder = new StatementUpdateMessageBuilder(wrapper);
			var message = builder.PopulateMessage();
			AssertEquals(branch.PK, message.EM_GB);
			AssertMultilineASCIIEquals("Message generated",
@"B  1111XJ5SU                                  8888XJ5021   <<MSGNO PLACEHOLDER>>
H1111XJ5  12345   3091907  09                                                   
Y  1111XJ5SU",
				message.EM_FormattedMessageText);

			message = builder.PopulateMessage(1);
			Assert(@"No need to delay SU message.", message.EM_HeldUntilDate.IsEmpty);
		}

		[TestDate(2015, 07, 25)]
		public void TestACEStatementEndToEndTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "12345678";
			declaration.US_SchDEntry = "1111";
			declaration.US_EntryType = "01";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2015, 07, 26);

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var declarationWrapper = new StatementDeleteTransactionWrapper(declaration);
			declarationWrapper.PreliminaryStatementPrintDate = new ZDateTime(2015, 07, 26);
			declarationWrapper.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declarationWrapper.PeriodicStatementMonth = MonthList.Codes._09;

			var builder = new StatementUpdateMessageBuilder(declarationWrapper);
			var message = builder.PopulateMessage();

			AssertMultilineASCIIEquals("Message generated",
@"B  8888XJ5SU                                               <<MSGNO PLACEHOLDER>>
H1111XJ5  123456783072615  09                                                   
Y  8888XJ5SU",
				message.EM_FormattedMessageText);

			statementHeader.B2_ProcessPort = "1111";
			statementHeader.B2_EntryFilerCode = "XJ5";
			statementHeader.B2_ProcessDate = new ZDateTime(2015, 07, 20);
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statementLine.B3_EntryProcessPort = "1111";
			statementLine.B3_EntryNum = "12345678";

			var statementWrapper = new StatementDeleteTransactionWrapper(statementLine);
			statementWrapper.PreliminaryStatementPrintDate = new ZDateTime(2015, 07, 26);
			statementWrapper.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statementWrapper.PeriodicStatementMonth = MonthList.Codes._09;

			builder = new StatementUpdateMessageBuilder(statementWrapper);
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated",
@"B  1111XJ5SU                                               <<MSGNO PLACEHOLDER>>
H1111XJ5  123456783072615  09                                                   
Y  1111XJ5SU",
				message.EM_FormattedMessageText);

			var pfMessage = statementHeader.Messages.AddNew(typeof(MQEDIMessage));
			pfMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			pfMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;

			builder = new StatementUpdateMessageBuilder(statementWrapper);
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated",
@"B  1111XJ5SU                                               <<MSGNO PLACEHOLDER>>
H1111XJ5  123456783072615  09                                                   
Y  1111XJ5SU",
				message.EM_FormattedMessageText);
		}

		public void TestReconciliationStatementMessageWithAcceptedMessage()
		{
			DeclarationTestHelper.SetProcessingDistrictPortCode("ZXXZ");
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryMode = EntryModeList.Codes.RLF;

			var reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_EntryFilerCode = "XJ5";
			var reconEntry = reconDeclaration.ReconEntry;
			var entry = reconEntry.GetEntry();
			entry.EntryNumber = "12345678";

			var sntMsg = (MQEDIMessage)reconEntry.Messages.AddNew(typeof(MQEDIMessage));
			sntMsg.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			sntMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummary;
			sntMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			sntMsg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sntMsg.EM_Status = EDIMessage.Status.Sent;
			sntMsg.EM_MessageText =
@"B  1101SV9RE                                  ZXXZXJ5  1   <<MSGNO PLACEHOLDER>>
10ASV9  32212431 1101B00227358   X 58-123456789            891 NA1US            
11CHRISTINA RUSZCZAK  12159051100    CHRISTINA.RUSZCZAK@WISETECHGLOBAL.COM      
20SV9  73057174                                                                 
20SV9  73057125                                                                 
901                                                                             
Y  1101SV9RE";

			var rcvMsg = (MQEDIMessage)reconEntry.Messages.AddNew(typeof(MQEDIMessage));
			rcvMsg.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			rcvMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummaryResponse;
			rcvMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			rcvMsg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			rcvMsg.EM_Status = EDIMessage.Status.Received;
			rcvMsg.EM_MessageText =
@"B001101SV9RX                                  ZXXZXJ5  1   <<MSGNO PLACEHOLDER>>E0 RECONS 000001 REF ID: SV9  32212431 B00227358                                Y  1101SV9RX00001";

			var declarationWrapper = new StatementDeleteTransactionWrapper(reconDeclaration);
			declarationWrapper.PreliminaryStatementPrintDate = new ZDateTime(2017, 11, 13);
			declarationWrapper.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declarationWrapper.PeriodicStatementMonth = MonthList.Codes._09;

			var builder = new StatementUpdateMessageBuilder(declarationWrapper);
			MQEDIMessage message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated",
@"B      XJ5SU                                  ZXXZXJ5  1   <<MSGNO PLACEHOLDER>>
H    XJ5  123456783111317  09                                                   
Y      XJ5SU",
				message.EM_FormattedMessageText);
		}

		public void TestReconciliationStatementMessageWithNoAcceptedMessage()
		{
			DeclarationTestHelper.SetProcessingDistrictPortCode("ZXXZ");
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryMode = EntryModeList.Codes.RLF;

			var reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_EntryFilerCode = "XJ5";
			var entry = reconDeclaration.ReconEntry.GetEntry();
			entry.EntryNumber = "12345678";

			var declarationWrapper = new StatementDeleteTransactionWrapper(reconDeclaration);
			declarationWrapper.PreliminaryStatementPrintDate = new ZDateTime(2017, 11, 13);
			declarationWrapper.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declarationWrapper.PeriodicStatementMonth = MonthList.Codes._09;

			var builder = new StatementUpdateMessageBuilder(declarationWrapper);
			MQEDIMessage message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated",
@"B      XJ5SU                                               <<MSGNO PLACEHOLDER>>
H    XJ5  123456783111317  09                                                   
Y      XJ5SU",
				message.EM_FormattedMessageText);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_EntryMode = EntryModeList.Codes.Paired;
			var reconDeclaration2 = new ReconDeclaration(declaration2);
			reconDeclaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration2.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration2.US_EntryFilerCode = "XJ5";
			var entry2 = reconDeclaration2.ReconEntry.GetEntry();
			entry2.EntryNumber = "12345666";

			var declarationWrapper2 = new StatementDeleteTransactionWrapper(reconDeclaration2);
			declarationWrapper2.PreliminaryStatementPrintDate = new ZDateTime(2017, 11, 13);
			declarationWrapper2.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declarationWrapper2.PeriodicStatementMonth = MonthList.Codes._09;

			var builder2 = new StatementUpdateMessageBuilder(declarationWrapper2);
			var message2 = builder2.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated",
@"B      XJ5SU                                               <<MSGNO PLACEHOLDER>>
H    XJ5  123456663111317  09                                                   
Y      XJ5SU",
				message2.EM_FormattedMessageText);
		}

		[ExpectNoExceptions]
		public void TestConvertInvalidStatmentTypeTo0()
		{
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_EntryFilerCode = "XJ5";
			statementHeader.B2_ProcessDate = new ZDateTime(2007, 9, 19);

			StatementDeleteTransactionWrapper wrapper = new StatementDeleteTransactionWrapper(statementLine);
			wrapper.PreliminaryStatementPrintDate = new ZDateTime(2007, 9, 19);
			wrapper.PaymentType = "X";

			StatementUpdateMessageBuilder builder = new StatementUpdateMessageBuilder(wrapper);
			MQEDIMessage message = builder.PopulateMessage();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "10001127";
		}
	}
}
