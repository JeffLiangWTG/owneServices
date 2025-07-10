using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMWaybill
	{
		ZString AirportOfOrigin { get; }
		ZString PermitToProceedDestinationAirport { get; }
		ZDecimal NumberOfPieces { get; }
		ZString WeightCode { get; }
		ZDecimal Weight { get; }
		ZString CargoDescription { get; }
		ZDate DateOfArrivalAtThePermitToProceedDestinationAirport { get; }
	}
}
