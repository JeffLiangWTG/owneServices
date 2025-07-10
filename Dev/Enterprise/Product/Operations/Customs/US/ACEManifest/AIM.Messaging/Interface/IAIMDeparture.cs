using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMDeparture
	{
		ZString FlightNumber { get; }
		ZDate DateOfScheduledArrival { get; }
		ZDate LiftoffDate { get; }
		ZString LiftoffTime { get; }
		ZString ActualImportingCarrier { get; }
		ZString ActualFlightNumber { get; }
	}
}
