namespace Enterprise.Customs.DutyCalculator;

public static class RateCalculationVisitorFactory
{
	public static IRateCalculationVisitorCreator CreateVisitor(RateCalculationVisitorMode visitorMode)
	{
		return visitorMode switch
		{
			RateCalculationVisitorMode.Default => new RateCalculationParticipatingResultOnlyVisitor.Creator(),
			RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults => new RateCalculationIncludeNonParticipatingMinMaxResultVisitor.Creator(),
			_ => throw new ArgumentOutOfRangeException(nameof(visitorMode), visitorMode, null)
		};
	}
}
