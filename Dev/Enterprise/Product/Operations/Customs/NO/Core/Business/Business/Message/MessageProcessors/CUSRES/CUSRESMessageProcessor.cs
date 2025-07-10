using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NO.Business;

sealed class CUSRESMessageProcessor : IMessageProcessor
{
	void IMessageProcessor.ProcessMessage(EDIMessage message, LoggingInformation logger)
	{
		using var traceLogger = new TraceLogger($"processing EDIMessage with PK: [{message.PK}]", logger);
		message.EM_Status = ProcessMessageCore(message, logger);
	}

	static string ProcessMessageCore(EDIMessage message, LoggingInformation logger)
	{
		if ((CUSRESHeaderProvider.New(message) is not ICUSRESHeader cusresHeader))
		{
			return LogMessageAndReturnErrorStatus(logger, LogType.Error, $"EDIMessage with PK: [{message.PK}], Not a CUSRES message.");
		}

		if (message.EM_LinkedObject is not CusEntryHeader cusEntryHeader)
		{
			return LogMessageAndReturnErrorStatus(logger, LogType.Error, $"EDIMessage with PK: [{message.PK}], Linked object is not CusEntryHeader.");
		}

		var messageType = cusresHeader.MessageType;

		if (messageType == DocumentMessageNameCodedList.GoodsDeclarationForExportation
			|| messageType == DocumentMessageNameCodedList.GoodsDeclarationForImportation)
		{
			return ProcessImportExportDeclaration(message, cusEntryHeader, cusresHeader, logger);
		}

		if (messageType == DocumentMessageNameCodedList.CustomsDeliveryNote)
		{
			return ProcessCustomsDeliveryNote(message, cusEntryHeader, cusresHeader, logger);
		}

		return ProcessUnknownMessageType(message, logger, messageType);
	}

	static string ProcessUnknownMessageType(EDIMessage message, LoggingInformation logger, string messageType)
	{
		logger.Log(LogType.Error, FormattableString.Invariant($"System was not able to identify the message type: [{messageType}] for EDIMessage with PK: [{message.PK}]."));
		return EDIMessageStatusList.Codes.Error;
	}

	static string ProcessImportExportDeclaration(EDIMessage message, CusEntryHeader cusEntryHeader, ICUSRESHeader cusresHeader, LoggingInformation logger)
	{
		var messageType = cusresHeader.MessageType;
		var messageFunction = cusresHeader.MessageFunction;
		var messageTextCodes = cusresHeader.MessageTextCodes;
		var requestedControlAction = cusresHeader.RequestedControlAction;

		if (messageFunction == MessageFunctionCodedList.Response)
		{
			ProcessImportExportDeclarationForResponseMessageFunction(message, messageType, messageFunction, messageTextCodes, requestedControlAction, cusEntryHeader, logger);
		}
		else if (messageFunction == MessageFunctionCodedList.NotAccepted)
		{
			ProcessImportExportDeclarationForNotAcceptedMessageFunction(cusEntryHeader, cusresHeader);
		}
		else if (messageFunction == MessageFunctionTypeZZ)
		{
			UpdateEntryAndPhaseStatusAndLogEvent(cusEntryHeader, UniversalReferenceConstants.CusEntryStatus.IUR, CustomsEntryPhaseStatusList.Codes.Finalized);
		}
		else
		{
			return LogMessageAndReturnErrorStatus(logger, LogType.Warning, $"EDIMessage with PK: [{message.PK}], Unknown MessageFunction: [{messageFunction}] for MessageType: [{messageType}].");
		}

		return EDIMessageStatusList.Codes.ProcessedOK;
	}

	static string ProcessCustomsDeliveryNote(EDIMessage message, CusEntryHeader cusEntryHeader, ICUSRESHeader cusresHeader, LoggingInformation logger)
	{
		ZString entryStatus;
		var messageFunction = cusresHeader.MessageFunction;

		if (messageFunction == MessageFunctionCodedList.Response)
		{
			entryStatus = UniversalReferenceConstants.CusEntryStatus.UAR;
		}
		else if (messageFunction == MessageFunctionCodedList.Approval)
		{
			entryStatus = UniversalReferenceConstants.CusEntryStatus.TKR;
		}
		else
		{
			return LogMessageAndReturnErrorStatus(logger, LogType.Warning, $"EDIMessage with PK: [{message.PK}], Unknown MessageFunction: [{messageFunction}] for MessageType: [{cusresHeader.MessageType}].");
		}

		UpdateEntryAndPhaseStatusAndLogEvent(cusEntryHeader, entryStatus, CustomsEntryPhaseStatusList.Codes.Finalized);
		cusEntryHeader.SetEntryReleaseNumber(entryReleaseNumber: cusresHeader.ReleaseNumber, issueDate: cusresHeader.ReleaseDate, expiryDate: cusresHeader.LimitDate);
		cusEntryHeader.SetEntryReleaseDate(cusresHeader.ReleaseDate);

		return EDIMessageStatusList.Codes.ProcessedOK;
	}

