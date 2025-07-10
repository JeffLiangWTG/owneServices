using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.Globalization;
using System.IO;
using System.Reflection;
using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	sealed class ExpressDeliveryServiceIDTest
	{
		[TestCase(2023, 11, 07)]
		public void TestExpressDeliveryServiceID(int year, int month, int day)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.ExpressDeliveryServiceIDConfigFileInputPath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"ExpressDeliveryServiceIDs\Output\ExpressDeliveryServiceID_Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.ExpressDeliveryServiceIDs.Output.ExpressDeliveryServiceID_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var inputDataPath = ApplicationConfig.ExpressDeliveryServiceIDDataFileInputPath;
			new ExpressDeliveryServiceIDParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
