using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class FormulaExtractor : IFormulaExtractor
	{
		readonly IMeasuringUnitTransformer measuringUnitTransformer;
		readonly IFormulaExtractor agriculturalComponentFormulaExtractor;
		readonly IConditionFormulaExtractor conditionFormulaExtractor;

		public FormulaExtractor(IMeasuringUnitTransformer measuringUnitTransformer, IFormulaExtractor agriculturalComponentFormulaExtractor, IConditionFormulaExtractor conditionFormulaExtractor)
		{
			Argument.NotNull(measuringUnitTransformer, nameof(measuringUnitTransformer));
			Argument.NotNull(agriculturalComponentFormulaExtractor, nameof(agriculturalComponentFormulaExtractor));

			this.measuringUnitTransformer = measuringUnitTransformer;
			this.agriculturalComponentFormulaExtractor = agriculturalComponentFormulaExtractor;
			this.conditionFormulaExtractor = conditionFormulaExtractor;
		}

		public IEnumerable<IFormulaExtractionResult> GetFormula(string rawRateFormula, string rateCode, string reductionIndicator)
		{
			Argument.NotNullOrEmpty(rawRateFormula, nameof(rawRateFormula));
			Argument.NotNullOrEmpty(rateCode, nameof(rateCode));
			Argument.NotNull(reductionIndicator, nameof(reductionIndicator));
			rawRateFormula = rawRateFormula.Replace(",", "");
			if (rawRateFormula.StartsWith("Cond:", StringComparison.Ordinal))
			{
				var conditionFormula = conditionFormulaExtractor.GetFormula(rawRateFormula, rateCode);
				return new[] { conditionFormula };
			}

			if (Regex.Match(rawRateFormula, FormulaExtractorHelper.AgricuturalComponentFormulaRegex).Length != 0)
			{
				return agriculturalComponentFormulaExtractor.GetFormula(rawRateFormula, rateCode, reductionIndicator);
			}

			var rawRateFormulaWithoutEur = new Regex(FormulaExtractorHelper.EURRegex).Replace(rawRateFormula.Trim(), string.Empty);
			var rawRateFormulaWithoutAgriculturalComponent = new Regex(FormulaExtractorHelper.AgricuturalComponentFormulaRegex).Replace(rawRateFormulaWithoutEur, string.Empty);
			var rawRateFormulaWithUom = measuringUnitTransformer.Transform(rawRateFormulaWithoutAgriculturalComponent);

			if (rawRateFormulaWithUom == "0")
			{
				return new[] { new FormulaExtractionResult(rawRateFormulaWithUom, rateCode) };
			}

			var rateFormula = new Regex(FormulaExtractorHelper.ZeroPercentageRegex).Replace(rawRateFormulaWithUom, "0");
			rateFormula = new Regex(FormulaExtractorHelper.ZeroFormulaRegex).Replace(rawRateFormulaWithUom, "0");
			rateFormula = FormulaExtractorHelper.GetValueForDuty(rateFormula);

			var formula = FormulaExtractorHelper.AdjustForBoundFormula(rateFormula);
			return new[] { new FormulaExtractionResult(formula, rateCode) };
		}
	}
}
