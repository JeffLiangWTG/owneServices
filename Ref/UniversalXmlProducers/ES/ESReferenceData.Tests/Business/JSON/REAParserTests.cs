using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class REAParserTests : TestCase
{
	protected string MeasuresFile => "measures_can.json";
	protected string MeasuresCodesFile => "measures_can_codes.json";

	string GetUOMJsonContent() => File.ReadAllText(Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\RefCusMap_UOM.json")).Replace("\r\n", "");

	[Test]
	public void TestValidConvertToXMLFile()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new REAParserForTest(DateProvider.Object);

		string measuresJsonContent = File.ReadAllText(Path.Combine(inputPath, MeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(inputPath, MeasuresCodesFile)).Replace("\r\n", "");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, outputFile, outputFileCodeList);

		var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_REA_VALID_CODES.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));

		expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusCodeListZZ_ES_REA_VALID_CODES.xml");
		xml = File.ReadAllText(outputFileCodeList);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestMissingUOMMapRecords()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new REAParserForTest(DateProvider.Object);

		var errors = parser.ConvertToXMLFile(string.Empty, string.Empty, string.Empty, outputFile, outputFileCodeList);

		Assert.That(errors, Does.Contain("Unable to load UOMMap records."));
	}

	[Test]
	public void TestInvalidOrMissingTariffToExport()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new REAParserForTest(DateProvider.Object);

		string measuresJsonContent = File.ReadAllText(Path.Combine(inputPath, "measures_can_invalid_tariff.json")).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(inputPath, MeasuresCodesFile)).Replace("\r\n", "");

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, outputFile, outputFileCodeList);

		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'tariff' or missing REA Codes for the current tariff code. Details:
Tariff: 
REA codes: 1
"));

		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'tariff' or missing REA Codes for the current tariff code. Details:
Tariff: 020120200011
REA codes: 1
"));

		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'tariff' or missing REA Codes for the current tariff code. Details:
Tariff: 02012020
REA codes: 1
"));
	}

	[Test]
	public void TestInvalidColumnsToExport()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new REAParserForTest(DateProvider.Object);

		string measuresJsonContent = File.ReadAllText(Path.Combine(inputPath, "measures_can_invalid_columns.json")).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(inputPath, MeasuresCodesFile)).Replace("\r\n", "");

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, outputFile, outputFileCodeList);

		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'Aid amount' or missing 'REA Code' or 'Description'. Details:
Aid amount for Direct Consumption: 
Aid amount for Transformation: 64 EUR TN
REA code: 
Description: 
"));
		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'Aid amount' or missing 'REA Code' or 'Description'. Details:
Aid amount for Direct Consumption: 64 EUR TN
Aid amount for Transformation: 
REA code: T001
Description: Carne de animales de la especie bovina, fresca o refrigerada. En canales o medias canales. La parte anterior de  una canal o media canal sin deshuesar, con el cuello y la paletilla, siempre que contenga más de diez costillas. De bovinos machos adultos
"));
		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'Aid amount' or missing 'REA Code' or 'Description'. Details:
Aid amount for Direct Consumption: 32 EUR TN
Aid amount for Transformation:  EUR TN
REA code: T002
Description: Carne de animales de la especie bovina, fresca o refrigerada. En canales o medias canales. La parte anterior de  una canal o media canal sin deshuesar con el cuello y la paletilla, siempre que contenga más de diez costillas. Los demás
"));
		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'Aid amount' or missing 'REA Code' or 'Description'. Details:
Aid amount for Direct Consumption:  EUR TN
Aid amount for Transformation: 85 EUR TN
REA code: T003
Description: Carne de animales de la especie bovina, fresca o refrigerada. En canales o medias canales. Los demás. De bovinos machos adultos
"));
		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'Aid amount' or missing 'REA Code' or 'Description'. Details:
Aid amount for Direct Consumption: 32 EUR TN
Aid amount for Transformation: X EUR TN
REA code: T004
Description: Carne de animales de la especie bovina, fresca o refrigerada. En canales o medias canales. Los demás. Los demás
"));
		Assert.That(errorMessage, Does.Contain(@"Unable to import REA Codes as missing or invalid 'Aid amount' or missing 'REA Code' or 'Description'. Details:
Aid amount for Direct Consumption: X EUR TN
Aid amount for Transformation: 85 EUR TN
REA code: T005
Description: Carne de animales de la especie bovina, fresca o refrigerada. Los demás cortes (trozos) sin deshuesar. Cuartos llamados ""compensados"". De bovinos machos adultos
"));
	}

	#region Proposed tests

	[Test]
	public void TestMeasureCodesTypeMinItems()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new REAParserForTest(DateProvider.Object);

		string measuresJsonContent = File.ReadAllText(Path.Combine(inputPath, MeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = "[ ]";

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, outputFile, outputFileCodeList);

		Assert.That(errorMessage, Does.Contain("Error in Measures Codes processing. Details: Item count is 0."));
	}

	[Test]
	public void TestEmptyMeasures()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = new REAParserForTest(DateProvider.Object);

		string measuresJsonContent = File.ReadAllText(Path.Combine(CommonInputPath, "schema_error_measures_es.json")).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(inputPath, MeasuresCodesFile)).Replace("\r\n", "");

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, outputFile, outputFileCodeList);

		Assert.That(errorMessage, Does.Contain("Error in Measures processing. Details: Item count is 0."));
	}

	#endregion

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusTariffZZ_ES_REA_VALID_CODES.xml");
		outputFileCodeList = Path.Combine(FileHelper.OutputFolder, "RefCusCodeListZZ_ES_REA_VALID_CODES.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\REA");
		CommonInputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input");
	}
	string outputFile;
	string outputFileCodeList;
	string inputPath;

	protected string CommonInputPath { get; set; }

	protected override string TestClassName => nameof(REAParserTests);

	class REAParserForTest(IDateTimeProvider dateTimeProvider) : REAParser(dateTimeProvider, "Test")
	{
		readonly string inputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Business\JSON\TestFiles\Input\REA");

		protected override Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, bool isESTariff, bool checkExactTariff)
		{
			var result = tariffCode switch
			{
				"02011000" or "0201100010" => Path.Combine(inputPath, "RefCusTariffQueryES1.json"),
				"02012020" or "0201202010" => Path.Combine(inputPath, "RefCusTariffQueryES2.json"),
				_ => Path.Combine(inputPath, "RefCusTariffQueryEUN.json"),
			};
			return new Uri(result);
		}
	}
}
