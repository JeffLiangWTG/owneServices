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

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.MeursingRate
{
	public class MeursingRatesParserTest
	{
		[Test]
		public void GetCreateTariff()
		{
			var tariffs = MeursingRatesParser.CreateTariff(Data);
			Assert.That(tariffs.Count, Is.EqualTo(504));

			var tariff = tariffs.First();
			var record = Data.First();
			Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo(record.CodeNumber));
			Assert.That(tariff.ZZ1_Description, Is.EqualTo("7000 Meursing Additional Code"));

			var tariff7004 = tariffs.FirstOrDefault(x => x.ZZ1_TariffCode == "7004");
			Assert.IsNotNull(tariff7004, "Tariff 7004 should not be null");

			var rate7004T1 = tariff7004.RefCusRates.FirstOrDefault(r => r.ZZ2_RateFormulaDerivedFrom == "T1");
			var rate7004T2 = tariff7004.RefCusRates.FirstOrDefault(r => r.ZZ2_RateFormulaDerivedFrom == "T2");

			Assert.IsNotNull(rate7004T1, "Rate for T1 should not be null");
			Assert.IsNotNull(rate7004T2, "Rate for T2 should not be null");

			Assert.That(rate7004T1.ZZ2_RateFormula, Does.StartWith("39.91"), "T1 rate formula should start with 39.91 with dot");
			Assert.That(rate7004T2.ZZ2_RateFormula, Does.StartWith("34.11"), "T2 rate formula should start with 34.11 with dot");

			foreach (var t in tariffs)
			{
				foreach (var rate in t.RefCusRates)
				{
					Assert.IsFalse(rate.ZZ2_RateFormula.Contains(","), "Rate formula should not contain commas");
				}
			}
		}

		[Test]
		public void GenerateTariffDataXML()
		{
			string outputFilePath = Path.Combine(OutputFolder, "RefTariffZZ_TR_MEU.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.MeursingRates.TestFiles.Output.RefTariffZZ_TR_MEU.xml");
			MeursingRatesParser.GenerateUniversalReferenceData(outputFilePath);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));

			var parser = new MeursingRatesParserForExceptionTest();
			parser.GenerateUniversalReferenceData(string.Empty);
			Assert.That(parser.ErrorMessage, Does.Contain("GenerateUniversalReferenceData failed"));
			Assert.That(parser.ErrorMessage, Does.Contain("Invalid data."));
		}

		[Test]
		public void GetWriterConfiguration()
		{
			var writerConfig = MeursingRatesParser.GetMeursingRateWriterConfiguration();

			var refType = typeof(RefCusTariff);
			var entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));

			refType = typeof(RefCusRate);
			entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.RefCusApplicabilities))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormula))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RX_NKCurrencyOverride))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_NKRateCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType))));

			refType = typeof(RefCusApplicability);
			entityConfig = writerConfig.GetConfiguration(refType);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_AdditionalCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_StartDate))));
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

			MeursingRatesParser = new MeursingRatesParserForTest();
			DataFilePath = Path.Combine(TempFolder, "MeursingRates.docx");
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.MeursingRates.TestFiles.Input.MeursingRates.docx");
			Data = MeursingRatesLoader.LoadData(DataFilePath);
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
		MeursingRatesParserForTest MeursingRatesParser;
		IEnumerable<MeursingRates> Data;
		string DataFilePath;
	}

	public class MeursingRatesParserForTest : MeursingRatesParser
	{
		public new XmlWriterConfiguration GetMeursingRateWriterConfiguration() => MeursingRatesParser.GetMeursingRateWriterConfiguration();

		public new IEnumerable<RefCusTariff> CreateTariff(IEnumerable<MeursingRates> tariffs) => base.CreateTariff(tariffs);

		protected override DateTime PublicationDateTime => new DateTime(2022, 05, 01, 00, 00, 00);

	}

	public class MeursingRatesParserForExceptionTest : MeursingRatesParser
	{
		protected override IEnumerable<RefCusTariff> CreateTariff(IEnumerable<MeursingRates> tariffs)
		{
			throw new InvalidDataException("Invalid data.");
		}

	}
}
