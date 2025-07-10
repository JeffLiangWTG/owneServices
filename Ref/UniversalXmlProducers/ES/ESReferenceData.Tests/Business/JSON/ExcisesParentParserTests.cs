using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public abstract class ExcisesParentParserTests : TestCase
{
	protected abstract ExcisesParentParser ParserToRun { get; }
	protected abstract string MeasuresFile { get; }
	protected abstract string MeasuresCodesFile { get; }
	protected abstract string ExciseBaseTypeFile { get; }
	protected abstract string ExpectedXML { get; }

	protected string GetUOMJsonContent() => File.ReadAllText(Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\RefCusMap_UOM.json")).Replace("\r\n", "");

	[Test]
	public void TestValidConvertToXMLFile()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresCodesFile)).Replace("\r\n", "");
		string exciseJsonContent = File.ReadAllText(Path.Combine(InputPath, ExciseBaseTypeFile)).Replace("\r\n", "");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		var expectedXML = TestHelper.ReadManifestResourceContent(ExpectedXML);
		Assert.That(errors, Is.EqualTo(string.Empty));
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestCheckCodeDescriptionAreValid()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, InvalidMeasuresDescFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(InputPath, InvalidMeasuresCodesFile)).Replace("\r\n", "");
		string exciseJsonContent = File.ReadAllText(Path.Combine(InputPath, ExciseBaseTypeFile)).Replace("\r\n", "");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		Assert.That(errors, Does.Contain(CodeDescriptionAreValidErrorMessage1));

		Assert.That(errors, Does.Contain(CodeDescriptionAreValidErrorMessage2));
	}
	protected abstract string InvalidMeasuresDescFile { get; }
	protected abstract string InvalidMeasuresCodesFile { get; }
	protected abstract string CodeDescriptionAreValidErrorMessage1 { get; }
	protected abstract string CodeDescriptionAreValidErrorMessage2 { get; }

	[Test]
	public void TestInvalidRateFormula()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, InvalidMeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresCodesFile)).Replace("\r\n", "");
		string exciseJsonContent = File.ReadAllText(Path.Combine(InputPath, ExciseBaseTypeFile)).Replace("\r\n", "");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		Assert.That(errors, Does.Contain(InvalidRateFormulaErrorMessage1));

		Assert.That(errors, Does.Contain(InvalidRateFormulaErrorMessage2));

		Assert.That(errors, Does.Contain(InvalidRateFormulaErrorMessage3));

		Assert.That(errors, Does.Contain(InvalidRateFormulaErrorMessage4));
	}
	protected abstract string InvalidMeasuresFile { get; }
	protected abstract string InvalidRateFormulaErrorMessage1 { get; }
	protected abstract string InvalidRateFormulaErrorMessage2 { get; }
	protected abstract string InvalidRateFormulaErrorMessage3 { get; }
	protected abstract string InvalidRateFormulaErrorMessage4 { get; }

	[Test]
	public void TestInvalidTariff()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, InvalidMeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresCodesFile)).Replace("\r\n", "");
		string exciseJsonContent = File.ReadAllText(Path.Combine(InputPath, ExciseBaseTypeFile)).Replace("\r\n", "");

		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		Assert.That(errors, Does.Contain(InvalidTariffErrorMessage));
	}
	protected abstract string InvalidTariffErrorMessage { get; }

	[Test]
	public void TestExciseBaseTypeMinItems()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresCodesFile)).Replace("\r\n", "");
		string exciseJsonContent = "{ }";

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(ExciseBaseTypeMinItemsErrorMessage));
	}
	protected abstract string ExciseBaseTypeMinItemsErrorMessage { get; }

	[Test]
	public void TestMeasureCodesTypeMinItems()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = "[ ]";
		string exciseJsonContent = File.ReadAllText(Path.Combine(InputPath, ExciseBaseTypeFile)).Replace("\r\n", "");

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(MeasureCodesTypeMinItemsErrorMessage));
	}
	protected abstract string MeasureCodesTypeMinItemsErrorMessage { get; }

	[Test]
	public void TestEmptyMeasures()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(CommonInputPath, "schema_error_measures_es.json")).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresCodesFile)).Replace("\r\n", "");
		string exciseJsonContent = File.ReadAllText(Path.Combine(InputPath, ExciseBaseTypeFile)).Replace("\r\n", "");

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain("Error in Measures processing. Json content is empty."));
	}

	[Test]
	public void TestInvalidJsonFormat()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = "[";
		string measuresCodesJsonContent = "[";
		string exciseJsonContent = "[";

		var errorMessage = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);

		Assert.That(errorMessage, Does.Contain(@"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'CargoWise.RefDbRepo.ESReferenceData.Business.MeasuresSchema' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly."));
	}

	[Test]
	public void TestChangeRegionDateTime()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var parser = ParserToRun;

		string measuresJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresFile)).Replace("\r\n", "");
		string measuresCodesJsonContent = File.ReadAllText(Path.Combine(InputPath, MeasuresCodesFile)).Replace("\r\n", "");
		string exciseJsonContent = File.ReadAllText(Path.Combine(InputPath, ExciseBaseTypeFile)).Replace("\r\n", "");

		SetCurrentCultureShortDatePattern("MM-dd-yyyy");
		var errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);
		Assert.That(errors, Is.EqualTo(string.Empty));

		SetCurrentCultureShortDatePattern("dd-MM-yyyy");
		errors = parser.ConvertToXMLFile(GetUOMJsonContent(), measuresJsonContent, measuresCodesJsonContent, exciseJsonContent, outputFile);
		Assert.That(errors, Is.EqualTo(string.Empty));

		static void SetCurrentCultureShortDatePattern(string shortDatePattern)
		{
			var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
			culture.DateTimeFormat.ShortDatePattern = shortDatePattern;
			culture.DateTimeFormat.LongTimePattern = string.Empty;
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
