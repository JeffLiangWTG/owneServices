using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Edifact.V902.Messages.CUSRES;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.NOCustoms, typeof(Enterprise.Customs.NO.Business.EDIFactMessageUnpacker))]

namespace Enterprise.Customs.NO.Business;

sealed class EDIFactMessageUnpacker : IUniversalCustomsInterchangeUnpacker
{
	IUniversalCustomsInterchangeUnpackerResult IUniversalCustomsInterchangeUnpacker.Unpack(EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EDIMessage outgoingMessage,
		LoggingInformation logger)
	{
		_ = Argument.NotNull(interchange, nameof(interchange));
		_ = Argument.NotNull(logger, nameof(logger));
		using var traceLogger = new TraceLogger($"Interchange [{interchange.PK}] Unpacking", logger);

		return UnpackCore(interchange, logger);
	}

	static EDIInterchangeUnpackerResult UnpackCore(EDIInterchange interchange, LoggingInformation logger)
	{
		var messageText = interchange.EI_BodyText;
		var messages = EdiMessageFactory.GetResponseMessages(messageText)
			.WhereNotNull()
			.Select(r => CreateEdiMessage(r.Message, r.MessageText, interchange, logger))
			.ToArray();

		logger.Log(LogType.Information, FormattableString.Invariant($"Interchange [{interchange.PK}] Unpacking Generated Message(s) Count: [{messages.Length}]"));

		return new (messages);
	}

	static EDIMessage CreateEdiMessage(CUSRESMessage responseMessage, string messageText, EDIInterchange interchange, LoggingInformation logger)
	{
		var factory = interchange.Factory;
		var message = interchange.ContainedMessages.AddNew();
		message.EM_MessageText = messageText;
		message.EM_ApplicationCode = interchange.EI_ApplicationCode;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = interchange.EI_InterchangeType;
		message.EM_GB = interchange.EI_GB;

		if (responseMessage is null)
		{
			message.EM_Status = EDIMessage.Status.Error;
			logger.LogError(FormattableString.Invariant($"Unable to process part of the message from an Interchange: [{interchange.PK}]. Message Id: [{message.PK}]"));
			return message;
		}

		ICUSRESHeader header = new CUSRESHeaderProvider(factory, responseMessage);
		var declarationId = header.DeclarationID;

		if (!TryGetLinkedCusEntryHeader(factory, declarationId, out var entryHeader))
		{
			message.EM_Status = EDIMessage.Status.Error;
			logger.LogError(FormattableString.Invariant($"Unable to find linked entry header for Declaration Id: [{declarationId}] from an Interchange: [{interchange.PK}]. Message Id: [{message.PK}]"));
			return message;
		}

		message.EM_GB = entryHeader.Branch.PK;
		message.EM_LinkUniqueID = entryHeader.PK;
		message.EM_LinkTable = entryHeader.TableName;
		message.EM_LinkedObject = entryHeader;
		message.EM_Status = EDIMessage.Status.Queued;
		return message;
	}

	static bool TryGetLinkedCusEntryHeader(BusinessObjectFactory factory, ZString declarationId, out CusEntryHeader cusEntryHeader)
	{
		var cusEntryHeaderLoader = new CusEntryHeader.Loader(factory);
		cusEntryHeader = cusEntryHeaderLoader.GetByBGMReferenceNumber(declarationId.Left(declarationId.Length - 2));
		return cusEntryHeader is not null;
	}
}
