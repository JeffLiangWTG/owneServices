using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class BanderolTariffDataParserTest
	{
		[Test]
		public void GetTariffsWithRateWithRateUOM()
		{
			var tariffs = TariffParser.GetTariffsWithRateWithRateUOM();
			Assert.That(tariffs.Count, Is.EqualTo(37));

			var tariff = tariffs.First();
			var tariffRate = CsvLoader.GetBanderolTariffRates().First(r => r.TariffCode == tariff.ZZ1_TariffCode);
			Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo(tariffRate.TariffCode));
			Assert.That(tariff.ZZ1_Description, Is.EqualTo(tariffRate.Description));
			Assert.That(tariff.ZZ1_ZZI_NKTariffType, Is.EqualTo(Business.Constants.TariffType.Code.ETRBN));
			Assert.That(tariff.ZZ1_StartDate, Is.EqualTo(Business.Constants.MinimumDateTime));
			Assert.That(tariff.ZZ1_EndDate, Is.EqualTo(Business.Constants.MaximumDateTime));
			Assert.That(tariff.ZZ1_IAMUnique, Is.EqualTo(0));

			var refCusRates = tariff.RefCusRates;
			Assert.That(refCusRates.Count, Is.EqualTo(1));

			var refCusRate = refCusRates.First();
			Assert.That(refCusRate.ZZ2_RateFormula, Is.EqualTo(tariffRate.RateFormula));
			Assert.That(refCusRate.ZZ2_StartDate, Is.EqualTo(Business.Constants.MinimumDateTime));
			Assert.That(refCusRate.ZZ2_EndDate, Is.EqualTo(Business.Constants.MaximumDateTime));
			Assert.That(refCusRate.ZZ2_ZY1_NKRateCode, Is.EqualTo(Business.Constants.TariffRateCode.Code._75));
			Assert.That(refCusRate.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo(Business.Constants.TariffRateType.Code.BAN));

			var refCusRateUOMs = refCusRate.RefCusRateUOMs;
			Assert.That(refCusRateUOMs.Count, Is.EqualTo(1));

			var refCusRateUOM = refCusRateUOMs.First();
			Assert.That(refCusRateUOM.ZXG_UOM, Is.EqualTo(tariffRate.UOM));
		}

		[Test]
		public void GetValidTariffCodes()
		{
			var tariffCodes = new string[] { "392210000011", "392210000011", "" };
			var validTariffCodes = TariffParser.GetValidTariffCodes(tariffCodes);
			Assert.That(validTariffCodes.Count, Is.EqualTo(1));
			Assert.That(TariffParser.ErrorMessage, Does.Contain("Duplicated code exists"));
			Assert.That(TariffParser.ErrorMessage, Does.Contain("Empty code exists"));
		}

		[Test]
		public void GetRefTariffWriterConfiguration()
		{
			var config = TariffParser.GetRefTariffWriterConfiguration();
			Assert.That(config, Is.Not.Null);

			var refType = typeof(RefCusRateCode);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCode.ZY1_RateCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCode.ZY1_Description))));

			refType = typeof(RefCusTariff);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_IAMUnique))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusRates))));

			refType = typeof(RefCusRate);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_NKRateCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormula))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.RefCusRateUOMs))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RX_NKCurrencyOverride))));

			refType = typeof(RefCusRateUOM);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateUOM.ZXG_UOM))));
		}

		[Test]
		public void GenerateTariffDataXML()
		{
			string outputFilePath = Path.Combine(OutputFolder, "RefTariffZZ_TR_ETRBN.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Output.RefTariffZZ_TR_ETRBN.xml");
			TariffParser.GenerateTariffUniversalReferenceData(outputFilePath);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));

			var parser = new BanderolTariffDataParserForExceptionTest();
			parser.GenerateTariffUniversalReferenceData(string.Empty);
			Assert.That(parser.ErrorMessage, Does.Contain("GetTariffCodeList failure"));
			Assert.That(parser.ErrorMessage, Does.Contain("GenerateTariffUniversalReferenceData failure"));
		}

		[SetUp]
		public void Setup()
		{
			OutputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(OutputFolder);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);

			TariffParser = new BanderolTariffDataParserForTest();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string OutputFolder;
		string TempFolder;
		BanderolTariffDataParserForTest TariffParser;
	}

	public class BanderolTariffDataParserForTest : BanderolTariffDataParser
	{
		public new IEnumerable<RefCusTariff> GetTariffsWithRateWithRateUOM() => base.GetTariffsWithRateWithRateUOM();

		public new XmlWriterConfiguration GetRefTariffWriterConfiguration() => BanderolTariffDataParser.GetRefTariffWriterConfiguration();

		public new IEnumerable<string> GetValidTariffCodes(IEnumerable<string> tariffCodes) => base.GetValidTariffCodes(tariffCodes);

		protected override DateTime PublicationDateTime => new DateTime(2021, 01, 06, 15, 27, 30);
	}

	public class BanderolTariffDataParserForExceptionTest : BanderolTariffDataParser
	{
		protected override IEnumerable<RefCusTariff> GetTariffsWithRateWithRateUOM()
		{
			throw new InvalidDataException("Invalid data.");
		}
	}
}
