using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class VATAdditionalCodesUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.VATAdditionalCodesFileName = "CANAAI2.xml";

			var error = Errors.No;
			var generator = new VATAdditionalCodesUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 09, 14), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRVATAdditionalCodesOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.FRVATAdditionalCodesOutputFile));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.AreEqual(expectedFileContent, actualFileContent);
			File.Delete(outputFileName);
		}

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.VATAdditionalCodesFileName = @"InconsistentDatesTest\CANAAI2_FORTEST.xml";

			var error = Errors.No;
			var generator = new VATAdditionalCodesUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 09, 14), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRVATAdditionalCodesOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);

			Assert.True(actualFileContent.Contains("<ZZD_Code>1001</ZZD_Code>"), "1001 has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZD_Code>1002</ZZD_Code>"), "1002's start date is greater than end date.");
			File.Delete(outputFileName);
		}
	}
}
