using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public abstract class JsonTariffParserTests : TestCase
{
	protected abstract JsonTariffParser ParserToRun { get; }
	protected abstract string MeasuresFile { get; }
	protected abstract string FootnotesFile { get; }
	protected abstract string ExpectedXML { get; }

	protected abstract string TaxOrFeeType { get; }
	protected string GetTaxOrFeeJsonContent() => TaxOrFeeType != null ? File.ReadAllText(Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), $@"Business\JSON\TestFiles\Input\RefCusTaxOrFee_{TaxOrFeeType}.json")).Replace("\r\n", "") : TaxOrFeeType;

	[Test]
	public void TestValidConvertToXMLFile()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresFile)).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(InputPath, FootnotesFile)).Replace("\r\n", "");

		var errors = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile, GetTaxOrFeeJsonContent());

		var expectedXML = TestHelper.ReadManifestResourceContent(ExpectedXML);
		Assert.That(errors, Is.EqualTo(string.Empty));
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestEmptyMeasuresAndFootnotes()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;
		string measuresJsonContent = File.ReadAllText(Path.Combine(CommonInputPath, "schema_error_measures_es.json")).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(CommonInputPath, "schema_error_measures_notes.json")).Replace("\r\n", "");
		var errorMessage = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(@"Measures process schema. Json is null or empty."));

		Assert.That(errorMessage, Does.Contain(EmptyMeasuresAndFootnotesErrorMessage));
	}
	protected abstract string EmptyMeasuresAndFootnotesErrorMessage { get; }

	[Test]
	public void TestInvalidJsonMeasuresAndFootnotes()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;
		string measuresJsonContent = File.ReadAllText(Path.Combine(CommonInputPath, "invalid_measures_es.json")).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(CommonInputPath, "invalid_measures_notes.json")).Replace("\r\n", "");
		var errorMessage = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(@"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'CargoWise.RefDbRepo.ESReferenceData.Business.MeasuresSchema' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly."));

		Assert.That(errorMessage, Does.Contain(InvalidJsonMeasuresAndFootnotesErrorMessage1));

		Assert.That(errorMessage, Does.Contain(InvalidJsonMeasuresAndFootnotesErrorMessage2));
	}
	protected abstract string InvalidJsonMeasuresAndFootnotesErrorMessage1 { get; }
	protected abstract string InvalidJsonMeasuresAndFootnotesErrorMessage2 { get; }

	[Test]
	public void TestInvalidDataMeasuresAndFootnotes()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;
		string measuresJsonContent = File.ReadAllText(Path.Combine(CommonInputPath, "invalid_parse_measures_es.json")).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(InputPath, FootnotesFile)).Replace("\r\n", "");
		var errorMessage = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(InvalidDataMeasuresAndFootnotesErrorMessage));
	}
	protected abstract string InvalidDataMeasuresAndFootnotesErrorMessage { get; }

	[Test]
	public void TestFootnoteNotFoundInFootnotes()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;
		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, "footnote_not_found_measures_es.json")).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(InputPath, FootnotesFile)).Replace("\r\n", "");
		var errorMessage = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(FootnoteNotFoundInFootnotesErrorMessage));
	}
	protected abstract string FootnoteNotFoundInFootnotesErrorMessage { get; }

	[Test]
	public void TestInvalidDuty()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;
		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, InvalidDutyMeasuresFile)).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(InputPath, FootnotesFile)).Replace("\r\n", "");
		var errorMessage = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(InvalidDutyErrorMessage));
	}
	protected abstract string InvalidDutyMeasuresFile { get; }
	protected abstract string InvalidDutyErrorMessage { get; }

	[Test]
	public void TestInvalidTariffAndDuty()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;
		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, InvalidTariffAndDutyFile)).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(InputPath, FootnotesFile)).Replace("\r\n", "");
		var errorMessage = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile, GetTaxOrFeeJsonContent());

		Assert.That(errorMessage, Does.Contain(InvalidTariffAndDutyErrorMessage1));
		Assert.That(errorMessage, Does.Contain(InvalidTariffAndDutyErrorMessage2));
		Assert.That(errorMessage, Does.Contain(InvalidTariffAndDutyErrorMessage3));

		var expectedXML = TestHelper.ReadManifestResourceContent(InvalidTariffAndDutyExpectedXML);
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}
	protected abstract string InvalidTariffAndDutyFile { get; }
	protected abstract string InvalidTariffAndDutyErrorMessage1 { get; }
	protected abstract string InvalidTariffAndDutyErrorMessage2 { get; }
	protected abstract string InvalidTariffAndDutyErrorMessage3 { get; }
	protected abstract string InvalidTariffAndDutyExpectedXML { get; }

	[Test]
	public void TestChangeRegionDateTime()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresFile)).Replace("\r\n", "");
		string footnotesJsonContent = File.ReadAllText(Path.Combine(InputPath, FootnotesFile)).Replace("\r\n", "");

		SetCurrentCultureShortDatePattern("MM-dd-yyyy");
		var errors = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile, GetTaxOrFeeJsonContent());
		Assert.That(errors, Is.EqualTo(string.Empty));

		SetCurrentCultureShortDatePattern("dd-MM-yyyy");
		errors = parser.ConvertToXMLFile(measuresJsonContent, footnotesJsonContent, outputFile);
		Assert.That(errors, Is.EqualTo(string.Empty));

		static void SetCurrentCultureShortDatePattern(string shortDatePattern)
		{
			var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
			culture.DateTimeFormat.ShortDatePattern = shortDatePattern;
			culture.DateTimeFormat.LongTimePattern = "";
			Thread.CurrentThread.CurrentCulture = culture;
		}
	}

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusTariffZZ_ES_VALID.xml");
		CommonInputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input");
	}
	string outputFile;

	protected string InputPath { get; set; }

	protected string CommonInputPath { get; set; }
}
