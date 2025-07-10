using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class RefCusPreferenceTest
	{
		[TestCase(2, 2, 2023)]
		public void RefCusPreferences(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigFile = ApplicationConfig.PreferenceConfigFileInputPath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"RefCusTrades\Output\KRRefCusPreference_Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.RefCusTrades.Output.KRRefCusPreference_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, @"DutyRateClassificationCodes.xlsx");
			new RefCusPreferenceParser(inputConfigFile, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
