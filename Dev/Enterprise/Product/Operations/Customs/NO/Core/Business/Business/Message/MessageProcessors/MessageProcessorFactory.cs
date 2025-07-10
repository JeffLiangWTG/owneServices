using System;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

sealed class MessageProcessorFactory
{
	public static IMessageProcessor GetMessageProcessor(EDIMessage message, LoggingInformation logger)
	{
		var messageType = message.EM_MessageType.ToString();
		return messageType switch
		{
			EDIMessageConstants.MessageTypes.CUSRES => new CUSRESMessageProcessor(),
			_ => HandleDefault(message, logger),
		};
	}

	static IMessageProcessor HandleDefault(EDIMessage message, LoggingInformation logger)
	{
		logger.Log(LogType.Error, FormattableString.Invariant($"EDIMessage with PK: [{message.PK}] with MessageType: [{message.EM_MessageType}], Found no message processor."));
		return null;
	}
}
