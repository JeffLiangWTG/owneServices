using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FXIBillBlockGenerator : AIMBlockGenerator
	{
		public FXIBillBlockGenerator(IAIMMessageHeader billMessageHeader)
		{
			this.billMessageHeader = (IBillMessageHeader)Argument.NotNull(billMessageHeader, nameof(billMessageHeader));
		}

		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(billMessageHeader);
			PopulateAirWayBill(billMessageHeader.AirWaybill);
			PopulateWayBillDetails(billMessageHeader.Waybill);
			PopulateCBPEntryDetails(billMessageHeader.CBPEntryDetail);
			PopulateShipper(billMessageHeader.Shipper);
			PopulateConsignee(billMessageHeader.Consignee);
			PopulateCBPShipmentDescription(billMessageHeader.CPBShipmentDescription);
			PopulateFDAFreightIndicator(billMessageHeader.FDAFreightIndicator);
		}

		readonly IBillMessageHeader billMessageHeader;
	}
}
