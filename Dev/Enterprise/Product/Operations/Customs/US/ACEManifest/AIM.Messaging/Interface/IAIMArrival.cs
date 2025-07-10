using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMArrival
	{
		ZString FlightNumber { get; }
		ZDate ScheduledArrivalDate { get; }
		ZString PartArrivalReference { get; }
		ZBool IsBoardedQuantity { get; }
		ZDecimal BoardedPieceCount { get; }
		ZString WeightCode { get; }
		ZDecimal Weight { get; }
	}
}
