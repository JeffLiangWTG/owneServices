using System;

namespace Enterprise.Freight.Integration
{
	public interface IPortMatch
	{
		string Unloco { get; }

		DateTimeOffset? ArrivalTime { get; }

		DateTimeOffset? DepartureTime { get; }
	}
}
