using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TR.ServiceTasks.Testing
{
	public static class ServiceTaskTestHelper
	{
		public static EDIInterchange CreateEDIInterchange(string interchangeType, string status, string direction, BusinessObjectFactory factory)
		{
			var messageTrackingID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");

			var interchange = factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TRCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = "eHub";
			interchange.EI_To = "TR Customs";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = messageTrackingID;

			return interchange;
		}

		public static TRManifestMessage CreateEdiMessage(string bodyText, string status, string direction, BusinessObjectFactory factory)
		{
			var message = factory.New<TRManifestMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRO;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;

			return message;
		}

		public static ETradeEDIMessage CreateEtradeEdiMessage(string bodyText, string status, string direction, BusinessObjectFactory factory)
		{
			var message = factory.New<ETradeEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRE;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;

			return message;
		}

		public static T CreateEdiMessage<T>(string messageType, string bodyText, string status, string direction, BusinessObjectFactory factory) where T : EDIMessage
		{
			var message = factory.New<T>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;

			return message;
		}
	}
}
