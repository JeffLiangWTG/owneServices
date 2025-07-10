using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

sealed class XLGInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
{
	IUniversalCustomsInterchangeUnpackerResult IUniversalCustomsInterchangeUnpacker.Unpack(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EDIMessage outgoingMessage,
		LoggingInformation logger)
	{
		return UnpackCore(interchange, outgoingMessage, logger);
	}

	static EDIInterchangeUnpackerResult UnpackCore(EDIInterchange interchange, EDIMessage outgoingMessage, LoggingInformation logger)
	{
		logger.Log(LogType.Information, FormattableString.Invariant($"The XLGInterchangeUnpacker [{interchange.PK}] Unpacking Started."));

		(var isValid, var universalEvent) = XTFailureInterchangeHandler.IsValidUniversalEvent(interchange.EI_BodyText);
		if (!isValid)
		{
			return new EDIInterchangeUnpackerResult((NoResString)"The xT interchange does not contain a valid Universal XML Event in the body text.");
		}

		var universalEventWrapper = new UniversalEventWrapper(universalEvent);
		var ediMessageType = GetEDIMessageType(universalEventWrapper);
		if (ediMessageType.IsNullOrEmpty())
		{
			return new EDIInterchangeUnpackerResult((NoResString)$"The xT interchange contains an invalid Universal EventType: [{universalEventWrapper.EventType}].");
		}

		var ediMessage = CreateEdiMessage(interchange, outgoingMessage, ediMessageType, universalEventWrapper.GetResponseMessage());
		logger.Log(LogType.Information, FormattableString.Invariant($"The XLGInterchangeUnpacker [{interchange.PK}] Unpacking Finished with EDIMessage: [{ediMessage.PK}]."));

		return new EDIInterchangeUnpackerResult([ediMessage]);
	}

	static EDIMessage CreateEdiMessage(EDIInterchange interchange, EDIMessage outgoingMessage, string ediMessageType, string messageText)
	{
		var message = interchange.ContainedMessages.AddNew();

		message.EM_ApplicationCode = interchange.EI_ApplicationCode;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = ediMessageType;
		message.EM_MessageText = messageText;
		message.EM_GB = interchange.EI_GB;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_EM_RequestMessage = outgoingMessage.PK;
		message.EM_LinkUniqueID = outgoingMessage.EM_LinkUniqueID;
		message.EM_LinkTable = outgoingMessage.EM_LinkTable;

		return message;
	}

	static string GetEDIMessageType(UniversalEventWrapper universalEventWrapper) =>
		universalEventWrapper switch
		{
			{ IsAcknowledgement: true } => xTMessageConstants.MessageTypes.Codes.Acknowledgement,
			{ IsRejection: true } => xTMessageConstants.MessageTypes.Codes.Error,
			_ => null
		};
}
