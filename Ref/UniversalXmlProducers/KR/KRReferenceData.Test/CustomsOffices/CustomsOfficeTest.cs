using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class CustomsOfficeTest
	{
		[TestCase(06, 09, 2023)]
		public void TestCustomsOffice_2023(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.CustomsOfficeConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"CustomsOffices\Output\KRCustomsOffice_2023Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.CustomsOffices.Output.KRCustomsOffice_2023Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, @"CustomsOffices\2023\CustomsOfficeDataFile_20230906.xlsx");
			new CustomsOfficeParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}

		[TestCase(01, 04, 2024)]
		public void TestCustomsOffice_2024(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.CustomsOfficeConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"CustomsOffices\Output\KRCustomsOffice_2024Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.CustomsOffices.Output.KRCustomsOffice_2024Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, @"CustomsOffices\2024\CustomsOfficeDataFile_20240401.xlsx");
			new CustomsOfficeParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
