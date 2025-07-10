using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class OtherGovernmentTest
	{
		[Test]
		public void OtherGovernment()
		{
			var publicationDate = DateTime.ParseExact(ApplicationConfig.OtherGovernmentPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			var inputConfigPath = ApplicationConfig.OtherGovernmentConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"OtherGovernment\Output\KROtherGovernment_Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.OtherGovernment.Output.KROtherGovernment_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var inputDataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.OtherGovernmentDataFilePath);
			new OtherGovernmentParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
