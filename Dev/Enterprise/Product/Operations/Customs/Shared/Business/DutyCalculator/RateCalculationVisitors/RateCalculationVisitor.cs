using Antlr4.Runtime;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

public abstract class RateCalculationVisitor : RateFormulaBaseVisitor<RateFormulaResult>, IRateCalculationVisitor
{
	protected RateCalculationVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener)
	{
		this.rateCalcData = Argument.NotNull(rateCalcData, nameof(rateCalcData));
		this.errorListener = Argument.NotNull(errorListener, nameof(errorListener));
	}

	readonly IUniversalRateCalcData rateCalcData;
	readonly FormulaErrorListener errorListener;

	public static IDutyCalculationResult CalculateDuties(IUniversalRateCalcData calculationData, string formulaString, IRateCalculationVisitorCreator formulaVisitorCreator)
	{
		var errorListener = new FormulaErrorListener();
		var expressionTree = GetExpressionContextTree(formulaString, errorListener);

		if (errorListener.Errors.Any())
		{
			return new DutyCalculationResult(errorListener);
		}

		var calculationVisitor = formulaVisitorCreator.NewVisitor(calculationData, errorListener);
		var overallFormulaResult = calculationVisitor.VisitFullFormulaExpression(expressionTree);
		var result = new DutyCalculationResult(overallFormulaResult, errorListener);
		return result;
	}

	public static IDutyCalculationResult CalculateParticipatingFees(IUniversalRateCalcData rateCalcData, string formulaString)
	{
		return CalculateDuties(rateCalcData, formulaString, new RateCalculationParticipatingResultOnlyVisitor.Creator());
	}

	static RateFormulaParser.ExpressionContext GetExpressionContextTree(string formulaString, FormulaErrorListener errorListener)
	{
		var input = new AntlrInputStream(formulaString);
		var lexer = new RateFormulaLexer(input);
		lexer.RemoveErrorListener(ConsoleErrorListener<int>.Instance);

		var tokens = new CommonTokenStream(lexer);
		var parser = new RateFormulaParser(tokens);
		parser.RemoveErrorListener(ConsoleErrorListener<IToken>.Instance);
		parser.AddErrorListener(errorListener);

		return parser.expression();
	}

	RateFormulaResult IRateCalculationVisitor.VisitFullFormulaExpression(RateFormulaParser.ExpressionContext expressionTree)
	{
		return VisitExpression(expressionTree);
	}

	#region BasicArithmeticExpressions

	public override RateFormulaResult VisitExpression(RateFormulaParser.ExpressionContext context)
	{
		var firstExp = context.multiplyingExpression();
		var result = Visit(firstExp);
		foreach (var contExp in context.children.OfType<RateFormulaParser.ContExpressionContext>())
		{
			var contResult = Visit(contExp);
			result.ResultAmount += contResult.ResultAmount;
			var parent = context.parent;
			if (contExp.operand is { Type: RateFormulaLexer.PLUS } ||  parent == null)
			{
				result.AddIntermediateResultsRange(contResult.IntermediateResults);
			}
		}

		return result;
	}

	public override RateFormulaResult VisitContExpression(RateFormulaParser.ContExpressionContext context)
	{
		var result = Visit(context.contExp);
		if (context.operand.Type == RateFormulaLexer.MINUS)
		{
			result.ResultAmount = 0 - result.ResultAmount;
			foreach (var intermediateResult in result.IntermediateResults)
			{
				intermediateResult.Amount = 0 - intermediateResult.Amount;
			}
		}
		return result;
	}

	public override RateFormulaResult VisitMultiplyingExpression(RateFormulaParser.MultiplyingExpressionContext context)
	{
		var result = Visit(context.atom());

		var timesExpContentExpressions = new List<RateFormulaParser.TimesExpContext>();
		var divExpContentExpressions = new List<RateFormulaParser.DivExpContext>();

		foreach (var contExp in context.children)
		{
			switch (contExp)
			{
				case RateFormulaParser.TimesExpContext timesExpContext:
					timesExpContentExpressions.Add(timesExpContext);
					break;
				case RateFormulaParser.DivExpContext divExpContext:
					divExpContentExpressions.Add(divExpContext);
					break;
			}
		}

		foreach (var contExp in timesExpContentExpressions)
		{
			VisitMultiplyingExpressionWithTimesExpContent(contExp, result);
		}

		foreach (var contExp in divExpContentExpressions)
		{
			VisitMultiplyingExpressionWithDivExpContext(contExp, result);
		}

		return result;
	}

	protected virtual void VisitMultiplyingExpressionWithDivExpContext(RateFormulaParser.DivExpContext contExp, RateFormulaResult result)
	{
		var contResult = Visit(contExp);

		if (contResult.ResultAmount != decimal.Zero)
		{
			foreach (var item in result.IntermediateResults)
			{
				item.Rate /= contResult.ResultAmount;
				item.Amount /= contResult.ResultAmount;
			}
		}

		if (result.ResultAmount != decimal.Zero)
		{
			foreach (var item in contResult.IntermediateResults)
			{
				item.Rate /= result.ResultAmount;
				item.Amount /= result.ResultAmount;
			}
		}

		if (contResult.ResultAmount != decimal.Zero)
		{
			result.ResultAmount /= contResult.ResultAmount;
		}

		result.AddIntermediateResultsRange(contResult.IntermediateResults);
	}

	protected virtual void VisitMultiplyingExpressionWithTimesExpContent(RateFormulaParser.TimesExpContext contExp, RateFormulaResult result)
	{
		var contResult = Visit(contExp);

		foreach (var item in result.IntermediateResults)
		{
			item.Rate *= contResult.ResultAmount;
			item.Amount *= contResult.ResultAmount;
		}

		foreach (var item in contResult.IntermediateResults)
		{
			item.Rate *= result.ResultAmount;
			item.Amount *= result.ResultAmount;
		}

		result.ResultAmount *= contResult.ResultAmount;
		result.AddIntermediateResultsRange(contResult.IntermediateResults);
	}

	public override RateFormulaResult VisitTimesExp(RateFormulaParser.TimesExpContext context)
	{
		return Visit(context.contExp);
	}

	public override RateFormulaResult VisitDivExp(RateFormulaParser.DivExpContext context)
	{
		return Visit(context.contExp);
	}

	#endregion

	#region MAX/MIN/ROUND/IF Expressions

	public override RateFormulaResult VisitMaxExpression(RateFormulaParser.MaxExpressionContext context)
	{
		var op1 = Visit(context.opleft);
		var op2 = Visit(context.opright);
		var condition = (op1.ResultAmount >= op2.ResultAmount);
		return VisitMinMaxCondition(condition, op1, op2);
	}

	public override RateFormulaResult VisitMinExpression(RateFormulaParser.MinExpressionContext context)
	{
		var op1 = Visit(context.opleft);
		var op2 = Visit(context.opright);
		var condition = (op1.ResultAmount <= op2.ResultAmount);
		return VisitMinMaxCondition(condition, op1, op2);
	}

	public override RateFormulaResult VisitRoundExpression(RateFormulaParser.RoundExpressionContext context)
	{
		var result = Visit(context.expression());
		result.ResultAmount = decimal.Round(result.ResultAmount, Utils.GetIntegerNumber(context.Integer().GetText(), errorListener), MidpointRounding.AwayFromZero);
		return result;
	}

	public override RateFormulaResult VisitIfExpression(RateFormulaParser.IfExpressionContext context)
	{
		var result = EvaluateBoolExpression(context.boolExpression())
			? Visit(context.trueExp)
			: Visit(context.falseExp);
		return result;
	}

	protected abstract RateFormulaResult VisitMinMaxCondition(bool condition, RateFormulaResult opTrue, RateFormulaResult opFalse);

	#region boolExpressions

	protected bool EvaluateBoolExpression(RateFormulaParser.BoolExpressionContext context)
	{
		var result = EvaluateBoolAndExpression(context.basicExp);
		foreach (var contExp in context._contExp)
		{
			result |= EvaluateBoolAndExpression(contExp);
		}
		return result;
	}

	bool EvaluateBoolAndExpression(RateFormulaParser.BoolAndExpressionContext context)
	{
		var result = EvaluateBoolAtom(context.basicExp);
		foreach (var contExp in context._contExp)
		{
			result &= EvaluateBoolAtom(contExp);
		}
		return result;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
	protected bool EvaluateBoolAtom(RateFormulaParser.BoolAtomContext context)
	{
		var result = false;
		var boolEQContext = context as RateFormulaParser.BoolEQContext;
		if (boolEQContext != null)
		{
			result = EvaluateBoolEQ(boolEQContext);
		}
		else
		{
			var boolNEContext = context as RateFormulaParser.BoolNEContext;
			if (boolNEContext != null)
			{
				result = EvaluateBoolNE(boolNEContext);
			}
			else
			{
				var boolGTContext = context as RateFormulaParser.BoolGTContext;

				if (boolGTContext != null)
				{
					result = EvaluateBoolGT(boolGTContext);
				}
				else
				{
					var boolLTContext = context as RateFormulaParser.BoolLTContext;
					if (boolLTContext != null)
					{
						result = EvaluateBoolLT(boolLTContext);
					}
					else
					{
						var boolGTEQContext = context as RateFormulaParser.BoolGTEQContext;
						if (boolGTEQContext != null)
						{
							result = EvaluateBoolGTEQ(boolGTEQContext);
						}
						else
						{
							var boolLTEQContext = context as RateFormulaParser.BoolLTEQContext;
							if (boolLTEQContext != null)
							{
								result = EvaluateBoolLTEQ(boolLTEQContext);
							}
							else
							{
								var boolWRAPContext = context as RateFormulaParser.BoolWRAPContext;
								if (boolWRAPContext != null)
								{
									result = EvaluateBoolWRAP(boolWRAPContext);
								}
								else
								{
									var boolHASContext = context as RateFormulaParser.BoolHASContext;
									if (boolHASContext != null)
									{
										result = EvaluateBoolHAS(boolHASContext);
									}
									else
									{
										errorListener.Report(FormulaVisitErrorType.SyntaxError, "Encounter unknown bool expression.");
									}
								}
							}
						}
					}
				}
			}
		}
		return result;
	}

	bool EvaluateBoolHAS(RateFormulaParser.BoolHASContext context)
	{
		return EvaluateHasExpression(context.hasExpression());
	}

	bool EvaluateBoolEQ(RateFormulaParser.BoolEQContext context)
	{
		var result = Visit(context.op1).ResultAmount == Visit(context.op2).ResultAmount;
		return result;
	}

	bool EvaluateBoolNE(RateFormulaParser.BoolNEContext context)
	{
		var result = Visit(context.op1).ResultAmount != Visit(context.op2).ResultAmount;
		return result;
	}

	bool EvaluateBoolGT(RateFormulaParser.BoolGTContext context)
	{
		var result = Visit(context.op1).ResultAmount > Visit(context.op2).ResultAmount;
		return result;
	}

	bool EvaluateBoolGTEQ(RateFormulaParser.BoolGTEQContext context)
	{
		var result = Visit(context.op1).ResultAmount >= Visit(context.op2).ResultAmount;
		return result;
	}

	bool EvaluateBoolLT(RateFormulaParser.BoolLTContext context)
	{
		var result = Visit(context.op1).ResultAmount < Visit(context.op2).ResultAmount;
		return result;
	}

	bool EvaluateBoolLTEQ(RateFormulaParser.BoolLTEQContext context)
	{
		var result = Visit(context.op1).ResultAmount <= Visit(context.op2).ResultAmount;
		return result;
	}

	bool EvaluateBoolWRAP(RateFormulaParser.BoolWRAPContext context)
	{
		return EvaluateBoolExpression(context.boolExpression());
	}

	protected virtual bool EvaluateHasExpression(RateFormulaParser.HasExpressionContext context)
	{
		var typeToCheck = context.opleft.Text.Trim('"');
		var valueToCheck = context.opright.Text.Trim('"');
		return rateCalcData.AdditionalInformationList.Contains(new Tuple<string, string>(typeToCheck, valueToCheck));
	}

	#endregion

	#endregion

	#region VisitAtom

	public override RateFormulaResult VisitAtomExpression(RateFormulaParser.AtomExpressionContext context)
	{
		return Visit(context.exp);
	}

	public override RateFormulaResult VisitNumber(RateFormulaParser.NumberContext context)
	{
		var value = context.numberBody.Text;
		decimal result = 0M;
		var isParsed = decimal.TryParse(value, out result);
		if (!isParsed)
		{
			errorListener.Report(FormulaVisitErrorType.SyntaxError, FormattableString.Invariant($"Failed to convert \"{value}\" to Decimal"));
		}
		if (context.PERCENT() != null)
		{
			result /= 100M;
		}
		return new RateFormulaResult(result);
	}

	#endregion

	#region Getting Values from UniversalRateCalcData

	public override RateFormulaResult VisitReservedVFD(RateFormulaParser.ReservedVFDContext context)
	{
		var resultAmount = rateCalcData.ValueForDuty;
		var result = new RateFormulaResult(resultAmount);
		result.AddIntermediateResult(resultAmount);
		return result;
	}

	public override RateFormulaResult VisitReservedCV(RateFormulaParser.ReservedCVContext context)
	{
		var resultAmount = rateCalcData.CustomsValue;
		var result = new RateFormulaResult(resultAmount);
		result.AddIntermediateResult(resultAmount);
		return result;
	}

	public override RateFormulaResult VisitReservedDOV(RateFormulaParser.ReservedDOVContext context)
	{
		return new RateFormulaResult(rateCalcData.DateOfValuation.ToIsoDateOnlyNumericValue());
	}

	public override RateFormulaResult VisitUomPlaceHolder(RateFormulaParser.UomPlaceHolderContext context)
	{
		var result = new RateFormulaResult(0m);
		var uomCode = context.UOMCode().GetText().ToUpperInvariant().Trim('[', ']');

		if (rateCalcData.UnitOfMeasureValueList.TryGetValue(uomCode, out decimal resultAmount))
		{
			result.AddIntermediateResult(resultAmount, uomCode);
			result.ResultAmount = resultAmount;
		}
		else
		{
			errorListener.Report(FormulaVisitErrorType.UOMNotFound, FormattableString.Invariant($"Unit of Measure code: {uomCode} is not specified, using 0 for calculation"));
			result.AddIntermediateResult(0m, uomCode);
		}

		return result;
	}

	public override RateFormulaResult VisitVarCountrySpecificKeyword(RateFormulaParser.VarCountrySpecificKeywordContext context)
	{
		var result = new RateFormulaResult(0m);
		var countrySpecificValueName = context.code.Text.ToUpperInvariant();

		if (rateCalcData.CountrySpecificValueList.TryGetValue(countrySpecificValueName, out decimal resultAmount))
		{
			result.AddIntermediateResult(resultAmount, countrySpecificValueName);
			result.ResultAmount = resultAmount;
		}
		else
		{
			errorListener.Report(FormulaVisitErrorType.CountrySpecificValueNotFound, FormattableString.Invariant($"Country Specific Value: {countrySpecificValueName} is not specified, using 0 for calculation"));
			result.AddIntermediateResult(0m, countrySpecificValueName);
		}

		return result;
	}

	public override RateFormulaResult VisitMeursingPlaceHolder(RateFormulaParser.MeursingPlaceHolderContext context)
	{
		var result = new RateFormulaResult(0m);
		var measuringCode = context.MEURSINGCode().GetText().ToUpperInvariant().Trim('#', '#');

		if (rateCalcData.MeursingExpressionList.TryGetValue(measuringCode, out var meursingExpression))
		{
			var expressionTree = GetExpressionContextTree(meursingExpression, errorListener);
			result = VisitExpression(expressionTree);
			result.IntermediateResults.ForEach(r => r.OriginalMeursingExpression = measuringCode);
		}
		else
		{
			errorListener.Report(FormulaVisitErrorType.MeursingExpressionNotFound, FormattableString.Invariant($"Measuring Code: {measuringCode} is not specified, using 0 for calculation"));
			result.AddIntermediateResult(0m, measuringCode);
		}

		return result;
	}

	#endregion
}
