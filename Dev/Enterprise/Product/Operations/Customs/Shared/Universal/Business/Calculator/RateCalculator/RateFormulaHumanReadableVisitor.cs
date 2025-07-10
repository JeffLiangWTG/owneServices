using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Result = Enterprise.Customs.Universal.RateFormulaHumanReadableVisitor.Result;

namespace Enterprise.Customs.Universal
{
	class RateFormulaHumanReadableVisitor : RateFormulaBaseVisitor<Result>
	{
		public RateFormulaHumanReadableVisitor(RateFormulaHumanReadableVisitorData data, FormulaErrorListener errorListener)
		{
			this.data = Argument.NotNull(data, "data");
			this.errorListener = Argument.NotNull(errorListener, "errorListener");
		}

		readonly RateFormulaHumanReadableVisitorData data;
		readonly FormulaErrorListener errorListener;

		#region BasicArithmeticExpressions

		public override Result VisitExpression(RateFormulaParser.ExpressionContext context)
		{
			var firstExp = context.multiplyingExpression();
			var result = Visit(firstExp);
			foreach (var contExp in context.children.OfType<RateFormulaParser.ContExpressionContext>())
			{
				var rightHandResult = Visit(contExp);
				result = new Result(result.HumanReadableString + rightHandResult.HumanReadableString, null, context);
			}

			return errorListener.Errors.Any() ? new Result(null, 0, context) : result;
		}

		public override Result VisitContExpression(RateFormulaParser.ContExpressionContext context)
		{
			var result = Visit(context.contExp);
			if (context.operand.Type == RateFormulaLexer.PLUS)
			{
				var humanReadableString = Res.GetString("E39289E9-BAB9-4A9E-BF9B-EE33E1A43DA8", " plus {0}", result.HumanReadableString);
				result = new Result(humanReadableString, result.Number, context);
			}
			else if (context.operand.Type == RateFormulaLexer.MINUS)
			{
				var humanReadableString = Res.GetString("03920425-5791-4139-9060-F94C5627A3D9", " minus {0}", result.HumanReadableString);
				result = new Result(humanReadableString, 0 - result.Number, context);
			}
			return result;
		}

		Stack<Result> DivisionCapture => divisionCapture ?? (divisionCapture = new Stack<Result>());
		Stack<Result> divisionCapture;

