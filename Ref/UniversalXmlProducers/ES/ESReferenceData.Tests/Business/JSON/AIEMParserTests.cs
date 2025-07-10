using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class AIEMParserTests : JsonTariffParserTests
{
	protected override string TestClassName => nameof(AIEMParserTests);
	protected override JsonTariffParser ParserToRun => new AIEMParser(DateProvider.Object, string.Empty);
	protected override string MeasuresFile => "measures_can.json";
	protected override string FootnotesFile => "measures_can_notes.json";
	protected override string ExpectedXML => "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_Can_AIEM_VALID.xml";

	protected override string TaxOrFeeType => null;

	protected override string EmptyMeasuresAndFootnotesErrorMessage => @"Footnotes process schema. Json is null or empty.
Unable to locate AIEM records";

	protected override string InvalidJsonMeasuresAndFootnotesErrorMessage1 => @"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'CargoWise.RefDbRepo.ESReferenceData.Business.FootnoteSchema' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly.";
	protected override string InvalidJsonMeasuresAndFootnotesErrorMessage2 => "Unable to locate AIEM records";

	protected override string InvalidDataMeasuresAndFootnotesErrorMessage => @"Unable to locate AIEM records";

	protected override string FootnoteNotFoundInFootnotesErrorMessage => @"Unable to import AIEM Relationship as footnote code not found in the footnotes json. Details:
Footnote: test";

	protected override string InvalidDutyMeasuresFile => "invalid_duty_measures_can.json";
	protected override string InvalidDutyErrorMessage => @"Unable to import AIEM Relationship as invalid 'duty'. Details:
Duty: 31%";

	protected override string InvalidTariffAndDutyFile => "invalid_data_measures_can.json";
	protected override string InvalidTariffAndDutyErrorMessage1 => @"Unable to import AIEM Relationship as missing or invalid 'tariff'. Details:
Tariff: 01012100
";
	protected override string InvalidTariffAndDutyErrorMessage2 => @"Unable to import AIEM Relationship as missing or invalid 'tariff'. Details:
Tariff: 
";
	protected override string InvalidTariffAndDutyErrorMessage3 => @"Unable to import AIEM Relationship as invalid 'duty'. Details:
Duty: 2x%";
	protected override string InvalidTariffAndDutyExpectedXML => "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_Can_AIEM_INVALID_DATA.xml";

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		InputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\AIEM");
	}
}
