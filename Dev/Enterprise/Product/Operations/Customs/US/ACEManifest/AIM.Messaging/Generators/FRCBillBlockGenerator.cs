using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FRCBillBlockGenerator : AIMBlockGenerator
	{
		public FRCBillBlockGenerator(IAIMMessageHeader billMessageHeader)
		{
			this.billMessageHeader = (IBillMessageHeader)Argument.NotNull(billMessageHeader, nameof(billMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(billMessageHeader);
			PopulateAirWayBill(billMessageHeader.AirWaybill);
			PopulateWayBillDetails(billMessageHeader.Waybill);
			PopulateShipper(billMessageHeader.Shipper);
			PopulateConsignee(billMessageHeader.Consignee);
			PopulateCBPShipmentDescription(billMessageHeader.CPBShipmentDescription);
			PopulateFDAFreightIndicator(billMessageHeader.FDAFreightIndicator);
			PopulateReasonForAmendment(billMessageHeader.ReasonForAmendment);
		}

		readonly IBillMessageHeader billMessageHeader;
	}
}
