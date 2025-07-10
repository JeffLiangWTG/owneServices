using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public static class DutyCalculatorHelper
	{
		public static decimal Calculate(IUniversalRateCalcData universalRateCalcData, ZString rateFormula, int dutyDecimalPlaces, ZString formulaSpecificQuestion, ZDecimal formulaSpecificValue, bool shouldTruncate, bool canBeNegative)
		{
			var calculator = new UniversalRateCalculator(rateFormula, universalRateCalcData);
			if (!formulaSpecificQuestion.IsEmpty)
			{
				calculator.AddAnswer(formulaSpecificQuestion, formulaSpecificValue);
			}
			var result = shouldTruncate ? Math.Truncate(calculator.Calculate()) : Utilities.Round(calculator.Calculate(), dutyDecimalPlaces);

			return canBeNegative ? result : Math.Max(result, 0m);
		}
	}
}
