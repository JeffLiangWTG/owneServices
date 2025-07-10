using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	class ApplicationConfigTests
	{
		[Test]
		public void TestOutputDirectory()
		{
			Assert.That(ApplicationConfig.OutputDirectory, Is.EqualTo(@"..\..\UXmlFiles"));
		}

		[Test]
		public void TestExchangeRatesStartURL()
		{
			Assert.That("https://unipass.customs.go.kr:38010/ext/rest/trifFxrtInfoQry/retrieveTrifFxrtInfo?crkyCn=j270f135j182f073h020y080m3", Is.EqualTo(ApplicationConfig.ExchangeRatesStartURL));
		}

		[Test]
		public void TestTariffConfigFileInputPath()
		{
			Assert.That(ApplicationConfig.TariffConfigFileInputPath, Is.EqualTo(@"Res\Tariffs\2017\Tariffs_2017Configuration.xml"));
		}

		[Test]
		public void TestTariffDataFileInputPath()
		{
			Assert.That(ApplicationConfig.TariffDataFileInputPath, Is.EqualTo(@"Res\Tariffs\2017\Tariffs_2017DataFile.xlsx"));
		}

		[Test]
		public void TestNonGAReasonExportConfigFileInputPath()
		{
			Assert.That(ApplicationConfig.NonGAReasonExportConfigFileInputPath, Is.EqualTo(@"Res\NonGAReasonTypes\NonGAReasonConfiguration_Export.xml"));
		}

		[Test]
		public void TestNonGAReasonExportDataFileInputPath()
		{
			Assert.That(ApplicationConfig.NonGAReasonExportDataFileInputPath, Is.EqualTo(@"Res\NonGAReasonTypes\2022\NonGAReasonType_2022.xls"));
		}

		[Test]
		public void TestNonGAReasonImportConfigFileInputPath()
		{
			Assert.That(ApplicationConfig.NonGAReasonImportConfigFileInputPath, Is.EqualTo(@"Res\NonGAReasonTypes\NonGAReasonConfiguration_Import.xml"));
		}

		[Test]
		public void TestNonGAReasonImportDataFileInputPath()
		{
			Assert.That(ApplicationConfig.NonGAReasonImportDataFileInputPath, Is.EqualTo(@"Res\NonGAReasonTypes\2022\NonGAReasonType_2022.xls"));
		}

		[Test]
		public void TestPreferenceConfigFileInputPath()
		{
			Assert.That(ApplicationConfig.PreferenceConfigFileInputPath, Is.EqualTo(@"Res\RefCusPreferenceConfiguration_2023.xml"));
		}

		[Test]
		public void TestPreferenceDataFileInputPath()
		{
			Assert.That(ApplicationConfig.PreferenceDataFileInputPath, Is.EqualTo(@"Res\DutyRateClassificationCodes.xlsx"));
		}

		[Test]
		public void TestPreferenceMapConfigFileInputPath()
		{
			Assert.That(ApplicationConfig.PreferenceRefCusMapConfigFileInputPath, Is.EqualTo(@"Res\PreferenceRefCusMapConfiguration.xml"));
		}

		[Test]
		public void TestPreferenceRefCusMapDataFileInputPath()
		{
			Assert.That(ApplicationConfig.PreferenceRefCusMapDataFileInputPath, Is.EqualTo(@"Res\DutyRateClassificationCodes.xlsx"));
		}

		[Test]
		public void TestDutyRateDataFileInputPath()
		{
			Assert.That(ApplicationConfig.DutyRateDataFileInputPath, Is.EqualTo(@"Res\DutyRates\2020\DutyRate_2020DataFile_0.xlsx"));
		}

		[Test]
		public void TestExportFTAType()
		{
			Assert.That(ApplicationConfig.ExportFTATypeConfigFilePath, Is.EqualTo(@"Res\ExportFTACodesConfiguration.xml"));
			Assert.That(ApplicationConfig.ExportFTATypeDataFilePath, Is.EqualTo(@"Res\ExportFTATypeDataFile.xlsx"));
		}

		[Test]
		public void TestOGAImport()
		{
			Assert.That(ApplicationConfig.OGAImportConfigFileInputPath, Is.EqualTo(@"Res\OGA\2018\OGA_2018Configuration_Import.xml"));
			Assert.That(ApplicationConfig.OGAImportDataFileInputPath, Is.EqualTo(@"Res\OGA\2018\OGA_2018DataFile.xls"));
		}

		[Test]
		public void TestOGAExport()
		{
			Assert.That(ApplicationConfig.OGAExportConfigFileInputPath, Is.EqualTo(@"Res\OGA\2018\OGA_2018Configuration_Export.xml"));
			Assert.That(ApplicationConfig.OGAExportDataFileInputPath, Is.EqualTo(@"Res\OGA\2018\OGA_2018DataFile.xls"));
		}

		[Test]
		public void TestTradeGroup()
		{
			Assert.That(ApplicationConfig.TradeGroupConfigFilePath, Is.EqualTo(@"Res\TradeGroupAndCountryEntityConfiguration.xml"));
			Assert.That(ApplicationConfig.TradeGroupDataFilePath, Is.EqualTo(@"Res\TradeGroupAndCountry.xlsx"));
		}

		[Test]
		public void TestWCONomenclature()
		{
			Assert.That(ApplicationConfig.WCONomenclatureDataFileInputPathPDF, Is.EqualTo(@"Res\WCOCopiedNomenclatures\2017Edition\HSK_2021.pdf"));
			Assert.That(ApplicationConfig.WCONomenclatureDataFileInputPathXLS, Is.EqualTo(@"Res\WCOCopiedNomenclatures\2017Edition\HSK_2021ForWCONomenclature.xlsx"));
			Assert.That(ApplicationConfig.WCONomenclatureConfigFileInputPath, Is.EqualTo(@"Res\WCOCopiedNomenclatures\2017Edition\WCONomenclatureEntityConfiguration_2021.xml"));
		}

		[Test]
		public void TestKRNomenclature()
		{
			Assert.That(ApplicationConfig.KRNomenclatureConfigFileInputPath, Is.EqualTo(@"Res\Nomenclatures\2017\KRNomenclatures_2017Configuration.xml"));
			Assert.That(ApplicationConfig.KRNomenclatureDataFileInputPath, Is.EqualTo(@"Res\Tariffs\2017\Tariffs_2017DataFile.xlsx"));
		}

		[Test]
		public void TestDomesticTaxRates()
		{
			Assert.That(ApplicationConfig.DomesticTaxRatesConfigFileInputPath, Is.EqualTo(@"Res\DomesticTaxRates\DomesticTaxRatesConfiguration.xml"));
			Assert.That(ApplicationConfig.DomesticTaxRatesDataFileInputPath, Is.EqualTo(@"Res\DomesticTaxRates\DomesticTaxRates2020_1.xlsx"));
		}

		[Test]
		public void TestETC()
		{
			Assert.That("http://localhost:37016/odata/", Is.EqualTo(ApplicationConfig.SafeDataUpdateUri));
			Assert.That("2021/01/01", Is.EqualTo(ApplicationConfig.WCOPublicationDate));
			Assert.That("2017/01/01", Is.EqualTo(ApplicationConfig.HSCodePublicationDate));
			Assert.That("2018/01/01", Is.EqualTo(ApplicationConfig.OGAPublicationDate));
			Assert.That("2022/12/01", Is.EqualTo(ApplicationConfig.NonGAReasonPublicationDate));
		}
	}
}
