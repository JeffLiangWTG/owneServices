using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CusPollingTransactionStatuses = Enterprise.Core.Constants.Customs.CusPollingTransactionStatus.Codes;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(FaultMessageProcessor))]
sealed class FaultMessageProcessorTestCPT : FaultMessageProcessorTestBase<FaultMessageProcessor, EDIMessage>
{
	[TestDate(2025, 1, 6, 13, 43, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestCusPollingTransactionFault()
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		CombineAssertions(() => FaultsForTest.ForEach(AssertCusPollingTransactionFault));
		return;

		void AssertCusPollingTransactionFault(FaultMessageTestHelper.FaultTestInfo faultTestInfo)
		{
			var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff, status: CusPollingTransactionStatuses.AWR);
			cusPollingTransaction.CPT_TransactionID = ZGuid.NewZGuid().ToString();
			var oldStatusTime = ZDateTime.UtcNow.AddMinutes(30);
			var oldEarliestStartTime = oldStatusTime.AddHours(1);
			cusPollingTransaction.CPT_StatusTimeUtc = oldStatusTime;
			cusPollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = oldEarliestStartTime;

			var (processedOk, transmitMessage, faultMessage) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustoms,
				messageType: Constants.EdiMessageMessageType.CusPollingTransaction,
				messageSubType: Constants.EDIMessageSubType.CusPollingTransaction,
				transmitMessageLinkedObject: cusPollingTransaction);
			if (!processedOk)
			{
				return;
			}

			AssertEquals("Validating CusPollingTransaction status", CusPollingTransactionStatuses.OPN, cusPollingTransaction.CPT_Status);

			var reopeningTransactionLogMessage = $"{faultTestInfo.ExpectedFaultName} in message EM_MessageNum. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}]";
			cusPollingTransaction.AssertHasLogMessagePart($"{faultTestInfo}: Fault message has 'got fault' log message", Events.ErrorReport, reopeningTransactionLogMessage);
			transmitMessage.AssertHasLogMessagePart($"{faultTestInfo}: Transmit message has 'got fault' log message", Events.ErrorReport, reopeningTransactionLogMessage);

			AssertCusPollingClonedAsBacklogWithNewEdiMessage(faultTestInfo, cusPollingTransaction, oldStatusTime, oldEarliestStartTime, expectedMessageText: "TEST");

			cusPollingTransaction.Delete();
		}
	}

	public void TestCusPollingTransactionFaultWithReopen()
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		CombineAssertions(() => FaultsForTest.ForEach(AssertCusPollingTransactionFaultWithReopen));
		return;

		void AssertCusPollingTransactionFaultWithReopen(FaultMessageTestHelper.FaultTestInfo faultTestInfo)
		{
			var oldStatusTime = new ZDateTime(2024, 01, 01, 11, 49, 59);
			var oldEarliestStartTime = oldStatusTime.AddHours(2);
			var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff);
			cusPollingTransaction.CPT_TransactionID = ZGuid.NewZGuid().ToString();
			cusPollingTransaction.CPT_StatusTimeUtc = oldStatusTime;
			cusPollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = oldEarliestStartTime;

			var (processedOk, transmitMessage, faultMessage) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustoms,
				messageType: Constants.EdiMessageMessageType.CusPollingTransaction,
				messageSubType: Constants.EDIMessageSubType.CusPollingTransaction,
				transmitMessageLinkedObject: cusPollingTransaction);
			if (!processedOk)
			{
				return;
			}

			AssertEquals($"{faultTestInfo}: Validating CusPollingTransaction status", CusPollingTransactionStatuses.OPN, cusPollingTransaction.CPT_Status);
			AssertNotEquals($"{faultTestInfo}: CusPollingTransaction status time should be updated", oldStatusTime, cusPollingTransaction.CPT_StatusTimeUtc);
			AssertEquals($"{faultTestInfo}: CusPollingTransaction New status time", oldEarliestStartTime, cusPollingTransaction.CPT_StatusTimeUtc);
			AssertNotEquals($"{faultTestInfo}: CusPollingTransaction CPT_EarliestTimeOfNextAttemptUtc should be updated", oldEarliestStartTime, cusPollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);

			faultMessage.AssertHasLogMessagePart($"{faultTestInfo}: Fault message has 'transaction reopening' log message", Events.MessagePendingProcessing, $"CusPollingTransaction {cusPollingTransaction.PK} reopened");
			faultMessage.AssertHasLogMessagePart($"{faultTestInfo}: Fault message has 'transaction reopening' log message", Events.MessagePendingProcessing, $"Created duplicate message for {cusPollingTransaction.PK} attempt:{cusPollingTransaction.CPT_NumberOfAttempts}");

			AssertCusPollingClonedAsBacklogWithNewEdiMessage(faultTestInfo, cusPollingTransaction, oldStatusTime, oldEarliestStartTime, "TEST");

			cusPollingTransaction.Delete();
		}
	}

	public void TestBacklogCusPollingTransactionProcessing_Count()
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		CombineAssertions(() => FaultsForTest.ForEach(AssertBacklogCusPollingTransactionProcessing));
		return;

		void AssertBacklogCusPollingTransactionProcessing(FaultMessageTestHelper.FaultTestInfo faultTestInfo)
		{
			var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG);
			cusPollingTransaction.CPT_TransactionID = ZGuid.NewZGuid().ToString();
			cusPollingTransaction.CPT_NumberOfAttempts = 0;

			var (processedOk, transmitMessage, faultMessage) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustoms,
				messageType: Constants.EdiMessageMessageType.CusPollingTransaction,
				messageSubType: Constants.EDIMessageSubType.CusPollingTransaction,
				transmitMessageLinkedObject: cusPollingTransaction);
			if (!processedOk)
			{
				return;
			}

			AssertEquals("Validating CusPollingTransaction NumberOfAttempts", (ZByte)1, cusPollingTransaction.CPT_NumberOfAttempts);

			var expectedFaultLogMessage = $"{faultTestInfo.ExpectedFaultName} in message EM_MessageNum. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}]";
			cusPollingTransaction.AssertHasLogMessagePart($"{faultTestInfo}: CusPollingTransaction has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			transmitMessage.AssertHasLogMessagePart($"{faultTestInfo}: Transmit message has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			serviceLogger.AssertHasLogMessagePart($"{faultTestInfo}: Service has fault log message", LogType.Error, expectedFaultLogMessage);

			const string createdDuplicateMessageLogMessage = "Created duplicate message";
			faultMessage.AssertHasNoLogMessagePart($"{faultTestInfo}: Fault message has no 'Created duplicate message' log message", Events.MessagePendingProcessing, createdDuplicateMessageLogMessage);

			cusPollingTransaction.Delete();
		}
	}

	public void TestBacklogCusPollingTransactionProcessing_Count_AWR()
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		CombineAssertions(() => FaultsForTest.ForEach(AssertBacklogCusPollingTransactionProcessing));
		return;

		void AssertBacklogCusPollingTransactionProcessing(FaultMessageTestHelper.FaultTestInfo faultTestInfo)
		{
			var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG);
			cusPollingTransaction.CPT_TransactionID = ZGuid.NewZGuid().ToString();
			cusPollingTransaction.CPT_Status = CusPollingTransactionStatuses.AWR;
			cusPollingTransaction.CPT_NumberOfAttempts = 0;

			var (processedOk, transmitMessage, faultMessage) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustoms,
				messageType: Constants.EdiMessageMessageType.CusPollingTransaction,
				messageSubType: Constants.EDIMessageSubType.CusPollingTransaction,
				transmitMessageLinkedObject: cusPollingTransaction);
			if (!processedOk)
			{
				return;
			}

			var expectedFaultLogMessage = $"{faultTestInfo.ExpectedFaultName} in message EM_MessageNum. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}]";
			cusPollingTransaction.AssertHasLogMessagePart($"{faultTestInfo}: CusPollingTransaction has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			transmitMessage.AssertHasLogMessagePart($"{faultTestInfo}: Transmit message has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			serviceLogger.AssertHasLogMessagePart($"{faultTestInfo}: Service has fault log message", LogType.Error, expectedFaultLogMessage);

			AssertEquals($"{faultTestInfo}: Validating CusPollingTransaction NumberOfAttempts", (ZByte)1, cusPollingTransaction.CPT_NumberOfAttempts);

			const string createdDuplicateMessageLogMessage = "Created duplicate message";
			faultMessage.AssertHasLogMessagePart($"{faultTestInfo}: Fault message has no 'Created duplicate message' log message", Events.MessagePendingProcessing, createdDuplicateMessageLogMessage);

			const string newMessageWasPopulatedLogMessage = "was populated as duplicate";
			cusPollingTransaction.AssertHasLogMessagePart($"{faultTestInfo}: CusPollingTransaction has no 'New message was populated' log message", Events.MessagePendingProcessing, newMessageWasPopulatedLogMessage);

			AssertDuplicatedEdiMessage(faultTestInfo, transmitMessage, cusPollingTransaction);

			cusPollingTransaction.Delete();
		}
	}

	public void TestBacklogCusPollingTransactionProcessing_Closed()
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		CombineAssertions(() => FaultsForTest.ForEach(AssertBacklogCusPollingTransactionProcessing));
		return;

		void AssertBacklogCusPollingTransactionProcessing(FaultMessageTestHelper.FaultTestInfo faultTestInfo)
		{
			var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG);
			cusPollingTransaction.CPT_TransactionID = ZGuid.NewZGuid().ToString();
			cusPollingTransaction.CPT_Status = CusPollingTransactionStatuses.OPN;
			cusPollingTransaction.CPT_NumberOfAttempts = (ZByte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value;

			var (processedOk, transmitMessage, faultMessage) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustoms,
				messageType: Constants.EdiMessageMessageType.CusPollingTransaction,
				messageSubType: Constants.EDIMessageSubType.CusPollingTransaction,
				transmitMessageLinkedObject: cusPollingTransaction);
			if (!processedOk)
			{
				return;
			}

			AssertEquals("Validating CusPollingTransaction status", CusPollingTransactionStatuses.ERR, cusPollingTransaction.CPT_Status);

			var expectedFaultLogMessage = $"{faultTestInfo.ExpectedFaultName} in message EM_MessageNum. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}]";
			cusPollingTransaction.AssertHasLogMessagePart($"{faultTestInfo}: CusPollingTransaction has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			transmitMessage.AssertHasLogMessagePart($"{faultTestInfo}: Transmit message has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			serviceLogger.AssertHasLogMessagePart($"{faultTestInfo}: Service has fault log message", LogType.Error, expectedFaultLogMessage);

			const string createdDuplicateMessageLogMessage = "Created duplicate message";
			faultMessage.AssertHasNoLogMessagePart($"{faultTestInfo}: Fault message has no 'Created duplicate message' log message", Events.MessagePendingProcessing, createdDuplicateMessageLogMessage);

			const string newMessageWasPopulatedLogMessage = "was populated as duplicate";
			cusPollingTransaction.AssertHasNoLogMessagePart($"{faultTestInfo}: CusPollingTransaction has no 'New message was populated' log message", Events.MessagePendingProcessing, newMessageWasPopulatedLogMessage);
		}
	}

	public void TestBacklogCusPollingTransactionProcessing_Closed_AWR()
	{
		var staff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		CombineAssertions(() => FaultsForTest.ForEach(AssertBacklogCusPollingTransactionProcessing));
		return;

		void AssertBacklogCusPollingTransactionProcessing(FaultMessageTestHelper.FaultTestInfo faultTestInfo)
		{
			var cusPollingTransaction = Factory.CreatePollingTransactionForStaff(staff, type: CusPollingTransactionTypes.BLG);
			cusPollingTransaction.CPT_TransactionID = ZGuid.NewZGuid().ToString();
			cusPollingTransaction.CPT_Status = CusPollingTransactionStatuses.AWR;
			cusPollingTransaction.CPT_NumberOfAttempts = (ZByte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value;

			var (processedOk, transmitMessage, faultMessage) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustoms,
				messageType: Constants.EdiMessageMessageType.CusPollingTransaction,
				messageSubType: Constants.EDIMessageSubType.CusPollingTransaction,
				transmitMessageLinkedObject: cusPollingTransaction);
			if (!processedOk)
			{
				return;
			}

			AssertEquals("Validating CusPollingTransaction status", CusPollingTransactionStatuses.ERR, cusPollingTransaction.CPT_Status);

			var expectedFaultLogMessage = $"{faultTestInfo.ExpectedFaultName} in message EM_MessageNum. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}]";
			cusPollingTransaction.AssertHasLogMessagePart($"{faultTestInfo}: CusPollingTransaction has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			transmitMessage.AssertHasLogMessagePart($"{faultTestInfo}: Transmit message has fault log message", Events.ErrorReport, expectedFaultLogMessage);
			serviceLogger.AssertHasLogMessagePart($"{faultTestInfo}: Service has fault log message", LogType.Error, expectedFaultLogMessage);

			const string createdDuplicateMessageLogMessage = "Created duplicate message";
			faultMessage.AssertHasNoLogMessagePart($"{faultTestInfo}: Fault message has no 'Created duplicate message' log message", Events.MessagePendingProcessing, createdDuplicateMessageLogMessage);

			const string newMessageWasPopulatedLogMessage = "was populated as duplicate";
			cusPollingTransaction.AssertHasNoLogMessagePart($"{faultTestInfo}: CusPollingTransaction has no 'New message was populated' log message", Events.MessagePendingProcessing, newMessageWasPopulatedLogMessage);
		}
	}

	void AssertCusPollingClonedAsBacklogWithNewEdiMessage(FaultMessageTestHelper.FaultTestInfo faultTestInfo, CusPollingTransaction transaction, ZDateTime oldStatusTime, ZDateTime oldEarliestTimeOfNextAttempt, string expectedMessageText)
	{
		var query = new ZQuery()
			.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, transaction.CPT_ApplicationCode)
			.AddToFilter(CusPollingTransactionSchema.CPT_Type, CusPollingTransactionTypes.BLG)
			.AddToFilter(CusPollingTransactionSchema.CPT_Status, CusPollingTransactionStatuses.AWR)
			.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, transaction.CPT_TransactionID);
		var clonedBlg = Factory.Load<CusPollingTransaction>(query).Single();

		AssertEquals($"{faultTestInfo}: cloned CPT_StatusReason", string.Empty, clonedBlg.CPT_StatusReason);
		AssertEquals($"{faultTestInfo}: cloned CPT_StatusTimeUtc", oldStatusTime, clonedBlg.CPT_StatusTimeUtc);
		AssertEquals($"{faultTestInfo}: cloned CPT_EarliestTimeOfNextAttemptUtc", oldEarliestTimeOfNextAttempt, clonedBlg.CPT_EarliestTimeOfNextAttemptUtc);
		AssertEquals($"{faultTestInfo}: cloned CPT_NumberOfAttempts", (ZByte)1, clonedBlg.CPT_NumberOfAttempts);
		AssertEquals($"{faultTestInfo}: cloned CPT_ParentTableCode", GlbExternalPasswordSchema.Constants.Prefix, clonedBlg.CPT_ParentTableCode);
		AssertEquals($"{faultTestInfo}: cloned CPT_ParentID", transaction.CPT_ParentID, clonedBlg.CPT_ParentID);
		AssertEquals($"{faultTestInfo}: cloned CPT_TransactionID", transaction.CPT_TransactionID, clonedBlg.CPT_TransactionID);

		query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, clonedBlg.PK);
		var ediMessage = Factory.Load<EDIMessage>(query).Single();
		AssertEquals($"{faultTestInfo}: New EdiMessage should be open", ediMessage.EM_Status, EDIMessageStatusList.Codes.Queued);
		AssertEquals($"{faultTestInfo}: EM_MessageText", expectedMessageText, ediMessage.EM_MessageText);
	}

	void AssertDuplicatedEdiMessage(FaultMessageTestHelper.FaultTestInfo faultTestInfo, EDIMessage message, CusPollingTransaction transaction)
	{
		var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, transaction.PK);
		var newEdiMessage = Factory.Load<EDIMessage>(query).Single(x => x.EM_Status == EDIMessageStatusList.Codes.Queued);

		AssertNotEquals($"{faultTestInfo}: EM_PK", newEdiMessage.PK, message.PK);
		AssertEquals($"{faultTestInfo}: EM_Status", EDIMessageStatusList.Codes.Queued, newEdiMessage.EM_Status);
		AssertEquals($"{faultTestInfo}: EM_RetryCount", newEdiMessage.EM_RetryCount, transaction.CPT_NumberOfAttempts);
		AssertEquals($"{faultTestInfo}: EM_IsActive", ZBool.True, newEdiMessage.EM_IsActive);
		AssertEquals($"{faultTestInfo}: EM_IsTestMessage", ZBool.False, newEdiMessage.EM_IsTestMessage);
		AssertEquals($"{faultTestInfo}: EM_GP", message.EM_GP, newEdiMessage.EM_GP);
		AssertEquals($"{faultTestInfo}: EM_MessageText", message.EM_MessageText, newEdiMessage.EM_MessageText);
		AssertEquals($"{faultTestInfo}: EM_SendWithMessageErrors", message.EM_SendWithMessageErrors, newEdiMessage.EM_SendWithMessageErrors);
		AssertEquals($"{faultTestInfo}: EM_ApplicationCode", message.EM_ApplicationCode, newEdiMessage.EM_ApplicationCode);
		AssertEquals($"{faultTestInfo}: EM_ApplicationReference", message.EM_ApplicationReference, newEdiMessage.EM_ApplicationReference);
		AssertEquals($"{faultTestInfo}: EM_MessageType", message.EM_MessageType, newEdiMessage.EM_MessageType);
		AssertEquals($"{faultTestInfo}: EM_MessageSubType", message.EM_MessageSubType, newEdiMessage.EM_MessageSubType);
		AssertEquals($"{faultTestInfo}: EM_MessageOwner", message.EM_MessageOwner, newEdiMessage.EM_MessageOwner);
		AssertEquals($"{faultTestInfo}: EM_ReceiveTransmit", message.EM_ReceiveTransmit, newEdiMessage.EM_ReceiveTransmit);
		AssertEquals($"{faultTestInfo}: EM_GB", message.EM_GB, newEdiMessage.EM_GB);
		AssertEquals($"{faultTestInfo}: EM_GE", message.EM_GE, newEdiMessage.EM_GE);
		AssertEquals($"{faultTestInfo}: EM_EI", ZGuid.Empty, newEdiMessage.EM_EI);
		AssertNotEquals($"{faultTestInfo}: EM_EI should not be copied", newEdiMessage.EM_EI, message.EM_EI);
	}

	protected override FaultMessageProcessor MessageProcessor => messageProcessor ??= new (serviceLogger);
	new FaultMessageProcessor messageProcessor;
}
