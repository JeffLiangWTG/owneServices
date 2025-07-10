using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateCalculationVisitorFactory))]
sealed class RateCalculationVisitorFactoryTest  : TestCase
{
	public void TestCreateVisitor_ForDefault()
	{
		var visitor = RateCalculationVisitorFactory.CreateVisitor(RateCalculationVisitorMode.Default);
		AssertNotNull(visitor);
		AssertType<RateCalculationParticipatingResultOnlyVisitor.Creator>(visitor);
	}

	public void TestCreateVisitor_ForIncludeNotParticipatingMinMaxResults()
	{
		var visitor = RateCalculationVisitorFactory.CreateVisitor(RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults);
		AssertNotNull(visitor);
		AssertType<RateCalculationIncludeNonParticipatingMinMaxResultVisitor.Creator>(visitor);
	}
}
