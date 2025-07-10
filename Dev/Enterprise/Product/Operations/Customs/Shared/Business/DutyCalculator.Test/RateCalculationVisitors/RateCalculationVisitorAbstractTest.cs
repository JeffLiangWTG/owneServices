using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateCalculationVisitor))]
public abstract class RateCalculationVisitorAbstractTest : RateCalculationVisitorTestBase
{
	public void TestCalculateDutiesFromFormulaWithMaxExpressions()
	{
		testingData.ValueForDuty = 10000;
		testingData.UnitOfMeasureValueList["DTN"] = 1000;
		var formulaForTesting = "MAX(MAX((VFD * 0.041) + 20.28 * [DTN], (VFD * 0.093) + (13.62 * [DTN])), 35.150 * [DTN])";
		var formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(410m, 0.041m, 10000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(20280m, 20.28m, 1000m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(930m, 0.093m, 10000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(13620m, 13.62m, 1000m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(35150m, 35.15m, 1000m, "DTN") { ParticipatingExpression = true },
		};
		CalculateAndAssert(formulaForTesting, 35150m, formulaExpressionResults);

		testingData.ValueForDuty = 10000;
		testingData.UnitOfMeasureValueList["DTN"] = 1000;
		formulaForTesting = "MAX(MAX(MAX(VFD * 0.041, 20.28 * [DTN]), MAX(VFD * 0.093, 13.62 * [DTN])), MAX(35.150 * [DTN], VFD * 0.093))";
		formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(410m, 0.041m, 10000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(20280m, 20.28m, 1000m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(930m, 0.093m, 10000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(13620m, 13.62m, 1000m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(35150m, 35.15m, 1000m, "DTN") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(930m, 0.093m, 10000m, "%") { ParticipatingExpression = false },
		};
		CalculateAndAssert(formulaForTesting, 35150m, formulaExpressionResults);
	}

	public void TestCalculateDutiesFromFormulaWithMaxExpressionResultDeterminedByVfdValue()
	{
		var formulaForTesting = "MAX(15.66*[KGM]+0.1313*44.9096*[KGM],21*[KGM]+0.03*VFD)";

		// MAX left param expected to win
		testingData.ValueForDuty = 1022;
		var vfd1022FormulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(863.22618m, 15.66m, 55.123m, "KGM") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(325.03996194904m, 5.89663048m, 55.123m, "KGM") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(1157.583m, 21m, 55.123m, "KGM") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(30.66m, 0.03m, 1022m, "%") { ParticipatingExpression = false },
		};
		CalculateAndAssert(formulaForTesting, 1188.26614194904m, vfd1022FormulaExpressionResults);

		// MAX right param expected to win
		testingData.ValueForDuty = 1023;
		var vfd1023FormulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(863.22618m, 15.66m, 55.123m, "KGM") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(325.03996194904m, 5.89663048m, 55.123m, "KGM") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(1157.583m, 21m, 55.123m, "KGM") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(30.69m, 0.03m, 1023m, "%") { ParticipatingExpression = true },
		};
		CalculateAndAssert(formulaForTesting, 1188.273m, vfd1023FormulaExpressionResults);
	}

	public void TestCalculateDutiesFromFormulaWithSubtraction()
	{
		var formulaForTesting = "MIN(MIN((VFD * 0.041) + (0), (VFD * 0.093) + (27.250 * [DTN])), 35.150 * [DTN]) - MIN(VFD * 0.041, 35.150 * [DTN])";
		var formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(205.02009m, 0.041m, 5000.49m, "%") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(465.04557m, 0.093m, 5000.49m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(377.38525m, 27.25m, 13.849m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(486.79235m, 35.15m, 13.849m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(-205.02009m, 0.041m, 5000.49m, "%") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(-486.79235m, 35.15m, 13.849m, "DTN") { ParticipatingExpression = false },
		};
		CalculateAndAssert(formulaForTesting, 0m, formulaExpressionResults);
	}

	public void TestCalculateDutiesFromFormulaWithMinExpressions()
	{
		testingData.ValueForDuty = 10000;
		testingData.UnitOfMeasureValueList["DTN"] = 1000;
		var formulaForTesting = "MIN(MIN((VFD * 0.041) + 20.28 * [DTN], (VFD * 0.093) + (13.62 * [DTN])), 35.150 * [DTN])";
		var formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(410m, 0.041m, 10000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(20280m, 20.28m, 1000m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(930m, 0.093m, 10000m, "%") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(13620m, 13.62m, 1000m, "DTN") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(35150m, 35.15m, 1000m, "DTN") { ParticipatingExpression = false },
		};
		CalculateAndAssert(formulaForTesting, 14550m, formulaExpressionResults);
	}

	public void TestCalculateDutiesFromFormulaWithMaxMinExpressions()
	{
		testingData.ValueForDuty = 20000m;
		testingData.UnitOfMeasureValueList["NAR"] = 1000;
		var formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(500m, 0.5m, 1000m, "NAR") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(540m, 0.027m, 20000m, "%") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(920m, 0.046m, 20000m, "%") { ParticipatingExpression = false },
		};
		CalculateAndAssert("MIN(MAX(0.500 * [NAR], VFD * 0.027), VFD * 0.046)", 540m, formulaExpressionResults);

		testingData.ValueForDuty = 20000m;
		testingData.UnitOfMeasureValueList["NAR"] = 1500;
		formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(750m, 0.5m, 1500m, "NAR") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(540m, 0.027m, 20000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(920m, 0.046m, 20000m, "%") { ParticipatingExpression = false },
		};
		CalculateAndAssert("MIN(MAX(0.500 * [NAR], VFD * 0.027), VFD * 0.046)", 750m, formulaExpressionResults);

		testingData.ValueForDuty = 20000m;
		testingData.UnitOfMeasureValueList["NAR"] = 2000;
		formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(1000m, 0.5m, 2000m, "NAR") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(540m, 0.027m, 20000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(920m, 0.046m, 20000m, "%") { ParticipatingExpression = true },
		};
		CalculateAndAssert("MIN(MAX(0.500 * [NAR], VFD * 0.027), VFD * 0.046)", 920m, formulaExpressionResults);
	}

	public void TestCalculateDutiesFromFormulaWithMeasuringPlaceHolderNotInMeasuringExpressionList()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(0m, 10m, 0m, "EAR(1)"),
		};

		var expectedErrors = new[]
		{
			new ErrorInformation(FormulaVisitErrorType.MeursingExpressionNotFound, "Measuring Code: EAR(1) is not specified, using 0 for calculation"),
		};

		CalculateAndAssert("#EAR(1)# * 10", 0m, expectedIntermediateResults, testingData, FormulaVisitorCreator, expectedErrors);
	}

	public void TestCalculateIntermediateResultsNotCreatedForComplexPart()
	{
		testingData.ValueForDuty = 1000;
		testingData.UnitOfMeasureValueList["TNE"] = 2;

		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(1306m, 653m, 2m, "TNE"),
		};
		CalculateAndAssert("(1153.000 - (VFD/[TNE])) * [TNE]", 1306m, expectedIntermediateResults);
	}

	public void TestCalculateIntermediateResultsNotCreatedForComplexPart_2()
	{
		testingData.ValueForDuty = 1000;
		testingData.UnitOfMeasureValueList["TNE"] = 2;

		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(250, 0.25m, 1000m, "%"),
		};
		CalculateAndAssert("(VFD / (1 - 0.2) * 0.2)", 250, expectedIntermediateResults);
	}

	public void TestCalculateIntermediateResults_UnchangedWithRedundantParenthenses()
	{
		testingData.ValueForDuty = 1000;
		testingData.UnitOfMeasureValueList["ASVX"] = 2;
		testingData.UnitOfMeasureValueList["ASV"] = 3;

		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(246, 123, 2m, "ASVX"),
		new DutyCalculationIntermediateResult(0m, 0m, 3, "ASV"),
		};

		CalculateAndAssert("123 * [ASVX] + 0 * [ASV]", 246, expectedIntermediateResults);
		CalculateAndAssert("(123 * [ASVX] + 0 * [ASV])", 246, expectedIntermediateResults);
	}

	protected override void SetUp()
	{
		base.SetUp();
		testingData = GetTestingData();
	}
	protected RateCalcDataForTesting testingData;

	protected IRateCalculationVisitorCreator FormulaVisitorCreator => formulaVisitorCreator ??= NewFormulaVisitorCreator();
	IRateCalculationVisitorCreator formulaVisitorCreator;

	protected abstract IRateCalculationVisitorCreator NewFormulaVisitorCreator();

	protected void CalculateAndAssert(string formulaForTesting, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedResults)
	{
		CalculateAndAssert(formulaForTesting, expectedFinalResultAmount, expectedResults, testingData, FormulaVisitorCreator);
	}
}
