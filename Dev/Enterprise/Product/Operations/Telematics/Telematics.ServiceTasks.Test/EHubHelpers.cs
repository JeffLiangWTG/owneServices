using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	static class EHubHelpers
	{
		public static ZGuid CreateEHubMessage(BusinessObjectFactory factory, string appCode, string status, bool active, string messageType, string messageSubType, string contents, string messageReceiveTransmit)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = appCode;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_From = "TELEMATIC";
			interchange.EI_To = Env.CurrentCompany.GetLicenceCode();

			var message = factory.New<EDIMessage>();
			message.EM_EI = interchange.PK;

			message.EM_ApplicationCode = appCode;
			message.EM_IsActive = active;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = messageReceiveTransmit;
			message.EM_Status = status;

			message.EM_MessageText = contents;

			return message.PK;
		}
	}
}
