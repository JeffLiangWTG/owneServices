using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CusPollingTransaction = Enterprise.Customs.Business.CusPollingTransaction;
using CusPollingTransactionStatuses = Enterprise.Core.Constants.Customs.CusPollingTransactionStatus.Codes;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;
using EntryHeaderStatus = Enterprise.Customs.Common.Shared.MessageStatusList.Codes;
using MessageTypes = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusPollingTransactionsHelperTest : TestCaseWithFactory
{
	[TestDate(year: 2024, month: 2, day: 12, hour: 8, minute: 30, second: 0)]
	public void TestCreateCusPollingTransaction() => CombineAssertions(() =>
	{
		var utcNow = ZDateTime.UtcNow;

		var passwordPK = ZGuid.NewZGuid();
		Factory.TryCreateNewCusPollingTransaction(passwordPK);
		var qr = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, passwordPK);
		var transaction = Factory.LoadTop1<CusPollingTransaction>(qr);
		AssertEquals("CPT_ApplicationCode", ApplicationCodes.PLCustoms, transaction.CPT_ApplicationCode);
		AssertEquals("CPT_Type", CusPollingTransactionTypes.PLC, transaction.CPT_Type);
		AssertEquals("CPT_Status", CusPollingTransactionStatuses.OPN, transaction.CPT_Status);
		AssertEquals("CPT_StatusReason", ZString.Empty, transaction.CPT_StatusReason);
		AssertEquals("CPT_StatusTimeUtc", utcNow.AddHours(-1).ToSmallDateTime(), transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", utcNow.ToSmallDateTime(), transaction.CPT_EarliestTimeOfNextAttemptUtc);
		AssertEquals("CPT_NumberOfAttempts", (ZByte)0, transaction.CPT_NumberOfAttempts);
		AssertEquals("CPT_ParentTableCode", GlbExternalPasswordSchema.Constants.Prefix, transaction.CPT_ParentTableCode);
		AssertEquals("CPT_ParentID", passwordPK, transaction.CPT_ParentID);
		AssertEquals("CPT_TransactionID", new ZString(passwordPK), transaction.CPT_TransactionID);
	});

	[TestDate(year: 2024, month: 2, day: 12, hour: 8, minute: 30, second: 0)]
	public void TestCreateCusPollingTransaction_AvoidDuplicates()
	{
		var utcNow = ZDateTime.UtcNow;

		var passwordPK = ZGuid.NewZGuid();
		Factory.TryCreateNewCusPollingTransaction(passwordPK);
		Factory.TryCreateNewCusPollingTransaction(passwordPK);

		var qr = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, passwordPK);
		var transactions = Factory.Load<CusPollingTransaction>(qr);
		AssertEquals(1, transactions.Length);
	}

	[TestDate(year: 2024, month: 2, day: 12, hour: 8, minute: 30, second: 0)]
	public void TestCreateBacklogs() => CombineAssertions(() =>
	{
		var utcNow = ZDateTime.UtcNow;

		var passwordPK = ZGuid.NewZGuid();
		Factory.TryCreateNewCusPollingTransaction(passwordPK);
		var qr = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, passwordPK);
		var cptTransaction = Factory.LoadTop1<CusPollingTransaction>(qr);
		cptTransaction.CPT_StatusTimeUtc = utcNow.AddHours(-2);
		var expectedCPT_StatusTimeUtc = cptTransaction.CPT_StatusTimeUtc;
		var expectedCPT_EarliestTimeOfNextAttemptUtc = expectedCPT_StatusTimeUtc.AddHours(1);
		var blgTransactions = cptTransaction.CreateBacklogs(Factory);

		AssertEquals("Created backlogs count", 1, blgTransactions.Count);
		var blgTransaction = blgTransactions.Single();

		AssertEquals("BLG CPT_ApplicationCode", ApplicationCodes.PLCustoms, blgTransaction.CPT_ApplicationCode);
		AssertEquals("BLG CPT_Type", CusPollingTransactionTypes.BLG, blgTransaction.CPT_Type);
		AssertEquals("BLG CPT_Status", CusPollingTransactionStatuses.OPN, blgTransaction.CPT_Status);
		AssertEquals("BLG CPT_StatusReason", ZString.Empty, blgTransaction.CPT_StatusReason);
		AssertEquals("BLG CPT_StatusTimeUtc", expectedCPT_StatusTimeUtc, blgTransaction.CPT_StatusTimeUtc);
		AssertEquals("BLG CPT_EarliestTimeOfNextAttemptUtc", expectedCPT_EarliestTimeOfNextAttemptUtc, blgTransaction.CPT_EarliestTimeOfNextAttemptUtc);
		AssertEquals("BLG CPT_NumberOfAttempts", (ZByte)0, blgTransaction.CPT_NumberOfAttempts);
		AssertEquals("BLG CPT_ParentTableCode", GlbExternalPasswordSchema.Constants.Prefix, blgTransaction.CPT_ParentTableCode);
		AssertEquals("BLG CPT_ParentID", passwordPK, blgTransaction.CPT_ParentID);
		AssertEquals("BLG CPT_TransactionID", new ZString(passwordPK), blgTransaction.CPT_TransactionID);

		AssertEquals("PLC CPT_StatusTimeUtc", utcNow.AddHours(-1), cptTransaction.CPT_StatusTimeUtc);
		AssertEquals("PLC CPT_EarliestTimeOfNextAttemptUtc", utcNow, cptTransaction.CPT_EarliestTimeOfNextAttemptUtc);
	});

	public void TestProcessBacklogFault_NumberOfAttempts() => CombineAssertions(() =>
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG);
		cusPollingTransaction.CPT_NumberOfAttempts = 0;
		var serviceLog = new LoggingInformation();
		var transmitMessage = Factory.New<EDIMessage>();
		cusPollingTransaction.ProcessBacklogFault(serviceLog, transmitMessage, faultLogMessage: "Test message");
		AssertEquals("CPT_NumberOfAttempts", (ZByte)1, cusPollingTransaction.CPT_NumberOfAttempts);

		cusPollingTransaction.AssertHasLogMessagePart("CusPollingTransaction log", Events.ErrorReport, "Test message");
		serviceLog.AssertHasLogMessagePart("Service has fault log message", LogType.Error, "Test message");
	});

	public void TestProcessBacklogFault_Closed() => CombineAssertions(() =>
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG);
		cusPollingTransaction.CPT_NumberOfAttempts = (ZByte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value;
		var serviceLog = new LoggingInformation();
		var transmitMessage = Factory.New<EDIMessage>();
		cusPollingTransaction.ProcessBacklogFault(serviceLog, transmitMessage, faultLogMessage: "Test message");
		AssertEquals("CPT_Status", CusPollingTransactionStatuses.ERR, cusPollingTransaction.CPT_Status);

		cusPollingTransaction.AssertHasLogMessagePart("CusPollingTransaction log", Events.ErrorReport, "Test message");
		serviceLog.AssertHasLogMessagePart("Service has fault log message", LogType.Error, "Test message");
	});

	public void TestProcessInboundFaultMessage_WrongStatuses()
	{
		var dataDo = new ZDateTime(2023, 07, 28, 23, 59, 59, 999);
		var transitMessageText =
"<GetDocumentsRequest xmlns=\"http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0\"> " +
	"<pobrany>0</pobrany> " +
	"<dataOd>2023-07-22T00:00:00</dataOd> " +
	$"<dataDo>{dataDo.ToString("yyyy-MM-ddTHH:mm:ss.fff")}</dataDo> " +
	"<allEmployees>true</allEmployees> " +
"</GetDocumentsRequest>";

		var logger = new LoggingInformation();
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff);
		var (fault, transmitMessage, faultMessage) = CreateFaultMessageTestData(transitMessageText);

		var transactionsCount = Factory.GetTableHitCount(CusPollingTransaction.Schema.TableName);
		var messagesCount = Factory.GetTableHitCount(EDIMessage.Schema.TableName);

		CombineAssertions(() =>
		{
			AssertWrongStatus(CusPollingTransactionStatuses.CLS);
			AssertWrongStatus(CusPollingTransactionStatuses.ERR);
		});

		void AssertWrongStatus(ZString wrongStatus)
		{
			cusPollingTransaction.CPT_NumberOfAttempts = 0;
			cusPollingTransaction.CPT_Status = wrongStatus;
			cusPollingTransaction.ProcessInboundFaultMessage(fault, logger, faultMessage, transmitMessage);

			AssertEquals($"{wrongStatus}: New transactions not created", transactionsCount, Factory.GetTableHitCount(CusPollingTransaction.Schema.TableName));
			AssertEquals($"{wrongStatus}: New messages not created", messagesCount, Factory.GetTableHitCount(EDIMessage.Schema.TableName));
			AssertEquals($"{wrongStatus}: Transaction status not changed", wrongStatus, cusPollingTransaction.CPT_Status);
			AssertEquals($"{wrongStatus}: Number of attempts not changed", (ZByte)0, cusPollingTransaction.CPT_NumberOfAttempts);

			var expectedLogMessage = $"Get a {fault.FaultName} by CusPollingTransaction status is [{wrongStatus}].";
			transmitMessage.AssertHasLogMessagePart($"{wrongStatus}: transmit message has error", Events.ErrorReport, expectedLogMessage);
			faultMessage.AssertHasLogMessagePart($"{wrongStatus}: fault message has error", Events.ErrorReport, expectedLogMessage);
			logger.AssertHasLogMessagePart($"{wrongStatus}: service message has error", LogType.Error, expectedLogMessage);
		}
	}

	[TestDate(year: 2024, month: 2, day: 12, hour: 8, minute: 30, second: 0)]
	public void TestProcessInboundFaultMessagePLC()
	{
		var dataDo = new ZDateTime(2023, 07, 28, 23, 59, 59, 999);
		var transmitMessageText =
"<GetDocumentsRequest xmlns=\"http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0\"> " +
	"<pobrany>0</pobrany> " +
	"<dataOd>2023-07-22T00:00:00</dataOd> " +
	$"<dataDo>{dataDo.ToString("yyyy-MM-ddTHH:mm:ss.fff")}</dataDo> " +
	"<allEmployees>true</allEmployees> " +
"</GetDocumentsRequest>";

		var logger = new LoggingInformation();
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var originalTransaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.PLC);
		var utcNow = ZDateTime.UtcNow.TrimSeconds();
		var originalStatusTime = utcNow.AddHours(-2);
		var originalEarliestTimeOfNextAttemptUtc = utcNow.AddHours(-1);
		originalTransaction.CPT_NumberOfAttempts = 0;
		originalTransaction.CPT_StatusTimeUtc = originalStatusTime;
		originalTransaction.CPT_EarliestTimeOfNextAttemptUtc = originalEarliestTimeOfNextAttemptUtc;
		var (fault, transmitMessage, faultMessage) = CreateFaultMessageTestData(transmitMessageText);

		originalTransaction.ProcessInboundFaultMessage(fault, logger, faultMessage, transmitMessage);
		CombineAssertions(() =>
		{
			var blgTransaction = AssertCreatedBlgTransaction();
			var blgMessage = AssertCreatedMessage(blgTransaction, transmitMessage);

			transmitMessage.AssertHasLogMessagePart("Backlog generated", Events.ErrorReport, "Backlog generated");

			AssertTransactionReopened(originalTransaction);

			var transactionReopenedMessage = $"CusPollingTransaction {originalTransaction.PK} reopened";
			faultMessage.AssertHasLogMessagePart("Fault message: transaction reopened", Events.MessagePendingProcessing, transactionReopenedMessage);
		});

		CusPollingTransaction AssertCreatedBlgTransaction()
		{
			var query = new ZQuery { OrderBy = CusPollingTransactionSchema.CPT_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(CusPollingTransactionSchema.CPT_Type, CusPollingTransactionTypes.BLG);
			var createBlgTransaction = Factory.Load<CusPollingTransaction>(query).Single();
			AssertNotNull("Blg transaction", createBlgTransaction);

			AssertEquals("New BLG Transaction: CPT_ApplicationCode", ApplicationCodes.PLCustoms, createBlgTransaction.CPT_ApplicationCode);
			AssertEquals("New BLG Transaction: CPT_Type", CusPollingTransactionTypes.BLG, createBlgTransaction.CPT_Type);
			AssertEquals("New BLG Transaction: CPT_Status", CusPollingTransactionStatuses.AWR, createBlgTransaction.CPT_Status);
			AssertEquals("New BLG Transaction: CPT_StatusReason", ZString.Empty, createBlgTransaction.CPT_StatusReason);
			AssertEquals("New BLG Transaction: CPT_StatusTimeUtc", originalStatusTime, createBlgTransaction.CPT_StatusTimeUtc);
			AssertEquals("New BLG Transaction: CPT_EarliestTimeOfNextAttemptUtc", originalEarliestTimeOfNextAttemptUtc, createBlgTransaction.CPT_EarliestTimeOfNextAttemptUtc);
			AssertEquals("New BLG Transaction: CPT_NumberOfAttempts", (ZByte)1, createBlgTransaction.CPT_NumberOfAttempts);
			AssertEquals("New BLG Transaction: CPT_ParentTableCode", GlbExternalPasswordSchema.Constants.Prefix, createBlgTransaction.CPT_ParentTableCode);
			AssertEquals("New BLG Transaction: CPT_ParentID", originalTransaction.CPT_ParentID, createBlgTransaction.CPT_ParentID);
			AssertEquals("New BLG Transaction: CPT_TransactionID", originalTransaction.CPT_TransactionID, createBlgTransaction.CPT_TransactionID);

			return createBlgTransaction;
		}

		EDIMessage AssertCreatedMessage(CusPollingTransaction blgTransaction, EDIMessage originalMessage)
		{
			var query = new ZQuery { OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending }
				.AddToFilter(EDIMessageSchema.EM_LinkTable, CusPollingTransaction.Schema.TableName)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, blgTransaction.PK);
			var createdEdiMessage = Factory.Load<EDIMessage>(query).Single();
			AssertNotNull("Blg transaction message", createdEdiMessage);

			AssertEquals("New BLG Message: EM_Status", EDIMessageStatusList.Codes.Queued, createdEdiMessage.EM_Status);
			AssertEquals("New BLG Message: EM_RetryCount", blgTransaction.CPT_NumberOfAttempts, createdEdiMessage.EM_RetryCount);
			AssertEquals("New BLG Message: EM_HeldUntilDate", ZDateTime.UtcNow.Add(GetHeldUntilDateBuffer()), createdEdiMessage.EM_HeldUntilDate);
			AssertEquals("New BLG Message: EM_IsActive", ZBool.True, createdEdiMessage.EM_IsActive);
			AssertEquals("New BLG Message: EM_IsTestMessage", originalMessage.EM_IsTestMessage, createdEdiMessage.EM_IsTestMessage);
			AssertEquals("New BLG Message: EM_GP", originalMessage.EM_GP, createdEdiMessage.EM_GP);
			AssertEquals("New BLG Message: EM_LinkedObject", blgTransaction, createdEdiMessage.EM_LinkedObject);
			AssertEquals("New BLG Message: EM_MessageText", originalMessage.EM_MessageText, createdEdiMessage.EM_MessageText);
			AssertEquals("New BLG Message: EM_SendWithMessageErrors", originalMessage.EM_SendWithMessageErrors, createdEdiMessage.EM_SendWithMessageErrors);
			AssertEquals("New BLG Message: EM_ApplicationCode", originalMessage.EM_ApplicationCode, createdEdiMessage.EM_ApplicationCode);
			AssertEquals("New BLG Message: EM_ApplicationReference", originalMessage.EM_ApplicationReference, createdEdiMessage.EM_ApplicationReference);
			AssertEquals("New BLG Message: EM_MessageType", originalMessage.EM_MessageType, createdEdiMessage.EM_MessageType);
			AssertEquals("New BLG Message: EM_MessageSubType", originalMessage.EM_MessageSubType, createdEdiMessage.EM_MessageSubType);
			AssertEquals("New BLG Message: EM_MessageOwner", originalMessage.EM_MessageOwner, createdEdiMessage.EM_MessageOwner);
			AssertEquals("New BLG Message: EM_ReceiveTransmit", originalMessage.EM_ReceiveTransmit, createdEdiMessage.EM_ReceiveTransmit);
			AssertEquals("New BLG Message: EM_GB", originalMessage.EM_GB, createdEdiMessage.EM_GB);
			AssertEquals("New BLG Message: EM_GE", originalMessage.EM_GE, createdEdiMessage.EM_GE);

			return createdEdiMessage;

			TimeSpan GetHeldUntilDateBuffer() => (byte)blgTransaction.CPT_NumberOfAttempts switch
			{
				1 => Constants.CusPollingTransaction.TimeBufferFor1stRetry,
				2 => Constants.CusPollingTransaction.TimeBufferFor2ndRetry,
				3 or 4 => Constants.CusPollingTransaction.TimeBufferFor3rdOr4thRetry,
				_ => Constants.CusPollingTransaction.TimeBufferForOtherRetry
			};
		}

		void AssertTransactionReopened(CusPollingTransaction transaction)
		{
			AssertEquals("Original transaction status", CusPollingTransactionStatuses.OPN, transaction.CPT_Status);
			AssertEquals("Original transaction status time", originalEarliestTimeOfNextAttemptUtc, transaction.CPT_StatusTimeUtc);
			var expectedEarliestTimeOfNextAttempt = utcNow - originalStatusTime < Constants.CusPollingTransaction.TimeBufferFor1stRetry
				? originalStatusTime + Constants.CusPollingTransaction.TimeBufferFor1stRetry
				: utcNow;
			AssertEquals("Original transaction EarliestTimeOfNextAttempt", expectedEarliestTimeOfNextAttempt, transaction.CPT_EarliestTimeOfNextAttemptUtc);
		}
	}

	public void TestProcessInboundFaultMessageBLG_OPN()
	{
		var dataDo = new ZDateTime(2023, 07, 28, 23, 59, 59, 999);
		var transitMessageText =
"<GetDocumentsRequest xmlns=\"http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0\"> " +
	"<pobrany>0</pobrany> " +
	"<dataOd>2023-07-22T00:00:00</dataOd> " +
	$"<dataDo>{dataDo.ToString("yyyy-MM-ddTHH:mm:ss.fff")}</dataDo> " +
	"<allEmployees>true</allEmployees> " +
"</GetDocumentsRequest>";

		var logger = new LoggingInformation();
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var transaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG, status: CusPollingTransactionStatuses.OPN);
		transaction.CPT_NumberOfAttempts = 1;
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow.AddHours(-1);
		transaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddHours(-1);

		var (fault, transmitMessage, faultMessage) = CreateFaultMessageTestData(transitMessageText);
		transmitMessage.EM_LinkedObject = transaction;

		CombineAssertions(() =>
		{
			for (byte numberOfAttempts = 1; numberOfAttempts < PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value; numberOfAttempts++)
			{
				AssertProcessInboundFaultMessage(numberOfAttempts);

				AssertEquals("numberOfAttempts + 1", numberOfAttempts + 1, transaction.CPT_NumberOfAttempts);
				AssertEquals("transaction status not updated", CusPollingTransactionStatuses.OPN, transaction.CPT_Status);
			}
			AssertProcessInboundFaultMessage((byte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value);

			AssertEquals("numberOfAttempts not updated", (ZByte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value, transaction.CPT_NumberOfAttempts);
			AssertEquals("transaction closed", CusPollingTransactionStatuses.ERR, transaction.CPT_Status);
		});

		void AssertProcessInboundFaultMessage(byte numberOfAttempts)
		{
			transaction.ProcessInboundFaultMessage(fault, logger, faultMessage, transmitMessage);

			var faultLogMessage = $"{fault.FaultName} in message {faultMessage.EM_MessageNum}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}]";
			transaction.AssertHasLogMessagePart($"{numberOfAttempts}: Transaction has fault log message", Events.ErrorReport, faultLogMessage);
			logger.AssertHasLogMessagePart($"{numberOfAttempts}: Service has fault log message", LogType.Error, faultLogMessage);
			transmitMessage.AssertHasLogMessagePart($"{numberOfAttempts}: Transmit message has fault log message", Events.ErrorReport, faultLogMessage);
		}
	}

	[TestDate(year: 2024, month: 2, day: 12, hour: 8, minute: 30, second: 0)]
	public void TestProcessInboundFaultMessageBLG_AWR()
	{
		var dataDo = new ZDateTime(2023, 07, 28, 23, 59, 59, 999);
		var transitMessageText =
"<GetDocumentsRequest xmlns=\"http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0\"> " +
	"<pobrany>0</pobrany> " +
	"<dataOd>2023-07-22T00:00:00</dataOd> " +
	$"<dataDo>{dataDo.ToString("yyyy-MM-ddTHH:mm:ss.fff")}</dataDo> " +
	"<allEmployees>true</allEmployees> " +
"</GetDocumentsRequest>";

		var logger = new LoggingInformation();
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var transaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG, status: CusPollingTransactionStatuses.AWR);
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow.AddHours(-1);
		transaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddHours(-1);

		var (fault, transmitMessage, faultMessage) = CreateFaultMessageTestData(transitMessageText);
		transmitMessage.EM_LinkedObject = transaction;

		var getTransactionMessagesQuery = new ZQuery { OrderBy = EDIMessageSchema.EM_RetryCount.Name + OrderByClause.Descending }
			.AddToFilter(EDIMessageSchema.EM_LinkTable, CusPollingTransaction.Schema.TableName)
			.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, transaction.PK);
		AssertEquals("Check that transaction has only one message", transmitMessage.PK, Factory.Load<EDIMessage>(getTransactionMessagesQuery).Single().PK);
		var knownTransactionMessagePKs = new HashSet<ZGuid> { transmitMessage.PK };
		EDIMessage newTransactionMessage;

		CombineAssertions(() =>
		{
			for (byte numberOfAttempts = 0; numberOfAttempts < PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value; numberOfAttempts++)
			{
				AssertProcessInboundFaultMessage(numberOfAttempts);

				AssertEquals($"{numberOfAttempts}: numberOfAttempts", numberOfAttempts + 1, transaction.CPT_NumberOfAttempts);
				AssertEquals($"{numberOfAttempts}: transaction status not updated", CusPollingTransactionStatuses.AWR, transaction.CPT_Status);

				newTransactionMessage = Factory.LoadTop1<EDIMessage>(getTransactionMessagesQuery);
				Assert($"{numberOfAttempts}: New message is unknown", !knownTransactionMessagePKs.Contains(newTransactionMessage.PK));
				knownTransactionMessagePKs.Add(newTransactionMessage.PK);

				AssertNewMessage(newTransactionMessage);
				faultMessage.AssertHasLogMessagePart($"{numberOfAttempts}: fault message has 'created duplicated message' log", Events.MessagePendingProcessing, $"Created duplicate message for {transaction.PK} attempt:{transaction.CPT_NumberOfAttempts}");
			}
			AssertProcessInboundFaultMessage((byte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value);

			AssertEquals("numberOfAttempts not updated", (ZByte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value, transaction.CPT_NumberOfAttempts);
			AssertEquals("transaction closed", CusPollingTransactionStatuses.ERR, transaction.CPT_Status);

			newTransactionMessage = Factory.LoadTop1<EDIMessage>(getTransactionMessagesQuery);
			Assert("No new messages", knownTransactionMessagePKs.Contains(newTransactionMessage.PK));
		});

		void AssertProcessInboundFaultMessage(byte numberOfAttempts)
		{
			transaction.ProcessInboundFaultMessage(fault, logger, faultMessage, transmitMessage);

			var faultLogMessage = $"{fault.FaultName} in message {faultMessage.EM_MessageNum}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}]";
			transaction.AssertHasLogMessagePart($"{numberOfAttempts}: Transaction has fault log message", Events.ErrorReport, faultLogMessage);
			logger.AssertHasLogMessagePart($"{numberOfAttempts}: Service has fault log message", LogType.Error, faultLogMessage);
			transmitMessage.AssertHasLogMessagePart($"{numberOfAttempts}: Transmit message has fault log message", Events.ErrorReport, faultLogMessage);
		}

		void AssertNewMessage(EDIMessage newMessage)
		{
			var numberOfAttempts = transaction.CPT_NumberOfAttempts;
			AssertEquals($"{numberOfAttempts}: new message EM_Status", EDIMessageStatusList.Codes.Queued, newMessage.EM_Status);
			AssertEquals($"{numberOfAttempts}: new message EM_RetryCount", numberOfAttempts, newMessage.EM_RetryCount);
			AssertEquals($"{numberOfAttempts}: new message EM_HeldUntilDate", ZDateTime.UtcNow.Add(GetHeldUntilDateBuffer()), newMessage.EM_HeldUntilDate);
			AssertEquals($"{numberOfAttempts}: new message EM_IsActive", ZBool.True, newMessage.EM_IsActive);
			AssertEquals($"{numberOfAttempts}: new message EM_IsTestMessage", transmitMessage.EM_IsTestMessage, newMessage.EM_IsTestMessage);
			AssertEquals($"{numberOfAttempts}: new message EM_GP", transmitMessage.EM_GP, newMessage.EM_GP);
			AssertEquals($"{numberOfAttempts}: new message EM_LinkedObject", transaction, newMessage.EM_LinkedObject);
			AssertEquals($"{numberOfAttempts}: new message EM_MessageText", transmitMessage.EM_MessageText, newMessage.EM_MessageText);
			AssertEquals($"{numberOfAttempts}: new message EM_SendWithMessageErrors", transmitMessage.EM_SendWithMessageErrors, newMessage.EM_SendWithMessageErrors);
			AssertEquals($"{numberOfAttempts}: new message EM_ApplicationCode", transmitMessage.EM_ApplicationCode, newMessage.EM_ApplicationCode);
			AssertEquals($"{numberOfAttempts}: new message EM_ApplicationReference", transmitMessage.EM_ApplicationReference, newMessage.EM_ApplicationReference);
			AssertEquals($"{numberOfAttempts}: new message EM_MessageType", transmitMessage.EM_MessageType, newMessage.EM_MessageType);
			AssertEquals($"{numberOfAttempts}: new message EM_MessageSubType", transmitMessage.EM_MessageSubType, newMessage.EM_MessageSubType);
			AssertEquals($"{numberOfAttempts}: new message EM_MessageOwner", transmitMessage.EM_MessageOwner, newMessage.EM_MessageOwner);
			AssertEquals($"{numberOfAttempts}: new message EM_ReceiveTransmit", transmitMessage.EM_ReceiveTransmit, newMessage.EM_ReceiveTransmit);
			AssertEquals($"{numberOfAttempts}: new message EM_GB", transmitMessage.EM_GB, newMessage.EM_GB);
			AssertEquals($"{numberOfAttempts}: new message EM_GE", transmitMessage.EM_GE, newMessage.EM_GE);

			TimeSpan GetHeldUntilDateBuffer() => (byte)numberOfAttempts switch
			{
				1 => Constants.CusPollingTransaction.TimeBufferFor1stRetry,
				2 => Constants.CusPollingTransaction.TimeBufferFor2ndRetry,
				3 or 4 => Constants.CusPollingTransaction.TimeBufferFor3rdOr4thRetry,
				_ => Constants.CusPollingTransaction.TimeBufferForOtherRetry
			};
		}
	}

	(ICommonFault fault, EDIMessage transmitMessage, EDIMessage faultMessage) CreateFaultMessageTestData(ZString transmitMessageText)
	{
		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		cusEntryHeader.CH_Status = EntryHeaderStatus.Sent;
		var transmitMessage = FaultMessageTestHelper.CreateTransmitMessage<EDIMessage>(Factory,
			ApplicationCodes.PLCustoms,
			messageType: MessageTypes.Export,
			messageSubType: Constants.EDIMessageSubType.Declaration,
			transmitMessageLinkedObject: cusEntryHeader);
		transmitMessage.EM_MessageText = transmitMessageText;

		var faultText = typeof(InboundInterchangeProcessorLegacyTest).Assembly.GetTestFile("Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles.BusinessErrorFault.xml");
		var fault = GetFaultFromString(faultText);
		var faultMessage = FaultMessageTestHelper.CreateFaultMessage(transmitMessage, faultText);
		return (fault, transmitMessage, faultMessage);
	}

	public static ICommonFault GetFaultFromString(ZString xml)
	{
		using var xmlReader = XmlHelper.CreateReaderAndGotoRootNode(new StringReader(xml));
		if (SoapHelper.IsSoap(xmlReader))
		{
			xmlReader.MoveToSoapBody();
		}
		var dataProviderFactory = new DataProviderFactory(RecognizableMessages.All);
		return dataProviderFactory.NewOrNull<ICommonFault>(xmlReader);
	}
}
