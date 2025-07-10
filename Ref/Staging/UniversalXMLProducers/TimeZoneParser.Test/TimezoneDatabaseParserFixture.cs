using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser.Test
{
	[TestFixture]
	public class TimezoneDatabaseParserFixture
	{
		[Test]
		public void GetDaylightSavingTransitionsFixture()
		{
			var testZone = dateTimeZones.First(x => x.Id == "America/Santiago");
			var transitions = parser.GetDaylightSavingTransitionsTest(testZone, testYear);

			Assert.AreEqual(1, transitions.Count(x => x.IsoLocalStart == new LocalDateTime(2018, 5, 12, 23, 0)
			&& x.IsoLocalEnd == new LocalDateTime(2018, 8, 12, 0, 0, 0)));
			Assert.AreEqual(1, transitions.Count(x => x.IsoLocalStart == new LocalDateTime(2018, 8, 12, 1, 0)
			&& x.IsoLocalEnd == new LocalDateTime(2019, 4, 7, 0, 0, 0)));
			Assert.AreEqual(1, transitions.Count(x => x.IsoLocalStart == new LocalDateTime(2019, 4, 6, 23, 0)
			&& x.IsoLocalEnd == new LocalDateTime(2019, 9, 8, 0, 0, 0)));
			Assert.AreEqual(1, transitions.Count(x => x.IsoLocalStart == new LocalDateTime(2019, 9, 8, 1, 0)
			&& x.IsoLocalEnd == new LocalDateTime(2020, 4, 5, 0, 0, 0)));
			Assert.AreEqual(1, transitions.Count(x => x.IsoLocalStart == new LocalDateTime(2020, 4, 4, 23, 0)
			&& x.IsoLocalEnd == new LocalDateTime(2020, 9, 6, 0, 0, 0)));
			Assert.AreEqual(1, transitions.Count(x => x.IsoLocalStart == new LocalDateTime(2020, 9, 6, 1, 0)
			&& x.IsoLocalEnd == new LocalDateTime(2021, 4, 4, 0, 0, 0)));
		}

		[Test]
		public void ParseZoneFixture()
		{
			var zoneName = "America/Santiago";
			var testZone = dateTimeZones.First(x => x.Id == zoneName);
			var result = parser.ParseZoneTest(testZone);

			var rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "STA" && x.R4_FromYear == 2020);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2020, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(2023, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("SEP", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("SUN", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "END" && x.R4_FromYear == 2019);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2019, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(2028, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("APR", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("SUN", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			zoneName = "Australia/Sydney";
			testZone = dateTimeZones.First(x => x.Id == zoneName);
			result = parser.ParseZoneTest(testZone);

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "STA" && x.R4_FromYear == 2008);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2008, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(0, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("OCT", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("SUN", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "END" && x.R4_FromYear == 2008);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2008, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(0, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("APR", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("SUN", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			zoneName = "Africa/Cairo";
			testZone = dateTimeZones.First(x => x.Id == zoneName);
			result = parser.ParseZoneTest(testZone);

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "STA" && x.R4_FromYear == 2014);
			Assert.AreEqual(2, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2014, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("MAY", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(3, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("FRI", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");
			Assert.AreEqual(2014, rule.Last().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("AUG", rule.Last().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.Last().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("FRI", rule.Last().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "END" && x.R4_FromYear == 2014);
			Assert.AreEqual(2, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2014, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("JUN", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(5, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("FRI", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");
			Assert.AreEqual(2014, rule.Last().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("SEP", rule.Last().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(5, rule.Last().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("FRI", rule.Last().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			zoneName = "Asia/Dhaka";
			testZone = dateTimeZones.First(x => x.Id == zoneName);
			result = parser.ParseZoneTest(testZone);

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "STA" && x.R4_FromYear == 2009);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2009, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(2009, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("JUN", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(3, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("SAT", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "END" && x.R4_FromYear == 2010);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2010, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(2010, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("JAN", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("FRI", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			zoneName = "America/Argentina/San_Juan";
			testZone = dateTimeZones.First(x => x.Id == zoneName);
			result = parser.ParseZoneTest(testZone);

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "STA" && x.R4_FromYear == 1999);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(1999, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(1999, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("OCT", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("SUN", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");

			rule = result.RefTimeZones.First(x => x.R2_Type == "DLS")
				.RefTimeZoneRules.Where(x => x.R4_StartOrEndRule == "END" && x.R4_FromYear == 2000);
			Assert.AreEqual(1, rule.Count(), $"{zoneName} - Count");
			Assert.AreEqual(2000, rule.First().R4_FromYear, $"{zoneName} - R4_FromYear");
			Assert.AreEqual(2000, rule.First().R4_ToYear, $"{zoneName} - R4_ToYear");
			Assert.AreEqual("MAR", rule.First().R4_DaylightSavingMonth, $"{zoneName} - R4_DaylightSavingMonth");
			Assert.AreEqual(1, rule.First().R4_DaylightSavingDayCount, $"{zoneName} - R4_DaylightSavingDayCount");
			Assert.AreEqual("FRI", rule.First().R4_DaylightSavingDayName, $"{zoneName} - R4_DaylightSavingDayName");
		}

		[Test]
		public void GetTimeZoneRuleFixture()
		{
			var zoneInterval = new ZoneIntervalWtg(new ZoneInterval("Test", Instant.FromDateTimeOffset(new DateTime(1999, 6, 30, 0, 0, 0)), Instant.FromDateTimeOffset(new DateTimeOffset(2000, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0))), Offset.FromHours(0),Offset.FromHours(1)));
			var result = parser.GetTimeZoneRuleTest(zoneInterval);
			Assert.AreEqual(2000, result.First(x => x.R4_StartOrEndRule == "END").R4_FromYear);
			Assert.AreEqual("JAN", result.First(x => x.R4_StartOrEndRule == "END").R4_DaylightSavingMonth);
		}

		[SetUp]
		public void Setup()
		{
			string sourceTzDb = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\tzdb2021e.nzd");
			parser = new TimezoneDatabaseParserTest(sourceTzDb, 2018, true);

			using (var stream = File.OpenRead(sourceTzDb))
			{
				var source = TzdbDateTimeZoneSource.FromStream(stream);
				var provider = new DateTimeZoneCache(source);
				dateTimeZones = provider.GetAllZones().ToList();
			}
		}

		int testYear = 2018;
		TimezoneDatabaseParserTest parser;
		List<DateTimeZone> dateTimeZones;
	}
}
