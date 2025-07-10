using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business
{
	public sealed class XTDelivery : EServicesDelivery
	{
		protected override string GetRecipientID(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination;
		}

		protected override string InterchangeQueuedStatus
		{
			get { return EDIInterchangeStatusList.Codes.Queued; }
		}

		protected override string TransportType
		{
			get { return EDIInterchangeTransportTypeList.Codes.xT; }
		}
	}
}
