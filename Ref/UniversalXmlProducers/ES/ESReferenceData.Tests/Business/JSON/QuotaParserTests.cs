using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class QuotaParserTests : TestCase
{
	const string MeasuresFile = "measures_can.json";
	const string FootnotesFile = "measures_can_notes.json";

	string GetUOMJsonContent() => File.ReadAllText(Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\RefCusMap_UOM.json")).Replace("\r\n", "");

	[Test]
	public void TestValidConvertToXMLFile()
	{
		string measuresJsonContent = GetFileContent(inputPath, MeasuresFile);
		string footnotesJsonContent = GetFileContent(inputPath, FootnotesFile);

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);

		var expectedXML = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_Quota_VALID_CODES.xml");
		Assert.That(errors, Is.EqualTo(string.Empty));
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestMissingUOMMapRecords()
	{
		var errors = parser.ConvertToXMLFile(string.Empty, string.Empty, string.Empty, outputFile);

		Assert.That(errors, Does.Contain("Unable to load UOMMap records."));
	}

	[Test]
	public void TestDuplicatedTariff()
	{
		string measuresJsonContent = GetFileContent(inputPath, "measures_can_duplicated_tariff.json");
		string footnotesJsonContent = GetFileContent(inputPath, FootnotesFile);

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);
		Assert.That(errors, Does.Contain("Cannot process duplicated info for the code: 0303000000"));
	}

	[Test]
	public void TestUnableToLocateRecords()
	{
		string measuresJsonContent = GetFileContent(inputPath, "measures_can_no_quota_records.json");
		string footnotesJsonContent = GetFileContent(inputPath, FootnotesFile);

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);
		Assert.That(errors, Does.Contain("Unable to locate Quota records."));
	}

	[Test]
	public void TestEmptyMeasuresAndFootnotes()
	{
		string measuresJsonContent = GetFileContent(commonInputPath, "schema_error_measures_es.json");
		string footnotesJsonContent = GetFileContent(commonInputPath, "schema_error_measures_notes.json");
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Measures process schema. Json is null or empty.
Footnotes process schema. Json is null or empty.
Unable to locate Quota records."));
	}

	[Test]
	public void TestInvalidTariffAndDuty()
	{
		string measuresJsonContent = GetFileContent(inputPath, "measures_can_invalid_data.json");
		string footnotesJsonContent = GetFileContent(inputPath, FootnotesFile);

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Unable to import Quota as missing or invalid 'tariff' or 'duty'. Details:
Tariff: 030300000"));

		Assert.That(errors, Does.Contain(@"Unable to import Quota as missing or invalid 'tariff' or 'duty'. Details:
Tariff: "));

		Assert.That(errors, Does.Contain(@"Unable to import Quota as missing or invalid 'tariff' or 'duty'. Details:
Tariff: 0303000000
Duty: 2x%"));
	}

	[Test]
	public void TestInvalidJsonMeasuresAndFootnotes()
	{
		string measuresJsonContent = GetFileContent(commonInputPath, "invalid_measures_es.json");
		string footnotesJsonContent = GetFileContent(commonInputPath, "invalid_measures_notes.json");
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'CargoWise.RefDbRepo.ESReferenceData.Business.MeasuresSchema' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly."));

		Assert.That(errors, Does.Contain(@"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'CargoWise.RefDbRepo.ESReferenceData.Business.FootnoteSchema' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly."));

		Assert.That(errors, Does.Contain("Unable to parse any record for Measures or Footnotes."));

		Assert.That(errors, Does.Contain("Unable to locate Quota records"));
	}

	[Test]
	public void TestInvalidDataMeasuresAndFootnotes()
	{
		string measuresJsonContent = GetFileContent(commonInputPath, "invalid_parse_measures_es.json");
		string footnotesJsonContent = GetFileContent(inputPath, FootnotesFile);
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Unable to parse any record for Measures or Footnotes.
Unable to locate Quota records"));
	}

	[Test]
	public void TestFootnoteNotFoundInFootnotes()
	{
		string measuresJsonContent = GetFileContent(inputPath, "footnote_not_found_measures_es.json");
		string footnotesJsonContent = GetFileContent(inputPath, FootnotesFile);
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, footnotesJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Unable to import Quota as footnote code not found in the footnotes json. Details:
Footnote: test"));
	}

	string GetFileContent(string inputPath, string fileName) => File.ReadAllText(Path.Combine(inputPath, fileName)).Replace("\r\n", "");

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusTariffZZ_ES_Quota_VALID_CODES.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\Quota");
		commonInputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input");

		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 3, 25));
		parser = new QuotaParserForTest(DateProvider.Object, "Test");
	}
	string outputFile;
	string inputPath;
	string commonInputPath;
	QuotaParserForTest parser;

	protected override string TestClassName => nameof(QuotaParserTests);

	class QuotaParserForTest(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : QuotaParser(dateTimeProvider, refDbServiceURI)
	{
		readonly string inputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Business\JSON\TestFiles\Input\Quota");

		protected override Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, bool isESTariff, bool checkExactTariff)
		{
			var result = tariffCode switch
			{
				"0303" or "0303201000" => Path.Combine(inputPath, "RefCusTariffQueryES1.json"),
				"0304" or "0304101010" => Path.Combine(inputPath, "RefCusTariffQueryES2.json"),
				"0306" => Path.Combine(inputPath, "RefCusTariffQueryES3.json"),
				_ => Path.Combine(inputPath, "RefCusTariffQueryEUN.json"),
			};
			return new Uri(result);
		}
	}
}
