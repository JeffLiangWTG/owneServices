using System.Collections.Generic;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

sealed class UniversalEventUnpackingStrategy : IXmlInterchangeUnpackingStrategy
{
	public IReadOnlyCollection<XmlQualifiedName> SupportedXmlNodes => [UniversalMessaging.Event.RootNode];

	public EDIInterchangeUnpackerResult Unpack(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		XmlReader reader,
		ISimpleLogger logger)
	{
		if (outgoingMessage is null)
		{
			return new EDIInterchangeUnpackerResult(errorReason:
				(NoResString)"Interchange processing failed because related transmit message couldn't be located.");
		}

		if (!reader.TryMoveToNode(UniversalMessaging.Event.EventTypeNodeName, nodeNamespaceURI: null))
		{
			return new EDIInterchangeUnpackerResult(errorReason: (NoResString)"UniversalEvent of unknown type!");
		}

		var eventType = reader.ReadElementContentAsString();
		var messageSubType = GetMessageSubType(eventType);
		if (string.IsNullOrEmpty(messageSubType))
		{
			return new EDIInterchangeUnpackerResult(errorReason: (NoResString)$"Unsupported UniversalEvent type: {eventType}!");
		}

		var faultMessage = interchange.ContainedMessages.AddNew(outgoingMessage.GetType());
		faultMessage.EM_GB = outgoingMessage.EM_GB;
		faultMessage.EM_MessageType = outgoingMessage.EM_MessageType;
		faultMessage.EM_MessageSubType = messageSubType;
		faultMessage.EM_ApplicationCode = outgoingMessage.EM_ApplicationCode;
		faultMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		faultMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
		faultMessage.EM_IsActive = true;
		faultMessage.EM_LinkedObject = outgoingMessage.EM_LinkedObject;
		using var interchangeReadContext = interchange.GetReadContext();
		faultMessage.SetEM_MessageTextSource(new TextReaderSource(LargeMessageHelper.GetStreamFromNode(interchangeReadContext.PayloadXmlReader)));
		faultMessage.EM_IsTestMessage = outgoingMessage.EM_IsTestMessage;
		interchange.Logs.AddNew(Events.InterchangeAcknowledged, $"Created fault message {faultMessage.EM_MessageNum} for transmit message {outgoingMessage.EM_MessageNum}.");

		return new EDIInterchangeUnpackerResult([faultMessage]);
	}

	string GetMessageSubType(string eventType) => eventType switch
	{
		UniversalMessaging.Event.Types.Rejection => EDIMessageSubType.UniversalRejection,
		_ => null,
	};
}