		public override Result VisitMultiplyingExpression(RateFormulaParser.MultiplyingExpressionContext context)
		{
			var result = Visit(context.atom());
			if (result.HumanReadableString != null)
			{
				foreach (var contExp in context.children.OfType<RateFormulaParser.TimesExpContext>())
				{
					var rightHandOperand = Visit(contExp);
					if (result.LastParserRuleContext is RateFormulaParser.UomPlaceHolderContext)
					{
						result = new Result(GetMultiplyingExpressionUomPlaceHolderContextString(rightHandOperand, result), rightHandOperand.Number, result.HasUnitsOfMeasure, context);
					}
					else if (rightHandOperand.LastParserRuleContext is RateFormulaParser.UomPlaceHolderContext)
					{
						var hasUoM = result.HasUnitsOfMeasure || rightHandOperand.HasUnitsOfMeasure;
						result = new Result(GetMultiplyingExpressionUomPlaceHolderContextString(result, rightHandOperand), result.Number, hasUoM, context);
					}
					else if (result.LastParserRuleContext is RateFormulaParser.ReservedVFDContext && rightHandOperand.Number != null)
					{
						result = new Result(GetMultiplyingExpressionReservedVFDContext(rightHandOperand), rightHandOperand.Number, result.HasUnitsOfMeasure, context);
					}
					else if (rightHandOperand.LastParserRuleContext is RateFormulaParser.ReservedVFDContext && result.Number != null)
					{
						var hasUoM = result.HasUnitsOfMeasure || rightHandOperand.HasUnitsOfMeasure;
						result = new Result(GetMultiplyingExpressionReservedVFDContext(result), result.Number, hasUoM, context);
					}
					else if (result.LastParserRuleContext is RateFormulaParser.VarCountrySpecificKeywordContext && rightHandOperand.LastParserRuleContext is RateFormulaParser.NumberContext)
					{
						result = new Result(GetMultiplyingExpressionVarCountrySpecificKeywordContext(rightHandOperand, result), null, context);
					}
					else if (rightHandOperand.LastParserRuleContext is RateFormulaParser.VarCountrySpecificKeywordContext && result.LastParserRuleContext is RateFormulaParser.NumberContext)
					{
						result = new Result(GetMultiplyingExpressionVarCountrySpecificKeywordContext(result, rightHandOperand), null, context);
					}
					else if (rightHandOperand.LastParserRuleContext is RateFormulaParser.NumberContext)
					{
						result = new Result(GetMultiplyingExpressionUomPlaceHolderContextString(rightHandOperand, result), rightHandOperand.Number, result.HasUnitsOfMeasure, context);
					}
					else
					{
						var humanReadableString = Res.GetString(
													"98470AA4-8D05-4049-8C8B-4D6D38A68442",
													"{0} multiplied by {1}",
													result.HumanReadableString,
													rightHandOperand.HumanReadableString);
						result = new Result(humanReadableString, null, context);
					}
				}
				foreach (var contExp in context.children.OfType<RateFormulaParser.DivExpContext>())
				{
					var rightHandOperand = Visit(contExp);
					if (rightHandOperand.LastParserRuleContext is RateFormulaParser.UomPlaceHolderContext)
					{
						var humanReadableString = Res.GetString(
							"CE86C003-7717-4D4B-95BC-F9330545514C",
							"{0} per {1}",
							data.CurrencyCode,
							rightHandOperand.HumanReadableString);
						DivisionCapture.Push(new Result(humanReadableString, result.Number, context));
						result = new Result(result.HumanReadableString, null, context);
					}
					else
					{
						var humanReadableString = Res.GetString(
							"7DD396F9-2194-4567-9679-FBC116362760",
							"{0} / {1}",
							result.HumanReadableString,
							rightHandOperand.HumanReadableString);
						result = new Result(humanReadableString, null, context);
					}
				}
			}
			return result;
		}

		string GetMultiplyingExpressionUomPlaceHolderContextString(Result result1, Result result2)
		{
			if (result1.HasUnitsOfMeasure)
			{
				return Res.GetString(
					"995e35b0-6810-4211-8f9d-54ff030f30e0",
					"{0} and {1}",
					result1.HumanReadableString,
					result2.HumanReadableString);
			}
			return Res.GetString(
				"1D2D4E19-52EB-4F0A-9952-370C905E0A62",
				"{0} {1} per {2}",
				result1.Number,
				data.CurrencyCode,
				result2.HumanReadableString);
		}

		string GetMultiplyingExpressionReservedVFDContext(Result result)
		{
			FormattableString formattableString = $"{result.Number:P1}";
			return Res.GetString(
				"EECD2168-44DA-4E1B-9F99-16AA19469897",
				"{0} of the Value for Duty",
				formattableString.ToString(Culture.GetCultureForLanguage(Res.CurrentLanguage))
			);
		}

		string GetMultiplyingExpressionVarCountrySpecificKeywordContext(Result result1, Result result2)
		{
			FormattableString formattableString = $"{result1.Number:P1}";
			return Res.GetString(
				"8D403559-6E38-40F3-9788-0144C14312F7",
				"{0} of {1}",
				formattableString.ToString(Culture.GetCultureForLanguage(Res.CurrentLanguage)),
				result2.HumanReadableString
			);
		}

		public override Result VisitTimesExp(RateFormulaParser.TimesExpContext context)
		{
			return Visit(context.contExp);
		}

		public override Result VisitDivExp(RateFormulaParser.DivExpContext context)
		{
			return Visit(context.contExp);
		}

		#endregion

		#region MAX/MIN/ROUND/IF Expressions

