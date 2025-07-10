using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

sealed class RateCalculationVisitorWithCombinedMultiplicationSubResults : RateCalculationVisitor
{
	RateCalculationVisitorWithCombinedMultiplicationSubResults(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener) : base(rateCalcData, errorListener)
	{
	}

	public class Creator : IRateCalculationVisitorCreator
	{
		IRateCalculationVisitor IRateCalculationVisitorCreator.NewVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener)
		{
			return new RateCalculationVisitorWithCombinedMultiplicationSubResults(rateCalcData, errorListener);
		}
	}

	protected override RateFormulaResult VisitMinMaxCondition(bool condition, RateFormulaResult resultWhenTrue, RateFormulaResult resultWhenFalse)
	{
		return condition ? resultWhenTrue : resultWhenFalse;
	}

	protected override void VisitMultiplyingExpressionWithTimesExpContent(RateFormulaParser.TimesExpContext contExp, RateFormulaResult result)
	{
		var contResult = Visit(contExp);

		if (result is { IntermediateResults.Count: 0 })
		{
			foreach (var item in contResult.IntermediateResults)
			{
				item.Rate *= result.ResultAmount;
				item.Amount *= result.ResultAmount;
			}
			result.AddIntermediateResultsRange(contResult.IntermediateResults);
		}
		else
		{
			foreach (var item in result.IntermediateResults)
			{
				item.Amount *= contResult.ResultAmount;
				if (item.Rate == 1)
				{
					item.Rate = contResult.ResultAmount;
				}
				else
				{
					item.BaseValue *= contResult.ResultAmount;
				}
			}
		}

		result.ResultAmount *= contResult.ResultAmount;
	}
}
