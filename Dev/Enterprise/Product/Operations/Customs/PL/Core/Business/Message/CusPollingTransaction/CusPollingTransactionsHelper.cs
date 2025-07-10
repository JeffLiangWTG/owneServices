using System;
using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CusPollingTransactionStatus = Enterprise.Core.Constants.Customs.CusPollingTransactionStatus.Codes;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.Business;

public static class CusPollingTransactionsHelper
{
	internal static bool TryCreateNewCusPollingTransaction(this BusinessObjectFactory factory, ZGuid passwordPK)
	{
		var utcTimeNow = ZDateTime.UtcNow.TrimSeconds();

		if (factory.Exists(typeof(CusPollingTransaction), new ZQuery().AddToFilter(CusPollingTransactionSchema.CPT_ParentID, passwordPK)))
		{
			return false;
		}

		var transaction = factory.New<CusPollingTransaction>();
		transaction.CPT_ApplicationCode = ApplicationCodes.PLCustoms;
		transaction.CPT_Type = CusPollingTransactionTypes.PLC;
		transaction.CPT_Status = CusPollingTransactionStatus.OPN;
		transaction.CPT_StatusReason = ZString.Empty;
		transaction.CPT_StatusTimeUtc = utcTimeNow.AddHours(-1);
		transaction.CPT_EarliestTimeOfNextAttemptUtc = utcTimeNow;
		transaction.CPT_NumberOfAttempts = 0;
		transaction.CPT_ParentTableCode = GlbExternalPasswordSchema.Constants.Prefix;
		transaction.CPT_ParentID = passwordPK;
		transaction.CPT_TransactionID = new ZString(passwordPK);

		return true;
	}

	internal static void DeleteCusPollingTransactions(this BusinessObjectFactory factory, ZGuid passwordPK)
		=> factory.Load<CusPollingTransaction>(GetQueryCusPollingTransactionsByPasswordPK(passwordPK))
			.DeleteAll();

