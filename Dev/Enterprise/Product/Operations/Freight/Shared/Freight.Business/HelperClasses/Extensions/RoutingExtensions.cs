using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business
{
	static class RoutingExtensions
	{
		public static bool IsValidToTryMatch(this ScheduleInfo schedule)
		{
			return schedule.Carrier.Length == 2
				&& 0 < schedule.FlightNumber && schedule.FlightNumber <= 9999
				&& (schedule.DepartureDate.IsValid && (schedule.ArrivalDate.IsEmpty || schedule.ArrivalDate.IsValid)
				|| schedule.ArrivalDate.IsValid && (schedule.DepartureDate.IsEmpty || schedule.DepartureDate.IsValid))
				&& ((schedule.DepartureDate.IsValid && schedule.DepartureDate >= ZDate.Today) || (schedule.ArrivalDate.IsValid && schedule.ArrivalDate >= ZDate.Today));
		}
	}
}
