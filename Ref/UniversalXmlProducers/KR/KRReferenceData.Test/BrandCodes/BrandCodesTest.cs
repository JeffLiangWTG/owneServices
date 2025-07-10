using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class BrandCodesTest
	{
		[Test]
		public void BrandCodes()
		{
			var publicationDate = DateTime.ParseExact(ApplicationConfig.BrandCodesPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			var inputConfigPath = ApplicationConfig.BrandCodesConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"BrandCodes\Output\KRBrandCodes_Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.BrandCodes.Output.KRBrandCodes_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var inputDataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.BrandCodesDataFilePath);
			new BrandCodesParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
