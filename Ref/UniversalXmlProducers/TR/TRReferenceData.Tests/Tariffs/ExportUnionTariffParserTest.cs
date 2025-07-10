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

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class ExportUnionTariffParserTest
	{
		[Test]
		public void GetTariffs()
		{
			var tariffs = Parser.GetTariffs(Data);
			Assert.That(tariffs.Count, Is.EqualTo(12157));

			var tariff = tariffs.First();
			var record = Data.First();
			Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo(record.Code));
			Assert.That(tariff.ZZ1_Description, Is.EqualTo(record.Description));
		}

		[Test]
		public void GetWriterConfiguration()
		{
			var writerConfig = Parser.GetWriterConfiguration();
			var refType = typeof(RefCusTariff);
			var entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
		}

		[Test]
		public void GenerateXML()
		{
			var outputFolder = GetOutputFolder();
			string outputFilePath = Path.Combine(outputFolder, "RefTariffZZ_TR_TREUA.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Output.RefTariffZZ_TR_TREUA.xml");
			Parser.GenerateUniversalReferenceData(outputFilePath);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));

			Parser.GenerateUniversalReferenceData(string.Empty);
			Assert.That(Parser.ErrorMessage, Does.Contain("GenerateUniversalReferenceData failed. Exception: Value cannot be null. (Parameter 'path')"));
		}

		string GetOutputFolder()
		{
			var outputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(outputFolder);
			return outputFolder;
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			Parser = new ExportUnionTariffParserForTest();
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			DataFilePath = Path.Combine(TempFolder, "TR Export Union Export Tariff Additional Codes.xlsx");
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.TR Export Union Export Tariff Additional Codes.xlsx");
			Data = ExportUnionTariffLoader.LoadData(DataFilePath);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		ExportUnionTariffParserForTest Parser;
		IEnumerable<CodeDescriptionPair> Data;
		string TempFolder;
		string DataFilePath;
	}

	public class ExportUnionTariffParserForTest : ExportUnionTariffParser
	{
		public new XmlWriterConfiguration GetWriterConfiguration() => base.GetWriterConfiguration();

		public new IEnumerable<RefCusTariff> GetTariffs(IEnumerable<CodeDescriptionPair> data) => base.GetTariffs(data);

		protected override DateTime PublicationDateTime => new DateTime(2021, 07, 22, 15, 27, 30);
	}
}
