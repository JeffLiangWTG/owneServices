using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMArrivalForManifestMsg : IAIMArrival
	{
		public AIMArrivalForManifestMsg(AdditionalMessageInformation additionalMessageInformation)
		{
			this.additionalMessageInformation = Argument.NotNull(additionalMessageInformation, "additionalMessageInformation");
		}

		readonly AdditionalMessageInformation additionalMessageInformation;

		#region IAIMArrival Implementation

		public ZString FlightNumber => AIMFlightHelper.CreatePaddedFlightNumberForMessage(additionalMessageInformation.AM_FlightNo, additionalMessageInformation.Header.AMA_CarrierCode);
		public ZDate ScheduledArrivalDate => additionalMessageInformation.AM_FlightArrivalDate;
		public ZString PartArrivalReference => additionalMessageInformation.AM_FlightReference;
		public ZBool IsBoardedQuantity => additionalMessageInformation.AM_IsSplitShipment;
		public ZDecimal BoardedPieceCount => (ZDecimal)additionalMessageInformation.AM_BoardedQty;
		public ZString WeightCode => additionalMessageInformation.AM_BoardedWeightUQ;
		public ZDecimal Weight => additionalMessageInformation.AM_BoardedWeight;

		#endregion
	}
}
