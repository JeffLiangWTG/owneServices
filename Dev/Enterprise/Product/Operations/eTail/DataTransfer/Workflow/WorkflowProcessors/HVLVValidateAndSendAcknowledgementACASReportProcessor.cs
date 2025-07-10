using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVValidateAndSendAcknowledgementACASReportProcessor : HVLVValidateAndSendACASReportProcessor
	{
		public HVLVValidateAndSendAcknowledgementACASReportProcessor(ForwardingShipment shipment) : base(shipment)
		{
		}

		protected override ACASReportAction AcasReportAction => ACASReportAction.SendAcknowledgement;

		protected override string ACASStatusCanBeSend => HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
	}
}
