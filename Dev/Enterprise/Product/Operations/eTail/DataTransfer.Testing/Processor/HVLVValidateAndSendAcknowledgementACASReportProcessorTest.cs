using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class HVLVValidateAndSendAcknowledgementACASReportProcessorTest : HVLVValidateAndSendACASReportProcessorTest
	{
		protected override HVLVConsignment GetTestConsignment(ZGuid shipmentPK, string waybillNumber)
		{
			var consignment = base.GetTestConsignment(shipmentPK, waybillNumber);
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			return consignment;
		}

		protected override HVLVValidateAndSendACASReportProcessor GetSendACASReportProcessor(ForwardingShipment shipment)
		{
			return new HVLVValidateAndSendAcknowledgementACASReportProcessor(shipment);
		}
	}
}
