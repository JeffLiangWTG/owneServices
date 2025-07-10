using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.ExemptionCode
{
	public class ETradeExemptionCodesParserTest
	{
		[Test]
		public void GetCreateTariff()
		{
			var tariffs = ETradeExemptionCodesParser.CreateTariff(Data);
			Assert.That(tariffs.Count, Is.EqualTo(22));

			var tariff = tariffs.First();
			var record = Data.First();
			Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo(record.ExemptionCode));
			Assert.That(tariff.ZZ1_Description, Is.EqualTo(record.ExemptionDescEnglish));
		}

		[Test]
		public void GenerateTariffDataXML()
		{
			string outputFilePath = Path.Combine(OutputFolder, "RefTariffZZ_TR_ETR.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.ETradeExemptionCodes.TestFiles.Output.RefTariffZZ_TR_ETR.xml");
			ETradeExemptionCodesParser.GenerateUniversalReferenceData(outputFilePath);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));

			var parser = new ETradeExemptionCodesParserForExceptionTest();
			parser.GenerateUniversalReferenceData(string.Empty);
			Assert.That(parser.ErrorMessage, Does.Contain("GenerateUniversalReferenceData failed"));
			Assert.That(parser.ErrorMessage, Does.Contain("Invalid data."));
		}

		[Test]
		public void GetWriterConfiguration()
		{
			var writerConfig = ETradeExemptionCodesParser.GetETradeExemptionCodesWriterConfiguration();
			var refType = typeof(RefCusTariffLanguage);
			var entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffLanguage.ZX7_ZX6_NKLanguage))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffLanguage.ZX7_Description))));

			refType = typeof(RefCusTariff);
			entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffLanguages))));

			refType = typeof(RefCusRate);
			entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormula))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormulaDerivedFrom))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.RefCusApplicabilities))));

			refType = typeof(RefCusApplicability);
			entityConfig = writerConfig.GetConfiguration(refType);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping))));
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

			ETradeExemptionCodesParser = new ETradeExemptionCodesParserForTest();
			DataFilePath = Path.Combine(TempFolder, "ETradeExemptionCodes.xlsx");
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.ETradeExemptionCodes.TestFiles.Input.ETradeExemptionCodes.xlsx");
			Data = ETradeExemptionCodesLoader.LoadData(DataFilePath);
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
		ETradeExemptionCodesParserForTest ETradeExemptionCodesParser;
		IEnumerable<ETradeExemptionCodes> Data;
		string DataFilePath;
	}

	public class ETradeExemptionCodesParserForTest : ETradeExemptionCodesParser
	{
		public new XmlWriterConfiguration GetETradeExemptionCodesWriterConfiguration() => ETradeExemptionCodesParser.GetETradeExemptionCodesWriterConfiguration();

		public new IEnumerable<RefCusTariff> CreateTariff(IEnumerable<ETradeExemptionCodes> tariffs) => base.CreateTariff(tariffs);

		protected override DateTime PublicationDateTime => new DateTime(2022, 05, 01, 00, 00, 00);

	}

	public class ETradeExemptionCodesParserForExceptionTest : ETradeExemptionCodesParser
	{
		protected override IEnumerable<RefCusTariff> CreateTariff(IEnumerable<ETradeExemptionCodes> tariffs)
		{
			throw new InvalidDataException("Invalid data.");
		}

	}
}
