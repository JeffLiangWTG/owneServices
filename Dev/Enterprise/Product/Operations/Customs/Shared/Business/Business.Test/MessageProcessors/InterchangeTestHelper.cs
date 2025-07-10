using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business.Testing
{
	public static class InterchangeTestHelper
	{
		public const string DefaultApplicationCode = ApplicationCodeList.Codes.SYS;
		public const string DefaultFrom = "TEST FROM";
		public const string DefaultTo = "TEST TO";
		public const string DefaultReceiveTransmit = EDIInterchange.Direction.Receive;
		public const string DefaultInterchangeStatus = EDIInterchange.Status.Queued;
		public const string DefaultMessageStatus = EDIMessage.Status.Queued;
		public const string DefaultInterchangeType = EDIInterchangeTypeList.Codes.XDC;
		public const string DefaultMessageType = EDIMessageTypeList.Codes.XDC;
		public const string DefaultApplicationReference = "EDIMessageCreationForTest";

		public static TEDIInterchange CreateInterchangeForTest<TEDIInterchange>(this BusinessObjectFactory factory,
			ZString applicationCode = default,
			ZGuid sessionGuid = default,
			ZString from = default,
			ZString to = default,
			ZString interchangeType = default,
			ZString receiveTransmit = default,
			ZString interchangeStatus = default,
			ZString messageBody = default,
			ZString num = default)
			where TEDIInterchange : EDIInterchange
		{
			var interchange = factory.New<TEDIInterchange>();
			interchange.EI_SessionGUID = sessionGuid.IsEmpty ? ZGuid.NewZGuid() : sessionGuid;
			interchange.EI_ApplicationCode = applicationCode.IsDefault ? new ZString(DefaultApplicationCode) : applicationCode;
			interchange.EI_ReceiveTransmit = receiveTransmit.IsDefault ? new ZString(DefaultReceiveTransmit) : receiveTransmit;
			interchange.EI_From = from.IsDefault ? new ZString(DefaultFrom) : from;
			interchange.EI_To = to.IsDefault ? new ZString(DefaultTo) : to;
			interchange.EI_Status = interchangeStatus.IsDefault ? new ZString(DefaultInterchangeStatus) : interchangeStatus;
			interchange.EI_InterchangeType = interchangeType.IsDefault ? new ZString(DefaultInterchangeType) : interchangeType;
			interchange.EI_BodyText = messageBody;
			if (!num.IsDefault)
			{
				interchange.EI_InterchangeNum = num;
			}
			return interchange;
		}

		public static (TEDIInterchange Interchange, TEDIMessage Message) CreateInterchangeAndMessageForTest<TEDIInterchange, TEDIMessage>(
			this BusinessObjectFactory factory,
			ZString applicationCode = default,
			ZGuid sessionGuid = default,
			ZString from = default,
			ZString to = default,
			ZString interchangeType = default,
			ZString receiveTransmit = default,
			ZString interchangeStatus = default,
			ZString messageStatus = default,
			ZString messageBody = default,
			ZString num = default,
			ZString messageType = default,
			ZString messageApplicationReference = default)
			where TEDIInterchange : EDIInterchange
			where TEDIMessage : EDIMessage
		{
			sessionGuid = sessionGuid.IsDefault ? ZGuid.NewZGuid() : sessionGuid;
			receiveTransmit = receiveTransmit.IsDefault ? new ZString(DefaultReceiveTransmit) : receiveTransmit;

			var interchange = CreateInterchangeForTest<TEDIInterchange>(factory,
				sessionGuid: sessionGuid,
				applicationCode: applicationCode,
				from: from,
				to: to,
				interchangeType: interchangeType,
				receiveTransmit: receiveTransmit,
				interchangeStatus: interchangeStatus,
				messageBody: messageBody,
				num: num);

			var message = (TEDIMessage)interchange.ContainedMessages.AddNew(typeof(TEDIMessage));
			message.EM_ApplicationCode = applicationCode.IsDefault ? new ZString(DefaultApplicationCode) : applicationCode;
			message.EM_EI = interchange.PK;
			message.EM_MessageType = messageType.IsDefault ? new ZString(DefaultMessageType) : messageType;
			message.EM_MessageText = messageBody;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_ApplicationReference = messageApplicationReference.IsDefault ? new ZString(DefaultApplicationReference) : messageApplicationReference;
			message.EM_Status = messageStatus.IsDefault ? new ZString(DefaultMessageStatus) : messageStatus;

			return (interchange, message);
		}
	}
}
