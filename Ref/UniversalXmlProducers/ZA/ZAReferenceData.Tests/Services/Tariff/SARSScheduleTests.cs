using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class SARSScheduleTests
	{
		[TestCase("1P1", "", "", "1P1")]
		[TestCase("12A", "104", "108", "12A")]
		[TestCase("12B", "118", "130", "12B")]
		[TestCase("13A", "147", "147", "13A")]
		[TestCase("13B", "148", "148", "13B")]
		[TestCase("13C", "149", "149", "13C")]
		[TestCase("13D", "151", "151", "13D")]
		[TestCase("13E", "152", "155", "13E")]
		[TestCase("15A", "195", "195", "15A")]
		[TestCase("15B", "197", "197", "15B")]
		[TestCase("16A", "193", "193", "16A")]
		[TestCase("17A", "191", "191", "17A")]
		[TestCase("1P8", "196", "196", "1P8")]

		[TestCase("2", "201", "217", "2P1")]
		[TestCase("2", "250", "260", "2P3")]

		[TestCase("3", "303", "321", "3P1")]
		[TestCase("3", "334", "392", "3P2")]

		[TestCase("4", "403", "414", "4P1")]
		[TestCase("4", "460", "460", "4P2")]
		[TestCase("4", "470", "490", "4P3")]
		[TestCase("4", "495", "496", "4P4")]
		[TestCase("4", "497", "497", "4P5")]
		[TestCase("4", "498", "498", "4P6")]

		[TestCase("5", "501", "521", "5P1")]
		[TestCase("5", "522", "522", "5P2")]
		[TestCase("5", "532", "538", "5P3")]
		[TestCase("5", "540", "541", "5P4")]
		[TestCase("5", "550", "551", "5P5")]
		[TestCase("5", "560", "561", "5P6")]

		[TestCase("61A", "618", "618", "61A")]
		[TestCase("61B", "619", "619", "61B")]
		[TestCase("61C", "620", "620", "61C")]
		[TestCase("61D", "621", "621", "61D")]
		[TestCase("61E", "622", "622", "61E")]
		[TestCase("61F", "623", "623", "61F")]
		[TestCase("61G", "624", "624", "61G")]
		[TestCase("6P2", "630", "635", "6P2")]
		[TestCase("6P3", "670", "672", "6P3")]
		[TestCase("6P4", "680", "681", "6P4")]
		[TestCase("6P5", "690", "691", "6P5")]
		[TestCase("6P6", "692", "692", "6P6")]

		[TestCase("XXX", "001", "002", "0P0")]
		public void Get(string scheduleTypeCode, string itemNumberLow, string itemNumberHigh, string expectedScheduleType)
		{
			Assert.That(SARSSchedule.Get(scheduleTypeCode, itemNumberLow).ScheduleType, Is.EqualTo(expectedScheduleType), $"ScheduleTypeCode: {scheduleTypeCode} ItemNumberLow: {itemNumberLow}");
			Assert.That(SARSSchedule.Get(scheduleTypeCode, itemNumberHigh).ScheduleType, Is.EqualTo(expectedScheduleType), $"ScheduleTypeCode: {scheduleTypeCode} ItemNumberHigh: {itemNumberHigh}");
		}

		[Test]
		public void SchedulesWithImportCountryTradeGroup()
		{
			var expectedSchedules = new string[] { "2P1", "2P2", "2P3" };

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var schedType = schedule.ScheduleType;

				if (expectedSchedules.Contains(schedType))
				{
					Assert.That(schedule.HasImportCountry, Is.EqualTo(true), $"{schedType} should have 'HasImportCountry' = true");
				}
				else
				{
					Assert.That(schedule.HasImportCountry, Is.EqualTo(false), $"{schedType} should have 'HasImportCountry' = false");
				}
			}
		}

		[Test]
		public void SchedulesWithNullTradeGroup()
		{
			var expectedSchedules = new string[] { "12A", "12B", "13A", "13B", "13C", "13D", "13E", "13F", "15A", "15B", "17A", "1P8", "3P1", "3P2", "4P1", "4P2", "4P3", "4P4", "4P5", "4P6" };

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var schedType = schedule.ScheduleType;

				if (expectedSchedules.Contains(schedType))
				{
					Assert.That(schedule.HasNoTradeGroup, Is.EqualTo(true), $"{schedType} should have 'HasNoTradeGroup' = true");
				}
				else
				{
					Assert.That(schedule.HasNoTradeGroup, Is.EqualTo(false), $"{schedType} should have 'HasNoTradeGroup' = false");
				}
			}
		}

		[Test]
		public void DutyFormula()
		{
			var expected = new Dictionary<string, string>
			{
				{ "3P1", "1P1" },
				{ "3P2", "1P1" },
				{ "4P1", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "4P2", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "4P3", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "4P4", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "4P5", "13A+13B+13C+13D+13E" },
				{ "4P6", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "5P1", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "5P2", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "5P3", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "5P4", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "5P5", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "5P6", "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B" },
				{ "61A", "12A+12B" },
				{ "61B", "12A+12B" },
				{ "61C", "12A+12B" },
				{ "61D", "12A+12B" },
				{ "61E", "12A+12B" },
				{ "61F", "12A+12B" },
				{ "61G", "12A+12B" },
				{ "6P2", "12A+12B" },
				{ "6P3", "15A+15B" },
				{ "6P4", "12A+12B" },
				{ "6P5", "12A+12B" },
				{ "6P6", "13F" }
			};

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var schedType = schedule.ScheduleType;

				if (expected.ContainsKey(schedType))
				{
					Assert.That(schedule.DutyFormula, Is.EqualTo(expected[schedType]), $"{schedType} Formula expected");
				}
				else
				{
					Assert.That(schedule.DutyFormula, Is.EqualTo(string.Empty), $"{schedType} No formula expected");
				}
			}
		}

		[Test]
		public void TariffType()
		{
			var expected = new Dictionary<string, string>
			{
				{ "12A", "12A" },
				{ "12B", "12B" },
				{ "13A", "13A" },
				{ "13B", "13B" },
				{ "13C", "13C" },
				{ "13D", "13D" },
				{ "13E", "13E" },
				{ "13F", "13F" },
				{ "15A", "15A" },
				{ "15B", "15B" },
				{ "16A", "16A" },
				{ "17A", "17A" },
				{ "61A", "6P1" },
				{ "61B", "6P1" },
				{ "61C", "6P1" },
				{ "61D", "6P1" },
				{ "61E", "6P1" },
				{ "61F", "6P1" },
				{ "61G", "6P1" },
			};

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var schedType = schedule.ScheduleType;

				if (expected.ContainsKey(schedType))
				{
					Assert.That(schedule.GetTariffType(), Is.EqualTo(expected[schedType]), $"{schedType} Override/formatted Tariff Type");
				}
				else
				{
					Assert.That(schedule.GetTariffType(), Is.EqualTo(schedType), $"{schedType} No tariff type override");
				}
			}
		}

		[Test]
		public void ScheduleType()
		{
			var expected = new Dictionary<string, string>
			{
				{ "1P1", "1P1" },
				{ "12A", "12A" }
			};

			foreach (var kvp in expected)
			{
				var schedule = SARSSchedule.Get(kvp.Key, "");

				Assert.That(schedule.ScheduleType, Is.EqualTo(kvp.Value));
			}
		}

		[Test]
		public void CreateRelationship()
		{
			var expectedSchedules = new string[] { "1P1" };

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var schedType = schedule.ScheduleType;

				if (expectedSchedules.Contains(schedType))
				{
					Assert.That(schedule.CreateRelationship, Is.EqualTo(false), $"{schedType} should have 'CreateRelationship' = false");
				}
				else
				{
					Assert.That(schedule.CreateRelationship, Is.EqualTo(true), $"{schedType} should have 'HasNoTradeGroup' = true");
				}
			}
		}

		[Test]
		public void IsValidForStandardRate()
		{
			var expectedSchedules = new string[] { "5P1", "5P2", "5P3", "5P4", "5P5", "5P6", "61A", "61B", "61C", "61D", "61E", "61F", "61G", "6P2", "6P3", "6P4", "6P5", "6P6" };

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var schedType = schedule.ScheduleType;

				if (expectedSchedules.Contains(schedType))
				{
					Assert.That(schedule.IsValidForStandardRate, Is.EqualTo(false), $"{schedType} should have 'IsValidForStandardRate' = false");
				}
				else
				{
					Assert.That(schedule.IsValidForStandardRate, Is.EqualTo(true), $"{schedType} should have 'IsValidForStandardRate' = true");
				}
			}
		}

		[Test]
		public void RateType()
		{
			var expected = new Dictionary<string, string>
			{
				{ "1P1", "DTY" },
				{ "12A", "EXC" },
				{ "12B", "EX1" },
				{ "13A", "LVY" },
				{ "13B", "LVY" },
				{ "13C", "LVY" },
				{ "13D", "LVY" },
				{ "13E", "LVY" },
				{ "13F", "LVY" },
				{ "15A", "LVY" },
				{ "15B", "LVY" },
				{ "16A", "DTY" },
				{ "17A", "LVY" },
				{ "1P8", "LVY" },
				{ "2P1", "ADD" },
				{ "2P2", "ADD" },
				{ "2P3", "ADD" },
				{ "3P1", "REB" },
				{ "3P2", "REB" },
				{ "4P1", "REB" },
				{ "4P2", "REB" },
				{ "4P3", "REB" },
				{ "4P4", "REB" },
				{ "4P5", "REB" },
				{ "4P6", "REB" },
				{ "5P1", "REF" },
				{ "5P2", "REF" },
				{ "5P3", "REF" },
				{ "5P4", "REF" },
				{ "5P5", "REF" },
				{ "5P6", "REF" },
				{ "61A", "REF" },
				{ "61B", "REF" },
				{ "61C", "REF" },
				{ "61D", "REF" },
				{ "61E", "REF" },
				{ "61F", "REF" },
				{ "61G", "REF" },
				{ "6P2", "REF" },
				{ "6P3", "REF" },
				{ "6P4", "REF" },
				{ "6P5", "REF" },
				{ "6P6", "REF" },
			};

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var schedType = schedule.ScheduleType;

				if (expected.ContainsKey(schedType))
				{
					Assert.That(schedule.RateType, Is.EqualTo(expected[schedType]), $"{schedType} Rate Type");
				}
				else
				{
					Assert.Fail($"Schedule {schedule.ScheduleType} is missing from the RateType test");
				}
			}
		}
	}
}
