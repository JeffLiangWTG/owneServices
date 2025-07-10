using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public class FormulaExtractor
	{
		public static List<Tuple<string, string>> GetFormula(string value)
		{
			if (value.StartsWith("COND: ")) return GetConditionRateFormula(value);
			value = new Regex(EURRegex).Replace(value, string.Empty);
			value = TransformKeyWords(value);

			if (value == null) return new List<Tuple<string, string>>();
			value = value.ToUpper();
			value = new Regex(ZeroFormulaRegex).Replace(value, "0");

			value = GetValueForDuty(value);
			return GenerateFormulas(value);
		}

		public static List<Tuple<string, string>> GetConditionRateFormula(string value)
		{
			value = value.Substring(6);
			value = new Regex(EURRegex).Replace(value, string.Empty);
			var ranges = value.Split(';');
			string VFDPercentage = "";
			string result = "";
			foreach (var range in ranges)
			{
				var values = range.Trim().Substring(1).Split(':');
				if (values.Length == 2)
				{
					values[0] = RemoveKeyWords(values[0]).Trim();
					values[1] = TransformKeyWords(values[1]).Trim();
					if (string.IsNullOrEmpty(VFDPercentage))
					{
						VFDPercentage = values[1];
						result += GetValueForDuty(values[1]) + " + ";
						values[1] = values[1].Replace(VFDPercentage, string.Empty);
					}
					values[1] = values[1].Replace(VFDPercentage + " + ", string.Empty);
					if (values[0] != "0") result += "if (VFD/[DTN] > " + values[0];
					if (string.IsNullOrEmpty(GetValueForDuty(values[1]))) values[1] = "0";
					if (values[0] != "0")
					{
						result += "," + GetValueForDuty(values[1]) + ",(";
					}
					else
					{
						result += GetValueForDuty(values[1]) + ")";
					}
				}
			}
			var neededClosingBrackets = result.Count(f => f == '(') - result.Count(f => f == ')');
			result = result + string.Concat(Enumerable.Repeat(")", neededClosingBrackets));
			return new List<Tuple<string, string>> { new Tuple<string, string>(result, "A00") };
		}

		static string GetValueForDuty(string value)
		{
			var matches = new Regex(PercentageRegex).Matches(value);
			foreach (Match match in matches)
			{
				value = value.Replace(match.Value, "VFD * " + (Convert.ToDecimal(match.Groups[1].Value) / 100).ToString());
			}
			return value;
		}

		static string TransformKeyWords(string value)
		{
			Argument.NotNull(value, nameof(value));
			foreach (var keyword in keyWords)
			{
				value = value.Replace(keyword, " * [" + keyword.Replace(" ", "") + "]");
			}
			return value;
		}

		static string RemoveKeyWords(string value)
		{
			Argument.NotNull(value, nameof(value));
			foreach (var keyword in keyWords)
			{
				value = value.Replace(keyword, string.Empty);
			}
			return value;
		}

		static List<Tuple<string, string>> GenerateFormulas(string transformedValue)
		{
			Argument.NotNull(transformedValue, nameof(transformedValue));
			var result = new List<Tuple<string, string>>();
			var a20Matches = new Regex(A20FormulaRegex).Matches(transformedValue);
			if (a20Matches.Count != 0)
			{
				foreach (Match match in a20Matches)
				{
					transformedValue = transformedValue.Replace(match.Value, "[" + match.Value + "]");
				}
			}
			var split = a20Matches.Count != 0 ? transformedValue.Split('+') : new[] { transformedValue };
			foreach (var item in split)
			{
				var a20Match = Regex.Match(item, A20FormulaRegex);
				var ratecode = a20Match.Length == 0 ? "A00" : "A20";
				var maxFormula = Regex.Match(item, BoundFormulaRegex);
				if (maxFormula.Length == 0)
				{
					result.Add(new Tuple<string, string>(item.Trim(), ratecode));
				}
				else
				{
					result.Add(new Tuple<string, string>(GetBoundFormula(maxFormula.ToString(), item.Replace(maxFormula.ToString(), string.Empty)), ratecode));
				}
			}
			return result;
		}

		static bool IsFormulaSpecialCase(string value)
		{
			return Regex.Match(value, A20SpecialCaseRegex).Length != 0;
		}

		static string GetBoundFormula(string boundFormula, string value)
		{
			var formulaType = boundFormula.Substring(0, 3);
			return (formulaType == "MIN" ? "MAX" : "MIN") + "(" + value.Trim() + ", " + boundFormula.Replace(formulaType, string.Empty).Trim() + ")";
		}

		private static string[] keyWords = { "DTN", "TNE", "KGM P" };
		static string ZeroFormulaRegex = @"^(0|\.)+ ?%+|(?<![0-9])0 \* \[[A-Z%]*\]|[A-Z]* ?\* ?0";
		static string EURRegex = @"\s?EUR\s?";
		static string PercentageRegex = @"(\d+\.?((?<=\.)\d+)?)\s?%";
		static string A20FormulaRegex = @"(EAR|EA|ADSZR|ADSZ|ADFMR|ADFM)\([0-9]\)";
		static string BoundFormulaRegex = @"(MAX|MIN)+[\s][A-Z0-9. *\[\]]*";
		static string A20SpecialCaseRegex = @"((\d+\.?((?<=\.)\d+)?)\s?%\s[+]\s)(EAR|EA|ADSZR|ADSZ|ADFMR|ADFM)\s?(MAX) ?((\d+\.?((?<=\.)\d+)?)\s?%\s?[+]\s?)(EAR|EA|ADSZR|ADSZ|ADFMR|ADFM)\s?(MAX)\s?(\d+\.?((?<=\.)\d+)?)\s?(EUR)\s?(DTN)";
	}
}
