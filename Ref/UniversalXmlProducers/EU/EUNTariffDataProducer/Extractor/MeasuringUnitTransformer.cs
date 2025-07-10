using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class MeasuringUnitTransformer : IMeasuringUnitTransformer
	{
		public string Transform(string rawRateFormula)
		{
			Argument.NotNullOrEmpty(rawRateFormula, nameof(rawRateFormula));
			return TransformCore(rawRateFormula);
		}

		public string ReplaceUom(string rawRateFormula, string replacementValue)
		{
			Argument.NotNullOrEmpty(rawRateFormula, nameof(rawRateFormula));
			return TransformCore(rawRateFormula, replacementValue);
		}

		public string[] GetUoms(string rawRateFormula)
		{
			Argument.NotNullOrEmpty(rawRateFormula, nameof(rawRateFormula));
			var result = new List<string>();
			foreach (var uomCode in UomCodeLookup.UomCodes)
			{
				var match = Regex.Match(rawRateFormula, uomCode.ActualUomCode).Value;
				if (!string.IsNullOrEmpty(match))
				{
					result.Add(match);
				}
			}
			return result.ToArray();
		}

		static string TransformCore(string rawRateFormula, string overrideValue = null)
		{
			Argument.NotNullOrEmpty(rawRateFormula, nameof(rawRateFormula));

			var transformedRateFormula = rawRateFormula;

			foreach (var uomCode in UomCodeLookup.UomCodes)
			{
				var uomRegEx = uomCode.RawUomCode.Replace(" ", @"\s?");
				var pattern = @"(?<!\[)\s?/?\s?" + uomRegEx + @"\s?(\(\S+\))?\s?(?![a-zA-Z\]])";

				var evaluator = new Regex(pattern);
				var replacementValue = $" * [{uomCode.ActualUomCode}]";

				if (overrideValue != null)
				{
					replacementValue = overrideValue;
				}

				transformedRateFormula = evaluator.Replace(transformedRateFormula, replacementValue);
			}

			transformedRateFormula = transformedRateFormula.Replace("]+", "] +");

			if (string.IsNullOrEmpty(transformedRateFormula))
			{
				transformedRateFormula = "0";
			}
			return transformedRateFormula;
		}
	}
}
