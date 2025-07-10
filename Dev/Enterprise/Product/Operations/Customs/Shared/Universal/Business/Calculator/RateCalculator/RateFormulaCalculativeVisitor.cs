using System;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class RateFormulaCalculativeVisitor : RateFormulaBaseVisitor<decimal>
	{
		public RateFormulaCalculativeVisitor(UniversalRateCalcDataWrapper rateCalcData)
		{
			this.rateCalcData = Argument.NotNull(rateCalcData, "rateCalcData");
		}

		readonly UniversalRateCalcDataWrapper rateCalcData;
		public FormulaErrorListener errorListener { get { return rateCalcData.errorListener; } }

		#region BasicArithmeticExpressions

		public override decimal VisitExpression(RateFormulaParser.ExpressionContext context)
		{
			decimal result = 0m;
			var firstExp = context.multiplyingExpression();
			result = Visit(firstExp);
			foreach (var contExp in context.children.OfType<RateFormulaParser.ContExpressionContext>())
			{
				result += Visit(contExp);
			}

			return result;
		}

		public override decimal VisitContExpression(RateFormulaParser.ContExpressionContext context)
		{
			var result = Visit(context.contExp);
			if (context.operand.Type == RateFormulaLexer.MINUS)
			{
				result = 0 - result;
			}
			return result;
		}

		public override decimal VisitMultiplyingExpression(RateFormulaParser.MultiplyingExpressionContext context)
		{
			var result = Visit(context.atom());
			foreach (var contExp in context.children.OfType<RateFormulaParser.TimesExpContext>())
			{
				result *= Visit(contExp);
			}
			foreach (var contExp in context.children.OfType<RateFormulaParser.DivExpContext>())
			{
				result /= Visit(contExp);
			}
			return result;
		}

		public override decimal VisitTimesExp(RateFormulaParser.TimesExpContext context)
		{
			return Visit(context.contExp);
		}

		public override decimal VisitDivExp(RateFormulaParser.DivExpContext context)
		{
			return Visit(context.contExp);
		}

		#endregion

		#region MAX/MIN/ROUND/IF Expressions

		public override decimal VisitMaxExpression(RateFormulaParser.MaxExpressionContext context)
		{
			var op1 = Visit(context.opleft);
			var op2 = Visit(context.opright);
			return Math.Max(op1, op2);
		}

		public override decimal VisitMinExpression(RateFormulaParser.MinExpressionContext context)
		{
			var op1 = Visit(context.opleft);
			var op2 = Visit(context.opright);
			return Math.Min(op1, op2);
		}

		public override decimal VisitRoundExpression(RateFormulaParser.RoundExpressionContext context)
		{
			return decimal.Round(Visit(context.expression()), Utils.GetIntegerNumber(context.Integer().GetText(), errorListener), MidpointRounding.AwayFromZero);
		}

		public override decimal VisitIfExpression(RateFormulaParser.IfExpressionContext context)
		{
			var result = 0M;
			var condition = EvaluateBoolExpression(context.boolExpression());
			if (condition)
			{
				result = Visit(context.trueExp);
			}
			else
			{
				result = Visit(context.falseExp);
			}

			return result;
		}

		#region boolExpressions

		bool EvaluateBoolExpression(RateFormulaParser.BoolExpressionContext context)
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
											errorListener.Report(FormulaVisitErrorType.SyntaxError, (NoResString)"Encounter unknown bool expression.");
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
			var result = Visit(context.op1) == Visit(context.op2);
			return result;
		}

		bool EvaluateBoolNE(RateFormulaParser.BoolNEContext context)
		{
			var result = Visit(context.op1) != Visit(context.op2);
			return result;
		}

		bool EvaluateBoolGT(RateFormulaParser.BoolGTContext context)
		{
			var result = Visit(context.op1) > Visit(context.op2);
			return result;
		}

		bool EvaluateBoolGTEQ(RateFormulaParser.BoolGTEQContext context)
		{
			var result = Visit(context.op1) >= Visit(context.op2);
			return result;
		}

		bool EvaluateBoolLT(RateFormulaParser.BoolLTContext context)
		{
			var result = Visit(context.op1) < Visit(context.op2);
			return result;
		}

		bool EvaluateBoolLTEQ(RateFormulaParser.BoolLTEQContext context)
		{
			var result = Visit(context.op1) <= Visit(context.op2);
			return result;
		}

		bool EvaluateBoolWRAP(RateFormulaParser.BoolWRAPContext context)
		{
			return EvaluateBoolExpression(context.boolExpression());
		}

		bool EvaluateHasExpression(RateFormulaParser.HasExpressionContext context)
		{
			var typeToCheck = context.opleft.Text.Trim('"');
			var valueToCheck = context.opright.Text.Trim('"');
			return rateCalcData.AdditionalInformationList.Contains(new Tuple<string, string>(typeToCheck, valueToCheck));
		}

		#endregion

		#endregion

		#region VisitAtom

		public override decimal VisitAtomExpression(RateFormulaParser.AtomExpressionContext context)
		{
			return Visit(context.exp);
		}

		public override decimal VisitNumber(RateFormulaParser.NumberContext context)
		{
			var value = context.numberBody.Text;
			decimal result = 0M;
			var isParsed = decimal.TryParse(value, out result);
			if (!isParsed)
			{
				errorListener.Report(FormulaVisitErrorType.SyntaxError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Failed to convert \"{0}\" to Decimal", value));
			}
			if (context.PERCENT() != null)
			{
				result /= 100M;
			}
			return result;
		}

		#endregion

		#region Getting Values from UniversalRateCalcData

		public override decimal VisitReservedVFD(RateFormulaParser.ReservedVFDContext context)
		{
			return rateCalcData.ValueForDuty;
		}

		public override decimal VisitReservedCV(RateFormulaParser.ReservedCVContext context)
		{
			return rateCalcData.CustomsValue;
		}

		public override decimal VisitReservedDOV(RateFormulaParser.ReservedDOVContext context)
		{
			decimal result;
			return decimal.TryParse(rateCalcData.DateOfValuation.ToString("yyyyMMdd", CultureInfo.InvariantCulture), out result) ? result : decimal.Zero;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Message")]
		public override decimal VisitUomPlaceHolder(RateFormulaParser.UomPlaceHolderContext context)
		{
			var result = 0M;
			var uomCode = context.UOMCode().GetText().ToUpperInvariant().Trim('[', ']');
			if (!rateCalcData.UnitOfMeasureValueList.TryGetValue(uomCode, out result))
			{
				errorListener.Report(FormulaVisitErrorType.UOMNotFound, string.Format(CultureInfo.InvariantCulture, "Unit of Measure code: {0} is not specified, using 0 for calculation", uomCode));
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Message")]
		public override decimal VisitVarCountrySpecificKeyword(RateFormulaParser.VarCountrySpecificKeywordContext context)
		{
			var result = 0M;
			var countrySpecificValueName = context.code.Text.ToUpperInvariant();
			if (!rateCalcData.CountrySpecificValueList.TryGetValue(countrySpecificValueName, out result))
			{
				errorListener.Report(FormulaVisitErrorType.CountrySpecificValueNotFound, string.Format(CultureInfo.InvariantCulture, "Country Specific Value: {0} is not specified, using 0 for calculation", countrySpecificValueName));
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Message")]
		public override decimal VisitFormulaSpecificValue(RateFormulaParser.FormulaSpecificValueContext context)
		{
			var result = 0M;
			QuestionForFormulaSpecificValue questionWithAnswer = null;
			var question = context.questionToAsk.Text.Trim('"');
			var questionFormatted = question.ToUpperInvariant().Trim();
			if (!rateCalcData.FormulaSpecificValueList.TryGetValue(questionFormatted, out questionWithAnswer) || questionWithAnswer == null)
			{
				errorListener.Report(FormulaVisitErrorType.FormulaSpecificValueNotFound, string.Format(CultureInfo.InvariantCulture, "Formula Specific Value for question: {0} is not specified, using 0 for calculation", question));
			}
			return questionWithAnswer == null ? result : questionWithAnswer.Answer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Message")]
		public override decimal VisitMeursingPlaceHolder(RateFormulaParser.MeursingPlaceHolderContext context)
		{
			var result = 0M;
			var meursingCode = context.MEURSINGCode().GetText().ToUpperInvariant().Trim('#', '#');
			if (rateCalcData.MeursingExpressionList.TryGetValue(meursingCode, out string meursingExpression))
			{
				var input = new AntlrInputStream(meursingExpression);
				var lexer = new RateFormulaLexer(input);
				var tokens = new CommonTokenStream(lexer);
				var parser = new RateFormulaParser(tokens);
				parser.AddErrorListener(errorListener);
				var tree = parser.expression();
				result = new RateFormulaCalculativeVisitor(rateCalcData).Visit(tree);
			}
			else
			{
				errorListener.Report(FormulaVisitErrorType.MeursingExpressionNotFound, string.Format(CultureInfo.InvariantCulture, "Meursing code: {0} is not specified, using 0 for calculation", meursingCode));
			}
			return result;
		}

		#endregion
	}
}
