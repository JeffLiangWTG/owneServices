namespace Enterprise.Customs.Universal
{
	class ConditionFormulaCalculatorVisitor : RateFormulaCalculativeVisitor
	{
		public ConditionFormulaCalculatorVisitor(UniversalRateCalcDataWrapper rateCalcData)
			: base(rateCalcData)
		{
		}

		public override decimal VisitBoolExpression(RateFormulaParser.BoolExpressionContext context)
		{
			var result = VisitBoolAndExpression(context.basicExp) > 0m;
			foreach (var contExp in context._contExp)
			{
				result |= VisitBoolAndExpression(contExp) > 0m;
			}
			return result ? 1m : 0m;
		}

		public override decimal VisitBoolAndExpression(RateFormulaParser.BoolAndExpressionContext context)
		{
			var result = EvaluateBoolAtom(context.basicExp);
			foreach (var contExp in context._contExp)
			{
				result &= EvaluateBoolAtom(contExp);
			}
			return result ? 1m : 0m;
		}
	}
}
