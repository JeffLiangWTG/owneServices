using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class AdditionalPaymentReasonsTest
	{
		[TestCase(16, 11, 2023)]
		public void TestAdditionalPaymentReasons(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.AdditionalPaymentReasonsConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"AdditionalPaymentReasons\Output\KRAdditionalPaymentReasonsResult.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.AdditionalPaymentReasons.Output.KRAdditionalPaymentReasonsResult.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, @"AdditionalPaymentReasons\AdditionalPaymentReasonsDataFile.xlsx");
			new AdditonalPaymentReasonsParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
