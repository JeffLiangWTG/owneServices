using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ConditionCalculator
	{
		public ConditionCalculator(string formulaString, IUniversalRateCalcData rateCalcData)
		{
			this.formulaString = formulaString.Trim();
			this.rateCalcData = new UniversalRateCalcDataWrapper(rateCalcData, new FormulaErrorListener());
			Initialize();
		}

		readonly string formulaString;
		readonly UniversalRateCalcDataWrapper rateCalcData;
		RateFormulaParser.BoolExpressionContext tree;

		public void AddAnswer(ZString question, decimal answer)
		{
			QuestionForFormulaSpecificValue data;
			if (rateCalcData.FormulaSpecificValueList.TryGetValue(question.Trim().ToUpperInvariant(), out data))
			{
				data.SetUserAnswer(answer);
			}
		}

		public IEnumerable<ErrorInformation> Errors
		{
			get { return ErrorListener.Errors; }
		}

		public bool HasParseError
		{
			get { return Errors.Any(); }
		}

		FormulaErrorListener ErrorListener
		{
			get { return rateCalcData.errorListener; }
		}

		void Initialize()
		{
			var input = new AntlrInputStream(formulaString);
			var lexer = new RateFormulaLexer(input);
			lexer.RemoveErrorListener(ConsoleErrorListener<int>.Instance);

			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.RemoveErrorListener(ConsoleErrorListener<IToken>.Instance);
			parser.AddErrorListener(rateCalcData.errorListener);

			tree = parser.boolExpression();
			if (Errors.Any())
			{
				tree = null;
			}
			else
			{
				ExtraData();
			}
		}

		void ExtraData()
		{
			var input = new AntlrInputStream(FormattableString.Invariant($"IF({formulaString},0,1)"));
			var lexer = new RateFormulaLexer(input);
			lexer.RemoveErrorListener(ConsoleErrorListener<int>.Instance);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.RemoveErrorListener(ConsoleErrorListener<IToken>.Instance);
			parser.AddErrorListener(rateCalcData.errorListener);
			var visitor = new RateFormulaExtractionVisitor(rateCalcData);
			visitor.VisitExpression(parser.expression());
		}

		public bool Evaluate()
		{
			var result = false;

			if (string.IsNullOrEmpty(formulaString))
			{
				result = true;
			}
			else if (tree != null && !HasParseError)
			{
				try
				{
					var visitor = new ConditionFormulaCalculatorVisitor(rateCalcData);
					result = visitor.VisitBoolExpression(tree) > decimal.Zero && !rateCalcData.errorListener.Errors.Any();
				}
				catch (OverflowException e)
				{
					ErrorListener.Report(FormulaVisitErrorType.CalculationError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Error when evaluating condition base on the input and the formula \r{0}", e.Message));
				}
				catch (DivideByZeroException e)
				{
					ErrorListener.Report(FormulaVisitErrorType.CalculationError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Error when evaluating condition base on the input and the formula \r{0}", e.Message));
				}
			}
			return result;
		}
	}
}
