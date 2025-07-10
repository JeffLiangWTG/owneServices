using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

#pragma warning disable CA1005 // Avoid excessive parameters on generic types
public abstract class CPCParserTests<TItem, TClassMapItem, TConcessionItem, TClassMapConcessionItem> : TestCase
#pragma warning restore CA1005 // Avoid excessive parameters on generic types
	where TItem : IItem
	where TClassMapItem : ClassMap
	where TConcessionItem : IConcessionItem
	where TClassMapConcessionItem : ClassMap
{
	protected abstract Func<IDateTimeProvider, CPCParser<TClassMapItem, TClassMapConcessionItem>> ParserToRun { get; }
	protected abstract string System { get; }
	protected abstract string ExpectedEmptyCPCCodeMessage { get; }
	protected abstract string ExpectedEmptyCPCDescriptionMessage { get; }
	protected abstract string ExpectedEmptyConcessionCPCDescriptionMessage { get; }
	protected abstract string ExpectedEmptyCPCStartDateMessage { get; }
	protected abstract string ExpectedEmptyConcessionCPCStartDateMessage { get; }
	protected abstract string ExpectedInvalidCPCEndDateMessage { get; }
	protected abstract string ExpectedInvalidConcessionCPCEndDateMessage { get; }

	[Test]
	public void CreateRefCusCodeListXMLTest()
	{
		var parser = ParserToRun(DateProvider.Object);
		var cpcFileString = File.ReadAllText(ValidCPCFile);
		var concessionFileString = File.ReadAllText(ValidConcessionCPCFile);
		parser.ConvertRecordsToXMLFile<TItem, TConcessionItem>(cpcFileString, concessionFileString, outputFile);
		var expectedXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Output.RefCusCodeListZZ_ES_{System}_CPC_CODES.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void ByteOrderMarkFailureCPCFile()
	{
		var parser = ParserToRun(DateProvider.Object);
		var cpcFileString = '\uFEFF' + File.ReadAllText(ValidCPCFile);
		Assert.Throws<HeaderValidationException>(() => parser.ConvertRecordsToXMLFile<TItem, TConcessionItem>(cpcFileString, null, string.Empty));
	}

	[Test]
	public void DelimiterChangedCPCFile()
	{
		var delimiterChangedFile = Path.Combine(InputPath, $"{System}_CPC_DELIMITER.csv");
		var parser = ParserToRun(DateProvider.Object);
		Assert.Throws<HeaderValidationException>(() => parser.ConvertRecordsToXMLFile<TItem, TConcessionItem>(delimiterChangedFile, null, string.Empty));
	}

	[Test]
	public void DelimiterChangedConcessionCPCFile()
	{
		var delimiterChangedFile = Path.Combine(InputPath, $"{System}_CPC_DELIMITER.csv");
		var cpcFileString = File.ReadAllText(ValidCPCFile);
		var concessionFileString = File.ReadAllText(delimiterChangedFile);
		var parser = ParserToRun(DateProvider.Object);
		Assert.Throws<HeaderValidationException>(() => parser.ConvertRecordsToXMLFile<TItem, TConcessionItem>(cpcFileString, concessionFileString, string.Empty));
	}

	[Test]
	public void HeaderChangedCPCFile()
	{
		var headerChangedFile = Path.Combine(InputPath, $"{System}_CPC_HEADER.csv");
		var cpcFileString = File.ReadAllText(headerChangedFile);
		var parser = ParserToRun(DateProvider.Object);
		Assert.Throws<HeaderValidationException>(() => parser.ConvertRecordsToXMLFile<TItem, TConcessionItem>(cpcFileString, null, string.Empty));
	}

	[Test]
	public void HeaderChangedConcessionCPCFile()
	{
		var headerChangedFile = Path.Combine(InputPath, $"{System}_CPC_HEADER.csv");
		var cpcFileString = File.ReadAllText(ValidCPCFile);
		var concessionFileString = File.ReadAllText(headerChangedFile);
		var parser = ParserToRun(DateProvider.Object);
		Assert.Throws<HeaderValidationException>(() => parser.ConvertRecordsToXMLFile<TItem, TConcessionItem>(cpcFileString, concessionFileString, string.Empty));
	}

	[Test]
	public void NoRecordsCPCFile()
	{
		AssertCorrectMessage(Path.Combine(InputPath, $"{System}_CPC_EMPTY.csv"), ValidConcessionCPCFile, "Unable to locate any CPC Item records for CPC CSV. File may only contain header record. Input file details");
	}

	[Test]
	public void NoRecordsConcessionCPCFile()
	{
		AssertCorrectMessage(ValidCPCFile, Path.Combine(InputPath, $"{System}_CPC_CONCESSION_EMPTY.csv"), "Unable to locate any CPC Concession records for CPC CSV. File may only contain header record. Input file details");
	}

	[Test]
	public void CodeEmptyInCPCFile()
	{
		AssertCorrectMessage(Path.Combine(InputPath, $"{System}_CPC_INVALID_DATA.csv"), ValidConcessionCPCFile, ExpectedEmptyCPCCodeMessage);
	}

	[Test]
	public void DescriptionEmptyInCPCFile()
	{
		AssertCorrectMessage(Path.Combine(InputPath, $"{System}_CPC_INVALID_DATA.csv"), ValidConcessionCPCFile, ExpectedEmptyCPCDescriptionMessage);
	}

	[Test]
	public void DescriptionEmptyInConcessionCPCFile()
	{
		AssertCorrectMessage(ValidCPCFile, Path.Combine(InputPath, $"{System}_CPC_CONCESSION_INVALID_DATA.csv"), ExpectedEmptyConcessionCPCDescriptionMessage);
	}

	[Test]
	public void StartDateEmptyInCPCFile()
	{
		AssertCorrectMessage(Path.Combine(InputPath, $"{System}_CPC_INVALID_DATA.csv"), ValidConcessionCPCFile, ExpectedEmptyCPCStartDateMessage);
	}

	[Test]
	public void StartDateEmptyInConcessionCPCFile()
	{
		AssertCorrectMessage(ValidCPCFile, Path.Combine(InputPath, $"{System}_CPC_CONCESSION_INVALID_DATA.csv"), ExpectedEmptyConcessionCPCStartDateMessage);
	}

	[Test]
	public void EndDateInvalidInCPCFile()
	{
		AssertCorrectMessage(Path.Combine(InputPath, $"{System}_CPC_INVALID_DATA.csv"), ValidConcessionCPCFile, ExpectedInvalidCPCEndDateMessage);
	}

	[Test]
	public void EndDateInvalidInConcessionCPCFile()
	{
		AssertCorrectMessage(ValidCPCFile, Path.Combine(InputPath, $"{System}_CPC_CONCESSION_INVALID_DATA.csv"), ExpectedInvalidConcessionCPCEndDateMessage);
	}

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, $"RefCusCodeListZZ_ES_{System}_CPC_CODES.xml");
		InputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), $@"Business\CustomsProcedureCodes\TestFiles\Input\{System}");
		ValidCPCFile = Path.Combine(InputPath, $"{System}_CPC.csv");
		ValidConcessionCPCFile = Path.Combine(InputPath, $"{System}_CPC_CONCESSION.csv");
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
	}
	string outputFile;

	protected string InputPath { get; set; }

	protected string ValidCPCFile { get; set; }

	protected string ValidConcessionCPCFile { get; set; }

	protected void AssertCorrectMessage(string inputCPCFile, string inputConcessionCPCFile, string expectedMessage)
	{
		var parser = ParserToRun(DateProvider.Object);
		var cpcFileString = File.ReadAllText(inputCPCFile);
		var concessionFileString = File.ReadAllText(inputConcessionCPCFile);
		var errors = parser.ConvertRecordsToXMLFile<TItem, TConcessionItem>(cpcFileString, concessionFileString, outputFile);
		Assert.That(errors, Does.Contain(expectedMessage));
	}
}
