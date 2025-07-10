using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class OGARegulationCategoryTest
	{
		[TestCase(13, 11, 2023)]
		public void TestOGARegulationCategory(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.OGARegulationCategoryConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"OGARegulationCategory\Output\KROGARegulationCategoryResult.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.OGARegulationCategory.Output.KROGARegulationCategoryResult.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, @"OGARegulationCategory\OGARegulationCategoryDataFile.xlsx");
			new OGARegulationCategoryParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
