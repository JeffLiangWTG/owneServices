using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FRIBlockGenerator : AIMBlockGenerator
	{
		public FRIBlockGenerator(IAIMMessageHeader manifestMessageHeader)
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
			PopulateShipper(manifestMessageHeader.Shipper);
			PopulateConsignee(manifestMessageHeader.Consignee);
			PopulateTransferDetails(manifestMessageHeader.Transfer);
			PopulateFDAFreightIndicator(manifestMessageHeader.FDAFreightIndicator);
		}

		readonly IManifestMessageHeader manifestMessageHeader;
	}
}
