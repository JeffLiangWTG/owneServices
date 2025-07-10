using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public enum FormulaVisitErrorType
	{
		SyntaxError,
		UOMNotFound,
		CountrySpecificValueNotFound,
		FormulaSpecificValueNotFound,
		CalculationError,
		DuplicateKeyValue,
		LexicalError,
		MeursingExpressionNotFound,
	}

	public struct ErrorInformation
	{
		public ErrorInformation(FormulaVisitErrorType type, string errorMessage)
		{
			Type = type;
			ErrorMessage = errorMessage;
		}

		internal readonly FormulaVisitErrorType Type;
		internal readonly string ErrorMessage;

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", Type, ErrorMessage);
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ ErrorMessage.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ErrorInformation))
			{
				return false;
			}
			else
			{
				return Equals((ErrorInformation)obj);
			}
		}

		public bool Equals(ErrorInformation other)
		{
			return Type == other.Type && ErrorMessage == other.ErrorMessage;
		}

		public static bool operator ==(ErrorInformation obj1, ErrorInformation obj2)
		{
			return obj1.Equals(obj2);
		}

		public static bool operator !=(ErrorInformation obj1, ErrorInformation obj2)
		{
			return !obj1.Equals(obj2);
		}
	}

	public class FormulaErrorListener : BaseErrorListener
	{
		public IEnumerable<ErrorInformation> Errors
		{
			get { return errors ?? Enumerable.Empty<ErrorInformation>(); }
		}

		List<ErrorInformation> errors;

		public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
		{
			var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Syntax error at line {0} position {1}.", line, charPositionInLine + 1);
			Report(FormulaVisitErrorType.SyntaxError, message);
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
