using System.Linq;
using System;
using NUnit.Framework;
using System.IO;
using System.Xml.Linq;
using System.Globalization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	sealed class ImportTariffPlusDailyProducerFixture
	{
		[Test]
		public void Run()
		{
			TariffCodeExtractor.ClearCache();

			var testDataProvider = new SampleTestDataProvider(
				monthlyFilePublishDate: new DateTime(2024, 06, 30),
				dailyFilePublishDate: new DateTime(2024, 07, 02));

			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderMock(testDataProvider);
			var nomenclaturePlusDailyProducer = new NomenclaturePlusDailyProducerForTest(testDataProvider,
				dailyTariffUpdatesFileProvider,
				now: new DateTime(2024, 07, 02));

			var tariff = nomenclaturePlusDailyProducer.Run(produceXml: false);
			tariff.Tariffs.ToList().ForEach(t => t.ZZ1_TariffCode = EUNUtils.NormalizeTariffCode(t.ZZ1_TariffCode));

			var importTariffProducer = new ImportTariffPlusDailyProducerForTest(new DateTime(2024, 07, 02), dailyTariffUpdatesFileProvider, testDataProvider);
			importTariffProducer.Run(tariff.Tariffs);

			Assert.That(ActualImportEUNRateFile, Does.Exist);

			var actualXml = File.ReadAllText(ActualImportEUNRateFile);
			var expectedXml = File.ReadAllText(Path.Combine(TestHelper.BasePath, "ExpectedImportEUNRatesMergedWithDaily.xml"));
			Assert.That(actualXml, Is.EqualTo(expectedXml));

		}

		[Test]
		public void Run_WI00912940()
		{
			var expectedStartDate = new DateTime(2025, 6, 28);

			TariffCodeExtractor.ClearCache();

			var testDataProvider = new WI00912940TestDataProvider();

			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderMock(testDataProvider);
			var nomenclaturePlusDailyProducer = new NomenclaturePlusDailyProducerForTest(testDataProvider,
				dailyTariffUpdatesFileProvider,
				now: WI00912940TestDataProvider.DailyFilePublishDate);

			var tariff = nomenclaturePlusDailyProducer.Run(produceXml: false);
			tariff.Tariffs.ToList().ForEach(t => t.ZZ1_TariffCode = EUNUtils.NormalizeTariffCode(t.ZZ1_TariffCode));

			var nomenclatureFromDailyUpdate = tariff.Tariffs.Where(x => x.ZZ1_TariffCode.StartsWith("9701210010")).FirstOrDefault();
			Assert.That(nomenclatureFromDailyUpdate, Is.Not.Null);
			Assert.That(nomenclatureFromDailyUpdate?.ZZ1_StartDate, Is.EqualTo(expectedStartDate));

			var importTariffProducer = new ImportTariffPlusDailyProducerForTest(
				WI00912940TestDataProvider.DailyFilePublishDate,
				dailyTariffUpdatesFileProvider,
				testDataProvider);
			importTariffProducer.Run(tariff.Tariffs);

			var expectedImportEUNRateFile = Path.Combine(Path.GetDirectoryName(ApplicationConfig.ImportTariffUXmlFile), "ImportEUNRates_9.xml");

			Assert.That(expectedImportEUNRateFile, Does.Exist);
			var actualXml = File.ReadAllText(expectedImportEUNRateFile);
			var xdoc = XDocument.Parse(actualXml);
			var refCusTariffElements = xdoc.Element("UniversalReferenceData").Descendants("RefCusTariff");
			var tariffFromDailyUpdate = refCusTariffElements.Where(x => x.Element("ZZ1_TariffCode")?.Value?.StartsWith("9701210010") == true).FirstOrDefault();
			Assert.That(tariffFromDailyUpdate, Is.Not.Null);
			Assert.That(tariffFromDailyUpdate.Element("ZZ1_StartDate")?.Value, Is.EqualTo(expectedStartDate.ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture)));
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();
			CleanEnvironment();
		}

		[TearDown]
		public void TearDown()
		{
			CleanEnvironment();
		}

		void CleanEnvironment()
		{

			if (Directory.Exists(ApplicationConfig.DownloadsFolder))
			{
				Directory.Delete(ApplicationConfig.DownloadsFolder, true);
			}

			if (File.Exists(ActualImportEUNRateFile))
			{
				File.Delete(ActualImportEUNRateFile);
			}
		}

		string ActualImportEUNRateFile => Path.Combine(Path.GetDirectoryName(ApplicationConfig.ImportTariffUXmlFile), "ImportEUNRates_0.xml");
	}
}
