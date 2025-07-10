using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class HVLVValidateAndSendOriginalACASReportProcessorTest : HVLVValidateAndSendACASReportProcessorTest
	{
		protected override HVLVValidateAndSendACASReportProcessor GetSendACASReportProcessor(ForwardingShipment shipment)
		{
			return new HVLVValidateAndSendOriginalACASReportProcessor(shipment);
		}
	}
}
