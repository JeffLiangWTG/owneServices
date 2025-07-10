using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FXCBlockGenerator : AIMBlockGenerator
	{
		public FXCBlockGenerator(IAIMMessageHeader manifestMessageHeader)
		{
			this.manifestMessageHeader = (IManifestMessageHeader)Argument.NotNull(manifestMessageHeader, nameof(manifestMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(manifestMessageHeader);
			PopulateCargoControlLocation(manifestMessageHeader.CargoControlLine);
			PopulateAirWayBill(manifestMessageHeader.AirWaybill);
			PopulateWayBillDetails(manifestMessageHeader.Waybill);
			PopulateArrivalDetails(manifestMessageHeader.Arrival);
			PopulateAgent(manifestMessageHeader.Agent);
			PopulateCBPEntryDetails(manifestMessageHeader.CBPEntryDetail);
			PopulateShipper(manifestMessageHeader.Shipper);
			PopulateConsignee(manifestMessageHeader.Consignee);
			PopulateTransferDetails(manifestMessageHeader.Transfer);
			PopulateFDAFreightIndicator(manifestMessageHeader.FDAFreightIndicator);
			PopulateReasonForAmendment(manifestMessageHeader.ReasonForAmendment);
		}

		readonly IManifestMessageHeader manifestMessageHeader;
	}
}
