using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVValidateAndSendOriginalACASReportProcessor : HVLVValidateAndSendACASReportProcessor
	{
		public HVLVValidateAndSendOriginalACASReportProcessor(ForwardingShipment shipment) : base(shipment)
		{
		}

		protected override ACASReportAction AcasReportAction => ACASReportAction.SendOriginal;

		protected override string ACASStatusCanBeSend => string.Empty;
	}
}
