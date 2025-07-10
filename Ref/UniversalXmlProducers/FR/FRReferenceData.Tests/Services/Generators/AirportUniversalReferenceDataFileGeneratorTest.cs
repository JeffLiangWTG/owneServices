using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class AirportUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.AirportFileName = "AEROPORT.xml";
			ApplicationConfig.Instance.ZoneFileName = "ZONE.xml";
			var error = Errors.No;
			var generator = new AirportUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 02, 22), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRAirportOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.FRAirportOutputFile));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.AreEqual(expectedFileContent, actualFileContent);
			File.Delete(outputFileName);
		}

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.AirportFileName = @"InconsistentDatesTest\AEROPORT_FORTEST.xml";
			ApplicationConfig.Instance.ZoneFileName = @"InconsistentDatesTest\ZONE_FORTEST.xml";

			var error = Errors.No;
			var generator = new AirportUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 02, 22), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRAirportOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.True(actualFileContent.Contains("<ZZD_Code>AAA</ZZD_Code>"), "AAA has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZD_Code>AAB</ZZD_Code>"), "AAB's start date is greater than end date.");
			File.Delete(outputFileName);
		}
	}
}
