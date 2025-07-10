using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	class AIMArrival : IAIMArrival
	{
		public AIMArrival(AsycudaArrivalHeader arrivalHeader)
		{
			this.arrivalHeader = Argument.NotNull(arrivalHeader, "arrivalHeader");
		}

		readonly AsycudaArrivalHeader arrivalHeader;

		public ZString FlightNumber => AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrivalHeader.ATH_VoyageFlightNo, arrivalHeader.ManifestHeader.AMA_CarrierCode);

		public ZDate ScheduledArrivalDate => arrivalHeader.ATH_ETAAtDischargePort.Date;

		public ZString PartArrivalReference => arrivalHeader.ATH_Reference;
		public ZBool IsBoardedQuantity => ZBool.False;
		public ZDecimal BoardedPieceCount => ZDecimal.Zero;
		public ZString WeightCode => ZString.Empty;
		public ZDecimal Weight => ZDecimal.Zero;
	}
}
