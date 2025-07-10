using CargoWise.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

sealed class RateCalculationIncludeNonParticipatingMinMaxResultVisitor : RateCalculationVisitor
{
	RateCalculationIncludeNonParticipatingMinMaxResultVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener) : base(rateCalcData, errorListener)
	{
	}

	public class Creator : IRateCalculationVisitorCreator
	{
		IRateCalculationVisitor IRateCalculationVisitorCreator.NewVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener)
		{
			return new RateCalculationIncludeNonParticipatingMinMaxResultVisitor(rateCalcData, errorListener);
		}
	}

	protected override RateFormulaResult VisitMinMaxCondition(bool condition, RateFormulaResult opTrue, RateFormulaResult opFalse)
	{
		if (condition)
		{
			opFalse.IntermediateResults.ForEach(x => x.ParticipatingExpression = false);
			opTrue.AddIntermediateResultsRange(opFalse.IntermediateResults);
			return opTrue;
		}

		opTrue.IntermediateResults.ForEach(x => x.ParticipatingExpression = false);
		opFalse.AddIntermediateResultsRangeWithIndex(0, opTrue.IntermediateResults);
		return opFalse;
	}
}
