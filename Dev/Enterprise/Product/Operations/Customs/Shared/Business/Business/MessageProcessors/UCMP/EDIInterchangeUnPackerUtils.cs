using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public static class EDIInterchangeUnPackerUtils
	{
		public static EDIMessage CreateReceivedEDIMessage(EDIInterchange interchange, string bodyText)
		{
			var factory = interchange.Factory;
			var message = factory.New<EDIMessage>();
			EDIInterchangeUnPackerUtils.PopulateEDIMessage(message,
				interchange.EI_ApplicationCode,
				interchange.EI_InterchangeType,
				interchange.EI_InterchangeType,
				interchange.EI_InterchangeNum,
				bodyText);

			interchange.ContainedMessages.Add(message);
			return message;
		}

		public static void PopulateEDIMessage(EDIMessage message,
			ZString applicationCode,
			ZString messageType,
			string messageSubType,
			ZString messageNum,
			ZString messageText,
			string status = EDIMessageStatusList.Codes.Queued,
			bool isActive = true,
			string receiveTransmit = EDIMessage.Direction.Receive)
		{
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_Status = status;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = messageNum.Right(EDIMessageSchema.EM_MessageNum.MaxLength);
			message.EM_IsActive = isActive;
		}
	}
}
