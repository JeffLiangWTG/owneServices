using Enterprise.Freight.Integration;

namespace Enterprise.Freight.AIS
{
	public sealed class PortMatches : IPortMatches
	{
		public PortMatches()
		{
		}

		public PortMatches(PortMatch lastForeignPort, PortMatch firstArrivalPort)
		{
			LastForeignPort = lastForeignPort;
			FirstArrivalPort = firstArrivalPort;
		}

		public IPortMatch LastForeignPort { get; }
		public IPortMatch FirstArrivalPort { get; }
	}
}
