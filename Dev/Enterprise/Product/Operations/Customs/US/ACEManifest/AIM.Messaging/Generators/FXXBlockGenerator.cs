using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FXXBlockGenerator : AIMBlockGenerator
	{
		public FXXBlockGenerator(IAIMMessageHeader manifestMessageHeader)
		{
			this.manifestMessageHeader = (IManifestMessageHeader)Argument.NotNull(manifestMessageHeader, nameof(manifestMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(manifestMessageHeader);
			PopulateCargoControlLocation(manifestMessageHeader.CargoControlLine);
			PopulateAirWayBill(manifestMessageHeader.AirWaybill);
			PopulateArrivalDetails(manifestMessageHeader.Arrival);
			PopulateCBPEntryDetailsForCancellation();
			PopulateReasonForAmendment(manifestMessageHeader.ReasonForAmendment);
		}
		readonly IManifestMessageHeader manifestMessageHeader;
	}
}
