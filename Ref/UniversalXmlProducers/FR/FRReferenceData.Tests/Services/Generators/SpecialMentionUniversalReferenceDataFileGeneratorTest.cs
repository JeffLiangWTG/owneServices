using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class SpecialMentionUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.SpecialMentionFileName = "MENTION_SPECIALE.xml";
			var error = Errors.No;
			var generator = new SpecialMentionUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 02, 22), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRSpecialMentionOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.FRSpecialMentionOutputFile));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.AreEqual(expectedFileContent, actualFileContent);
			File.Delete(outputFileName);
		}

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.SpecialMentionFileName = @"InconsistentDatesTest\MENTION_SPECIALE_FORTEST.xml";

			var error = Errors.No;
			var generator = new SpecialMentionUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 02, 22), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRSpecialMentionOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.True(actualFileContent.Contains("<ZZD_Code>00100</ZZD_Code>"), "00100 has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZD_Code>00200</ZZD_Code>"), "00200's start date is greater than end date.");
			File.Delete(outputFileName);
		}
	}
}
