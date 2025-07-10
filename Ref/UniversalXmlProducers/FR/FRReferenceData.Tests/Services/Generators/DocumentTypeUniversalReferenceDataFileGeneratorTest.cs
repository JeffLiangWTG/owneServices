using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class DocumentTypeUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestAttributes()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var error = Errors.No;
			var generator = new DocumentTypeUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 04, 13), ref error);
			var result = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentTypeOutputFile));
			Assert.That(result.Contains(@"<RefCusCodeList>
    <ZZD_Code>2700</ZZD_Code>
    <ZZD_Description>certificat AGREX DST dématérialisé</ZZD_Description>
    <ZZD_EndDate>2079-06-06T23:59:00</ZZD_EndDate>
    <ZZD_StartDate>2015-01-19T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>DC44I</ZZD_ZZK_NKCodeType>
    <RefCusCodeListAttribute>
      <ZZE_Value>Y</ZZE_Value>
      <ZZE_ZXE_NKName>IsD48</ZZE_ZXE_NKName>
    </RefCusCodeListAttribute>
    <RefCusCodeListAttribute>
      <ZZE_Value>Y</ZZE_Value>
      <ZZE_ZXE_NKName>PERMIT</ZZE_ZXE_NKName>
    </RefCusCodeListAttribute>
  </RefCusCodeList>"));
			Assert.That(result.Contains(@"<RefCusCodeList>
    <ZZD_Code>2800</ZZD_Code>
    <ZZD_Description>dérogation à l'autorisation d'exportation de poudres et substances explosives - AEPE</ZZD_Description>
    <ZZD_EndDate>2020-05-31T00:00:00</ZZD_EndDate>
    <ZZD_StartDate>2005-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>DC44E</ZZD_ZZK_NKCodeType>
    <RefCusCodeListAttribute>
      <ZZE_Value>Y</ZZE_Value>
      <ZZE_ZXE_NKName>IsDTP</ZZE_ZXE_NKName>
    </RefCusCodeListAttribute>"));
			Assert.That(result.Contains(@"<RefCusCodeList>
    <ZZD_Code>2503</ZZD_Code>
    <ZZD_Description>Déclaration d'origine sur facture</ZZD_Description>
    <ZZD_EndDate>2014-05-13T00:00:00</ZZD_EndDate>
    <ZZD_StartDate>2005-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>DC44I</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>"));

			Assert.That(result.Contains(@"<RefCusCodeList>
    <ZZD_Code>L100</ZZD_Code>
    <ZZD_Description>Licence d'importation ""substances réglementées"" (ozone), délivrée par la Commission</ZZD_Description>
    <ZZD_EndDate>2079-06-06T23:59:00</ZZD_EndDate>
    <ZZD_StartDate>2000-10-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>DC44I</ZZD_ZZK_NKCodeType>
    <RefCusCodeListAttribute>
      <ZZE_Value>Y</ZZE_Value>
      <ZZE_ZXE_NKName>IsODS</ZZE_ZXE_NKName>
    </RefCusCodeListAttribute>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>L100</ZZD_Code>
    <ZZD_Description>Licence d'importation ""substances réglementées"" (ozone), délivrée par la Commission</ZZD_Description>
    <ZZD_EndDate>2079-06-06T23:59:00</ZZD_EndDate>
    <ZZD_StartDate>2000-10-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>DC44E</ZZD_ZZK_NKCodeType>
    <RefCusCodeListAttribute>
      <ZZE_Value>Y</ZZE_Value>
      <ZZE_ZXE_NKName>IsODS</ZZE_ZXE_NKName>
    </RefCusCodeListAttribute>
  </RefCusCodeList>"));

			Assert.That(result.Contains(@"<RefCusCodeList>
    <ZZD_Code>E013</ZZD_Code>
    <ZZD_Description>Licence d'exportation ""substances réglementées"" (ozone), délivrée par la Commission.</ZZD_Description>
    <ZZD_EndDate>2079-06-06T23:59:00</ZZD_EndDate>
    <ZZD_StartDate>2010-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>DC44I</ZZD_ZZK_NKCodeType>
    <RefCusCodeListAttribute>
      <ZZE_Value>Y</ZZE_Value>
      <ZZE_ZXE_NKName>IsODS</ZZE_ZXE_NKName>
    </RefCusCodeListAttribute>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>E013</ZZD_Code>
    <ZZD_Description>Licence d'exportation ""substances réglementées"" (ozone), délivrée par la Commission.</ZZD_Description>
    <ZZD_EndDate>2079-06-06T23:59:00</ZZD_EndDate>
    <ZZD_StartDate>2010-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>DC44E</ZZD_ZZK_NKCodeType>
    <RefCusCodeListAttribute>
      <ZZE_Value>Y</ZZE_Value>
      <ZZE_ZXE_NKName>IsODS</ZZE_ZXE_NKName>
    </RefCusCodeListAttribute>
  </RefCusCodeList>"));
		}

		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.DocumentTypeFileName = "TYPE_DOCUMENT_JOINT.xml";
			var error = Errors.No;
			var generator = new DocumentTypeUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 04, 13), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentTypeOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.FRDocumentTypeOutputFile)).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);
			File.Delete(outputFileName);
		}

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.DocumentTypeFileName = @"InconsistentDatesTest\TYPE_DOCUMENT_JOINT_FORTEST.xml";
			var error = Errors.No;
			var generator = new DocumentTypeUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2022, 04, 13), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentTypeOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.True(actualFileContent.Contains("<ZZD_Code>N270</ZZD_Code>"), "N270 has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZD_Code>N380</ZZD_Code>"), "N380's start date is greater than end date.");
			File.Delete(outputFileName);
		}

		[Test]
		public void TestGeneratorGatherTheGoodEntryLookingAtStartDate()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.DocumentTypeFileName = @"InconsistentDatesTest\TYPE_DOCUMENT_JOINT_FORTEST.xml";
			var error = Errors.No;
			var generator = new DocumentTypeUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2015, 06, 11), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentTypeOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.True(actualFileContent.Contains("<ZZD_Code>Y054</ZZD_Code>"), "Y054 is present in the list.");
			Assert.True(actualFileContent.Contains("<ZZD_StartDate>2015"), "Y054 date is 2015.");
			Assert.False(actualFileContent.Contains("<ZZD_StartDate>2025"), "Y054 date is not 2025.");
			Assert.False(actualFileContent.Contains("<ZZD_StartDate>2013"), "Y054 date is not 2013.");
			File.Delete(outputFileName);

			Assert.False(File.Exists(outputFileName));
			generator.GenerateFiles(new DateTime(2025, 06, 11), ref error);

			outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentTypeOutputFile);
			Assert.True(File.Exists(outputFileName));
			actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.True(actualFileContent.Contains("<ZZD_Code>Y054</ZZD_Code>"), "Y054 is present in the list.");
			Assert.True(actualFileContent.Contains("<ZZD_StartDate>2025"), "Y054 date is 2025.");
			Assert.False(actualFileContent.Contains("<ZZD_StartDate>2015"), "Y054 date is not 2015.");
			Assert.False(actualFileContent.Contains("<ZZD_StartDate>2013"), "Y054 date is not 2013.");

			File.Delete(outputFileName);

			Assert.False(File.Exists(outputFileName));
			generator.GenerateFiles(new DateTime(2015, 06, 01), ref error);

			outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRDocumentTypeOutputFile);
			Assert.True(File.Exists(outputFileName));
			actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.True(actualFileContent.Contains("<ZZD_Code>Y054</ZZD_Code>"), "Y054 is present in the list.");
			Assert.True(actualFileContent.Contains("<ZZD_StartDate>2015"), "Y054 date is 2015.");
			Assert.False(actualFileContent.Contains("<ZZD_StartDate>2025"), "Y054 date is not 2025.");
			Assert.False(actualFileContent.Contains("<ZZD_StartDate>2013"), "Y054 date is not 2013.");
		}
	}
}
