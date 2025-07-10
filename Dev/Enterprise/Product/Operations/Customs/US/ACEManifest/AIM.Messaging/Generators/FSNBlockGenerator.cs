using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FSNBlockGenerator : AIMBlockGenerator
	{
		public FSNBlockGenerator(IAIMMessageHeader arrivalMessageHeader)
		{
			this.arrivalMessageHeader = (IArrivalMessageHeader)Argument.NotNull(arrivalMessageHeader, nameof(arrivalMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(arrivalMessageHeader);
			PopulateCargoControlLocation(arrivalMessageHeader.CargoControlLine);
			PopulateAirWayBill(arrivalMessageHeader.AirWaybill);
			PopulateArrivalDetails(arrivalMessageHeader.Arrival);
			PopulateAirlineStatusNotification(arrivalMessageHeader.AirlineStatusNotification);
		}
		readonly IArrivalMessageHeader arrivalMessageHeader;
	}
}
