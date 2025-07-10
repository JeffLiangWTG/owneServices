using System.Linq;
using System;
using NUnit.Framework;
using System.IO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	sealed class ExportTariffPlusDailyProducerFixture
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
			tariff.Tariffs.ToList().ForEach(t => t.ZZ1_TariffCode = EUNUtils.NormalizeExportTariffCode(t.ZZ1_TariffCode));

			var importTariffProducer = new ExportTariffPlusDailyProducerForTest(new DateTime(2024, 07, 02), dailyTariffUpdatesFileProvider, testDataProvider);
			importTariffProducer.Run(tariff.Tariffs);

			Assert.That(ActualExportEUNRateFile, Does.Exist);
			Assert.That(File.ReadAllText(ActualExportEUNRateFile),
				Is.EqualTo(File.ReadAllText(Path.Combine(TestHelper.BasePath, "ExpectedExportEUNRatesMergedWithDaily.xml"))));
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

			if (File.Exists(ActualExportEUNRateFile))
			{
				File.Delete(ActualExportEUNRateFile);
			}
		}

		string ActualExportEUNRateFile => Path.Combine(Path.GetDirectoryName(ApplicationConfig.ImportTariffUXmlFile), "ExportEUNRates_0.xml");
	}
}
