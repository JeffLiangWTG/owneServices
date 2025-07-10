using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class AgriculturalComponentFormulaExtractor : IFormulaExtractor
	{
		readonly IMeasuringUnitTransformer measuringUnitTransformer;

		public AgriculturalComponentFormulaExtractor(IMeasuringUnitTransformer measuringUnitTransformer)
		{
			Argument.NotNull(measuringUnitTransformer, nameof(measuringUnitTransformer));

			this.measuringUnitTransformer = measuringUnitTransformer;
		}

		public IEnumerable<IFormulaExtractionResult> GetFormula(string rawRateFormula, string rateCode, string reductionIndicator)
		{
			Argument.NotNullOrEmpty(rawRateFormula, nameof(rawRateFormula));
			Argument.NotNullOrEmpty(rateCode, nameof(rateCode));
			Argument.NotNull(reductionIndicator, nameof(reductionIndicator));
			var result = new List<FormulaExtractionResult>();
			var rateFormula = TransformFormula(rawRateFormula, reductionIndicator);
			var formula = FormulaExtractorHelper.AdjustForBoundFormula(rateFormula);
			result.Add(new FormulaExtractionResult(formula, rateCode));

			return result;
		}

		string TransformFormula(string rawRateFormula, string reductionIndicator)
		{
			Argument.NotNull(rawRateFormula, nameof(rawRateFormula));
			Argument.NotNull(reductionIndicator, nameof(reductionIndicator));

			var rateFormulaWithoutEur = new Regex(FormulaExtractorHelper.EURRegex).Replace(rawRateFormula.Trim(), string.Empty);
			var rateFormulaWithUom = measuringUnitTransformer.Transform(rateFormulaWithoutEur);

			rateFormulaWithUom = new Regex(FormulaExtractorHelper.ZeroPercentageRegex).Replace(rateFormulaWithUom, "0");
			rateFormulaWithUom = new Regex(FormulaExtractorHelper.ZeroFormulaRegex).Replace(rateFormulaWithUom, "0");

			rateFormulaWithUom = FormulaExtractorHelper.GetValueForDuty(rateFormulaWithUom);

			var a20Matches = new Regex(FormulaExtractorHelper.AgricuturalComponentFormulaRegex).Matches(rateFormulaWithUom);
			if (a20Matches.Count != 0)
			{
				foreach (Match match in a20Matches)
				{
					if (string.IsNullOrEmpty(match.Value))
					{
						continue;
					}

					var replacementValue = !string.IsNullOrEmpty(reductionIndicator)
						? $"#{match.Value}({reductionIndicator})#"
						: $"[{match.Value}]";

					rateFormulaWithUom = rateFormulaWithUom.Replace(match.Value, replacementValue);
				}
			}
			return rateFormulaWithUom;
		}
	}
}
