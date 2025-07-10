using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class TariffCharacteristicNVEParserTest
	{
		[Test]
		public void TestXmlExport()
		{
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariffBRCharacteristic_BR_NVE.xml"))
			using (var expectedStreamNomenclature = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariffBRCharacteristic_Nomenclature_BR_NVE.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve_updated.xml"))
			{
				var publicationDate = new DateTime(2023, 03, 03, 00, 00, 00);
				var parser = new TariffCharacteristicNVEParser("BR Tariff Characteristic NVE", "BR Nomenclature Characteristic NVE");

				parser.ExportToXMLFile(inputStream, TestOutputFilePath, TestOutputFilePathNomenclature, publicationDate);
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				using (var outputStreamNomenclature = new FileStream(TestOutputFilePathNomenclature, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					StreamCompareHelper.CompareStreamContent(expectedStreamNomenclature, outputStreamNomenclature);
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePath);
			File.Delete(TestOutputFilePathNomenclature);
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariffBRCharacteristic_BR_NVE.xml");
		readonly string TestOutputFilePathNomenclature = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariffBRCharacteristic_Nomenclature_BR_NVE.xml");
	}
}
