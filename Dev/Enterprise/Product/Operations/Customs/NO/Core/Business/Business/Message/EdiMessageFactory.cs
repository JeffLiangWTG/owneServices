using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.V902.Messages.CUSRES;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.Business;

static class EdiMessageFactory
{
	public static SegmentGroup GetMessage(EDIMessage message)
	{
		_ = Argument.NotNull(message, nameof(message));
		return message.GetAutoEdifactMessageUsingNamedFactory(factory.Value, characterSet);
	}

	public static IEnumerable<(CUSRESMessage Message, string MessageText)> GetResponseMessages(string messageBody)
	{
		_ = Argument.NotNullOrEmpty(messageBody, nameof(messageBody));
		var segments = ResponseMessageTextSegmentsParser.ParseInterchangeBody(characterSet, messageBody);
		return segments.Select(message => (factory.Value.GetMessage(characterSet, message) as CUSRESMessage, message));
	}

	[ThreadSafe]
	static readonly NOCharacterSet characterSet = new();

	[ThreadSafe]
	static readonly Lazy<MessageFactory> factory = new(CreateMessageFactory);

	public static MessageFactory Factory => factory.Value;

	static MessageFactory CreateMessageFactory()
	{
		var messageFactory = new MessageFactory();
		messageFactory.AddRegisteredMessage(new(typeof(CUSRESMessage), "UN", "1", "902", "CUSRES"));
		return messageFactory;
	}
}
