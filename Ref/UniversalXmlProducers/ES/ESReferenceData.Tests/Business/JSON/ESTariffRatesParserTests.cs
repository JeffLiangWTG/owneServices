using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class ESTariffRatesParserTests : TestCase
{
	[Test]
	public void TestValidConvertToXMLFile()
	{
		var measuresJsonContent = FileHelper.GetFileContent(inputPath, "measures.jsonl");

		var errors = parser.ConvertToXMLFile(new StringReader(measuresJsonContent), outputFileRates);
		Assert.That(errors, Is.EqualTo((string.Empty, string.Empty)));

		var expectedXMLTariffs = TestHelper.ReadManifestResourceContentUTF8($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_TariffOneRates_VALID_CODES.xml");
		var xmlTariffs = File.ReadAllText(outputFileRates);
		Assert.That(xmlTariffs, Is.EqualTo(expectedXMLTariffs));
	}

	[Test]
	public void TestNoRecords()
	{
		var nomenclaturesJsonContent = "{}";
		var errors = parser.ConvertToXMLFile(new StringReader(nomenclaturesJsonContent), outputFileRates);

		Assert.That(errors.errors, Does.Contain(@"Measures load failed. Details:
Number of records: 0"));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestInvalidJsonl_Array()
	{
		var measuresJsonContent = FileHelper.GetFileContent(inputPath, "invalid_array.jsonl");
		var errors = parser.ConvertToXMLFile(new StringReader(measuresJsonContent), outputFileRates);

		Assert.That(errors.errors, Does.Contain(@"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException"));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestDifferentJsonl_Indentation()
	{
		var measuresJsonContent = FileHelper.GetFileContent(inputPath, "different_indentation_measures.jsonl");
		var errors = parser.ConvertToXMLFile(new StringReader(measuresJsonContent), outputFileRates);

		Assert.That(errors.errors, Does.Contain(string.Empty));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
		var xmlRates = File.ReadAllText(outputFileRates);
		Assert.That(xmlRates, Is.Not.EqualTo(string.Empty));
	}

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFileRates = Path.Combine(FileHelper.OutputFolder, "RefCusTariffZZ_TariffOneRates_VALID_CODES.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\TariffOne");
	}

	[SetUp]
	public void SetUp()
	{
		parser = new ESTariffRatesParserTest(DateProvider.Object);
	}

	ESTariffRatesParserTest parser;
	string outputFileRates;
	string inputPath;

	protected override string TestClassName => nameof(ESTariffRatesParserTests);

	class ESTariffRatesParserTest(IDateTimeProvider dateTimeProvider) : ESTariffRatesParser(dateTimeProvider, "URI")
	{
		readonly string inputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Business\JSON\TestFiles\Input\TariffOne");

		protected override Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, bool isESTariff, bool checkExactTariff)
		{
			var result = tariffCode switch
			{
				"9990000500" or "9990000300" => Path.Combine(inputPath, "RefCusTariffQueryES.json"),
				_ => Path.Combine(inputPath, "RefCusTariffQueryEUN.json"),
			};
			return new Uri(result);
		}
	}
}
