using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGRateFormulaExtractionVisitor : RateFormulaBaseVisitor<bool>
	{
		public SGRateFormulaExtractionVisitor(FormulaErrorListener errorListener, HashSet<ZString> unitOfMeasureValueList)
		{
			this.errorListener = Argument.NotNull(errorListener, "errorListener");
			this.unitOfMeasureValueList = Argument.NotNull(unitOfMeasureValueList, "unitOfMeasureValueList");
		}
		readonly FormulaErrorListener errorListener;
		readonly HashSet<ZString> unitOfMeasureValueList;

		public static SGTariffRate ExtractSGRate(string formulaString, HashSet<ZString> unitOfMeasureValueList)
		{
			SGTariffRate result = null;

			var errorListener = new FormulaErrorListener();
			var input = new AntlrInputStream(formulaString);
			var lexer = new RateFormulaLexer(input);
			lexer.RemoveErrorListener(ConsoleErrorListener<int>.Instance);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.RemoveErrorListener(ConsoleErrorListener<IToken>.Instance);
			parser.AddErrorListener(errorListener);

			var tree = parser.expression();
			if (!errorListener.Errors.Any())
			{
				var visitor = new SGRateFormulaExtractionVisitor(errorListener, unitOfMeasureValueList);
				result = visitor.ExtractSGRate(tree);
			}

			return result;
		}

		public SGTariffRate ExtractSGRate(RateFormulaParser.ExpressionContext tree)
		{
			SGTariffRate result = null;
			var hasErrors = VisitExpression(tree);
			if (!hasErrors)
			{
				if (hasVFD)
				{
					result = new SGTariffRate() { PercentageRate = rate * 100 };
				}
				else
				{
					result = new SGTariffRate() { UnitRate = rate, UnitQty = unitOfMeasureCode };
				}
			}
			return result;
		}

		public override bool VisitExpression(RateFormulaParser.ExpressionContext tree)
		{
			Reset();
			return base.VisitExpression(tree);
		}

		public override bool VisitReservedVFD([NotNull] RateFormulaParser.ReservedVFDContext context)
		{
			var hasError = hasVFD;
			hasVFD = true;
			return hasError;
		}

		public override bool VisitUomPlaceHolder([NotNull] RateFormulaParser.UomPlaceHolderContext context)
		{
			var hasError = !unitOfMeasureCode.IsEmpty;
			var uomCode = context.UOMCode().GetText().ToUpperInvariant().Trim('[', ']');
			if (unitOfMeasureValueList.Contains(uomCode))
			{
				unitOfMeasureCode = uomCode;
			}
			else
			{
				errorListener.Report(FormulaVisitErrorType.UOMNotFound, string.Format(CultureInfo.InvariantCulture, "Unit of Measure code: {0} is not specified", uomCode));
				hasError = true;
			}
			return hasError;
		}

		public override bool VisitNumber([NotNull] RateFormulaParser.NumberContext context)
		{
			var hasError = !rate.IsEmpty;
			var rateText = context.GetText();
			if (!ZDecimal.TryParse(rateText, out rate))
			{
				errorListener.Report(FormulaVisitErrorType.SyntaxError, string.Format(CultureInfo.InvariantCulture, "Rate value cannot be parsed: {0} ", rateText));
				hasError = true;
			}
			return hasError;
		}

		protected override bool AggregateResult(bool aggregate, bool nextResult)
		{
			return aggregate || nextResult;
		}

		public override bool VisitChildren(IRuleNode node)
		{
			if (!expectedTypesForSGFormulas.Contains(node.GetType()))
			{
				errorListener.Report(FormulaVisitErrorType.SyntaxError, string.Format(CultureInfo.InvariantCulture, "Unexpected expression context: {0} ", node.GetType().Name));
				return true;
			}

			return base.VisitChildren(node);
		}

		readonly HashSet<Type> expectedTypesForSGFormulas = new HashSet<Type>
		{
			typeof(RateFormulaParser.ExpressionContext),
			typeof(RateFormulaParser.VarReservedKeywordContext),
			typeof(RateFormulaParser.TimesExpContext),
			typeof(RateFormulaParser.AtomVariableContext),
			typeof(RateFormulaParser.AtomNumberContext),
			typeof(RateFormulaParser.VarUOMContext),
			typeof(RateFormulaParser.MultiplyingExpressionContext)
		};

		void Reset()
		{
			hasVFD = false;
			unitOfMeasureCode = ZString.Empty;
			rate = ZDecimal.Zero;
		}

		public ZBool hasVFD;
		public ZString unitOfMeasureCode;
		public ZDecimal rate;
	}
}
