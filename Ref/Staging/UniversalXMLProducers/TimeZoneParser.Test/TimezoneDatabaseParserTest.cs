using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NodaTime;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser.Test
{
	public class TimezoneDatabaseParserTest : TimezoneDatabaseParser
	{
		public TimezoneDatabaseParserTest(string timezoneDbFilePath, int yearToProcess, bool fullTimezoneSet) : base(timezoneDbFilePath,yearToProcess,fullTimezoneSet)
		{
		}

		public RefTimeZoneSet ParseZoneTest(DateTimeZone zone)
		{
			return ParseZone(zone);
		}

		public IEnumerable<ZoneIntervalWtg> GetDaylightSavingTransitionsTest(DateTimeZone timeZone, int year)
		{
			return GetDaylightSavingTransitions(timeZone, year);
		}

		public List<RefTimeZoneRule> GetTimeZoneRuleTest(ZoneIntervalWtg interval)
		{
			return GetTimeZoneRule(interval);
		}
	}
}
