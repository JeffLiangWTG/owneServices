using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateCalculationIncludeNonParticipatingMinMaxResultVisitor))]
sealed class RateCalculationIncludeNonParticipatingMinMaxResultVisitorTest : RateCalculationVisitorAbstractTest
{
	public void TestNewVisitor()
	{
		IRateCalculationVisitorCreator creator = new RateCalculationIncludeNonParticipatingMinMaxResultVisitor.Creator();
		var visitor = creator.NewVisitor(new RateCalcDataForTesting(), new FormulaErrorListener());
		AssertType<RateCalculationIncludeNonParticipatingMinMaxResultVisitor>(visitor);
	}

	public void TestVisitMinCondition()
	{
		var formulaForTesting = "MIN(VFD * 0.093, 12)";
		testingData.ValueForDuty = 10000;
		var formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(930m, 0.093m, 10000m, "%") { ParticipatingExpression = false },
		};
		CalculateAndAssert(formulaForTesting, 12m, formulaExpressionResults);
	}

	public void TestVisitMaxCondition()
	{
		var formulaForTesting = "MAX(VFD * 0.093, 12)";
		testingData.ValueForDuty = 10000;
		var formulaExpressionResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(930m, 0.093m, 10000m, "%") { ParticipatingExpression = true },
		};
		CalculateAndAssert(formulaForTesting, 930m, formulaExpressionResults);
	}

	protected override IDutyCalculationIntermediateResult[] GetApplicableExpectedResults(IDutyCalculationIntermediateResult[] allExpressionResults)
		=> allExpressionResults;

	protected override IRateCalculationVisitorCreator NewFormulaVisitorCreator()
		=> new RateCalculationIncludeNonParticipatingMinMaxResultVisitor.Creator();
}
