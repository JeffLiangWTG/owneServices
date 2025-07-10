using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACEStatementUpdateProcessorTest : ABIProcessorTest<ACEStatementUpdateProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			DoSetup();

			transmittedMessage.EM_MessageText = TransmittedMessageText;

			responseMessage.EM_MessageText = string.Format(
			"B018888XJ5{0}                                               ~000771              " +
			"H18888XJ5  10001127                                                             " +
			"H2FXJ5          P05                                                             " +
			"H38888XJ5  01149253371220702600000030408                                        " +
			"Y  8888XJ5{0}00001                                                               ", ResponseMessageType);

			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statementHeader.B2_PrintDate = new ZDateTime(2007, 9, 19);
			Factory.Save();
			dec.Logs.GetAllLogs().Load();
			AssertNull(dec.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ResponseMessageType));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);
			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);

			AssertEquals("message is linked", dec.PK, responseMessageLoaded.EM_LinkUniqueID);

			var declarationLoaded = statementHeaderLoaded.StatementLines[0].Declaration;
			AssertEquals("StatementHeader.B2_Status is updated", StatementHeaderStatusList.Codes.Preliminary, statementHeaderLoaded.B2_Status);
			AssertEquals("StatementLine.B3_Status is updated", StatementLineStatusList.Codes.Active, statementHeaderLoaded.StatementLines[0].B3_Status);

			AssertNotEquals("It should NOT be updated to what is returned in the response when failed", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, declarationLoaded.US_PaymentType);
			AssertEquals("", declarationLoaded.US_PeriodicStatementMM);
			AssertNotEquals("It should NOT be updated to what is returned in the response when failed", new ZDateTime(2007, 9, 19), declarationLoaded.US_PreliminaryStatementPrintDate);

			AssertNotNull("An email with subject containing 'Statement Delete/Add' should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Statement Delete/Add"); })));
			dec.Logs.GetAllLogs().Load();
			AssertNotNull(dec.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ResponseMessageType));
		}

		public void TestACEStatementUpdateProcessingQ7Block()
		{
			DoSetup();

			transmittedMessage.EM_MessageText = TransmittedMessageText;

			responseMessage.EM_MessageText = string.Format(
			"B018888XJ5{0}                                               ~000771              " +
			"H2 XJ5  00025277P05   NarrativeTextStart01NarrativeTextStart02                  " +
			"H38888XJ5  00025277371220702600000030408                                        " +
			"Y  8888XJ5{0}00001                                                               ", ResponseMessageType);

			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statementHeader.B2_PrintDate = new ZDateTime(2017, 9, 19);
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory = new BusinessObjectFactory();
			var responseMessageLoaded = factory.Load<MQEDIMessage>(responseMessage.PK);
			var statementHeaderLoaded = factory.Load<CusStatementHeader>(statementHeader.PK);

			var declarationLoaded = statementHeaderLoaded.StatementLines[0].Declaration;
			AssertEquals("StatementHeader.B2_Status is updated", StatementHeaderStatusList.Codes.Preliminary, statementHeaderLoaded.B2_Status);

			EmailDef emailGenerated = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("User should be advised entry has been updated", true, ((ZString)emailGenerated.Body).Contains("NarrativeTextStart01NarrativeTextStart02", StringComparison.OrdinalIgnoreCase));
		}

		public void TestACEIsSTUMessageAndAcceptedFor96UFailure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "00025277";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "00025277";
			statementLine.B3_EntryProcessPort = "8888";
			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			AssertEquals("Declaration in statement line", declaration, statementLine.Declaration);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			transmittedMessage = mock.Object;
			transmittedMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_MessageText = string.Format(
"B01270498G{0}                                  520198G  1   WHBIYHIYH_6984       " +
"H2704XJ5 000252777030114  02                                                    " +
"Y  270498G{0}00001                                                               ", TransmittedMessageType);

			responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			responseMessage.EM_Status = "QUE";
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_MessageText = string.Format(
			"B01270498G{0}                                  520198G  1   WHBIYHIYH_6984       " +
			"H18888XJ5  01149253 7030114B00002545                                            " +
			"H2FXJ5                                                                          " +
			"H38888XJ5  000252772714014BVD00007264707                                         " +
			"Y  270498G{0}00002                                                               ", ResponseMessageType);

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();

			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);
			AssertEquals("STU failed. Should not be deleted", "PRE", statementHeaderLoaded.B2_Status);
			AssertEquals("StatementLine.B3_Status is updated", StatementLineStatusList.Codes.DeletionPending, statementHeaderLoaded.StatementLines[0].B3_Status);
		}

		public void TestACEBranchCodeRequiredRejectionIsRecognised()
		{
			var statement = Factory.New<CusStatementHeader>();

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "01149253";
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			transmittedMessage.EM_MessageText = string.Format(
"B018888XJ5{0}                                               ~000771              " +
"H8888XJ5 011492532080112                                                        " +
"Y  8888XJ5{0}00001                                                               ", TransmittedMessageType);

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageText = string.Format(
			"B012704836{0}                                  5501836  1   ~000771              " +
			"H18888XJ5  01149253 2080112000114925                                            " +
			"H2FXJ5                                                                          " +
			"H38888XJ5  01149253371220702600000030408                                         " +
			"Y  2704836{0}00002                                                               ", ResponseMessageType);

			Factory.Save();

			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			statementLine.Reload();
			AssertEquals(StatementLineStatusList.Codes.DeletionPending, statementLine.B3_Status);
		}

		public void TestACEProcessForSuccessfulResponse()
		{
			DoSetup();

			AssertEquals("Pre-condition - Declaration Preliminary Statement Print Date", new ZDateTime(2007, 9, 17), dec.US_PreliminaryStatementPrintDate);

			transmittedMessage.EM_MessageText = string.Format(
"B018888XJ5{0}                                               ~000771              " +
"H8888XJ5 100011273092007                                                        " +
"Y  8888XJ5{0}00001                                                               ", TransmittedMessageType);

			responseMessage.EM_MessageText = string.Format(
			"B018888XJ5{0}                                               ~000771              " +
			"H18888XJ5100011272GBDATA REPLACED AS REQUESTED              3092007             " +
			"H2CXJ5          P05                                                             " +
			"H38888XJ5  01149253371220702600000030408                                        " +
			"Y  8888XJ5{0}00001                                                               ", ResponseMessageType);

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);
			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);

			AssertEquals("message is linked", dec.PK, responseMessageLoaded.EM_LinkUniqueID);
			AssertEquals("StatementHeader.B2_Status is updated", StatementHeaderStatusList.Codes.Preliminary, statementHeaderLoaded.B2_Status);
			AssertEquals("StatementLine.B3_Status is updated", "PEN", statementHeaderLoaded.StatementLines[0].B3_Status);

			var declarationLoaded = statementHeaderLoaded.StatementLines[0].Declaration;

			AssertEquals("entry header should have been updated with the data", PaymentTypeList.Codes.IndividualBasis, declarationLoaded.US_PaymentType);
			AssertEquals("", declarationLoaded.US_PeriodicStatementMM);
			AssertEquals(new ZDateTime(2007, 9, 17), declarationLoaded.US_PreliminaryStatementPrintDate);

			//re-load declaration
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals("Declaration Preliminary Statement Print Date should be updated to response value", new ZDateTime(2007, 9, 17), declaration.US_PreliminaryStatementPrintDate);

			AssertEquals("", declarationLoaded.US_PeriodicStatementMM);
		}

		ZString TransmittedMessageType
		{
			get { return ACEApplicationIdentifierCodeList.Codes.StatementUpdate; }
		}

		ZString ResponseMessageType
		{
			get { return ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse; }
		}

		void DoSetup()
		{
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			dec.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 9, 17);
			dec.ImportEntryNumber = "10001127";

			entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "10001127";
			Factory.Save();

			statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "10001127";
			statementLine.B3_EntryProcessPort = "8888";
			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			AssertEquals("Declaration in statement line", entry.Declaration, statementLine.Declaration);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			transmittedMessage = mock.Object;
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_MessageText = TransmittedMessageText;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			dec.Messages.Add(transmittedMessage);

			responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageNum = "~000771";
			dec.Messages.Add(responseMessage);

			Factory.Save();
		}

		ZString TransmittedMessageText
		{
			get
			{
				return string.Format("B018888XJ5{0}                                               ~000771              H8888XJ5  100011276091907                                                       Y  8888XJ5{0}00001", TransmittedMessageType);
			}
		}

		JobDeclaration dec;
		CusEntryHeader entry;

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;

		MQEDIMessage transmittedMessage;
		MQEDIMessage responseMessage;
	}
}
