using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public abstract class CodeListParserTests<T, TMap> : TestCase
	where TMap : ClassMap
{
	protected abstract CodeListParser<T, TMap> ParserToRun { get; }
	protected abstract string CodeListID { get; }
	protected abstract string System { get; }
	protected abstract string CodeListName { get; }

	[Test]
	public void CreateRefCusCodeListXMLTest()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var inputFile = Path.Combine(inputPath, $"{CodeListID}.csv");
		ParserToRun.ConvertRecordsToXMLFile(inputFile, outputFile);
		var expectedXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.{System}.TestFiles.Output.RefCusCodeListZZ_ES_{CodeListID}_CODES.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void CSVDelimiterHasChanged()
	{
		var inputFile = Path.Combine(inputPath, $"{CodeListID}_DELIMITER.csv");
		Assert.Throws<HeaderValidationException>(() => ParserToRun.ConvertRecordsToXMLFile(inputFile, string.Empty));
	}

	[Test]
	public void CSVFileHeaderChanged()
	{
		var inputFile = Path.Combine(inputPath, $"{CodeListID}_HEADER.csv");
		Assert.Throws<HeaderValidationException>(() => ParserToRun.ConvertRecordsToXMLFile(inputFile, string.Empty));
	}

	[Test]
	public void NoRecordsToProcess()
	{
		AssertCorrectErrorMessage($"{CodeListID}_EMPTY", $"Unable to locate any records for {CodeListName} CSV. File may only contain header record");
	}

	[Test]
	public void EmptyCodeErrorMessageTest()
	{
		if (this is ITestEmptyCodeErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedEmptyCodeErrorMessage);
		}
	}

	[Test]
	public void EmptyDescriptionErrorMessageTest()
	{
		if (this is ITestEmptyDescriptionErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedEmptyDescriptionErrorMessage);
		}
	}

	[Test]
	public void EmptyStartDateErrorMessageTest()
	{
		if (this is ITestEmptyStartDateErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedEmptyStartDateErrorMessage);
		}
	}

	[Test]
	public void InvalidStartDateErrorMessageTest()
	{
		if (this is ITestInvalidStartDateErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedInvalidStartDateErrorMessage);
		}
	}

	[Test]
	public void EmptyEndDateErrorMessageTest()
	{
		if (this is ITestEmptyEndDateErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedEmptyEndDateErrorMessage);
		}
	}

	[Test]
	public void InvalidEndDateErrorMessageTest()
	{
		if (this is ITestInvalidEndDateErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedInvalidEndDateErrorMessage);
		}
	}

	[Test]
	public void EmptyCharacterIndicationErrorMessageTest()
	{
		if (this is ITestEmptyCharacterIndicationErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedEmptyCharacterIndicationErrorMessage);
		}
	}

	[Test]
	public void EmptySpecialIndicationErrorMessageTest()
	{
		if (this is ITestEmptySpecialIndicationErrorMessage parserTester)
		{
			AssertCorrectErrorMessage($"{CodeListID}_INVALID_DATA", parserTester.ExpectedEmptySpecialIndicationErrorMessage);
		}
	}

	public void AssertCorrectErrorMessage(string inputFileName, string expectedErrorMessage)
	{
		var inputFile = Path.Combine(inputPath, $"{inputFileName}.csv");
		var errors = ParserToRun.ConvertRecordsToXMLFile(inputFile, outputFile);
		Assert.That(errors, Does.Contain(expectedErrorMessage));
	}

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, $"RefCusCodeListZZ_ES_{CodeListID}_CODES.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), $@"Business\CodeLists\{System}\TestFiles\Input");
	}
	string outputFile;
	string inputPath;
}
