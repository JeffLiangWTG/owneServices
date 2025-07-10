using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
public class CANEXCParserTests : ExcisesParentParserTests
{
	protected override string TestClassName => nameof(CANEXCParserTests);
	protected override ExcisesParentParser ParserToRun => new CANEXCParser(DateProvider.Object);
	protected override string MeasuresFile => "measures_can.json";
	protected override string MeasuresCodesFile => "measures_can_codes.json";
	protected override string ExciseBaseTypeFile => "excise_base_type.json";
	protected override string ExpectedXML => "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_ES_CANEXC_VALID.xml";

	protected override string InvalidMeasuresCodesFile => "invalid_measures_can_codes.json";

	protected override string InvalidMeasuresDescFile => "invalid_desc_measures_can.json";

	protected override string CodeDescriptionAreValidErrorMessage1 => @"Unable to import CANEX RateCode as missing or invalid 'code' or 'description'
Code: 
Description: Cerveza grado plato < 11 + 2,8% Vol.";

	protected override string CodeDescriptionAreValidErrorMessage2 => @"Unable to import CANEX RateCode as missing or invalid 'code' or 'description'
Code: 0A3
Description: ";

	protected override string InvalidMeasuresFile => "invalid_measures_can.json";

	protected override string InvalidRateFormulaErrorMessage1 => "Unable to import Tariff as invalid Rate Formula, format has changed 750.36 ER HG";

	protected override string InvalidRateFormulaErrorMessage2 => "Unable to import Tariff as invalid Rate Formula, format has changed 750.36 EUR HG + 750.36 EUR HG + 750.36 EUR HG";

	protected override string InvalidRateFormulaErrorMessage3 => @"Unable to import Tariff as invalid Rate Formula, format has changed HG + 750.36";

	protected override string InvalidRateFormulaErrorMessage4 => "Unable to import Tariff as invalid Rate Formula, format has changed 5d % + 750.36 EUR HG";

	protected override string InvalidTariffErrorMessage => @"Unable to import CANEX Relationship as missing or invalid 'tariff'. Details:
Tariff: 13021970";

	protected override string ExciseBaseTypeMinItemsErrorMessage => @"Error in Excise Base Type processing. Details: Required properties are missing from object: pvp.
Unable to locate CANEX records";

	protected override string MeasureCodesTypeMinItemsErrorMessage => @"Error in Measures Codes processing. Details: item count is 0.
Unable to locate CANEX records";

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		InputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\CANEXC");
	}
}