	static void ProcessImportExportDeclarationForResponseMessageFunction(
		EDIMessage message,
		string messageType,
		string messageFunction,
		ImmutableHashSet<ZString> messageTextCodes,
		string requestedControlAction,
		CusEntryHeader cusEntryHeader,
		LoggingInformation logger
	)
	{
		var entryStatus = default(ZString?);
		var phaseStatus = default(ZString?);
		var foundDesiredMessageTextCode = false;

		foreach (var messageTextCode in messageTextCodes.Where(i => i != EDIMessageConstants.MessageTextCodes.Code_999))
		{
			if (MessageCodeToEntryStatus.TryGetValue(messageTextCode, out var status))
			{
				entryStatus = status;
				if (status == UniversalReferenceConstants.CusEntryStatus.IUR)
				{
					phaseStatus = CustomsEntryPhaseStatusList.Codes.Finalized;
				}
				else if (status == UniversalReferenceConstants.CusEntryStatus.MEC)
				{
					phaseStatus = string.Empty;
				}

				foundDesiredMessageTextCode = true;
				break;
			}
			else if (ReminderCodes.Contains(messageTextCode))
			{
				phaseStatus = CustomsEntryPhaseStatusList.Codes.Reminder;
				foundDesiredMessageTextCode = true;
				break;
			}
		}

		if (!foundDesiredMessageTextCode)
		{
			entryStatus = GetEntryStatusByControlAction(message, messageType, messageFunction, requestedControlAction, logger);
			if (entryStatus is { } status &&
				status.ToString() is UniversalReferenceConstants.CusEntryStatus.MEG or UniversalReferenceConstants.CusEntryStatus.MED)
			{
				phaseStatus = string.Empty;
			}
		}
		UpdateEntryAndPhaseStatusAndLogEvent(cusEntryHeader, entryStatus, phaseStatus);
	}

	static void ProcessImportExportDeclarationForNotAcceptedMessageFunction(CusEntryHeader cusEntryHeader, ICUSRESHeader cusresHeader)
	{
		cusEntryHeader.CH_Status = UniversalReferenceConstants.CusEntryStatus.Rejected;
		if (cusEntryHeader.CH_EntryStatus.IsEmpty &&
			cusEntryHeader.CH_BGMReference == cusresHeader.DeclarationID)
		{
			cusEntryHeader.CH_BGMReference = ZString.Empty;
			DeleteDateForDuty(cusEntryHeader, cusresHeader);
		}
	}

	static void DeleteDateForDuty(CusEntryHeader cusEntryHeader, ICUSRESHeader cusresHeader)
	{
		var creationDate = cusresHeader.CreationDate.EndOfDay();
		if (cusEntryHeader.EntryInstruction is { } entryInstruction
			&& entryInstruction.CEI_DateForDuty <= creationDate)
		{
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
		}
	}

	static string GetEntryStatusByControlAction(EDIMessage message, string messageType, string messageFunction, string requestedControlAction, LoggingInformation logger)
	{
		if (requestedControlAction == ProcessingIndicatorCodedList.GoodsRequiredForExamination)
		{
			return UniversalReferenceConstants.CusEntryStatus.MEG;
		}

		if (requestedControlAction == ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced)
		{
			return UniversalReferenceConstants.CusEntryStatus.MED;
		}

		logger.Log(LogType.Error, FormattableString.Invariant($"EDIMessage with PK: [{message.PK}], Unknown MessageText Code for MessageType: {messageType}, Message Function: {messageFunction}."));
		return UniversalReferenceConstants.CusEntryStatus.MEC;
	}

	static void UpdateEntryAndPhaseStatusAndLogEvent(CusEntryHeader cusEntryHeader, ZString? entryStatus, ZString? phaseStatus)
	{
		cusEntryHeader.CH_EntryStatus = entryStatus ?? cusEntryHeader.CH_EntryStatus;
		cusEntryHeader.CH_PhaseStatus = phaseStatus ?? cusEntryHeader.CH_PhaseStatus;

		cusEntryHeader.Logs.AddNew(Events.CustomsEntryStatus, cusEntryHeader.CH_EntryStatus, ZDateTimeOffset.Now);
	}

	static string LogMessageAndReturnErrorStatus(LoggingInformation logger, LogType logType, string logMessage)
	{
		logger.Log(logType, FormattableString.Invariant($"{logMessage}"));
		return EDIMessageStatusList.Codes.Error;
	}

	const string MessageFunctionTypeZZ = "ZZ";

	static ImmutableDictionary<string, string> MessageCodeToEntryStatus => new Dictionary<string, string>
	{
		{ EDIMessageConstants.MessageTextCodes.Code_972, UniversalReferenceConstants.CusEntryStatus.MEM },
		{ EDIMessageConstants.MessageTextCodes.Code_950, UniversalReferenceConstants.CusEntryStatus.MEC },
		{ EDIMessageConstants.MessageTextCodes.Code_279, UniversalReferenceConstants.CusEntryStatus.IUR },
		{ EDIMessageConstants.MessageTextCodes.Code_357, UniversalReferenceConstants.CusEntryStatus.IUR },
		{ EDIMessageConstants.MessageTextCodes.Code_735, UniversalReferenceConstants.CusEntryStatus.IUR },
		{ EDIMessageConstants.MessageTextCodes.Code_736, UniversalReferenceConstants.CusEntryStatus.IUR },
		{ EDIMessageConstants.MessageTextCodes.Code_956, UniversalReferenceConstants.CusEntryStatus.IUR },
		{ EDIMessageConstants.MessageTextCodes.Code_973, UniversalReferenceConstants.CusEntryStatus.IUR }
	}.ToImmutableDictionary();

	static ImmutableHashSet<string> ReminderCodes => new HashSet<string>
	{
		EDIMessageConstants.MessageTextCodes.Code_958,
		EDIMessageConstants.MessageTextCodes.Code_980,
		EDIMessageConstants.MessageTextCodes.Code_981,
		EDIMessageConstants.MessageTextCodes.Code_982,
		EDIMessageConstants.MessageTextCodes.Code_983
	}.ToImmutableHashSet();
}
