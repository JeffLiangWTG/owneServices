using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	internal static class FormulaExtractorHelper
	{
		const string PercentageRegex = @"(\d+\.?((?<=\.)\d+)?)\s?%";
		const string BoundFormulaRegex = @"(MAX|MIN)+[\s][A-Z0-9. *\[\]]*";
		const string BoundFormulaSplitRegex = @"(^(MIN|MAX))*(MIN|MAX)";
		internal static string AgricuturalComponentFormulaRegex = @"EAR|EA|ADSZR|ADSZ|ADFMR|ADFM";
		internal static string EURRegex = @"\s?EUR\s?";
		internal static string ZeroFormulaRegex = @"^(0|\.)+ ?%+$|^(?<![0-9])0 \* \[[A-Z%]*\]|[A-Z]* ?\* ?0$|^0$|^NIHIL$";
		internal static string ZeroPercentageRegex = @"(?<![1-9]+)0[\.][0]+ %";

		internal static string GetValueForDuty(string rateFormula)
		{
			return GetPercentageFormula(rateFormula, "VFD");
		}

		internal static string GetCostInsuranceFreight(string rateFormula)
		{
			return GetPercentageFormula(rateFormula, "CIF");
		}

		static string GetPercentageFormula(string rateFormula, string unit)
		{
			if (string.IsNullOrEmpty(rateFormula))
			{
				return rateFormula;
			}

			if (rateFormula == "0.000 %")
			{
				return "0";
			}

			if (rateFormula.StartsWith("0.000 % + ", StringComparison.Ordinal))
			{
				rateFormula = rateFormula.Replace("0.000 % + ", "");
			}

			var matches = new Regex(PercentageRegex).Matches(rateFormula);
			var result = matches.Cast<Match>().OrderByDescending(x => x.Length).Aggregate(rateFormula,
				(current, match) => current.Replace(match.Value, $"{unit} * " + Convert.ToDecimal(match.Groups[1].Value, CultureInfo.InvariantCulture) / 100));

			if (string.IsNullOrEmpty(result) || result.StartsWith("0.000 *", StringComparison.Ordinal))
			{
				result = "0";
			}

			return result.TrimStart().TrimEnd();
		}

		internal static string AdjustForBoundFormula(string rateFormula)
		{
			Argument.NotNullOrEmpty(rateFormula, nameof(rateFormula));

			var boundFormulaMatch = Regex.Match(rateFormula, BoundFormulaRegex);
			var match = boundFormulaMatch.ToString();
			var formula = match.Length == 0
				? rateFormula
				: GetBoundFormulaFromRateFormula(rateFormula);
			return formula;
		}

		internal static string GetBoundFormulaFromRateFormula(string rateFormula)
		{
			Argument.NotNullOrEmpty(rateFormula, nameof(rateFormula));
			var result = string.Empty;
			var formulaParts = Regex.Split(rateFormula, BoundFormulaSplitRegex).ToList();
			if (formulaParts.Count % 2 != 0 && formulaParts.Count >= 3)
			{
				while (formulaParts.Count >= 3)
				{
					if (formulaParts[0] != null && formulaParts[1] != null && formulaParts[2] != null)
					{
						if (formulaParts[1] == "MAX")
						{
							result = $"MIN({formulaParts[0].Trim()}, {formulaParts[2].Trim()})";
						}
						if (formulaParts[1] == "MIN")
						{
							result = $"MAX({formulaParts[0]?.Trim()}, {formulaParts[2].Trim()})";
						}
						formulaParts.RemoveRange(1, 2);
						formulaParts[0] = result;
					}
				}
			}
			return result;
		}

		internal static string GetBoundFormula(string boundFormula, string baseValue)
		{
			Argument.NotNullOrEmpty(boundFormula, nameof(boundFormula));

			var rateFormula = "";
			var boundType = boundFormula.Substring(0, 3);
			boundFormula = boundFormula.Replace(boundType, string.Empty).Trim();

			var boundFormulaMatch = Regex.Match(boundFormula, BoundFormulaRegex);
			if (boundFormulaMatch.Length == 0)
			{
				rateFormula = $"{(boundType == "MIN" ? "MAX" : "MIN")}({baseValue.Trim()}, {boundFormula})";
				if (string.IsNullOrEmpty(rateFormula))
				{
					rateFormula = "0";
				}
				return rateFormula;
			}

			var match = boundFormulaMatch.ToString();
			string maxValue;
			string minValue;
			var innerBoundType = match.Substring(0, 3);
			if (boundType == "MAX")
			{
				maxValue = boundFormula.Replace(match, string.Empty);
				minValue = match.Replace(innerBoundType, string.Empty);
			}
			else
			{
				maxValue = match.Replace(innerBoundType, string.Empty);
				minValue = boundFormula.Replace(match, string.Empty);
			}

			rateFormula = $"MIN(MAX({baseValue.Trim()}, {minValue.Trim()}), {maxValue.Trim()})";

			if (string.IsNullOrEmpty(rateFormula))
			{
				rateFormula = "0";
			}
			return rateFormula;
		}
	}
}