		public override Result VisitMaxExpression(RateFormulaParser.MaxExpressionContext context)
		{
			var op1 = Visit(context.opleft);
			var op2 = Visit(context.opright);
			string humanReadableString;

			if (HasParentContext<RateFormulaParser.MinExpressionContext>(context))
			{
				humanReadableString = Res.GetString("84A68B97-0484-4ACB-BB11-51F985BB3D21", "Greater of {0}, and {1}", op1.HumanReadableString, op2.HumanReadableString);
			}
			else if (context.Parent?.Parent is RateFormulaParser.MultiplyingExpressionContext multiplyingExpressionContext && multiplyingExpressionContext.ChildCount > 1)
			{
				humanReadableString = Res.GetString("28163E84-27B2-4035-9143-2DE83419BC57", "Greater of {0} and {1},", op1.HumanReadableString, op2.HumanReadableString);
			}
			else
			{
				humanReadableString = Res.GetString("F04B8FA8-1FC2-4850-9722-D6122C203383", "{0}, or {1}, whichever is greater", op1.HumanReadableString, op2.HumanReadableString);
			}

			return new Result(humanReadableString, null, context);
		}

		bool HasParentContext<TContext>(RuleContext context)
			where TContext : RuleContext
		{
			do
			{
				if (context is TContext)
				{
					return true;
				}

				context = context.Parent;
			} while (context != null);

			return false;
		}

		bool HasChildContext<TContext>(IParseTree context)
			where TContext : RuleContext
		{
			for (var i = 0; i < context.ChildCount; i++)
			{
				var child = context.GetChild(i);
				if (child is TContext || HasChildContext<TContext>(child))
				{
					return true;
				}
			}

			return false;
		}

		public override Result VisitMinExpression(RateFormulaParser.MinExpressionContext context)
		{
			var op1 = Visit(context.opleft);
			var op2 = Visit(context.opright);
			string humanReadableString;

			if (HasChildContext<RateFormulaParser.MaxExpressionContext>(context.opleft))
			{
				humanReadableString = Res.GetString("5FD4C6EA-4496-4FE5-A449-97A77A5D161A", "{0}, but not more than {1}", op1.HumanReadableString, op2.HumanReadableString);
			}
			else if (HasChildContext<RateFormulaParser.MaxExpressionContext>(context.opright))
			{
				humanReadableString = Res.GetString("DEB3349F-2B90-43CE-A698-2F2C9B3EFFFF", "{0}, but not more than {1}", op2.HumanReadableString, op1.HumanReadableString);
			}
			else
			{
				humanReadableString = Res.GetString("7CF98818-12F7-4F72-8A04-47E9B59EFFC4", "{0}, or {1}, whichever is lesser", op1.HumanReadableString, op2.HumanReadableString);
			}

			return new Result(humanReadableString, null, context);
		}

		public override Result VisitRoundExpression(RateFormulaParser.RoundExpressionContext context)
		{
			var op = Visit(context.expression());
			var humanReadableString = Res.GetString("0B5BECC0-7EBD-4EF8-AF1B-20B53C30FFC7", "round {0} to {1} decimal places", op.HumanReadableString, context.Integer().GetText());
			return new Result(humanReadableString, null, context);
		}

		int depth;
		bool isNested;

		public override Result VisitIfExpression(RateFormulaParser.IfExpressionContext context)
		{
			isNested = depth > 0;
			depth++;
			var conditionResult = EvaluateBoolExpression(context.boolExpression());
			var trueResult = Visit(context.trueExp);
			var falseResult = Visit(context.falseExp);

			var humanReadableString = Res.GetString("5CB6F044-834E-4C63-AA06-9B26C7798EF2",
				"when {0} then {1}, otherwise",
				conditionResult.HumanReadableString,
				trueResult.HumanReadableString
			);

			humanReadableString += isNested ? System.Environment.NewLine : " ";
			depth--;
			humanReadableString += falseResult.HumanReadableString;
			return new Result(humanReadableString, null, context);
		}

		#region boolExpressions

