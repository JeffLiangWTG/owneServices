using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class CustomsDepartmentTest
	{
		[Test]
		public void TestCustomsDepartment()
		{
			var customsOfficeDepartmentPublicationDate = DateTime.ParseExact(ApplicationConfig.CustomsOfficeDepartmentPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			var inputConfigPath = ApplicationConfig.CustomsDepartmentConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"Departments\Output\KRCustomsDepartment_Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.Departments.Output.KRCustomsDepartment_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, @"Departments\CustomsDepartmentDataFile_20230906.xlsx");
			new CustomsDepartmentParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, customsOfficeDepartmentPublicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
