using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class IGICParserTests : JsonTariffParserTests
{
	protected override string TestClassName => nameof(IGICParserTests);
	protected override JsonTariffParser ParserToRun => new IGICParserTest(DateProvider.Object);
	protected override string MeasuresFile => "measures_can.json";
	protected override string FootnotesFile => "measures_can_notes.json";
	protected override string ExpectedXML => "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_Can_VALID.xml";

	protected override string TaxOrFeeType => "IGIC";

	protected override string EmptyMeasuresAndFootnotesErrorMessage => @"Footnotes process schema. Json is null or empty.
Unable to locate IGIC records";

	protected override string InvalidJsonMeasuresAndFootnotesErrorMessage1 => @"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'CargoWise.RefDbRepo.ESReferenceData.Business.FootnoteSchema' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly.";
	protected override string InvalidJsonMeasuresAndFootnotesErrorMessage2 => "Unable to locate IGIC records";

	protected override string InvalidDataMeasuresAndFootnotesErrorMessage => @"Unable to locate IGIC records";

	protected override string FootnoteNotFoundInFootnotesErrorMessage => @"Unable to import IGIC Applicability as footnote code not found in the footnotes json. Details:
Footnote: test";
	protected override string InvalidDutyMeasuresFile => "invalid_duty_measures_can.json";
	protected override string InvalidDutyErrorMessage => @"Unable to import IGIC Applicability as invalid 'duty'. Details:
Duty: 31%";

	protected override string InvalidTariffAndDutyFile => "invalid_data_measures_can.json";
	protected override string InvalidTariffAndDutyErrorMessage1 => @"Unable to import IGIC Applicabilities as missing or invalid 'tariff'. Details:
Tariff: 01012100
";
	protected override string InvalidTariffAndDutyErrorMessage2 => @"Unable to import IGIC Applicabilities as missing or invalid 'tariff'. Details:
Tariff: 
";
	protected override string InvalidTariffAndDutyErrorMessage3 => "Unable to import IGIC Applicability as missing or invalid 'duty' for the current tariff code.";
	protected override string InvalidTariffAndDutyExpectedXML => "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_Can_INVALID_DATA.xml";

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		InputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\IGIC");
	}

	class IGICParserTest(IDateTimeProvider dateTimeProvider) : IGICParser(dateTimeProvider, "URI")
	{
		readonly string inputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Business\JSON\TestFiles\Input\IGIC");

		protected override Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, bool isESTariff, bool checkExactTariff)
		{
			var result = tariffCode switch
			{
				"9619008900" or "8908000000" => Path.Combine(inputPath, "RefCusTariffQueryES.json"),
				_ => Path.Combine(inputPath, "RefCusTariffQueryEUN.json"),
			};
			return new Uri(result);
		}
	}
}
