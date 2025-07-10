using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

sealed class RateCalculationParticipatingResultOnlyVisitor : RateCalculationVisitor
{
	RateCalculationParticipatingResultOnlyVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener) : base(rateCalcData, errorListener)
	{
	}

	public class Creator : IRateCalculationVisitorCreator
	{
		IRateCalculationVisitor IRateCalculationVisitorCreator.NewVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener)
		{
			return new RateCalculationParticipatingResultOnlyVisitor(rateCalcData, errorListener);
		}
	}

	protected override RateFormulaResult VisitMinMaxCondition(bool condition, RateFormulaResult opTrue, RateFormulaResult opFalse)
	{
		return condition ? opTrue : opFalse;
	}
}
