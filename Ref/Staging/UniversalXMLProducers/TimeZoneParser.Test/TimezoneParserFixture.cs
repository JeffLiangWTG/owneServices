using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser.Test
{
	[TestFixture]
	public class TimezoneParserFixture
	{
		[Test]
		public void TimezoneParserTest()
		{
			var results = new TimezoneDatabaseParser(sourceTzDb, 2018, false).ParseTimezoneDatabase().RefTimeZoneSets.ToList();

			Assert.AreEqual(594, results.Count);

			var result = results.Where(x => x.R3_TimeZoneSetName == "Australia/Adelaide");
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(2, result.First().RefTimeZones.Length);
			Assert.AreEqual(630, result.First().RefTimeZones.First().R2_OffsetMinutesFromUTC);
			Assert.AreEqual(2, result.First().RefTimeZones.First().RefTimeZoneRules.Length);
			Assert.AreEqual(true, result.First().RefTimeZones.Last().RefTimeZoneRules == null);
			Assert.AreEqual(1, result.First().RefTimeZones.First().RefTimeZoneRules.Count(
				x => x.R4_FromYear == 2008
			&& x.R4_DaylightSavingDayCount == 1
			&& x.R4_DaylightSavingMonth == "OCT"
			&& x.R4_DaylightSavingDayName == "SUN"
			&& x.R4_StartOrEndRule == "STA"
			&& x.R4_DaylightSavingDate == new DateTime(2000, 1, 1, 3, 0, 0)));
			Assert.AreEqual(1, result.First().RefTimeZones.First().RefTimeZoneRules.Count(
				x => x.R4_FromYear == 2008
			&& x.R4_DaylightSavingDayCount == 1
			&& x.R4_DaylightSavingMonth == "APR"
			&& x.R4_DaylightSavingDayName == "SUN"
			&& x.R4_StartOrEndRule == "END"
			&& x.R4_DaylightSavingDate == new DateTime(2000, 1, 1, 2, 0, 0)));
		}

		[Test]
		public void TimezoneParserTestFor2022()
		{
			var results = new TimezoneDatabaseParser(sourceTzDb, 2022, false).ParseTimezoneDatabase().RefTimeZoneSets.ToList();
			Assert.AreEqual(594, results.Count);

			var result = results.Where(x => x.R3_TimeZoneSetName == "Africa/Juba");
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(1, result.First().RefTimeZones.Length);
			Assert.AreEqual(120, result.First().RefTimeZones.First().R2_OffsetMinutesFromUTC);
			Assert.AreEqual("CAT", result.First().RefTimeZones.First().R2_CivilianTimeZoneCode);

			result = results.Where(x => x.R3_TimeZoneSetName == "Asia/Amman");
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(2, result.First().RefTimeZones.Length);
			var timeZoneRules = result.First().RefTimeZones.First(x => x.R2_Type == "DLS").RefTimeZoneRules;
			Assert.AreEqual(2, timeZoneRules.Length);
			Assert.AreEqual(1, timeZoneRules.Count(x => x.R4_FromYear == 2022
			&& x.R4_ToYear == 2023
			&& x.R4_DaylightSavingDayCount == 5
			&& x.R4_DaylightSavingMonth == "FEB"
			&& x.R4_DaylightSavingDayName == "FRI"
			&& x.R4_StartOrEndRule == "STA"
			&& x.R4_DaylightSavingDate == new DateTime(2000, 1, 1, 1, 0, 0)));
			Assert.AreEqual(1, timeZoneRules.Count(x => x.R4_FromYear == 2014
			&& x.R4_ToYear == 0
			&& x.R4_DaylightSavingDayCount == 5
			&& x.R4_DaylightSavingMonth == "OCT"
			&& x.R4_DaylightSavingDayName == "FRI"
			&& x.R4_StartOrEndRule == "END"
			&& x.R4_DaylightSavingDate == new DateTime(2000, 1, 1, 0, 0, 0)));
		}

		[Test]
		public void TimezoneParserUsesSourceToCaptureZones()
		{
			var results2021 = new TimezoneDatabaseParser(sourceTzDb, 2022, false).ParseTimezoneDatabase().RefTimeZoneSets.ToList();
			Assert.That(results2021.Count, Is.EqualTo(594));
			var result = results2021.FirstOrDefault(x => x.R3_TimeZoneSetName == "America/Santiago");
			Assert.That(result, Is.Not.Null);
			Assert.That(result.RefTimeZones.Length, Is.GreaterThan(0));
			var timeZone = result.RefTimeZones.FirstOrDefault(x => x.RefTimeZoneRules.Length > 0);
			Assert.That(timeZone, Is.Not.Null);
			var rule = timeZone.RefTimeZoneRules.FirstOrDefault(x => x.R4_StartOrEndRule == "STA");
			Assert.That(rule.R4_FromYear, Is.EqualTo(2020));
			Assert.That(rule.R4_DaylightSavingDayCount, Is.EqualTo(1));
			Assert.That(rule.R4_DaylightSavingDayWeekDate, Is.EqualTo("MON"));
			Assert.That(rule.R4_DaylightSavingMonth, Is.EqualTo("SEP"));
			Assert.That(rule.R4_ToYear, Is.EqualTo(2023));

			var sourceTzDb2022 = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\tzdb2022c.nzd");
			var results2022 = new TimezoneDatabaseParser(sourceTzDb2022, 2022, false).ParseTimezoneDatabase().RefTimeZoneSets.ToList();
			
			result = results2022.FirstOrDefault(x => x.R3_TimeZoneSetName == "America/Santiago");
			Assert.That(result, Is.Not.Null);
			Assert.That(result.RefTimeZones.Length, Is.GreaterThan(0));
			timeZone = result.RefTimeZones.FirstOrDefault(x => x.RefTimeZoneRules.Length > 0);
			Assert.That(timeZone, Is.Not.Null);
			rule = timeZone.RefTimeZoneRules.FirstOrDefault(x => x.R4_StartOrEndRule == "STA");
			Assert.That(rule.R4_DaylightSavingDayWeekDate, Is.EqualTo("MON"));
			Assert.That(rule.R4_DaylightSavingMonth, Is.EqualTo("SEP"));
			Assert.That(rule.R4_DaylightSavingDayCount, Is.EqualTo(2));
			Assert.That(rule.R4_ToYear, Is.EqualTo(2022));
			Assert.That(rule.R4_FromYear, Is.EqualTo(2022));
		}

		[Test]
		public void ParseZoneWithGapsInDaylightSavingsApplicability()
		{
			var cairoRules = parseResult.Where(x => x.R3_TimeZoneSetName == "Africa/Cairo").Select(x => x.RefTimeZones.Where(y => y.R2_Type == "DLS").First().RefTimeZoneRules).First();
			Assert.AreEqual(false, cairoRules.Any(x => x.R4_FromYear >= 2011 && x.R4_ToYear <= 2013));
			Assert.AreEqual(false, cairoRules.Any(x => x.R4_ToYear == 0));
			Assert.AreEqual(false, cairoRules.Any(x => x.R4_ToYear >= 2015));
			Assert.AreEqual(true, cairoRules.Any(x => x.R4_FromYear == 2010 && x.R4_ToYear == 2010));
			Assert.AreEqual(true, cairoRules.Any(x => x.R4_FromYear == 2014 && x.R4_ToYear == 2014));
		}

		[Test]
		public void CompareFullXmlFixture()
		{
			var outputFile = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\TimezoneFull.xml");
			var expectedFile = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\TimezoneFullExpected.xml");

			parser.ExportXML(new DateTime(2022, 03, 10), outputFile);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(outputFile);
			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedFile);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void ValidateNoOverlappingRulesAreProduced()
		{
			var overlappingRules = new StringBuilder();
			Func<RefTimeZoneRule, string> monthDayNameDayCountKeySelector = refTimeZoneRule =>
				string.Format(CultureInfo.InvariantCulture, "Month: {0} Day of week: {1} Day count: {2}",
					monthNumbers[refTimeZoneRule.R4_DaylightSavingMonth], refTimeZoneRule.R4_DaylightSavingDayName, refTimeZoneRule.R4_DaylightSavingDayCount);
			foreach (var zone in parseResult)
			{
				var rules = zone.RefTimeZones?.Where(y => y.R2_Type == "DLS")?.FirstOrDefault()?.RefTimeZoneRules?.ToList();
				if (rules == null)
				{
					continue;
				}
				foreach (var year in Enumerable.Range(2010, 15))
				{
					var startRule = rules.Where(x => x.R4_StartOrEndRule == "STA" && x.R4_FromYear <= year && x.R4_ToYear >= year).ToList();
					var endRule = rules.Where(x => x.R4_StartOrEndRule == "END" && x.R4_FromYear <= year && x.R4_ToYear >= year).ToList();
					if (startRule.Count > 1)
					{
						var checkMonthDayNameDayCount = startRule.GroupBy(monthDayNameDayCountKeySelector);
						foreach (var item in checkMonthDayNameDayCount)
						{
							if (item.Count() > 1)
							{
								overlappingRules.AppendLine(CultureInfo.InvariantCulture, $"Zone: {zone.R3_TimeZoneSetName} Year: {year} Number of rules overlapping: {startRule.Count} Rule Type: STA");
							}
						}
					}
					if (endRule.Count > 1)
					{
						var checkMonthDayNameDayCount = startRule.GroupBy(monthDayNameDayCountKeySelector);
						foreach (var item in checkMonthDayNameDayCount)
						{
							if (item.Count() > 1)
							{
								overlappingRules.AppendLine(CultureInfo.InvariantCulture, $"Zone: {zone.R3_TimeZoneSetName} Year: {year} Number of rules overlapping: {endRule.Count} Rule Type: END");
							}
						}
					}
				}
			}
			Assert.AreEqual(0, overlappingRules.Length, overlappingRules.ToString());
		}

		[SetUp]
		public void Setup()
		{
			sourceTzDb = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\tzdb2021e.nzd");
			parser = new TimezoneDatabaseParser(sourceTzDb, 2022, true);
			parseResult = parser.ParseTimezoneDatabase().RefTimeZoneSets.ToList();
		}

		Dictionary<string, int> monthNumbers = new Dictionary<string, int> {
			{ "JAN", 1 },
			{ "FEB", 2 },
			{ "MAR", 3 },
			{ "APR", 4 },
			{ "MAY", 5 },
			{ "JUN", 6 },
			{ "JUL", 7 },
			{ "AUG", 8 },
			{ "SEP", 9 },
			{ "OCT", 10 },
			{ "NOV", 11 },
			{ "DEC", 12 },
		};

		TimezoneDatabaseParser parser;
		List<RefTimeZoneSet> parseResult;
		string sourceTzDb;
	}
}
