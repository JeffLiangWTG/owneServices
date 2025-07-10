using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ConditionFormulaExtractor : IConditionFormulaExtractor
	{
		readonly IMeasuringUnitTransformer measuringUnitTransformer;
		public ConditionFormulaExtractor(IMeasuringUnitTransformer measuringUnitTransformer)
		{
			Argument.NotNull(measuringUnitTransformer, nameof(measuringUnitTransformer));

			this.measuringUnitTransformer = measuringUnitTransformer;
		}

		readonly List<string> antiDumpingAllowableConditionTypes = new List<string> { "A", "F", "M" };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "Legacy.")]
		public IFormulaExtractionResult GetFormula(string rawRateFormula, string rateCode)
		{
			Argument.NotNullOrEmpty(rawRateFormula, nameof(rawRateFormula));
			Argument.NotNullOrEmpty(rateCode, nameof(rateCode));
			var rateFormula = "";
			rawRateFormula = rawRateFormula.Replace(",", "");

			if (rawRateFormula.StartsWith("Cond:", StringComparison.Ordinal))
			{
				rawRateFormula = rawRateFormula.Substring(6).Trim();
			}

			var rawRateFormulaWithoutEur = new Regex(FormulaExtractorHelper.EURRegex).Replace(rawRateFormula, string.Empty);
			var ranges = rawRateFormulaWithoutEur.Split(';');
			var uom = string.Empty;
			var upperLimit = string.Empty;
			var fmConditionsInARow = 0;
			foreach (var range in ranges)
			{
				var encounteredFM = false;
				var trimmedRange = range.TrimStart().TrimEnd();
				var conditionType = trimmedRange.Substring(0, 1);
				if (conditionType != "V" && conditionType != "L" && !antiDumpingAllowableConditionTypes.Contains(conditionType))
				{
					continue;
				}

				var trimmedRangeWithoutConditionType = trimmedRange.Substring(1);
				var values = trimmedRangeWithoutConditionType.Split(':');
				if (rawRateFormula.StartsWith("A cert", StringComparison.Ordinal) && conditionType == "A" && values.Length >= 2 && !string.IsNullOrEmpty(values[0]) && !string.IsNullOrEmpty(values[1]))
				{
					if (values.Length == 3 && !string.IsNullOrEmpty(values[2]))
					{
						var valueType = values[0];
						var certificate = Regex.Match(values[1].Trim(), @"^\S*")?.Value;
						if (!string.IsNullOrEmpty(certificate))
						{
							certificate = certificate.Replace("-", string.Empty);
						}
						var rate = measuringUnitTransformer.Transform(values[2]);

						if (valueType == " cert")
						{
							rateFormula += $@"If(has(""CERT"",""{certificate}""),{FormulaExtractorHelper.GetValueForDuty(rate)},";
						}
					}
					else if (values.Length == 2)
					{
						var rate = measuringUnitTransformer.Transform(values[1]);
						rateFormula += $"{FormulaExtractorHelper.GetValueForDuty(rate)})";
					}
				}
				else if (values.Length == 2 && Regex.IsMatch(values[0].Trim(), @"^\([0-9]{2,}\)$") && !string.IsNullOrEmpty(values[1]))
				{
					var rate = measuringUnitTransformer.Transform(values[1]);
					rateFormula += $"{FormulaExtractorHelper.GetValueForDuty(rate)}";
				}
				else if (values.Length == 2)
				{
					var rawConditionValue = measuringUnitTransformer.ReplaceUom(values[0], string.Empty);
					var conditionValue = rawConditionValue.Trim();

					if (conditionType == "V")
					{
						var rawAdditionalRate = measuringUnitTransformer.Transform(values[1]);
						var additionalRate = rawAdditionalRate.Trim();

						if (conditionValue != "0" && conditionValue != "0.000")
						{
							var uoms = measuringUnitTransformer.GetUoms(values[0]);
							if (uoms.Length > 0)
							{
								uom = uoms[0];
							}
							rateFormula += $"If(VFD/[{uom}] >= " + conditionValue;
						}

						if (string.IsNullOrEmpty(FormulaExtractorHelper.GetValueForDuty(additionalRate)))
						{
							additionalRate = "0";
						}

						if (conditionValue != "0" && conditionValue != "0.000")
						{
							rateFormula += ", " + FormulaExtractorHelper.GetValueForDuty(additionalRate) + ", ";
						}
						else
						{
							rateFormula += FormulaExtractorHelper.GetValueForDuty(additionalRate);
						}
					}
					if (conditionType == "L")
					{
						var rawAdditionalRate = measuringUnitTransformer.Transform(values[1]);
						var additionalRate = rawAdditionalRate.Trim();

						if (conditionValue != "0" && conditionValue != "0.000")
						{
							rateFormula += "If(CIF/[DTN] >= " + conditionValue;
						}

						if (string.IsNullOrEmpty(FormulaExtractorHelper.GetCostInsuranceFreight(additionalRate)))
						{
							additionalRate = "0";
						}

						if (conditionValue != "0" && conditionValue != "0.000")
						{
							rateFormula += ", " + FormulaExtractorHelper.GetCostInsuranceFreight(additionalRate) + ", ";
						}
						else
						{
							rateFormula += FormulaExtractorHelper.GetCostInsuranceFreight(additionalRate);
						}
					}
					else if (conditionType == "M" || conditionType == "F")
					{
						fmConditionsInARow++;
						encounteredFM = true;
						var rawConditionValue2 = measuringUnitTransformer.ReplaceUom(values[1], string.Empty);
						var conditionValue2 = rawConditionValue2.Trim();

						conditionValue = conditionValue.Replace(",", string.Empty);

						if (string.IsNullOrEmpty(uom))
						{
							var uoms = measuringUnitTransformer.GetUoms(values[0]);
							if (uoms.Length > 0)
							{
								uom = uoms[0];
							}
						}
						if (range == ranges[ranges.Length - 1])
						{
							if (fmConditionsInARow == 2)
							{
								rateFormula += $"{upperLimit} - VFD/[{uom}]) * [{uom}])";
							}
							else
							{
								rateFormula += $"{FormulaExtractorHelper.GetValueForDuty(conditionValue2)},({upperLimit} - VFD/[{uom}]) * [{uom}]))";
							}
						}
						else
						{
							if (string.IsNullOrEmpty(upperLimit))
							{
								upperLimit = conditionValue;
							}
							if (string.IsNullOrEmpty(rateFormula))
							{
								rateFormula += $"If(VFD/[{uom}] > {conditionValue},{conditionValue2},(";
							}
							else
							{
								rateFormula += $"If(VFD/[{uom}] < {conditionValue},";
							}
						}
					}

					if (!encounteredFM)
					{
						fmConditionsInARow = 0;
					}
				}
			}

			var openingBrackets = rateFormula.Count(f => f == '(');
			var closingBrackets = rateFormula.Count(f => f == ')');

			var neededClosingBrackets = openingBrackets - closingBrackets;
			if (neededClosingBrackets > 0)
			{
				rateFormula += string.Concat(Enumerable.Repeat(")", neededClosingBrackets));
			}

			if (string.IsNullOrEmpty(rateFormula))
			{
				rateFormula = "0";
			}

			return new FormulaExtractionResult(rateFormula, rateCode);
		}
	}
}
