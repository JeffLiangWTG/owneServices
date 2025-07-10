using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	sealed class DutyRatesTest
	{
		[Test]
		public void TestDutyRates_2020()
		{
			AssertDutyRates(2020, 01, 01, false);
		}
		[Test]
		public void TestDutyRates_2021()
		{
			AssertDutyRates(2021, 01, 01, false);
		}
		[Test]
		public void TestDutyRates_2022()
		{
			AssertDutyRates(2022, 01, 01, false);
		}
		[Test]
		public void TestDutyRates_2023()
		{
			AssertDutyRates(2023, 01, 01, false);
		}
		[Test]
		public void TestDutyRates_2024()
		{
			AssertDutyRates(2024, 01, 01, false);
		}
		[Test]
		public void TestDutyRates_2025()
		{
			AssertDutyRates(2025, 01, 01, false);
		}

		void AssertDutyRates(int year, int month, int day, bool isXls)
		{
			var publicationDate = new DateTime(year, month, day);
			var fileExtension = isXls ? "xls" : "xlsx";
			var inputConfigPath = ApplicationConfig.DutyRateConfigFileInputPath;
			var preferenceDataFileInputPath = ApplicationConfig.PreferenceDataFileInputPath;
			var preferenceConfigFilePath = ApplicationConfig.PreferenceConfigFileInputPath;
			var outputDirPath = Path.Combine(TestHelper.BaseTestFilePath, @"DutyRates\Output\Y" + year);
			var outputDirInfo = new DirectoryInfo(outputDirPath);
			if (outputDirInfo.Exists)
			{
				foreach (var file in outputDirInfo.GetFiles())
				{
					file.Delete();
				}
			}

			var expectedOutputFiles = new string[]
			{
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_0.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_1.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_2.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_3.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_4.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_5.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_6.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_7.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_8.xml", year)),
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.DutyRate_{0}_9.xml", year))
			};

			var outputFileCommonPath = Path.Combine(outputDirPath, "DutyRate.xml");
			var inputDataPath = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"DutyRates\Input\{0}\DutyRate_{0}DataFileSample.{1}", year, fileExtension));
			new DutyRateParserForTest(inputConfigPath, inputDataPath, preferenceDataFileInputPath, preferenceConfigFilePath).ConvertToXMLFile(outputFileCommonPath, publicationDate);
			for (var i = 0; i < 10; i++)
			{
				Assert.That(File.ReadAllText(outputDirPath + string.Format(CultureInfo.CurrentCulture, @"\DutyRate_{0}_{1}.xml", year, i)), Is.EqualTo(expectedOutputFiles[i]));
			}
		}

		class DutyRateParserForTest : DutyRateParser
		{
			public DutyRateParserForTest(string configFilePath, string dataFilePath, string preferenceDataFileInputPath, string preferenceConfigFilePath) : base(configFilePath, dataFilePath, preferenceDataFileInputPath, preferenceConfigFilePath)
			{
			}

			protected override DutyRateAdditionalDataUpdater AdditionalDataUpdater => new DutyRateAdditionalDataUpdaterForTest(PopulateTradeGroups(GetEntityConfigurationAndWorkbook(ApplicationConfig.PreferenceDataFileInputPath, ApplicationConfig.PreferenceConfigFileInputPath)));
		}
		class DutyRateAdditionalDataUpdaterForTest : DutyRateAdditionalDataUpdater
		{
			protected override ISafeRepository SafeRepository => new RefDataEntityLoaderTest().GetSafeRepository(nomenclatureInputData, tariffInputData);

			List<Tuple<string, string, string, string, DateTime, DateTime>> nomenclatureInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
				new Tuple<string, string, string, string, DateTime, DateTime>("01", "LIVE ANIMALS; ANIMAL PRODUCTS", "01", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01", "Live animals", "01.01", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("03", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", "01.03", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("0101", "Live horses, asses, mules and hinnies", "01.01..01", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01012", "Horses", "01.01..01.2", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("02", "VEGETABLE PRODUCTS", "02", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("07", "EDIBLE VEGETABLES AND CERTAIN ROOTS AND TUBERS", "02.07", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("13", "ARTICLES OF STONE, PLASTER, CEMENT, ASBESTOS, MICA OR SIMILAR MATERIALS; CERAMIC PRODUCTS; GLASS AND GLASSWARE", "13", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("69", "Ceramic products", "13.69", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6902", "Refractory bricks, blocks, tiles and similar refractory ceramic constructional goods, other than those of siliceous fossil meals or similar siliceous earths", "13.69.01.02", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6903", "Other refractory ceramic goods (for example, retorts, crucibles, muffles, nozzles, plugs, supports, cupels, tubes, pipes, sheaths, rods and slide gates), other than those of siliceous fossil meals or of similar siliceous earths", "13.69.01.03", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6904", "Ceramic building bricks, flooring blocks, support or filler tiles and the like", "13.69.02.04", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6905", "Roofing tiles, chimney-pots, cowls, chimney liners, architectural ornaments and other ceramic constructional goods", "13.69.02.05", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("II", "II.- OTHER CERAMIC PRODUCTS", "13.69.02", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("XI", "XI.- PROVITAMINS, VITAMINS AND HORMONES", "06.29.11", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("0503", "[DELETED]", "01.05..03", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),

				new Tuple<string, string, string, string, DateTime, DateTime>("01", "LIVE ANIMALS; ANIMAL PRODUCTS", "01", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01", "Live animals", "01.01", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("03", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", "01.03", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("0101", "Live horses, asses, mules and hinnies", "01.01..01", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01012", "Horses", "01.01..01.2", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("II", "II.- OTHER CERAMIC PRODUCTS", "13.69.02", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("XI", "XI.- PROVITAMINS, VITAMINS AND HORMONES", "06.29.11", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6))
			};

			List<Tuple<string, string, string, string, DateTime, DateTime>> tariffInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
				new Tuple<string, string, string, string, DateTime, DateTime>("010121", "Pure-bred breeding animals", "01.01..01.2.1", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("010121", "Pure-bred breeding animals", "01.01..01.2.1", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("010129", "Other", "01.01..01.2.9", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690210", "Containing by weight, singly or together, more than 50 % of the elements Mg, Ca or Cr, expressed as MgO, CaO or Cr2O3", "13.69.01.02.1", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690220", "Containing by weight more than 50 % of alumina(Al2O3), of silica(SiO2) or of a mixture or compound of these products", "13.69.01.02.2", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690290", "Other", "13.69.01.02.9", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690310", "Containing by weight more than 50 % of free carbon", "13.69.01.03.1", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690320", "Containing by weight more than 50 % of alumina (Al2O3) or of a mixture or compound of alumina and of silica (SiO2)", "13.69.01.03.2", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690390", "Other", "13.69.01.03.9", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6906", "Ceramic pipes, conduits, guttering and pipe fittings", "13.69.02.06", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6906", "Ceramic pipes, conduits, guttering and pipe fittings", "13.69.02.06", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("4204", "[DELETED]", "08.42..04", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00))
			};

			public DutyRateAdditionalDataUpdaterForTest(Dictionary<string, string> tradeGroups) : base(tradeGroups)
			{
			}
		}
	}
}
