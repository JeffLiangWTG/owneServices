using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;
namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class ArrivalMessageHeader : IArrivalMessageHeader
	{
		public ArrivalMessageHeader(AsycudaTransferBill transferBill, ZString statusCode)
		{
			this.statusCode = statusCode;

			bill = Argument.NotNull(transferBill, nameof(transferBill)).Bill;
			arrivalHeader = transferBill.TransferHeader.ArrivalHeader;
			manifestHeader = arrivalHeader.Header;
		}

		readonly AsycudaBill bill;
		readonly AsycudaArrivalHeader arrivalHeader;
		readonly AsycudaManifestHeader manifestHeader;
		readonly ZString statusCode;

		public ZString MessageType => Constants.AIMMessageSubTypes.FSN;

		public ZString Reference => bill.ABL_BillNumber;

		public IAIMCargoControlLocation CargoControlLine => cargoControlLine ?? (cargoControlLine = new AIMCargoControlLocation(manifestHeader));
		IAIMCargoControlLocation cargoControlLine;

		public IAIMAirWaybill AirWaybill => airWaybill ?? (airWaybill = new AIMAirWaybillForArrival(arrivalHeader, bill));
		IAIMAirWaybill airWaybill;

		public IAIMArrival Arrival => arrival ?? (arrival = new AIMArrival(arrivalHeader));
		IAIMArrival arrival;

		public IAIMAirlineStatusNotification AirlineStatusNotification => airlineStatusNotification ?? (airlineStatusNotification = new AirlineStatusNotification(statusCode));
		IAIMAirlineStatusNotification airlineStatusNotification;
	}
}
