using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class ExportFTATypeTest
	{
		[TestCase(1, 12, 2022)]
		public void ExportFTAType(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.ExportFTATypeConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"ExportFTAType\Output\KRExportFTAType_Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.ExportFTAType.Output.KRExportFTAType_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, "ExportFTATypeDataFile.xlsx");
			new ExportFTATypeParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
