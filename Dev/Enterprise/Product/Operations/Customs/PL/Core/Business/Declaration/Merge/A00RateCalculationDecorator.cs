using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.Business.Declaration;

public class A00RateCalculationDecorator : IRateCalculationVisitor
{
	public A00RateCalculationDecorator(IRateCalculationVisitor baseVisitor, IUniversalRateCalcData rateCalcData)
	{
		this.baseVisitor = Argument.NotNull(baseVisitor, nameof(baseVisitor));
		this.rateCalcData = Argument.NotNull(rateCalcData, nameof(rateCalcData));
	}

	readonly IRateCalculationVisitor baseVisitor;
	readonly IUniversalRateCalcData rateCalcData;

	RateFormulaResult IRateCalculationVisitor.VisitFullFormulaExpression(RateFormulaParser.ExpressionContext context)
	{
		var baseResult = baseVisitor.VisitFullFormulaExpression(context);
		return GroupIntermediateResults(baseResult);
	}

	#region Implementation

	RateFormulaResult GroupIntermediateResults(RateFormulaResult baseResult)
	{
		var intermediateResultsList = baseResult.IntermediateResults.ToList();
		if (intermediateResultsList.Count == 1 &&
			intermediateResultsList.Single().MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage)
		{
			return baseResult;
		}

		var results = new List<IDutyCalculationIntermediateResult>();
		foreach (var groupByOriginalMeursingExpressionRows in intermediateResultsList.GroupBy(x => x.OriginalMeursingExpression))
		{
			var amount = groupByOriginalMeursingExpressionRows.Sum(x => x.Amount);
			var mergedRow = new DutyCalculationIntermediateResult(amount, ZDecimal.Zero, rateCalcData.ValueForDuty, ZString.Empty)
			{
				OriginalMeursingExpression = groupByOriginalMeursingExpressionRows.Key,
			};
			results.Add(mergedRow);
		}

		var newResult = new RateFormulaResult(results.Sum(x => x.Amount));
		newResult.AddIntermediateResultsRange(results);
		return newResult;
	}

	#endregion
}
