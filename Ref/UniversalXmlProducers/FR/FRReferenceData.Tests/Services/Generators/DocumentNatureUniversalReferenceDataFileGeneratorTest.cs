using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class DocumentNatureUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.DocumentNatureFileName = "NATURE_DOC_PEC.xml";
			var error = Errors.No;
			var generator = new DocumentNatureUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 02, 22), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentNatureOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.FRDocumentNatureOutputFile));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.AreEqual(expectedFileContent, actualFileContent);
			File.Delete(outputFileName);
		}

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.DocumentNatureFileName = @"InconsistentDatesTest\NATURE_DOC_PEC_FORTEST.xml";
			var error = Errors.No;
			var generator = new DocumentNatureUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 02, 22), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentNatureOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.True(actualFileContent.Contains("<ZZD_Code>IM</ZZD_Code>"), "IM has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZD_Code>EX</ZZD_Code>"), "EX's start date is greater than end date.");
			File.Delete(outputFileName);
		}
	}
}
