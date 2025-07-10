using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Configuration;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public class ScrappedRecordParser : IScrappedRecordParser
	{
		public ScrappedRecordParser(IDataLookup tradeGroupLookup, IDataLookup taxOrFeeCodeLookup, IRateCodeDataLookup rateCodeDataLookup, DateTime publicationDate)
		{
			Argument.NotNull(tradeGroupLookup, nameof(tradeGroupLookup));
			Argument.NotNull(taxOrFeeCodeLookup, nameof(taxOrFeeCodeLookup));
			Argument.NotNull(rateCodeDataLookup, nameof(rateCodeDataLookup));

			_tradeGroupLookup = tradeGroupLookup;
			_taxOrFeeCodeLookup = taxOrFeeCodeLookup;
			_rateCodeDataLookup = rateCodeDataLookup;
			_publicationDate = publicationDate;
		}

		readonly IDataLookup _tradeGroupLookup;
		readonly IDataLookup _taxOrFeeCodeLookup;
		readonly IRateCodeDataLookup _rateCodeDataLookup;
		readonly DateTime _publicationDate;

		public ParsingResult GetRefCusVatApplicabilities(IScrappedRecord scrappedRecord, RefCusTariff refCusTariff)
		{
			Argument.NotNull(scrappedRecord, nameof(scrappedRecord));
			Argument.NotNull(refCusTariff, nameof(refCusTariff));
			if (string.IsNullOrEmpty(scrappedRecord.Measure.Formula))
			{
				return null;
			}

			if (!scrappedRecord.Measure.TradeGroup.Contains(MeasuresConstant.VatTradeGroup))
			{
				return new ParsingResult(refCusTariff.TariffCode, ErrorMessagesConstant.VatInformationNotErgaOmnes, string.Join(",", scrappedRecord.Measure.TradeGroup));
			}

			var taxOrFeeCode = _taxOrFeeCodeLookup.Lookup(scrappedRecord.Measure.Formula) ??
								_taxOrFeeCodeLookup.ReplacementLookup(refCusTariff.TariffCode, scrappedRecord.Measure.Formula);
			if (string.IsNullOrEmpty(taxOrFeeCode))
			{
				return new ParsingResult(refCusTariff.TariffCode, ErrorMessagesConstant.InvalidTaxOrFeeCode, scrappedRecord.Measure.Formula);
			}

			if (scrappedRecord.Requirement == null || scrappedRecord.Requirement.All(x => x == null))
			{
				var vatApplicability = new RefCusVatApplicability(taxOrFeeCode, string.Empty, _publicationDate);
				refCusTariff.VatApplicabilities.Add(vatApplicability);
				return null;
			}

			var refCusVatApplicability = new RefCusVatApplicability();
			var hasAdditionalCode = scrappedRecord.Requirement.Where(x => x != null).Any(x => string.Compare(x.RequirementType, MeasuresConstant.AdditionalCode, StringComparison.OrdinalIgnoreCase) == 0);
			if (hasAdditionalCode)
			{
				var requirement = scrappedRecord.Requirement.Where(x => x != null && string.Compare(x.RequirementType, MeasuresConstant.AdditionalCode, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
				refCusVatApplicability = new RefCusVatApplicability(taxOrFeeCode, requirement.RequirementDescription, _publicationDate);
			}
			else
			{
				refCusVatApplicability = new RefCusVatApplicability(taxOrFeeCode, string.Empty, _publicationDate);
			}
			refCusTariff.VatApplicabilities.Add(refCusVatApplicability);

			return null;
		}

		public ParsingResult GetRefCusConditions(IScrappedRecord scrappedRecord, RefCusTariff refCusTariff, List<CertificateData> certificateData)
		{
			Argument.NotNull(scrappedRecord, nameof(scrappedRecord));
			Argument.NotNull(refCusTariff, nameof(refCusTariff));
			Argument.NotNull(certificateData, nameof(certificateData));

			var tradeGroupCode = scrappedRecord.Measure.TradeGroup
				.Select(tradeGroup => _tradeGroupLookup.Lookup(tradeGroup))
				.FirstOrDefault(code => !string.IsNullOrEmpty(code));

			var refCusConditionValues = new List<RefCusConditionValue>();
			foreach (var certificate in certificateData)
			{
				refCusConditionValues.Add(new RefCusConditionValue(certificate.ConditionValueType, certificate.CertificateNumber));
			}

			var excludedTradeGroups = new List<RefCusExcludedTradeGroup>();
			if (scrappedRecord.Measure.Formula != null && scrappedRecord.Measure.Formula.StartsWith("CertificatoEscluso:", StringComparison.InvariantCultureIgnoreCase))
			{
				foreach (var excludedTradeGroup in scrappedRecord.Measure.Formula.Replace("CertificatoEscluso:", string.Empty).Split(',')) //CertificatoEscluso:AD, CH, FO, IS, LI, NO, SM
				{
					excludedTradeGroups.Add(new RefCusExcludedTradeGroup(excludedTradeGroup.Trim()));
				}
			}
			var additionalCode = scrappedRecord.Requirement.Where(r => string.Compare(r.RequirementType, MeasuresConstant.AdditionalCode, StringComparison.OrdinalIgnoreCase) == 0)
				.Select(r => r.RequirementDescription).FirstOrDefault();

			var applicability = new RefCusApplicability(tradeGroupCode, additionalCode, _publicationDate, excludedTradeGroups);

			var conditionType = string.Empty;
			if (!string.IsNullOrEmpty(scrappedRecord.Measure.Description) && ConditionConstants.conditionTypeDictionary.TryGetValue(scrappedRecord.Measure.Description, out conditionType))
			{
				var conditionTypeValue = ConditionConstants.conditionTypeDictionary.Keys.FirstOrDefault(x => x.StartsWith(scrappedRecord.Measure.Description, StringComparison.InvariantCultureIgnoreCase));
				conditionType = ConditionConstants.conditionTypeDictionary[conditionTypeValue];
			}
			else
			{
				conditionType = string.Empty;
			}

			var refCusCondition = new RefCusCondition(conditionType, scrappedRecord.Measure.Description, _publicationDate, DateTime.ParseExact(ApplicationConfig.DefaultEndDateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), applicability, refCusConditionValues);

			refCusTariff.CusConditions.Add(refCusCondition);

			return null;
		}

		public ParsingResult GetRefCusRates(IScrappedRecord scrappedRecord, string nationalSection, RefCusTariff refCusTariff, IRateGenerationStrategy rateGenerationStrategy)
		{
			Argument.NotNull(scrappedRecord, nameof(scrappedRecord));
			Argument.NotNull(refCusTariff, nameof(refCusTariff));

			if (string.IsNullOrEmpty(scrappedRecord.Measure.Formula))
			{
				return null;
			}

			var rateCode = _rateCodeDataLookup.Lookup(scrappedRecord.Measure.Description, refCusTariff.TariffCode);
			if (rateCode == null)
			{
				return new ParsingResult(refCusTariff.TariffCode, ErrorMessagesConstant.NoRateCodeFound, scrappedRecord.Measure.Description);
			}

			var tradeGroupCode = scrappedRecord.Measure.TradeGroup
				.Select(tradeGroup => _tradeGroupLookup.Lookup(tradeGroup))
				.FirstOrDefault(code => !string.IsNullOrEmpty(code));

			if (string.IsNullOrEmpty(tradeGroupCode))
			{
				return new ParsingResult(refCusTariff.TariffCode, ErrorMessagesConstant.TradeGroupNotFound, nationalSection);
			}

			var rateFormula = scrappedRecord.Measure.Formula;
			var rateUOMs = new HashSet<string>();
			if (rateFormula.Contains(MeasuresConstant.CurrencyToken))
			{
				var currencyIndex = rateFormula.IndexOf(MeasuresConstant.CurrencyToken, StringComparison.InvariantCultureIgnoreCase);
				var rate = rateFormula.Substring(0, currencyIndex).Trim();
				if (!decimal.TryParse(rate, out var parsedRate))
				{
					return new ParsingResult(refCusTariff.TariffCode, ErrorMessagesConstant.UnableToMapRateFormula, scrappedRecord.Measure.Formula);
				}

				var formulaIndex = currencyIndex + 5;
				var rawFormula = rateFormula.Substring(formulaIndex).Trim();

				(parsedRate, rawFormula) = ConvertSpecialFormulaAndRateIfNeeded(parsedRate, rawFormula);
				var formula = UomCodeLookupFactory.CreateLookup(rateCode.Code)
					.Lookup(rawFormula);

				if (string.IsNullOrEmpty(formula))
				{
					return new ParsingResult(refCusTariff.TariffCode, ErrorMessagesConstant.UnableToMapRateFormula, scrappedRecord.Measure.Formula);
				}
				if (parsedRate != 0)
				{
					rateFormula = $"{parsedRate} * [{formula}]";
					rateUOMs.Add(formula);
				}
				else
				{
					rateFormula = "0";
				}
			}
			else if (decimal.TryParse(rateFormula, out var percentage))
			{
				if (percentage == 0)
				{
					rateFormula = "0";
				}
				else if ((percentage / 100) <= 100)
				{
					rateFormula = $"{percentage / 100} * VFD";
				}
			}

			var additionalCode = string.Empty;
			var count = scrappedRecord.Requirement.Count(r => string.Compare(r.RequirementType, MeasuresConstant.AdditionalCode, StringComparison.OrdinalIgnoreCase) == 0);
			if (count > 1)
			{
				return new ParsingResult(refCusTariff.TariffCode, ErrorMessagesConstant.ExciseContainsMultipleCodes);
			}

			additionalCode = scrappedRecord.Requirement.Where(r => string.Compare(r.RequirementType, MeasuresConstant.AdditionalCode, StringComparison.OrdinalIgnoreCase) == 0)
				.Select(r => r.RequirementDescription).FirstOrDefault();

			var scrappedRate = new ScrappedRate()
			{
				RateCode = rateCode.Code,
				RateType = rateCode.RateType,
				RateFormula = rateFormula,
				StartDate = _publicationDate,
				Applicability = new ScrappedApplicability()
				{
					TradeGroup = tradeGroupCode,
					AdditionalCode = additionalCode,
					StartDate = _publicationDate,
				},
				MeasurementUnits = rateUOMs
			};

			rateGenerationStrategy.GenerateRates(refCusTariff, scrappedRate);
			return null;
		}

		static (decimal EffectiveRate, string EffectiveFormula) ConvertSpecialFormulaAndRateIfNeeded(decimal parsedRate, string rawFormula)
		{
			if (rawFormula == "1000 t")
			{
				return (parsedRate / 1000m, "1000 kg");
			}
			return (parsedRate, rawFormula);
		}
	}
}
