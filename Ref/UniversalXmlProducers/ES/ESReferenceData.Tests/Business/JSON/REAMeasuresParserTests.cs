using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class REAMeasuresParserTests : TestCase
{
	const string MeasuresFile = "canrea_measures_can.json";

	string GetUOMJsonContent() => File.ReadAllText(Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\RefCusMap_UOM.json")).Replace("\r\n", "");

	[Test]
	public void TestValidConvertToXMLFile()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 2, 26));
		var parser = new REAMeasuresParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(inputPath, MeasuresFile);

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_REA_Measures_VALID.xml");
		Assert.That(errors, Is.EqualTo(string.Empty));
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestMissingUOMMapRecords()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new REAMeasuresParserForTest(DateProvider.Object, "Test");

		var errors = parser.ConvertToXMLFile(string.Empty, string.Empty, outputFile);

		Assert.That(errors, Does.Contain("Unable to load UOMMap records."));
	}

	[Test]
	public void TestDuplicatedTariff()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 2, 26));
		var parser = new REAMeasuresParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(inputPath, "measures_can_duplicated_tariff.json");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);
		Assert.That(errors, Does.Contain("Cannot process duplicated info for the code: 0201100000"));
	}

	[Test]
	public void TestUnableToLocateRecords()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 2, 26));
		var parser = new REAMeasuresParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(inputPath, "measures_can_no_rea_measure_records.json");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);
		Assert.That(errors, Does.Contain("Unable to locate any records for REA Measure Codes."));
	}

	[Test]
	public void TestEmptyMeasures()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 2, 26));
		var parser = new REAMeasuresParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(commonInputPath, "schema_error_measures_es.json");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		Assert.That(errors, Does.Contain("Error in Measures processing. Details: Item count is 0."));
	}

	[Test]
	public void TestInvalidTariff()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 2, 26));
		var parser = new REAMeasuresParserForTest(DateProvider.Object, "Test");
		string measuresJsonContent = GetFileContent(inputPath, "measures_can_invalid_data.json");
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Unable to import REA Measure as missing or invalid 'tariff' or 'duty'. Details:
Tariff: 401120000"));

		Assert.That(errors, Does.Contain(@"Unable to import REA Measure as missing or invalid 'tariff' or 'duty'. Details:
Tariff: "));

		Assert.That(errors, Does.Contain(@"Unable to import REA Measure as missing or invalid 'tariff' or 'duty'. Details:
Tariff: 0201100000
Duty: 2x%"));
	}

	[Test]
	public void TestInvalidCondition()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 2, 26));
		var parser = new REAMeasuresParserForTest(DateProvider.Object, "Test");
		string measuresJsonContent = GetFileContent(inputPath, "measures_can_invalid_data.json");
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Unable to import REA Measure 'condition' as invalid format. Details:
Tariff: 0201202000
Condition:   B cert: N990 27):"));

		Assert.That(errors, Does.Contain(@"Unable to import REA Measure 'condition', description not found. Details:
Tariff: 8418308000
Condition:   4 cert: N990 (27):
Condition ID: 4"));
	}

	string GetFileContent(string inputPath, string fileName) => File.ReadAllText(Path.Combine(inputPath, fileName)).Replace("\r\n", "");

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusTariffZZ_ES_REA_Measures_VALID.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\REA");
		commonInputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input");
	}
	string outputFile;
	string inputPath;
	string commonInputPath;

	protected override string TestClassName => nameof(REAMeasuresParserTests);

	class REAMeasuresParserForTest(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : REAMeasuresParser(dateTimeProvider, refDbServiceURI)
	{
		readonly string inputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Business\JSON\TestFiles\Input\REA");

		protected override Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, bool isESTariff, bool checkExactTariff)
		{
			var result = tariffCode switch
			{
				"020110" or "0201100010" => Path.Combine(inputPath, "RefCusTariffQueryES1.json"),
				"02012020" or "84183080" or "0201202010" => Path.Combine(inputPath, "RefCusTariffQueryES2.json"),
				"24012020" => Path.Combine(inputPath, "RefCusTariffQueryES3.json"),
				_ => Path.Combine(inputPath, "RefCusTariffQueryEUN.json"),
			};
			return new Uri(result);
		}
	}
}
