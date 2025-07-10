using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class ForwarderIDsTest
	{
		[Test]
		public void ForwarderIDs()
		{
			var publicationDate = DateTime.ParseExact(ApplicationConfig.ForwarderIDsPublicationDate, Constants.PublicationDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).Date;
			var inputConfigPath = ApplicationConfig.ForwarderIDsConfigFilePath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"ForwarderIDs\Output\KRForwarderIDs_Result.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.ForwarderIDs.Output.KRForwarderIDs_Result.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var inputDataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.ForwarderIDsDataFilePath);
			new ForwarderIDsParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
