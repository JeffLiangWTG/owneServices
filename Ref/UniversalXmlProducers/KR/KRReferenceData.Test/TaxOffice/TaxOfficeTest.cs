using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class TaxOfficeTest
	{
		[Test]
		public void TaxOffice()
		{
			var publicationDate = DateTime.ParseExact(ApplicationConfig.TaxOfficePublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			var inputConfigPath = ApplicationConfig.TaxOfficeConfigFileInputPath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"TaxOffice\Output\KRTaxOffice_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.TaxOffice.Output.KRTaxOffice_Result.xml");

			var inputDataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.TaxOfficeDataFileInputPath);
			new TaxOfficeParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