	internal static ZQuery GetQueryCusPollingTransactionsByPasswordPK(ZGuid passwordPK)
		=> new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, ApplicationCodes.PLCustoms)
		.AddToFilter(CusPollingTransactionSchema.CPT_ParentID, passwordPK);

	internal static IReadOnlyCollection<CusPollingTransaction> CreateBacklogs(this CusPollingTransaction plcTransaction, BusinessObjectFactory requestFactory)
	{
		var result = new List<CusPollingTransaction>();
		var transactionHourPeriodStart = plcTransaction.CPT_StatusTimeUtc;
		var utcTime = ZDateTime.UtcNow.TrimSeconds();
		var oneHourTimeSpan = TimeSpan.FromHours(1);

		while (utcTime - transactionHourPeriodStart > oneHourTimeSpan)
		{
			var blgTransaction = plcTransaction.CreateBacklogWithStatus(CusPollingTransactionStatus.OPN, transactionHourPeriodStart, transactionHourPeriodStart.AddHours(1));
			result.Add(blgTransaction);
			transactionHourPeriodStart += oneHourTimeSpan;
		}

		plcTransaction.CPT_StatusTimeUtc = transactionHourPeriodStart;
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = utcTime - transactionHourPeriodStart < Constants.CusPollingTransaction.TimeBufferFor1stRetry
			? plcTransaction.CPT_StatusTimeUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry
			: utcTime;

		return result;
	}

	internal static CusPollingTransaction ProcessPlcFaultAndCreateBacklog(this CusPollingTransaction plcTransaction, ISimpleLogger serviceLog, EnterpriseEDIMessage transmitMessage, string faultLogMessage)
	{
		if (transmitMessage is null)
		{
			plcTransaction.CPT_Status = CusPollingTransactionStatus.ERR;
			return null;
		}

		plcTransaction.Logs.AddNew(Events.ErrorReport, faultLogMessage);
		serviceLog.Log(LogType.Error, faultLogMessage + $". Backlog transaction PK = {plcTransaction.PK}");
		transmitMessage.Logs.AddNew(Events.ErrorReport, faultLogMessage);
		var result = plcTransaction.CreateFaultBacklogWithMessage(transmitMessage);

		plcTransaction.ReopenPlcTransaction();
		return result;
	}

	internal static void ReopenPlcTransaction(this CusPollingTransaction plcTransaction)
	{
		var utcTime = ZDateTime.UtcNow.TrimSeconds();
		plcTransaction.CPT_StatusTimeUtc = plcTransaction.CPT_EarliestTimeOfNextAttemptUtc;
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = utcTime - plcTransaction.CPT_StatusTimeUtc < Constants.CusPollingTransaction.TimeBufferFor1stRetry
			? plcTransaction.CPT_StatusTimeUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry
			: utcTime;
		plcTransaction.CPT_Status = CusPollingTransactionStatus.OPN;
		plcTransaction.Logs.AddNew(Events.PeriodReopened, $"Reopened with CPT_StatusTimeUtc = {plcTransaction.CPT_StatusTimeUtc}, CPT_EarliestTimeOfNextAttemptUtc = {plcTransaction.CPT_EarliestTimeOfNextAttemptUtc}");
	}

	internal static bool ProcessBacklogFault(this CusPollingTransaction blgTransaction, ISimpleLogger serviceLog, EnterpriseEDIMessage transmitMessage, string faultLogMessage)
	{
		blgTransaction.Logs.AddNew(Events.ErrorReport, faultLogMessage);
		serviceLog.Log(LogType.Error, faultLogMessage + $". Backlog transaction PK = {blgTransaction.PK}");
		transmitMessage?.Logs.AddNew(Events.ErrorReport, faultLogMessage);

		if (blgTransaction.CPT_NumberOfAttempts < PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value && transmitMessage is not null)
		{
			blgTransaction.CPT_NumberOfAttempts += 1;
			var newMessage = blgTransaction.CreateMessageCopy(transmitMessage);
			blgTransaction.Logs.AddNew(Events.MessagePendingProcessing, $"New message {newMessage.EM_MessageNum} was populated as duplicate of {transmitMessage.EM_MessageNum}");
			return true;
		}

		blgTransaction.CPT_Status = CusPollingTransactionStatus.ERR;
		return false;
	}

	internal static void ProcessInboundFaultMessage(
		this CusPollingTransaction transaction,
		ICommonFault fault,
		ISimpleLogger serviceLogger,
		BaseEDIMessage faultMessage,
		EnterpriseEDIMessage transmitMessage)
	{
		if (transaction.CPT_Status == CusPollingTransactionStatus.CLS ||
			transaction.CPT_Status == CusPollingTransactionStatus.ERR)
		{
			LogHelper.Log(Events.ErrorReport, $"Get a {fault.FaultName} by CusPollingTransaction status is [{transaction.CPT_Status}].", serviceLogger, incomingMessage: faultMessage, transmitMessage: transmitMessage);
			return;
		}

		var faultLogMessage = $"{fault.FaultName} in message {faultMessage.EM_MessageNum}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}]";
		switch (transaction.CPT_Type)
		{
			case CusPollingTransactionTypes.PLC:
				var createdBacklog = transaction.ProcessPlcFaultAndCreateBacklog(serviceLogger, transmitMessage, faultLogMessage);
				if (createdBacklog is not null)
				{
					faultMessage.Logs.AddNew(Events.MessagePendingProcessing, $"CusPollingTransaction {transaction.PK} reopened");
					faultMessage.Logs.AddNew(Events.MessagePendingProcessing, $"Created duplicate message for {transaction.PK} attempt:{transaction.CPT_NumberOfAttempts}");
				}
				break;

			case CusPollingTransactionTypes.BLG:
				transaction.ProcessBacklogFault(serviceLogger, transmitMessage, faultLogMessage);
				if (transaction.CPT_Status == CusPollingTransactionStatus.AWR)
				{
					faultMessage.Logs.AddNew(Events.MessagePendingProcessing, $"Created duplicate message for {transaction.PK} attempt:{transaction.CPT_NumberOfAttempts}");
				}
				break;

			default:
				LogHelper.Log(Events.ErrorReport, $"Unsupported CusPollingTransaction type: [{transaction.CPT_Type}].", serviceLog: serviceLogger, incomingMessage: faultMessage, transmitMessage: transmitMessage);
				return;
		}
	}

	static CusPollingTransaction CreateFaultBacklogWithMessage(this CusPollingTransaction originalTransaction, EnterpriseEDIMessage originalMessage)
	{
		var newBacklogTransaction = originalTransaction.CreateBacklogWithStatus(CusPollingTransactionStatus.AWR, originalTransaction.CPT_StatusTimeUtc, originalTransaction.CPT_EarliestTimeOfNextAttemptUtc);
		newBacklogTransaction.CPT_NumberOfAttempts = 1;
		newBacklogTransaction.CPT_StatusTimeUtc = originalTransaction.CPT_StatusTimeUtc;
		newBacklogTransaction.CPT_EarliestTimeOfNextAttemptUtc = originalTransaction.CPT_EarliestTimeOfNextAttemptUtc;
		LogHelper.Log(Events.ErrorReport, $"Backlog generated {newBacklogTransaction.PK}", transmitMessage: originalMessage);

		newBacklogTransaction.CreateMessageCopy(originalMessage);
		originalMessage.Logs.AddNew(Events.ErrorReport, $"Message copy generated for backlog {newBacklogTransaction.PK}");

		return newBacklogTransaction;
	}

	static CusPollingTransaction CreateBacklogWithStatus(this CusPollingTransaction originalTransaction, string backlogStatus, ZDateTime statusTime, ZDateTime earliestTimeOfNextAttempt)
	{
		var newTransaction = originalTransaction.Factory.New<CusPollingTransaction>();
		newTransaction.CPT_ApplicationCode = ApplicationCodes.PLCustoms;
		newTransaction.CPT_Type = CusPollingTransactionTypes.BLG;
		newTransaction.CPT_Status = backlogStatus;
		newTransaction.CPT_StatusReason = ZString.Empty;
		newTransaction.CPT_StatusTimeUtc = statusTime;
		newTransaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttempt;
		newTransaction.CPT_NumberOfAttempts = 0;
		newTransaction.CPT_ParentTableCode = GlbExternalPasswordSchema.Constants.Prefix;
		newTransaction.CPT_ParentID = originalTransaction.CPT_ParentID;
		newTransaction.CPT_TransactionID = originalTransaction.CPT_TransactionID;

		return newTransaction;
	}

	static BaseEDIMessage CreateMessageCopy(this CusPollingTransaction transaction, EnterpriseEDIMessage originalMessage)
	{
		var newEdiMessage = originalMessage.Factory.New<EDIMessage>();
		newEdiMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
		newEdiMessage.EM_RetryCount = transaction.CPT_NumberOfAttempts;
		newEdiMessage.EM_HeldUntilDate = ZDateTime.UtcNow.Add(GetHeldUntilDateBuffer()).TrimSeconds();
		newEdiMessage.EM_IsActive = ZBool.True;
		newEdiMessage.EM_IsTestMessage = originalMessage.EM_IsTestMessage;
		newEdiMessage.EM_GP = originalMessage.EM_GP;
		newEdiMessage.EM_LinkedObject = transaction;
		newEdiMessage.EM_MessageText = originalMessage.EM_MessageText;
		newEdiMessage.EM_SendWithMessageErrors = originalMessage.EM_SendWithMessageErrors;
		newEdiMessage.EM_ApplicationCode = originalMessage.EM_ApplicationCode;
		newEdiMessage.EM_ApplicationReference = originalMessage.EM_ApplicationReference;
		newEdiMessage.EM_MessageType = originalMessage.EM_MessageType;
		newEdiMessage.EM_MessageSubType = originalMessage.EM_MessageSubType;
		newEdiMessage.EM_MessageOwner = originalMessage.EM_MessageOwner;
		newEdiMessage.EM_ReceiveTransmit = originalMessage.EM_ReceiveTransmit;
		newEdiMessage.EM_GB = originalMessage.EM_GB;
		newEdiMessage.EM_GE = originalMessage.EM_GE;

		return newEdiMessage;

		TimeSpan GetHeldUntilDateBuffer() => (byte)transaction.CPT_NumberOfAttempts switch
		{
			1 => Constants.CusPollingTransaction.TimeBufferFor1stRetry,
			2 => Constants.CusPollingTransaction.TimeBufferFor2ndRetry,
			3 or 4 => Constants.CusPollingTransaction.TimeBufferFor3rdOr4thRetry,
			_ => Constants.CusPollingTransaction.TimeBufferForOtherRetry
		};
	}
}