		Result EvaluateBoolExpression(RateFormulaParser.BoolExpressionContext context)
		{
			var result = EvaluateBoolAndExpression(context.basicExp);
			foreach (var contExp in context._contExp)
			{
				var contResult = EvaluateBoolAndExpression(contExp);
				result = new Result(Res.GetString("5103E4CD-F371-4BDF-98CF-8E2DA8FDAF0B", "{0} or {1}", result.HumanReadableString, contResult.HumanReadableString), null, context);
			}
			return result;
		}

		Result EvaluateBoolAndExpression(RateFormulaParser.BoolAndExpressionContext context)
		{
			var result = EvaluateBoolAtom(context.basicExp);
			foreach (var contExp in context._contExp)
			{
				var contResult = EvaluateBoolAtom(contExp);
				result = new Result(Res.GetString("055FB8FE-630B-4966-B728-0FB35A187D10", "{0} and {1}", result.HumanReadableString, contResult.HumanReadableString), null, context);
			}
			return result;
		}

		protected Result EvaluateBoolAtom(RateFormulaParser.BoolAtomContext context)
		{
			Result result = new Result("", 0m, context);
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
											errorListener.Report(FormulaVisitErrorType.SyntaxError, (NoResString)"Encounter unknown bool expression.");
										}
									}
								}
							}
						}
					}
				}
			}

			if (DivisionCapture.Count > 0)
			{
				result = new Result(Res.GetString("F066638A-D057-4DCF-9B1F-FB5C3CC695CC", "{0} {1}", result.HumanReadableString, DivisionCapture.Pop().HumanReadableString), null, context);
			}
			return result;
		}

		Result EvaluateBoolHAS(RateFormulaParser.BoolHASContext context)
		{
			return EvaluateHasExpression(context.hasExpression());
		}

		Result EvaluateBoolEQ(RateFormulaParser.BoolEQContext context)
		{
			var humanReadableString = Res.GetString("E3C17AB5-B18B-4ED1-B3F8-B283449EBBD4", "{0} is equal to {1}", Visit(context.op1).HumanReadableString, Visit(context.op2).HumanReadableString);
			return new Result(humanReadableString, null, context);
		}

		Result EvaluateBoolNE(RateFormulaParser.BoolNEContext context)
		{
			var humanReadableString = Res.GetString("72FFCA4F-2863-4533-9553-04845F16890E", "{0} is not equal to {1}", Visit(context.op1).HumanReadableString, Visit(context.op2).HumanReadableString);
			return new Result(humanReadableString, null, context);
		}

		Result EvaluateBoolGT(RateFormulaParser.BoolGTContext context)
		{
			var humanReadableString = Res.GetString("6B353B8B-3D3C-4494-92E0-BC203889F729", "{0} is greater than {1}", Visit(context.op1).HumanReadableString, Visit(context.op2).HumanReadableString);
			return new Result(humanReadableString, null, context);
		}

		Result EvaluateBoolGTEQ(RateFormulaParser.BoolGTEQContext context)
		{
			var humanReadableString = Res.GetString("DCB61F92-731A-4012-8E96-030FE21F5991", "{0} is greater than or equal to {1}", Visit(context.op1).HumanReadableString, Visit(context.op2).HumanReadableString);
			return new Result(humanReadableString, null, context);
		}

		Result EvaluateBoolLT(RateFormulaParser.BoolLTContext context)
		{
			var humanReadableString = Res.GetString("F8CEA413-C316-453E-98CF-24BDE0607FE6", "{0} is less than {1}", Visit(context.op1).HumanReadableString, Visit(context.op2).HumanReadableString);
			return new Result(humanReadableString, null, context);
		}

		Result EvaluateBoolLTEQ(RateFormulaParser.BoolLTEQContext context)
		{
			var humanReadableString = Res.GetString("158E28AE-4725-4961-AC18-0CD4277F21F3", "{0} is less than or equal to {1}", Visit(context.op1).HumanReadableString, Visit(context.op2).HumanReadableString);
			return new Result(humanReadableString, null, context);
		}

		Result EvaluateBoolWRAP(RateFormulaParser.BoolWRAPContext context)
		{
			return EvaluateBoolExpression(context.boolExpression());
		}

		Result EvaluateHasExpression(RateFormulaParser.HasExpressionContext context)
		{
			var typeToCheck = context.opleft.Text.Trim('"');
			var valueToCheck = context.opright.Text.Trim('"');
			if (!data.AdditionalInformationList.TryGetValue(typeToCheck, out var name))
			{
				name = typeToCheck;
			}
			return new Result(Res.GetString("7B560464-74ED-4030-8B82-1CB3E6603512", "{0} {1} presented", name, valueToCheck), null, context);
		}

		#endregion

		#endregion

		#region VisitAtom

		public override Result VisitAtomExpression(RateFormulaParser.AtomExpressionContext context)
		{
			return Visit(context.exp);
		}

		public override Result VisitNumber(RateFormulaParser.NumberContext context)
		{
			var value = context.numberBody.Text;
			var isParsed = decimal.TryParse(value, out var result);
			if (!isParsed)
			{
				errorListener.Report(FormulaVisitErrorType.SyntaxError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Failed to convert \"{0}\" to Decimal", value));
			}
			if (context.PERCENT() != null)
			{
				result /= 100M;
			}
			return new Result(value, result, context);
		}

		#endregion

		#region Getting Values from UniversalRateCalcData

		public override Result VisitReservedVFD(RateFormulaParser.ReservedVFDContext context)
		{
			return new Result(Res.GetString("68347E17-B47B-43BC-AC7D-E1F4D1BFE0A8", "value for duty"), null, true, context);
		}

		public override Result VisitReservedCV(RateFormulaParser.ReservedCVContext context)
		{
			return new Result(Res.GetString("315E5611-44AA-4629-95F5-9D44C8A6AEBC", "customs value"), null, context);
		}

		public override Result VisitReservedDOV(RateFormulaParser.ReservedDOVContext context)
		{
			return new Result(Res.GetString("01787E53-6136-4CF1-9C4A-7D54B810DB8F", "date of valuation"), null, context);
		}

		public override Result VisitUomPlaceHolder(RateFormulaParser.UomPlaceHolderContext context)
		{
			var uomCode = context.UOMCode().GetText().ToUpperInvariant().Trim('[', ']');
			if (data.UnitOfMeasureList.TryGetValue(uomCode, out var description))
			{
				uomCode = description;
			}
			return new Result(uomCode, null, true, context);
		}

		public override Result VisitVarCountrySpecificKeyword(RateFormulaParser.VarCountrySpecificKeywordContext context)
		{
			var countrySpecificValueName = context.code.Text.ToUpperInvariant();
			if (data.CountrySpecificList.TryGetValue(countrySpecificValueName, out var description))
			{
				countrySpecificValueName = description;
			}
			return new Result(countrySpecificValueName, null, context);
		}

		public override Result VisitFormulaSpecificValue(RateFormulaParser.FormulaSpecificValueContext context)
		{
			var question = context.questionToAsk.Text.Trim('"');
			return new Result(question, null, context);
		}

		public override Result VisitMeursingPlaceHolder(RateFormulaParser.MeursingPlaceHolderContext context)
		{
			var meursingHumanReadableString = context.MEURSINGCode().GetText().ToUpperInvariant().Trim('#', '#');
			if (data.MeursingExpressionList.TryGetValue(meursingHumanReadableString, out var description))
			{
				meursingHumanReadableString = description;
			}
			return new Result(meursingHumanReadableString, null, context);
		}

		#endregion

		public class Result
		{
			public string HumanReadableString { get; }
			public decimal? Number { get; }
			public bool HasUnitsOfMeasure { get; }
			public ParserRuleContext LastParserRuleContext { get; }

			public Result(string humanReadableString, decimal? number, ParserRuleContext lastParserRuleContext)
				: this(humanReadableString, number, false, lastParserRuleContext) { }

			public Result(string humanReadableString, decimal? number, bool hasUnitsOfMeasure, ParserRuleContext lastParserRuleContext)
			{
				HumanReadableString = humanReadableString;
				Number = number;
				LastParserRuleContext = lastParserRuleContext;
				HasUnitsOfMeasure = hasUnitsOfMeasure;
			}
		}
	}
}
