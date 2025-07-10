using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	public abstract class HsnTariffDTYRatesReaderBaseTest<T> where T : HsnTariffDTYRatesReader
	{
		internal string TempFolder;
		internal string ListFilePath;
		internal string ExplanationFilePath;
		internal ILogger Logger;
		internal T DtyRatesReader;
		string RefCusTradeGroupUpdateFilePath;

		protected abstract string ListFileName { get; }
		protected abstract string ExplanationFileName { get; }
		protected abstract Dictionary<string, (List<HsnTariffDTYRatePreferenceRule>, List<HsnTariffDTYRateFootnoteRule>)> ExpectedRulesByTariffCode { get; }

		protected abstract T CreateReader(string tempFolder, ILogger logger);

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TestContext.AddFormatter<TradeGroup>(TradeGroupFormatter);

			TempFolder = TestHelper.TempPath;
			Directory.CreateDirectory(TempFolder);
			ListFilePath = Path.Combine(TempFolder, ListFileName);
			ExplanationFilePath = Path.Combine(TempFolder, ExplanationFileName);
			RefCusTradeGroupUpdateFilePath = Path.Combine(TempFolder, "RefCusTradeGroupUpdate");
			TestHelper.SimulateDownload(ListFilePath, $"CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.{ListFileName}");
			TestHelper.SimulateDownload(ExplanationFilePath, $"CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.{ExplanationFileName}");
			TestHelper.SimulateDownload(RefCusTradeGroupUpdateFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.RefCusTradeGroupUpdate.json");
			Logger = Mock.Of<ILogger>();
			DtyRatesReader = CreateReader(TempFolder, Logger);
		}

		[Test]
		public void TryGetRule_ReturnRule()
		{
			foreach (var kvp in ExpectedRulesByTariffCode)
			{
				var tariffCode = kvp.Key;
				var (expectedPreferenceRules, expectedFootnoteRules) = kvp.Value;

				var result = DtyRatesReader.TryGetRule(tariffCode, out var rule);
				Assert.That(result, Is.True);

				var actualPreferenceRules = rule.PreferenceRules.ToList();

				for (int i = 0; i < expectedPreferenceRules.Count; i++)
				{
					var expected = expectedPreferenceRules[i];
					var actual = actualPreferenceRules[i];

					Assert.Multiple(() =>
					{
						Assert.That(actual.CodeInExcel, Is.EqualTo(expected.CodeInExcel));
						Assert.That(actual.Preference, Is.EqualTo(expected.Preference));
						Assert.That(actual.Formula, Is.EqualTo(expected.Formula));
						Assert.That(actual.RateType, Is.EqualTo(expected.RateType));
						Assert.That(actual.RateCode, Is.EqualTo(expected.RateCode));
						Assert.That(actual.StartDate, Is.EqualTo(expected.StartDate));
						Assert.That(actual.EndDate, Is.EqualTo(expected.EndDate));
						Assert.That(actual.Footnotes, Is.EqualTo(expected.Footnotes));
					});
				}

				if (expectedFootnoteRules.Any())
				{
					var actualFootnoteRules = rule.FootnoteRules.ToList();

					for (int i = 0; i < expectedFootnoteRules.Count; i++)
					{
						var expected = expectedFootnoteRules[i];
						var actual = actualFootnoteRules[i];

						Assert.Multiple(() =>
						{
							Assert.That(actual.Section, Is.EqualTo(expected.Section));
							Assert.That(actual.FootnoteCode, Is.EqualTo(expected.FootnoteCode));
							Assert.That(actual.AdditionalCode, Is.EqualTo(expected.AdditionalCode));
							Assert.That(actual.Formula, Is.EqualTo(expected.Formula));
							Assert.That(actual.RateType, Is.EqualTo(expected.RateType));
							Assert.That(actual.RateCode, Is.EqualTo(expected.RateCode));
							Assert.That(actual.StartDate, Is.EqualTo(expected.StartDate));
							Assert.That(actual.EndDate, Is.EqualTo(expected.EndDate));
						});
					}
				}
			}
		}

		[Test]
		public void TestTryGetRule_ReturnsFalse()
		{
			var tariffCode = "02011";
			var result = DtyRatesReader.TryGetRule(tariffCode, out var rule);

			Assert.That(result, Is.EqualTo(false));
			Assert.That(rule, Is.EqualTo(null));
		}

		[Test]
		public void TestTryGetOtherCountryGroupCode_WhenTariffCodeExists()
		{
			var tariffCode = "020110000000";
			var result = DtyRatesReader.TryGetOtherCountryGroupCode(tariffCode, out var groupCode);

			Assert.That(result, Is.EqualTo(true));
			Assert.That(groupCode, Is.EqualTo("DTYRATELISTDU"));
		}

		[Test]
		public void TestTryGetOtherCountryGroupCode_WhenTariffCodeDoesNotExist()
		{
			var tariffCode = "02011";
			var result = DtyRatesReader.TryGetOtherCountryGroupCode(tariffCode, out var groupCode);

			Assert.That(result, Is.EqualTo(false));
			Assert.That(groupCode, Is.EqualTo(null));
		}

		[Test]
		public void TestAdditionalTradeGroups()
		{
			using var tempConfig = TestHelper.TemporarilyUseApplicationConfig(nameof(ApplicationConfig.RefDbServiceURI), $"file://{TempFolder}");
			var result = DtyRatesReader.AdditionalTradeGroups;
			Assert.That(result.Count, Is.EqualTo(11));
			Assert.That(result[0].Code, Is.EqualTo("DTYRATELISTDU"));
			Assert.That(result[0].Description, Is.EqualTo("DTYRateList other countries trading group"));
			Assert.That(result[1].Description, Is.EqualTo("Included trading groups for DTYRateList, section: 4.FASIL, footnote: 1"));
			Assert.That(result[6].Description, Is.EqualTo("Included trading groups for DTYRateList, section: 8.FASIL, footnote: 8, additional code: 8.8"));
		}

		[TestCase(true, "EX")]
		[TestCase(false, "IN")]
		public void TestGetTradeGroupCode(bool isExclusion, string suffix)
		{
			var footnoteRule = new HsnTariffDTYRateFootnoteRule(
				"Section1", "Foot001", "Excluding1", "Partners1", "AdditionalCode1", "Formula2", "RateType2", "Rate002", DateTime.Now, DateTime.Now.AddDays(10));

			var expected = "SECTION1FOOT001ADDITIONALCODE1" + suffix;
			var result = HsnTariffDTYRatesReader.GetTradeGroupCode(footnoteRule, isExclusion);

			Assert.That(result, Is.EqualTo(expected));
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		private string TradeGroupFormatter(object val)
		{
			if (val is not TradeGroup tg)
				return string.Empty;

			var tgStr = $"Code: {tg.Code}, Description: {tg.Description}, StartDate: {tg.StartDate:O}, EndDate: {tg.EndDate:O}, Countries({tg.Countries.Count()}):";
			foreach (var tgc in tg.Countries)
			{
				tgStr += $"{Environment.NewLine}\t\t{tgc.Code}, {tgc.Description}, {tgc.StartDate:O}, {tgc.EndDate:O}";
			}

			return tgStr;
		}
	}
}
