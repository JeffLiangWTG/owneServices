using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class LexicalErrorListener : IAntlrErrorListener<int>
	{
		public IEnumerable<ErrorInformation> Errors
		{
			get { return errors ?? Enumerable.Empty<ErrorInformation>(); }
		}

		List<ErrorInformation> errors;

		public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
		{
			var message = string.Format(System.Globalization.CultureInfo.InvariantCulture, (NoResString)"Lexical error at line {0} position {1}.", line, charPositionInLine + 1);
			Report(FormulaVisitErrorType.LexicalError, message);
		}

		public void Report(FormulaVisitErrorType errorType, string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				if (errors == null)
				{
					errors = new List<ErrorInformation>();
				}
				errors.Add(new ErrorInformation(errorType, message));
			}
		}
	}
}
