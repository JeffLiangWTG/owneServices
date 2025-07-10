using NodaTime;
using NodaTime.TimeZones;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public class ZoneIntervalWtg
	{
		public Offset WallOffset { get; private set; }
		public string Name { get; private set; }
		public LocalDateTime IsoLocalEnd { get; private set; }
		public LocalDateTime IsoLocalStart { get; private set; }
		public bool HasEnd { get; private set; }
		public bool HasStart { get; private set; }
		public Offset Savings { get; private set; }
		public Offset StandardOffset { get; private set; }

		public ZoneIntervalWtg(ZoneInterval zoneInterval)
		{
			WallOffset = zoneInterval.WallOffset;
			Name = zoneInterval.Name;
			IsoLocalEnd = zoneInterval.HasEnd ? zoneInterval.IsoLocalEnd : new LocalDateTime(2079, 6, 6, 23, 59);
			IsoLocalStart = zoneInterval.HasStart ? zoneInterval.IsoLocalStart : new LocalDateTime(1900, 1, 1, 0, 0);
			HasEnd = true;
			HasStart = true;
			Savings = zoneInterval.Savings;
			StandardOffset = zoneInterval.StandardOffset;
		}
	}
}
