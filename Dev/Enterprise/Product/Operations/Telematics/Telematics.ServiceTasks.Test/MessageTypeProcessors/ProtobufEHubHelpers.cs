using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageTypeProcessors
{
	static class ProtobufEHubHelpers
	{
		public static ZGuid CreateEHubProtobufMessage(BusinessObjectFactory factory, byte[] data)
		{
			var body = $"<ProtobufData>{Convert.ToBase64String(data)}</ProtobufData>";

			return EHubHelpers.CreateEHubMessage(
				factory,
				ApplicationCodeList.Codes.Telematics,
				EDIMessageStatusList.Codes.Queued,
				true,
				EDIMessageTypeList.Codes.XDC,
				TelematicsMessageList.Codes.ProtobufData,
				body,
				ReceiveTransmitList.Codes.Receive);
		}
	}
}
