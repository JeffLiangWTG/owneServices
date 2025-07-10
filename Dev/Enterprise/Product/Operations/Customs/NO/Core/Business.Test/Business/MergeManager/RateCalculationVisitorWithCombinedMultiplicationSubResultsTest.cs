using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(RateCalculationVisitorWithCombinedMultiplicationSubResults))]
sealed class RateCalculationVisitorWithCombinedMultiplicationSubResultsTest : RateCalculationVisitorTestBase
{
	public void TestFormulaHavingMultiplicationWithMultipleUnits()
	{
		const string formulaForTesting = "5.14 * [NMB] * [LTR]";
		testingData.UnitOfMeasureValueList["NMB"] = 10;
		testingData.UnitOfMeasureValueList["LTR"] = 20;

		CalculateAndAssert(formulaForTesting, 1028, new []
		{
			new DutyCalculationIntermediateResult(1028, 5.14, 200, "NMB")
		});
	}

	public void TestFormulaHavingSimpleMultiplication()
	{
		const string formulaForTesting = "5.14 * [NMB]";
		testingData.UnitOfMeasureValueList["NMB"] = 10;
		CalculateAndAssert(formulaForTesting, 51.4m, new []
		{
			new DutyCalculationIntermediateResult(51.4m, 5.14, 10, "NMB")
		});
	}

	public void TestCalculateDutiesFromFormulaWithMaxExpression()
	{
		var formulaForTesting = "MAX(15.66*[KGM]+0.1313*44.9096*[KGM],22*[KGM])";
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(1212.706m, 22m, 55.123m, "KGM" ),
		};
		CalculateAndAssert(formulaForTesting, 1212.706m, expectedIntermediateResults);
	}

	public void TestCalculateDutiesFromFormulaWithMinExpression()
	{
		var formulaForTesting = "MIN(VFD * 0.01, 3.0 * [DTN])";
		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(41.547m, 3m, 13.849m, "DTN"),
		};
		CalculateAndAssert(formulaForTesting, 41.547m, expectedIntermediateResults);
	}

	public void TestCalculateDutiesWithComplexMultiplicationFormula()
	{
		testingData.ValueForDuty = 10000;
		testingData.UnitOfMeasureValueList["DTN"] = 1000;
		const string formulaForTesting = "MIN(MIN((VFD * 0.041) + 20.28 * [DTN], (VFD * 0.093) + (13.62 * [DTN])), 35.150 * [DTN])";
		CalculateAndAssert(formulaForTesting, 14550m, new []
		{
			new DutyCalculationIntermediateResult(410m, 0.041m, 10000m, "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(20280m, 20.28m, 1000m, "DTN") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(930m, 0.093m, 10000m, "%") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(13620m, 13.62m, 1000m, "DTN") { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(35150m, 35.15m, 1000m, "DTN") { ParticipatingExpression = false },
		});
	}

	public void TestNewVisitor()
	{
		IRateCalculationVisitorCreator creator = new RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator();
		var visitor = creator.NewVisitor(new RateCalcDataForTesting(), new FormulaErrorListener());
		AssertType<RateCalculationVisitorWithCombinedMultiplicationSubResults>(visitor);
	}

	protected override void SetUp()
	{
		base.SetUp();
		testingData = GetTestingData();
	}
	RateCalcDataForTesting testingData;

	protected override IDutyCalculationIntermediateResult[] GetApplicableExpectedResults(IDutyCalculationIntermediateResult[] allExpressionResults)
		=> allExpressionResults.Where(r => r.ParticipatingExpression).ToArray();

	void CalculateAndAssert(string formulaForTesting, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedResults)
	{
		CalculateAndAssert(formulaForTesting, expectedFinalResultAmount, expectedResults, testingData, FormulaVisitorCreator);
	}

	RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator FormulaVisitorCreator => formulaVisitorCreator ??= new RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator();
	RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator formulaVisitorCreator;
}
