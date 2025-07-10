using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class RateTests
	{
		[TestCase("Standard 1P1", "1P1", "", "", RateTypes.Standard, RateTypes.Standard)]
		[TestCase("Standard 2", "2", "AB", "", RateTypes.Standard, "")]
		[TestCase("Standard 2P1", "2P1", "AB,CD", "", RateTypes.Standard, "AB,CD")]
		[TestCase("Standard 2P2", "2P1", "AB", "", RateTypes.Standard, "AB")]
		[TestCase("Standard 2P3", "2P3", "AB", "", RateTypes.Standard, "AB")]
		[TestCase("Standard Null", "12A,12B,13A,13B,13C,13D,13E,15A,15B,17A,3P1,3P2,4P1,4P2,4P3,4P4,4P5,4P6", "", "", RateTypes.Standard, null)]
		[TestCase("SADC", "1P1", "", "", RateTypes.SADC, RateTypes.SADC)]
		[TestCase("EFTA", "1P1", "", "", RateTypes.EFTA, RateTypes.EFTA)]
		[TestCase("EU", "1P1", "", "", RateTypes.EU, "EUTRADE")]
		[TestCase("MERCORSUR By Country", "1P1", "", "AB,CD", RateTypes.MERCOSUR, "AB,CD")]
		[TestCase("MERCORSUR No Country", "1P1", "", "", RateTypes.MERCOSUR, "MERCOSUR")]
		[TestCase("AFCFTA By Country", "1P1", "", "AB,CD", RateTypes.AFCFTA, "AB,CD")]
		[TestCase("AFCFTA No Country", "1P1", "", "", RateTypes.AFCFTA, "AFCFTA")]
		public void GetTradeGroups(string testName, string scheduleTypes, string importedCountries, string rateCountries, string rateType, string expectedTradeGroups)
		{
			var logger = new TestLogger();
			var countryData = new Dictionary<string, string>() { { "AB", "AB" }, { "CD", "CD" } };

			foreach (var scheduleType in scheduleTypes.Split(','))
			{
				var expTradeGroups = expectedTradeGroups != null ? expectedTradeGroups.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries) : new string[] { null };

				var tariff = new TariffData
				{
					Schedule = SARSSchedule.Get(scheduleType, ""),
					ImportCountries = importedCountries.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).ToList()
				};
				var rate = new Rate
				{
					RateType = rateType,
					Countries = rateCountries
				};

				rate.ProcessUpdates(tariff, countryData, logger);

				var actualTradeGroups = rate.GetTradeGroups(tariff).ToArray();

				Assert.That(actualTradeGroups.Length, Is.EqualTo(expTradeGroups.Length), $"{testName}: Unexpected number of trade groups");

				for (int i = 0; i < actualTradeGroups.Length; i++)
				{
					Assert.That(actualTradeGroups[i], Is.EqualTo(expTradeGroups[i]), $"{testName}: Schedule: {scheduleType} Idx: {i}");
				}
			}
		}

		[Test]
		public void PreferenceUpdate()
		{
			var logger = new TestLogger();
			var tariff = new TariffData { Schedule = SARSSchedule.S1P1 };
			var rate = new Rate { RateType = RateTypes.SADC };

			rate.ProcessUpdates(tariff, null, logger);

			var actualPreference = rate.Preference;
			Assert.That(actualPreference, Is.EqualTo(Preferences.PreferentialRate));
		}

		[Test]
		public void SetupCountryCodes()
		{
			var countryData = new Dictionary<string, string>()
			{
				{ "BRAZIL", "BR" },
				{ "EU", "EUTRADE" }
			};
			var logger = new TestLogger();
			var tariff = new TariffData { Schedule = SARSSchedule.S1P1 };
			var rate = new Rate { Countries = "brazil, eu" };

			rate.ProcessUpdates(tariff, countryData, logger);

			Assert.That(rate.CountryCodes, Is.Not.Null);
			Assert.That(rate.CountryCodes.Count, Is.EqualTo(2));
			Assert.That(rate.CountryCodes[0], Is.EqualTo("BR"));
			Assert.That(rate.CountryCodes[1], Is.EqualTo("EUTRADE"));
		}

		[Test]
		public void FormulaAndUOMUpdates()
		{
			var logger = new TestLogger();
			var tariff = new TariffData { Schedule = SARSSchedule.S1P1 };
			var rate = new Rate { RateType = RateTypes.Standard, FormulaCode = "1216", Description = "8C/KG" };

			rate.ProcessUpdates(tariff, null, logger);

			Assert.That(rate.Formula, Is.EqualTo("0.08 * [KG]"));
			Assert.That(rate.UnitOfMeasureConverted, Is.EqualTo("KG"));
		}

		[Test]
		public void IsValid()
		{
			var tariff = new TariffData { Schedule = SARSSchedule.S1P1 };
			var rate = new Rate { RateType = RateTypes.Standard, Formula = "1.23 * VFD" };

			Assert.That(rate.IsValid(tariff), Is.EqualTo(true));

			rate.Formula = "";
			Assert.That(rate.IsValid(tariff), Is.EqualTo(false));

			rate.Formula = "4.56 * VFD";
			tariff.Schedule = SARSSchedule.Get("5P1", "");
			Assert.That(rate.IsValid(tariff), Is.EqualTo(false));

			rate.RateType = RateTypes.SADC;
			Assert.That(rate.IsValid(tariff), Is.EqualTo(false));

			tariff.Schedule = SARSSchedule.S1P1;
			Assert.That(rate.IsValid(tariff), Is.EqualTo(true));
		}

		[TestCase("Invalid Formula Code", "1234", "Free")]
		[TestCase("Regex mismatch", "1001", "Does your dog bite?")]
		public void LoggingWhenNoRateFormulaMatched(string testCase, string formulaCode, string description)
		{
			var logger = new TestLogger();
			var tariff = new TariffData { LineNumber = "32", TariffCode = "123.456", Schedule = SARSSchedule.S1P1 };
			var rate = new Rate { RateType = RateTypes.Standard, FormulaCode = formulaCode, Description = description };

			rate.ProcessUpdates(tariff, null, logger);

			Assert.That(logger.ErrorString, Contains.Substring($"Line: [32] Tariff: [123.456] Formula: [{formulaCode}:{description}] could not be matched"), testCase);
		}

		[Test]
		public void Key()
		{
			var rate1 = new Rate { RateType = RateTypes.Standard };
			Assert.That(rate1.Key, Is.EqualTo(RateTypes.Standard));

			var rate2 = new Rate { CountryCodes = new List<string> { "PY", "UY" } };
			Assert.That(rate2.Key, Is.EqualTo("PY,UY"));
		}
	}
}
