using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateCalculationVisitor))]
sealed class RateCalculationVisitorBaseOnlyTest	: RateCalculationVisitorAbstractTest
{
	public void TestCalculateDutiesFromFormulaWithIfExpressions()
	{
		testingData.ValueForDuty = 10000;
		testingData.UnitOfMeasureValueList["DTN"] = 1000;
		var formulaForTesting = "IF(VFD > 5000, (VFD * 0.041) + 20.28 * [DTN], (VFD * 0.093) + (13.62 * [DTN]))";
		var formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(410m, 0.041m, 10000m, "%" ),
			new DutyCalculationIntermediateResult(20280m, 20.28m, 1000m, "DTN"),
		};
		CalculateAndAssert(formulaForTesting, 20690m, formulaExpressionResults);

		testingData.ValueForDuty = 1000;
		testingData.UnitOfMeasureValueList["DTN"] = 1000;
		formulaForTesting = "IF(VFD > 5000, (VFD * 0.041) + 20.28 * [DTN], (VFD * 0.093) + (13.62 * [DTN]))";
		formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(93m, 0.093m, 1000m, "%"),
			new DutyCalculationIntermediateResult(13620m, 13.62m, 1000m, "DTN")
		};
		CalculateAndAssert(formulaForTesting, 13713m, formulaExpressionResults);
	}

	public void TestCalculateDutiesFromFormulaWithSingleKgmExpression()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(3086.888m, 56m, 55.123m, "KGM"),
		};
		CalculateAndAssert("56 * [KGM]", 3086.888m, expectedIntermediateResults);
	}

	public void TestCalculateDutiesFromFormulaWithSingleVfdExpression()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(150.0147m, 0.03m, 5000.49m, "%"),
		};
		CalculateAndAssert("0.03 * VFD", 150.0147m, expectedIntermediateResults);
	}

	public void TestCalculateDutiesFromFormulaWithKgmAndVfdExpressions()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(3086.888m, 56m, 55.123m, "KGM"),
			new DutyCalculationIntermediateResult(150.0147m, 0.03m, 5000.49m, "%"),
		};
		CalculateAndAssert("56 * [KGM] + 0.03 * VFD", 3236.9027m, expectedIntermediateResults);
	}

	public void TestCalculateDutiesFromFormulaWithHasExpression()
	{
		var formulaForTesting = "IF(HAS(\"VFD\",\"BB\"), 10*[KGM], 20*[KGM])";

		// HAS expression yields false
		var expectedResultsWhenHasYieldsFalse = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(1102.46m, 20m, 55.123m, "KGM"),
		};
		CalculateAndAssert(formulaForTesting, 1102.46m, expectedResultsWhenHasYieldsFalse);

		// HAS expression yields true
		testingData.AdditionalInformationList.Add(new Tuple<string, string>("VFD", "BB"));
		var expectedResultsWhenHasYieldsTrue = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(551.23m, 10m, 55.123m, "KGM"),
		};
		CalculateAndAssert(formulaForTesting, 551.23m, expectedResultsWhenHasYieldsTrue);
	}

	public void TestCalculateDutiesFromFormulaWithCountrySpecificValues()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(31.40m, 10m, 3.14m, "PI"),
			new DutyCalculationIntermediateResult(100.0098m, 0.02m, 5000.49m, "%"),
			new DutyCalculationIntermediateResult(4.23m, 3m, 1.41m, "SR2"),
		};
		CalculateAndAssert("10 * PI + 0.02 * VFD + 3 * SR2", 135.6398m, expectedIntermediateResults);
	}

	public void TestCalculateDutiesFromFormulaWithExpressionNotInRateCalcDataCountrySpecificValues()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(0m, 1m, 0m, "ZETA"),
		};

		var expectedErrors = new ErrorInformation[]
		{
			new ErrorInformation(FormulaVisitErrorType.CountrySpecificValueNotFound, "Country Specific Value: ZETA is not specified, using 0 for calculation"),
		};

		CalculateAndAssert("1 * ZETA", 0m, expectedIntermediateResults, testingData, FormulaVisitorCreator,  expectedErrors);
	}

	public void TestCalculateDutiesFromFormulaWithUnitOfMeasureNotInTheRateCalcDataUnitValueDictionary()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(0m, 10m, 0m, "TST"),
		};

		var expectedErrors = new ErrorInformation[]
		{
				new ErrorInformation(FormulaVisitErrorType.UOMNotFound, "Unit of Measure code: TST is not specified, using 0 for calculation"),
		};

		CalculateAndAssert("[TST]*10", 0m, expectedIntermediateResults, testingData, FormulaVisitorCreator, expectedErrors);
	}

	public void TestFormulaWithNumericValueTooLarge()
	{
		var largeNumberFormula = "99999999999999999999999999999";

		var expectedErrors = new ErrorInformation[]
		{
			new ErrorInformation(FormulaVisitErrorType.SyntaxError, $"Failed to convert \"{largeNumberFormula}\" to Decimal"),
		};

		CalculateAndAssert(largeNumberFormula, 0m, Array.Empty<IDutyCalculationIntermediateResult>(), testingData, FormulaVisitorCreator, expectedErrors);
	}

	public void TestInvalidFormula()
	{
		CalculateAndAssert("~",
			0m,
			Array.Empty<IDutyCalculationIntermediateResult>(),
			testingData,
			FormulaVisitorCreator,
			new ErrorInformation[] { new ErrorInformation(FormulaVisitErrorType.SyntaxError, "Syntax error at line 1 position 2.") });
	}

	public void TestCalculateDutiesFromFormulaWithMeasuringPlaceHolderInMeasuringExpressionList()
	{
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(200.0196m, 0.04m, 5000.49m, "%") { OriginalMeursingExpression =  "EA(1)" },
			new DutyCalculationIntermediateResult(138.490m, 10m, 13.849m, "DTN") { OriginalMeursingExpression = "ADFM(1)" },
		};

		CalculateAndAssert("#EA(1)# + #ADFM(1)#", 338.5096m, expectedIntermediateResults);
	}

	protected override IRateCalculationVisitorCreator NewFormulaVisitorCreator() => new RateCalculationVisitorForTest.Creator();

	protected override IDutyCalculationIntermediateResult[] GetApplicableExpectedResults(IDutyCalculationIntermediateResult[] allExpressionResults)
		=> allExpressionResults.Where(a => a.ParticipatingExpression).ToArray();

	class RateCalculationVisitorForTest : RateCalculationVisitor, IRateCalculationVisitor
	{
		RateCalculationVisitorForTest(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener) : base(rateCalcData, errorListener)
		{
		}

		public class Creator : IRateCalculationVisitorCreator
		{
			public IRateCalculationVisitor NewVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener)
			{
				return new RateCalculationVisitorForTest(rateCalcData, errorListener);
			}
		}

		protected override RateFormulaResult VisitMinMaxCondition(bool condition, RateFormulaResult opTrue, RateFormulaResult opFalse)
		{
			return condition ? opTrue : opFalse;
		}
	}
}
