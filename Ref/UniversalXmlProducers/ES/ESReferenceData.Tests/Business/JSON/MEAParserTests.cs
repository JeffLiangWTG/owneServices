using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class MEAParserTests : TestCase
{
	const string MeasuresFile = "measures_can.json";

	string GetUOMJsonContent() => File.ReadAllText(Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\RefCusMap_UOM.json")).Replace("\r\n", "");

	[Test]
	public void TestValidConvertToXMLFile()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 2, 26));
		var parser = new MEAParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(inputPath, MeasuresFile);

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_MEA_VALID_CODES.xml");
		Assert.That(errors, Is.EqualTo(string.Empty));
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestMissingUOMMapRecords()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new MEAParserForTest(DateProvider.Object, "Test");

		var errors = parser.ConvertToXMLFile(string.Empty, string.Empty, outputFile);

		Assert.That(errors, Does.Contain("Unable to load UOMMap records."));
	}

	[Test]
	public void TestDuplicatedTariff()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 2, 26));
		var parser = new MEAParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(inputPath, "measures_can_duplicated_tariff.json");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);
		Assert.That(errors, Does.Contain("Cannot process duplicated info for the code: 4011200000"));
	}

	[Test]
	public void TestUnableToLocateRecords()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 2, 26));
		var parser = new MEAParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(inputPath, "measures_can_no_mea_records.json");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);
		Assert.That(errors, Does.Contain("Unable to locate any records for MEA Codes."));
	}

	[Test]
	public void TestEmptyMeasures()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new MEAParserForTest(DateProvider.Object, "Test");

		string measuresJsonContent = GetFileContent(commonInputPath, "schema_error_measures_es.json");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Error in Measures processing. Details: Item count is 0.
Unable to locate any records for MEA Codes."));
	}

	[Test]
	public void TestInvalidTariff()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new MEAParserForTest(DateProvider.Object, "Test");
		string measuresJsonContent = GetFileContent(inputPath, "measures_can_invalid_data.json");
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Unable to import MEA as missing or invalid 'tariff' or 'duty'. Details:
Tariff: 401120000"));

		Assert.That(errors, Does.Contain(@"Unable to import MEA as missing or invalid 'tariff' or 'duty'. Details:
Tariff: "));

		Assert.That(errors, Does.Contain(@"Unable to import MEA as missing or invalid 'tariff' or 'duty'. Details:
Tariff: 4011200000
Duty: 2x%"));
	}

	[Test]
	public void TestInvalidCondition()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new MEAParserForTest(DateProvider.Object, "Test");
		string measuresJsonContent = GetFileContent(inputPath, "measures_can_invalid_data.json");
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, outputFile);

		Assert.That(errors, Does.Contain(@"Unable to import MEA 'condition' as invalid format. Details:
Tariff: 8415000000
Condition:   B cert: N990 27):"));

		Assert.That(errors, Does.Contain(@"Unable to import MEA 'condition', description not found. Details:
Tariff: 8418308000
Condition:   4 cert: N990 (27):
Condition ID: 4"));
	}

	string GetFileContent(string inputPath, string fileName) => File.ReadAllText(Path.Combine(inputPath, fileName)).Replace("\r\n", "");

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusTariffZZ_ES_MEA_VALID_CODES.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\MEA");
		commonInputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input");
	}
	string outputFile;
	string inputPath;
	string commonInputPath;

	protected override string TestClassName => nameof(MEAParserTests);

	class MEAParserForTest(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : MEAParser(dateTimeProvider, refDbServiceURI)
	{
		readonly string inputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Business\JSON\TestFiles\Input\MEA");

		protected override Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, bool isESTariff, bool checkExactTariff)
		{
			var result = tariffCode switch
			{
				"401120" or "4011201000" => Path.Combine(inputPath, "RefCusTariffQueryES1.json"),
				"8415" or "8415101010" => Path.Combine(inputPath, "RefCusTariffQueryES2.json"),
				"84183080" => Path.Combine(inputPath, "RefCusTariffQueryES3.json"),
				_ => Path.Combine(inputPath, "RefCusTariffQueryEUN.json"),
			};
			return new Uri(result);
		}
	}
}
