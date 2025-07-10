using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public sealed class EHubDelivery : EServicesDelivery
	{
		protected override string GetRecipientID(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination;
		}

		protected override string InterchangeQueuedStatus
		{
			get { return EDIInterchangeStatusList.Codes.eHubQueued; }
		}

		protected override string TransportType
		{
			get { return EDIInterchangeTransportTypeList.Codes.eHub; }
		}
	}
}
