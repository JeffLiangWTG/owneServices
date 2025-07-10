using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class UniversalCMMTestHelper
	{
		internal static UniversalEvent CreateUniversalEvent(BusinessObjectFactory factory, EDIMessage message)
		{
			var eventDataObject = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			return eventDataObject;
		}

		internal static EDIMessage CreateEDIMessage(BusinessObjectFactory factory, string messagePayload)
		{
			var ediMessage = factory.New<EDIMessage>();
			ediMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			ediMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			ediMessage.EM_MessageText = messagePayload;
			return ediMessage;
		}
	}
}
