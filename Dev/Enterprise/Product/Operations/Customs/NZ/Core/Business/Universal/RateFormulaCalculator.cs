using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business
{
	public static class RateFormulaCalculator
	{
		public static ZDecimal CalculateDuty(RateFormulaCalculativeVisitor formulaVisitor, ZString formulaString, bool omitVFDCalculation, bool omitUOMCalculation, bool saveVFDRate = false)
		{
			var result = 0M;
			formulaVisitor.Reset(omitVFDCalculation, omitUOMCalculation, saveVFDRate);
			var errorListener = formulaVisitor.errorListener;
			var errors = UniversalRateCalculator.GetRateFormulaParseError(formulaString, out var tree, errorListener, null, true);
			if (errors.IsEmpty && tree != null)
			{
				try
				{
					result = formulaVisitor.VisitExpression(tree);
				}
				catch (OverflowException e)
				{
					errorListener.Report(FormulaVisitErrorType.CalculationError, string.Format(CultureInfo.InvariantCulture, "Error when calculating rate base on the input and the formula \r{0}", e.Message));
				}
				catch (DivideByZeroException e)
				{
					errorListener.Report(FormulaVisitErrorType.CalculationError, string.Format(CultureInfo.InvariantCulture, "Error when calculating rate base on the input and the formula \r{0}", e.Message));
				}
			}

			return result;
		}
	}
}
