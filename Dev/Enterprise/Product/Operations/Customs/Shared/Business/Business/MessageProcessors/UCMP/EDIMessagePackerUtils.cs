using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public static class EDIMessagePackerUtils
	{
		public static void PopulateInterchange(EDIInterchange interchange, ZString applicationCode, ZString interchangeType,
			ZString from, ZString to, ZGuid branchPK, ZGuid externalPasswordPK, ZGuid sessionGuid,
			string status = EDIInterchangeStatusList.Codes.Queued,
			bool isActive = true,
			string receiveTransmit = EDIInterchange.Direction.Transmit,
			string transportType = EDIInterchangeTransportTypeList.Codes.xT
		)
		{
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = from;
			interchange.EI_To = to;
			interchange.EI_GB = branchPK;
			interchange.EI_GP = externalPasswordPK;
			interchange.EI_SessionGUID = sessionGuid;
			interchange.EI_ReceiveTransmit = receiveTransmit;
			interchange.EI_IsActive = isActive;
			interchange.EI_Status = status;
			interchange.EI_TransportType = transportType;
		}
	}
}
