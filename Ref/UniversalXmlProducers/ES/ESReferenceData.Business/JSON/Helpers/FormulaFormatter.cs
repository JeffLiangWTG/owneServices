using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public static class FormulaFormatter
	{
		public static class FormulaKeys
		{
			public const string Minimum = "MIN";
			public const string Maximum = "MAX";
			public const char Mixed = '+';
			public const string Specific = "EUR";
			public const string Percentage = "%";
			public const string Free = "0 %";
		}

		const string MinCW1Function = "MIN";
		const string MaxCw1Function = "MAX";
		const string FreeCW1Formula = "0";
		const string CustomsValueBase = "VFD";
		const string RetailPriceBase = "PVP";

		public static string ParseJsonFormula(string jsonFormat, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList, bool isAdValorem = true)
		{
			var result = string.Empty;
			if (jsonFormat.Contains(FormulaKeys.Maximum) && jsonFormat.Contains(FormulaKeys.Minimum))
			{
				result = ParseMaximumAndMinimum(jsonFormat, uomMapCodesList, isAdValorem);
			}
			else if (jsonFormat.Contains(FormulaKeys.Maximum))
			{
				result = ParseMaximum(jsonFormat, uomMapCodesList, isAdValorem);
			}
			else if (jsonFormat.Contains(FormulaKeys.Minimum))
			{
				result = ParseMinimum(jsonFormat, uomMapCodesList, isAdValorem);
			}
			else if (jsonFormat.Contains(FormulaKeys.Mixed.ToString()))
			{
				result = ParseMixed(jsonFormat, uomMapCodesList, isAdValorem);
			}
			else if (jsonFormat.Contains(FormulaKeys.Specific))
			{
				result = ParseSpecific(jsonFormat, uomMapCodesList);
			}
			else if (jsonFormat.Contains(FormulaKeys.Percentage))
			{
				if (string.Equals(jsonFormat, FormulaKeys.Free, System.StringComparison.Ordinal))
				{
					result = ParseFree();
				}
				else if (isAdValorem)
				{
					result = ParseAdValorem(jsonFormat);
				}
				else
				{
					result = ParseExciseRateBasedOnPVP(jsonFormat);
				}
			}

			return result;
		}

		static string ParseSpecific(string jsonFormat, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList)
		{
			var result = string.Empty;

			var uomArray = jsonFormat.Replace(FormulaKeys.Specific, "-").Split('-');
			if (uomArray.Length == 2)
			{
				var uom = GetUOM(uomArray[1].Trim(), uomMapCodesList);
				if (!string.IsNullOrEmpty(uom))
				{
					var valueToParse = uomArray[0].Trim().Replace(",", ".");
					if (double.TryParse(valueToParse, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double value))
					{
						result = string.Format(CultureInfo.InvariantCulture, "{0}*[{1}]", value.ToString("0.#####", CultureInfo.InvariantCulture), uom);
					}
				}
			}

			return result;
		}

		public static string GetUOM(string duty, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList)
		{
			string uom = duty ?? string.Empty;
			if (duty != null && uomMapCodesList != null)
			{
				var uomMap = uomMapCodesList.FirstOrDefault(x => x.ZZM_CustomsValue == duty);
				if (uomMap != null)
				{
					uom = uomMap.ZZM_CW1orCommercialValue;
				}
			}

			return uom;
		}

		static string ParseMixed(string jsonFormat, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList, bool isAdValorem)
		{
			var result = string.Empty;

			var mixedArray = jsonFormat.Split(FormulaKeys.Mixed);
			if (mixedArray.Length == 2)
			{
				string formulaPart1;
				if (isAdValorem)
				{
					formulaPart1 = ParseAdValorem(mixedArray[0]);
				}
				else
				{
					formulaPart1 = ParseExciseRateBasedOnPVP(mixedArray[0]);
				}
				var formulaPart2 = ParseSpecific(mixedArray[1], uomMapCodesList);

				if (!string.IsNullOrEmpty(formulaPart1) && !string.IsNullOrEmpty(formulaPart2))
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0} + {1}", formulaPart1, formulaPart2);
				}
			}

			return result;
		}

		static string GetMaxOrMinFormula(string jsonFormat, string formulaKey, string cw1Function, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList, bool isAdValorem)
		{
			var result = string.Empty;
			var maximumAray = jsonFormat.Replace(formulaKey, "-").Split('-');
			if (maximumAray.Length == 2)
			{
				string percentageFormula;
				if (isAdValorem)
				{
					percentageFormula = ParseAdValorem(maximumAray[0]);
				}
				else
				{
					percentageFormula = ParseExciseRateBasedOnPVP(maximumAray[0]);
				}

				var uomFormula = ParseSpecific(maximumAray[1], uomMapCodesList);
				if (!string.IsNullOrEmpty(percentageFormula) && !string.IsNullOrEmpty(uomFormula))
				{
					result = $"{cw1Function}({percentageFormula}, {uomFormula})";
				}
			}
			return result;
		}

		static string ParseMaximum(string jsonFormat, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList, bool isAdValorem)
			=> GetMaxOrMinFormula(jsonFormat, FormulaKeys.Maximum, MinCW1Function, uomMapCodesList, isAdValorem);

		static string ParseMinimum(string jsonFormat, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList, bool isAdValorem)
			=> GetMaxOrMinFormula(jsonFormat, FormulaKeys.Minimum, MaxCw1Function, uomMapCodesList, isAdValorem);

		static string ParseMaximumAndMinimum(string jsonFormat, List<RefCusMapUOMSchema.MapCodes> uomMapCodesList, bool isAdValorem)
		{
			var result = string.Empty;
			var maxMinArray = jsonFormat.Replace(FormulaKeys.Maximum, "-").Split('-');
			if (maxMinArray.Length == 2)
			{
				var minFormula = ParseMinimum(maxMinArray[0], uomMapCodesList, isAdValorem);
				var uomMaxFormula = ParseSpecific(maxMinArray[1], uomMapCodesList);
				if (!string.IsNullOrEmpty(minFormula) && !string.IsNullOrEmpty(uomMaxFormula))
				{
					result = $"{MinCW1Function}({minFormula}, {uomMaxFormula})";
				}
			}
			return result;
		}

		static string ParseFree() => FreeCW1Formula;

		static string ParseAdValorem(string jsonFormat) => GetCW1Formula(jsonFormat, CustomsValueBase);

		static string ParseExciseRateBasedOnPVP(string jsonFormat) => GetCW1Formula(jsonFormat, RetailPriceBase);

		static string GetCW1Formula(string jsonFormat, string rateBase)
		{
			(bool parsedSuccessfully, double value) GetPercentageValue(string formula)
			{
				var value = formula.Replace(FormulaKeys.Percentage, string.Empty).Trim().Replace(',', '.');
				return (double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double result), result);
			}

			var valueResult = GetPercentageValue(jsonFormat);

			var cw1Formula = string.Empty;
			if (valueResult.parsedSuccessfully)
			{
				var valueForCW1Formula = valueResult.value / 100;
				cw1Formula = $"{valueForCW1Formula.ToString("0.#####", CultureInfo.InvariantCulture)}*{rateBase}";
			}
			return cw1Formula;
		}
	}
}
