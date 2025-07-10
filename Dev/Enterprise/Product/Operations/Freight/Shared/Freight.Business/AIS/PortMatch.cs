using System;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.AIS
{
	public sealed class PortMatch : IPortMatch
	{
		public PortMatch(string unloco, DateTimeOffset? arrivalTime, DateTimeOffset? departureTime)
		{
			Unloco = unloco;
			ArrivalTime = arrivalTime;
			DepartureTime = departureTime;
		}

		public string Unloco { get; }
		public DateTimeOffset? ArrivalTime { get; }
		public DateTimeOffset? DepartureTime { get; }
	}
}
