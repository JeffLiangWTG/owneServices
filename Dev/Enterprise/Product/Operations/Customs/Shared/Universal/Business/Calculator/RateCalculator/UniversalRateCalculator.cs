using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class UniversalRateCalculator
	{
		public UniversalRateCalculator(string formulaString, IUniversalRateCalcData rateCalcData)
		{
			this.formulaString = formulaString.Trim();
			this.rateCalcData = new UniversalRateCalcDataWrapper(rateCalcData, new FormulaErrorListener());
			Initialize();
		}

		readonly string formulaString;
		readonly UniversalRateCalcDataWrapper rateCalcData;
		RateFormulaParser.ExpressionContext tree;

		public IEnumerable<QuestionForFormulaSpecificValue> GetQuestionForFormulaSpecificValues()
		{
			return rateCalcData.FormulaSpecificValueList.Values;
		}

		public void AddAnswer(ZString question, decimal answer)
		{
			QuestionForFormulaSpecificValue data;
			if (rateCalcData.FormulaSpecificValueList.TryGetValue(question.Trim().ToUpperInvariant(), out data))
			{
				data.SetUserAnswer(answer);
			}
		}

		public IEnumerable<ErrorInformation> Errors => ErrorListener.Errors;

		public bool HasParseError => Errors.Any();

		FormulaErrorListener ErrorListener => rateCalcData.errorListener;

		LexicalErrorListener LexicalErrorListener => new LexicalErrorListener();

		void Initialize()
		{
			var errors = GetRateFormulaParseError(formulaString, out tree, ErrorListener, LexicalErrorListener, true);
			if (errors.IsEmpty)
			{
				var visitor = new RateFormulaExtractionVisitor(rateCalcData);
				visitor.VisitExpression(tree);
			}
		}

		public static ZString GetRateFormulaParseError(ZString rateFormula, out RateFormulaParser.ExpressionContext tree, FormulaErrorListener parserErrorListener = null, LexicalErrorListener lexerErrorListener = null, bool isSkipLexicalError = false)
		{
			lexerErrorListener = lexerErrorListener ?? new LexicalErrorListener();
			parserErrorListener = parserErrorListener ?? new FormulaErrorListener();
			var errors = ZString.Empty;

			var input = new AntlrInputStream(rateFormula);
			var lexer = new RateFormulaLexer(input);
			lexer.AddErrorListener(lexerErrorListener);
			lexer.RemoveErrorListener(ConsoleErrorListener<int>.Instance);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.RemoveErrorListener(ConsoleErrorListener<IToken>.Instance);
			parser.AddErrorListener(parserErrorListener);
			tree = parser.expression();

			var parserErrors = parserErrorListener.Errors.Select(x => x.ErrorMessage);
			var lexerErrors = lexerErrorListener.Errors.Select(x => x.ErrorMessage);
			if (parserErrors.Any() || (!isSkipLexicalError && lexerErrors.Any()))
			{
				var errorList = isSkipLexicalError ? parserErrors : parserErrors.Concat(lexerErrors);
				errors = string.Join("\r\n", errorList);
				tree = null;
			}

			if (string.IsNullOrEmpty(errors) && tree == null)
			{
				errors = Res.GetString("5379AB13-20DD-4A97-A5C9-2DDA1598958A", "The formula is not valid to parse to do rate calculations.");
			}

			return errors;
		}

		public decimal Calculate()
		{
			var result = 0M;

			if (tree != null && !HasParseError)
			{
				try
				{
					var visitor = new RateFormulaCalculativeVisitor(rateCalcData);
					result = visitor.VisitExpression(tree);
				}
				catch (OverflowException e)
				{
					ErrorListener.Report(FormulaVisitErrorType.CalculationError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Error when calculating rate base on the input and the formula \r{0}", e.Message));
				}
				catch (DivideByZeroException e)
				{
					ErrorListener.Report(FormulaVisitErrorType.CalculationError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Error when calculating rate base on the input and the formula \r{0}", e.Message));
				}
			}
			return result;
		}
	}
}
