using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public class TimezoneDatabaseParser
	{
		readonly string timezoneDbFilePath;
		readonly int yearToProcess;
		readonly bool fullTimezoneSet;

		public TimezoneDatabaseParser(string timezoneDbFilePath, int yearToProcess, bool fullTimezoneSet)
		{
			Argument.NotNullOrEmpty(timezoneDbFilePath, nameof(timezoneDbFilePath));
			this.timezoneDbFilePath = timezoneDbFilePath;
			this.yearToProcess = yearToProcess;
			this.fullTimezoneSet = fullTimezoneSet;
		}

		public void ExportXML(DateTime publishTime, string filePath = null)
		{
			var result = ParseTimezoneDatabase();
			new TimezoneXmlProducer(fullTimezoneSet).ExportToXmlInBatch(result.RefTimeZoneSets, publishTime, filePath);
		}

		public TimezoneSetResult ParseTimezoneDatabase()
		{
			IDateTimeZoneProvider provider;
			var result = new List<RefTimeZoneSet>();
			using (var stream = File.OpenRead(timezoneDbFilePath))
			{
				var source = TzdbDateTimeZoneSource.FromStream(stream);
				provider = new DateTimeZoneCache(source);
				foreach (var zone in provider.GetAllZones())
				{
					result.Add(ParseZone(zone));
				}
			}
			return new TimezoneSetResult(result.ToArray());
		}

		static short ConvertSecondsToMinutes(int seconds)
		{
			return (short)(seconds / 60);
		}

		static RefTimeZone[] GetTimeZoneFromZoneIntervalArray(List<ZoneIntervalWtg> zoneIntervals)
		{
			var groupedZoneIntervals = zoneIntervals.GroupBy(x => new { SavingsSeconds = x.Savings.Seconds, WallOffsetSeconds = x.WallOffset.Seconds, x.Name, StartYear = x.HasStart ? x.IsoLocalStart.Year : 0, EndYear = x.HasEnd ? x.IsoLocalEnd.Year : 9999 }).ToList();
			var result = new List<RefTimeZone>();
			for (int i = groupedZoneIntervals.Count - 1; i >= 0; i--) //Use offset from latest transition
			{
				var currentZoneType = groupedZoneIntervals[i].Key.SavingsSeconds == 0 ? "STD" : "DLS";
				if (result.Count != 2 && !result.Any(x => x.R2_Type == currentZoneType))
				{
					var timezoneToAdd = new RefTimeZone()
					{
						R2_CivilianTimeZoneCode = groupedZoneIntervals[i].Key.Name,
						R2_OffsetMinutesFromUTC = ConvertSecondsToMinutes(groupedZoneIntervals[i].Key.WallOffsetSeconds),
						R2_Type = currentZoneType
					};
					result.Add(timezoneToAdd);
				}
			}
			return result.ToArray();
		}

		protected RefTimeZoneSet ParseZone(DateTimeZone zone)
		{
			var timezoneSet = new RefTimeZoneSet() { R3_IsActive = true, R3_TimeZoneSetName = zone.Id };
			var transitions = GetDaylightSavingTransitions(zone, yearToProcess);
			timezoneSet.RefTimeZones = GetTimeZoneFromZoneIntervalArray(transitions);

			var rules = new List<RefTimeZoneRule>();
			foreach (var transition in transitions.Where(x => x.Savings.Seconds != 0)) //get just the rule start and end transition list
			{
				rules.AddRange(GetTimeZoneRule(transition));
			}

			//merge rules
			var mergedRules = new List<RefTimeZoneRule>();
			RefTimeZoneRule currentStartRule = null;
			RefTimeZoneRule currentEndRule = null;
			foreach (var rule in rules)
			{
				var currentRule = rule.R4_StartOrEndRule == "STA" ? currentStartRule : currentEndRule;
				if (RuleNotExist(mergedRules, rule, currentRule))
				{
					mergedRules.Add(rule);
					if (rule.R4_StartOrEndRule == "STA")
					{
						currentStartRule = rule;
					}
					else
					{
						currentEndRule = rule;
					}
				}
				else
				{
					currentRule.R4_ToYear = rule.R4_ToYear >= yearToProcess + 99 ? 0 : rule.R4_ToYear; //set R4_ToYear to 0 for years that extend to end of time
				}
			}

			if (mergedRules.Count != 0) //If current zone has DLS rules add them
			{
				timezoneSet.RefTimeZones.First(x => x.R2_Type == "DLS").RefTimeZoneRules = mergedRules.ToArray();
			}

			if (!fullTimezoneSet) //if not fullTimezoneSet get current active record for each TimezoneSet
			{
				var dlsStart = timezoneSet.RefTimeZones?.FirstOrDefault(x => x.R2_Type == "DLS")?.RefTimeZoneRules?.Where(x => (x.R4_ToYear >= yearToProcess || x.R4_ToYear == 0) && x.R4_StartOrEndRule == "STA")?.FirstOrDefault();
				var dlsEnd = timezoneSet.RefTimeZones?.FirstOrDefault(x => x.R2_Type == "DLS")?.RefTimeZoneRules?.Where(x => (x.R4_ToYear >= yearToProcess || x.R4_ToYear == 0) && x.R4_StartOrEndRule == "END")?.FirstOrDefault();
				var latestTimezoneRules = new List<RefTimeZoneRule>();
				if (dlsStart != null)
				{
					latestTimezoneRules.Add(dlsStart);
				}
				if (dlsEnd != null)
				{
					latestTimezoneRules.Add(dlsEnd);
				}
				if (latestTimezoneRules.Count == 0)
				{
					timezoneSet.RefTimeZones = timezoneSet.RefTimeZones.Where(x => x.R2_Type == "STD").ToArray();
				}
				else
				{
					timezoneSet.RefTimeZones.First(x => x.R2_Type == "DLS").RefTimeZoneRules = latestTimezoneRules.ToArray();
				}
			}
			return timezoneSet;
		}

		static bool RuleNotExist(List<RefTimeZoneRule> mergedRules, RefTimeZoneRule rule, RefTimeZoneRule currentRule)
		{
			return mergedRules.All(x => x.R4_StartOrEndRule != rule.R4_StartOrEndRule) || (
									currentRule.R4_DaylightSavingDayName != rule.R4_DaylightSavingDayName
									|| currentRule.R4_DaylightSavingMonth != rule.R4_DaylightSavingMonth
									|| currentRule.R4_DaylightSavingDayCount != rule.R4_DaylightSavingDayCount
									|| currentRule.R4_DaylightSavingDate != rule.R4_DaylightSavingDate
									|| currentRule.R4_DaylightSavingDayWeekDate != rule.R4_DaylightSavingDayWeekDate
									|| currentRule.R4_TypeOfTime != rule.R4_TypeOfTime);
		}

		static protected List<RefTimeZoneRule> GetTimeZoneRule(ZoneIntervalWtg interval)
		{
			var result = new List<RefTimeZoneRule>();
			var startDate = interval.IsoLocalStart;
			var endDate = interval.IsoLocalEnd;

			result.Add(
			new RefTimeZoneRule()
			{
				R4_StartOrEndRule = "STA",
				R4_FromYear = startDate.Year,
				R4_ToYear = startDate.Year,
				R4_DaylightSavingDate = new DateTime(2000, 1, 1, endDate.Hour, endDate.Minute, 0),
				R4_DaylightSavingDayWeekDate = "MON",
				R4_DaylightSavingDayName = startDate.ToDateTimeUnspecified().ToString("ddd", CultureInfo.InvariantCulture).ToUpperInvariant(),
				R4_DaylightSavingMonth = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(startDate.Month).ToUpperInvariant(),
				R4_DaylightSavingDayCount = GetDayLightSavingDayCount(startDate)
			});
			result.Add(
			new RefTimeZoneRule()
			{
				R4_StartOrEndRule = "END",
				R4_FromYear = endDate.Year,
				R4_ToYear = endDate.Year,
				R4_DaylightSavingDate = new DateTime(2000, 1, 1, endDate.Hour, endDate.Minute, 0).AddSeconds(-interval.Savings.Seconds),
				R4_DaylightSavingDayWeekDate = "MON",
				R4_DaylightSavingDayName = endDate.ToDateTimeUnspecified().ToString("ddd", CultureInfo.InvariantCulture).ToUpperInvariant(),
				R4_DaylightSavingMonth = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(endDate.Month).ToUpperInvariant(),
				R4_DaylightSavingDayCount = GetDayLightSavingDayCount(endDate)
			});
			return result;
		}

		static byte GetDayLightSavingDayCount(LocalDateTime localDate)
		{
			Argument.NotNull(localDate, nameof(localDate));
			var weekNumber = Math.Ceiling(localDate.Day / 7.0);
			if (weekNumber >= 4)
			{
				if (localDate.PlusDays(7).Month != localDate.Month)
				{
					return 5;
				}
			}
			return (byte)weekNumber;
		}

		static protected List<ZoneIntervalWtg> GetDaylightSavingTransitions(DateTimeZone timeZone, int year)
		{
			var yearStart = new LocalDateTime(2000, 1, 1, 0, 0).InZoneLeniently(timeZone).ToInstant();
			var yearEnd = new LocalDateTime(year + 100, 1, 1, 0, 0).InZoneLeniently(timeZone).ToInstant();
			var result = new List<ZoneIntervalWtg>();
			foreach (var interval in timeZone.GetZoneIntervals(yearStart, yearEnd))
			{
				result.Add(new ZoneIntervalWtg(interval));
			}
			return result;
		}
	}
}
