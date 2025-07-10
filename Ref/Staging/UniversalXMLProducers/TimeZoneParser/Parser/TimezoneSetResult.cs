using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public class TimezoneSetResult
	{
		public IEnumerable<RefTimeZoneSet> RefTimeZoneSets { get; private set; }

		public TimezoneSetResult(IEnumerable<RefTimeZoneSet> refTimezoneSets)
		{
			Argument.NotNull(refTimezoneSets, nameof(refTimezoneSets));
			this.RefTimeZoneSets = refTimezoneSets;
		}
	}
}
