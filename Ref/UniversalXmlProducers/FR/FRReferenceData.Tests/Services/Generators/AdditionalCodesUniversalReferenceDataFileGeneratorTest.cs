using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class AdditionalCodesUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.AdditionalCodesFileName = "CANA_CACO_RESTIT.xml";
			ApplicationConfig.Instance.VATAdditionalCodesFileName = "CANAAI2.xml";

			var error = Errors.No;
			var generator = new AdditionalCodesUniversalReferencedataFileGeneratorForTest();
			generator.GenerateFiles(new DateTime(2022, 02, 28), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRAdditionalCodesOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.FRAdditionalCodesOutputFile)).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);

			Assert.False(actualFileContent.Contains("<ZZD_Code>1001</ZZD_Code>"));
			Assert.False(actualFileContent.Contains("<ZZD_Code>1002</ZZD_Code>"));
			Assert.False(actualFileContent.Contains("<ZZD_Code>1003</ZZD_Code>"));
			Assert.False(actualFileContent.Contains("<ZZD_Code>1011</ZZD_Code>"));
			Assert.False(actualFileContent.Contains("<ZZD_Code>1012</ZZD_Code>"));
			Assert.False(actualFileContent.Contains("<ZZD_Code>1013</ZZD_Code>"));
			Assert.False(actualFileContent.Contains("<ZZD_Code>1035</ZZD_Code>"));
		}

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.AdditionalCodesFileName = @"InconsistentDatesTest\CANA_CACO_RESTIT_FORTEST.xml";

			var error = Errors.No;
			var generator = new AdditionalCodesUniversalReferencedataFileGeneratorForTest();
			generator.GenerateFiles(new DateTime(2022, 02, 28), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRAdditionalCodesOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			
			Assert.True(actualFileContent.Contains("<ZZD_Code>R043</ZZD_Code>"), "R043 has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZD_Code>R045</ZZD_Code>"), "R045's start date is greater than end date.");
			File.Delete(outputFileName);
		}

		class AdditionalCodesUniversalReferencedataFileGeneratorForTest : AdditionalCodesUniversalReferenceDataFileGenerator
		{
			protected override string[] GetVatCanaList() => new string[] { "1001", "1002", "1003", "1011", "1012", "1013", "1035" };
		}
	}
}
