using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateCalculationParticipatingResultOnlyVisitor))]
sealed class RateCalculationParticipatingResultOnlyVisitorTest : RateCalculationVisitorAbstractTest
{
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

	public void TestNewVisitor()
	{
		IRateCalculationVisitorCreator creator = new RateCalculationParticipatingResultOnlyVisitor.Creator();
		var visitor = creator.NewVisitor(new RateCalcDataForTesting(), new FormulaErrorListener());
		AssertType<RateCalculationParticipatingResultOnlyVisitor>(visitor);
	}

	protected override IRateCalculationVisitorCreator NewFormulaVisitorCreator() => new RateCalculationParticipatingResultOnlyVisitor.Creator();

	protected override IDutyCalculationIntermediateResult[] GetApplicableExpectedResults(IDutyCalculationIntermediateResult[] allExpressionResults)
		=> allExpressionResults.Where(r => r.ParticipatingExpression).ToArray();
}
