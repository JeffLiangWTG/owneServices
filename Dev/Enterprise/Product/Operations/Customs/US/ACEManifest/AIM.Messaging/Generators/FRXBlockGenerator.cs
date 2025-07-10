using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FRXBlockGenerator : AIMBlockGenerator
	{
		public FRXBlockGenerator(IAIMMessageHeader manifestMessageHeader)
		{
			this.manifestMessageHeader = (IManifestMessageHeader)Argument.NotNull(manifestMessageHeader, nameof(manifestMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(manifestMessageHeader);
			PopulateCargoControlLocation(manifestMessageHeader.CargoControlLine);
			PopulateAirWayBill(manifestMessageHeader.AirWaybill);
			PopulateArrivalDetails(manifestMessageHeader.Arrival);
			PopulateReasonForAmendment(manifestMessageHeader.ReasonForAmendment);
		}
		readonly IManifestMessageHeader manifestMessageHeader;
	}
}
