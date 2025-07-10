using System;
using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CsvHelper;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class CSVProcessorParserTests : TestCase
{
	[Test]
	[TestCase(Constants.RefCusCodeListTypes.AI44E, Constants.DataSources.AI44E)]
	[TestCase(Constants.RefCusCodeListTypes.DC40A, Constants.DataSources.DC40A)]
	[TestCase(Constants.RefCusCodeListTypes.DC40E, Constants.DataSources.DC40E)]
	[TestCase(Constants.RefCusCodeListTypes.DC40N, Constants.DataSources.DC40N)]
	[TestCase(Constants.RefCusCodeListTypes.DC40T, Constants.DataSources.DC40T)]
	[TestCase(Constants.RefCusCodeListTypes.DC40W, Constants.DataSources.DC40W)]
	[TestCase(Constants.RefCusCodeListTypes.DC40X, Constants.DataSources.DC40X)]
	[TestCase(Constants.RefCusCodeListTypes.DC44H, Constants.DataSources.DC44H)]
	[TestCase(Constants.RefCusCodeListTypes.EXSEC, Constants.DataSources.EXSEC, "EXSEGU")]
	[TestCase(Constants.RefCusCodeListTypes.TD44E, Constants.DataSources.TD44E)]
	[TestCase(Constants.RefCusCodeListTypes.TD44G, Constants.DataSources.TD44G)]
	public void CreateRefCusCodeListXMLTest(string listType, string dataSource, string csvNameSuffix = null)
	{
		csvNameSuffix ??= listType;
		var parser = CSVProcessorFactory.GetCSVParser(listType, DateProvider.Object, "dd-MM-yyyy");
		var inputCSV = $"RefCusCodeList_{csvNameSuffix}.csv";
		var codeListFileString = File.ReadAllText(Path.Combine(inputPath, inputCSV));

		parser.ConvertRecordsToXMLFile(codeListFileString, outputFile, dataSource);

		var expectedXML = TestHelper.ReadManifestResourceContentUTF8($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.CSVProcessor.TestFiles.Output.RefCusCodeListZZ_ES_{listType}.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}


	[Test]
	public void DelimiterChangedCSVProcessorFile()
	{
		var delimiterChangedFile = Path.Combine(inputPath, "RefCusCodeList_DC40A_DELIMITER.csv");
		var parser = new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(DateProvider.Object, "dd/MM/yyyy", Constants.RefCusCodeListTypes.DC40A);
		Assert.Throws<HeaderValidationException>(() => parser.ConvertRecordsToXMLFile(delimiterChangedFile, string.Empty, Constants.DataSources.DC40A));
	}

	[Test]
	public void HeaderChangedCSVProcessorFile()
	{
		var headerChangedFile = Path.Combine(inputPath, "RefCusCodeList_DC40A_HEADER.csv");
		var c40FileString = File.ReadAllText(headerChangedFile);
		var parser = new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(DateProvider.Object, "dd/MM/yyyy", Constants.RefCusCodeListTypes.DC40A);
		Assert.Throws<HeaderValidationException>(() => parser.ConvertRecordsToXMLFile(c40FileString, string.Empty, Constants.DataSources.DC40A));
	}

	[Test]
	public void NoRecordsCSVProcessorFile()
	{
		AssertCorrectMessage(Path.Combine(inputPath, "RefCusCodeList_DC40A_EMPTY.csv"), "Unable to locate any CSVProcessor Item records for CSVProcessor CSV. File may only contain header record. Input file details");
	}

	[Test]
	public void CodeEmptyInCSVProcessorFile()
	{
		AssertCorrectMessage(Path.Combine(inputPath, "RefCusCodeList_DC40A_INVALID_DATA.csv"), ExpectedEmptyCSVProcessorCodeMessage);
	}
	string ExpectedEmptyCSVProcessorCodeMessage => @"Unable to import CSVProcessor record due to empty 'code', 'description', 'start date'. Details:
Code: 
Description: Information sheet INF3
Start Date: 22/11/2019";

	[Test]
	public void DescriptionEmptyInC40File()
	{
		AssertCorrectMessage(Path.Combine(inputPath, "RefCusCodeList_DC40A_INVALID_DATA.csv"), ExpectedEmptyCSVProcessorDescriptionMessage);
	}
	string ExpectedEmptyCSVProcessorDescriptionMessage => @"Unable to import CSVProcessor record due to empty 'code', 'description', 'start date'. Details:
Code: C612
Description: 
Start Date: 06/04/2021";

	[Test]
	public void StartDateEmptyOrInvalidInC40File()
	{
		AssertCorrectMessage(Path.Combine(inputPath, "RefCusCodeList_DC40A_INVALID_DATA.csv"), ExpectedEmptyOrInvalidCSVProcessorStartDateMessage);
	}
	string ExpectedEmptyOrInvalidCSVProcessorStartDateMessage => @"Unable to import CSVProcessor record due to empty 'code', 'description', 'start date'. Details:
Code: C620
Description: Proof of the customs status of Union goods T2LF
Start Date: 
Unable to import CSVProcessor record due to empty 'code', 'description', 'start date'. Details:
Code: C651
Description: AAD - Administrative Accompanying Document (EMCS)
Start Date: 22-11-2019
Unable to import CSVProcessor record due to empty 'code', 'description', 'start date'. Details:
Code: C658
Description: FAD - Fallback e-AD (EMCS)
Start Date: 2019/11/22";

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusCodeListZZ_ES_CSVProcessor.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\CodeLists\CSVProcessor\TestFiles\Input\");
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2022, 7, 29));
	}
	string outputFile;
	string inputPath;

	protected override string TestClassName => nameof(CSVProcessorParserTests);

	protected void AssertCorrectMessage(string inputC40File, string expectedMessage)
	{
		var parser = new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(DateProvider.Object, "dd/MM/yyyy", Constants.RefCusCodeListTypes.DC40A);
		var c40FileString = File.ReadAllText(inputC40File);
		var errors = parser.ConvertRecordsToXMLFile(c40FileString, outputFile, Constants.DataSources.DC40A);
		Assert.That(errors, Does.Contain(expectedMessage));
	}
}
