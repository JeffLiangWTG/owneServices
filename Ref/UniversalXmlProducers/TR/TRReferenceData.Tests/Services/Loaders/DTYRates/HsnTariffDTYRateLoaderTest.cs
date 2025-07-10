using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	[TestFixture]
	public class HsnTariffDTYRateLoaderTest
	{
		[Test]
		public void TestConstructor_WhenLoggerIsProvided()
		{
			var mockLogger = new Mock<ILogger>();

			var loader = new HsnTariffDTYRateLoader(mockLogger.Object);

			var assignedLogger = GetPrivateLogger(loader);
			Assert.That(mockLogger.Object, Is.EqualTo(assignedLogger));
		}

		[Test]
		public void TestConstructor_WhenLoggerIsNull()
		{
			var loader = new HsnTariffDTYRateLoader(null);

			var assignedLogger = GetPrivateLogger(loader);
			Assert.IsNotNull(assignedLogger, "Expected factory-created logger when input logger is null");
			Assert.IsInstanceOf<ILogger<HsnTariffDTYRateLoader>>(assignedLogger, "Expected logger to be of type ILogger<HsnTariffDTYRateLoader>");
		}

		[TestCase("700312991000")]
		[TestCase("700420100000")]
		public void TestGetRule_ReturnsExpectedRule(string tariffCode)
		{
			var result = loaderMock.Object.GetRule(tariffCode);

			Assert.That(result.TariffCode, Is.EqualTo(tariffCode));
			Assert.That(result.FootnoteRules, Is.Empty);
			Assert.That(result.PreferenceRules.Count, Is.EqualTo(12));

			var preferenceRule = result.PreferenceRules.ToList()[0];

			Assert.Multiple(() =>
			{
				Assert.That(preferenceRule.CodeInExcel, Is.EqualTo("AB"));
				Assert.That(preferenceRule.Preference, Is.EqualTo("AT"));
				Assert.That(preferenceRule.Formula, Is.EqualTo("0"));
				Assert.That(preferenceRule.RateType, Is.EqualTo("DTY"));
				Assert.That(preferenceRule.RateCode, Is.EqualTo("10"));
				Assert.That(preferenceRule.StartDate, Is.EqualTo(DtyRateConstants.DefaultStartDate));
				Assert.That(preferenceRule.EndDate, Is.EqualTo(DtyRateConstants.DefaultEndDate));
			});
		}

		[Test]
		public void TestGetRule__ReturnsNull_WhenNoRuleFound()
		{
			var result = loaderMock.Object.GetRule("123");

			Assert.That(result, Is.EqualTo(null));
		}

		[TestCase("700312991000")]
		[TestCase("700420100000")]
		public void TestGetOtherCountriesTradingGroupCode_ReturnsExpectedGroupCode(string tariffCode)
		{
			var expectedGroupCode = "DTYRATELISTDU";

			var result = loaderMock.Object.GetOtherCountriesTradingGroupCode(tariffCode);

			Assert.That(result, Is.EqualTo(expectedGroupCode));
		}

		[TestCase(true, "EX")]
		[TestCase(false, "IN")]
		public void TestGetTradingGroupCode(bool isExclusion, string suffix)
		{
			var footnoteRule = new HsnTariffDTYRateFootnoteRule(
					"Section1", "Foot001", "Excluding1", "Partners1", "AdditionalCode1", "Formula2", "RateType2", "Rate002", DateTime.Now, DateTime.Now.AddDays(10));

			var expected = "SECTION1FOOT001ADDITIONALCODE1" + suffix;
			var result = HsnTariffDTYRateLoader.GetTradingGroupCode(footnoteRule, isExclusion);

			Assert.That(result, Is.EqualTo(expected));
		}


		[Test]
		public void TestGetUnusedRules_ReturnsUnusedTariffCodes()
		{
			var inputTariffs = new List<RefCusTariff>
			{
				new RefCusTariff()
			};

			var expectedTariffCodes = new[] { "020110000000", "021099900000", "040110100000", "040140100011", "040790900000", "041090000019", "080111000000", "080112000000", "080211100000", "080211900000", "081210000000", "081050000000", "081290250000", "081290300000", "090111000000", "090112000000", "090121000000", "090122000000", "090190100000", "120921000000", "150710100000", "150710900000", "151321100019", "700312991000", "700420100000" };

			var result = loaderMock.Object.GetUnusedRules(inputTariffs).ToList();

			Assert.That(result.Count, Is.EqualTo(25));
			Assert.That(result, Is.EqualTo(expectedTariffCodes));
		}

		[Test]
		public void TestLoadAdditionalTradeGroups()
		{
			using var tempConfig = TestHelper.TemporarilyUseApplicationConfig(nameof(ApplicationConfig.RefDbServiceURI), $"file://{TempFolder}");
			var tradeGroups = loaderMock.Object.LoadAdditionalTradeGroups().ToArray();
			var expected = ExpectedTradeGroups();

			Assert.That(tradeGroups, Is.EquivalentTo(expected));
		}

		static ILogger GetPrivateLogger(HsnTariffDTYRateLoader loader)
		{
			var field = typeof(HsnTariffDTYRateLoader).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return (ILogger)field.GetValue(loader);
		}

		static List<TradeGroup> ExpectedTradeGroups()
		{
			var expectedTradeGroups = new List<TradeGroup>
			{
				new TradeGroup
				{
					Code = "DTYRATELISTDU",
					Description = "DTYRateList other countries trading group",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries =  new []
					{
						GetTradeGroupCountry("AG"),
						GetTradeGroupCountry("AI"),
					}
				},
				new TradeGroup
				{
					Code = "4FASIL1IN",
					Description = "Included trading groups for DTYRateList, section: 4.FASIL, footnote: 1",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("IR"),
					},
				},
				new TradeGroup
				{
					Code = "4FASIL2IN",
					Description = "Included trading groups for DTYRateList, section: 4.FASIL, footnote: 2",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("IR"),
					},
				},
				new TradeGroup
				{
					Code = "8FASIL1IN",
					Description = "Included trading groups for DTYRateList, section: 8.FASIL, footnote: 1",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("CL"),
					},
				},
				new TradeGroup
				{
					Code = "8FASIL2EX",
					Description = "Excluded trading groups for DTYRateList, section: 8.FASIL, footnote: 2",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("BA"),
					},
				},
				new TradeGroup
				{
					Code = "8FASIL7EX",
					Description = "Excluded trading groups for DTYRateList, section: 8.FASIL, footnote: 7",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("BA"),
						GetTradeGroupCountry("KR"),
						GetTradeGroupCountry("SG"),
						GetTradeGroupCountry("VE"),
					},
				},
				new TradeGroup
				{
					Code = "8FASIL888IN",
					Description = "Included trading groups for DTYRateList, section: 8.FASIL, footnote: 8, additional code: 8.8",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("GE"),
					},
				},
				new TradeGroup
				{
					Code = "9FASIL2IN",
					Description = "Included trading groups for DTYRateList, section: 9.FASIL, footnote: 2",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("CL"),
					},
				},
				new TradeGroup
				{
					Code = "12FASIL8128EX",
					Description = "Excluded trading groups for DTYRateList, section: 12.FASIL, footnote: 8, additional code: 12.8",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("GE"),
						GetTradeGroupCountry("BA"),
						GetTradeGroupCountry("KR"),
						GetTradeGroupCountry("VE"),
					},
				},
				new TradeGroup
				{
					Code = "12FASIL2IN",
					Description = "Included trading groups for DTYRateList, section: 12.FASIL, footnote: 2",
					StartDate = DtyRateConstants.DefaultStartDate,
					EndDate = DtyRateConstants.DefaultEndDate,
					Countries = new []
					{
						GetTradeGroupCountry("CL"),
					},
				},
				new TradeGroup
				{
					Code = "701IN",
					Description = "Included trading groups for DTYRateList, section: 70, footnote: (1)",
					StartDate = new DateTime(2023, 04, 14),
					EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					Countries = new []
					{
						GetTradeGroupCountry("IR", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("DZ", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("SD", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("BY", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("RS", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("TM", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("BA", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("AD", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("BS", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("BT", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("KM", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("CW", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("GQ", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("ET", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("IQ", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("LB", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("LY", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("ST", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("SO", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("SS", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("SY", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("TL", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("TM", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
						GetTradeGroupCountry("UZ", new DateTime(2023, 04, 14), new DateTime(2079, 06, 06, 23, 59, 0)),
					},
				},
			};
			return expectedTradeGroups;
		}

		static TradeGroupCountry GetTradeGroupCountry(string countryCode, DateTime? startDate = null, DateTime? endDate = null)
		{
			var country = RefCusTradeGroupHelper.GetCountry(countryCode);

			return new TradeGroupCountry
			{
				Code = country.ZZB_RN_NKTradeGroupCountryCode,
				Description = country.ZZB_Description,
				StartDate = startDate ?? DtyRateConstants.DefaultStartDate,
				EndDate = endDate ?? DtyRateConstants.DefaultEndDate,
			};
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TestContext.AddFormatter<TradeGroup>(TradeGroupFormatter);

			TempFolder = TestHelper.TempPath;
			Directory.CreateDirectory(TempFolder);
			ListFilePath = Path.Combine(TempFolder, "DTYRateList.xlsx");
			ExplanationFilePath = Path.Combine(TempFolder, "DTYRateExplanation.xlsx");
			RefCusTradeGroupUpdateFilePath = Path.Combine(TempFolder, "RefCusTradeGroupUpdate");
			TestHelper.SimulateDownload(ListFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.DTYRateList.xlsx");
			TestHelper.SimulateDownload(ExplanationFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.DTYRateExplanation.xlsx");
			TestHelper.SimulateDownload(RefCusTradeGroupUpdateFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.RefCusTradeGroupUpdate.json");
			Logger = Mock.Of<ILogger>();
			DtyRatesReader = new HsnTariffDTYRatesReaderForTest(TempFolder, Logger);
			loaderMock = new Mock<HsnTariffDTYRateLoader>(Mock.Of<ILogger>()) { CallBase = true };
			loaderMock.Setup(l => l.GetAllReaders())
				.Returns(() => new[] { DtyRatesReader });
		}

		string TradeGroupFormatter(object val)
		{
			if (!(val is TradeGroup tg))
			{
				return string.Empty;
			}

			var tgStr =
				$"Code: {tg.Code}, Description: {tg.Description}, StartDate: {tg.StartDate:O}, EndDate: {tg.EndDate:O}, Countries({tg.Countries.Count()}):";
			foreach (var tgc in tg.Countries)
			{
				tgStr += $"{Environment.NewLine}\t\t{tgc.Code}, {tgc.Description}, {tgc.StartDate:O}, {tgc.EndDate:O}";
			}

			return tgStr;
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		string ListFilePath;
		string ExplanationFilePath;
		string RefCusTradeGroupUpdateFilePath;
		ILogger Logger;
		HsnTariffDTYRatesReaderForTest DtyRatesReader;
		Mock<HsnTariffDTYRateLoader> loaderMock;
	}

	class HsnTariffDTYRatesReaderForTest : HsnTariffDTYRatesReader
	{
		public HsnTariffDTYRatesReaderForTest(string inputPath, ILogger logger) : base(inputPath, logger) { }
		public HsnTariffDTYRatesReaderForTest(ILogger logger) : base(logger) { }

		protected override string ListFileName => "DTYRateList.xlsx";
		protected override string ExplanationFileName => "DTYRateExplanation.xlsx";

		protected override HashSet<string> ListHeaders => new()
			{
				DtyRateConstants.TariffCode,
				DtyRateConstants.Footnote,
				"AB, BK", "GÜR", "B-HER", "G.KORE", "MLZ", "SNG", "KOS", "VNZ", "TPS-OIC", "D-8", "DÜ"
			};

		protected override HashSet<string> SearchHeaders => new() { DtyRateConstants.TariffCode };
		protected override HashSet<string> PreferenceHeaders => new() { "AB, BK", "GÜR", "B-HER", "G.KORE", "MLZ", "SNG", "KOS", "VNZ", "TPS-OIC", "D-8", "DÜ" };
		protected override string TariffCodeHeader => DtyRateConstants.TariffCode;
		protected override string FootnoteHeader => DtyRateConstants.Footnote;
	}
}
