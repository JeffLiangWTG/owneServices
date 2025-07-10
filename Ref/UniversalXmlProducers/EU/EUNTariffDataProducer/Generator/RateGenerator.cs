using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RateGenerator : IRateGenerator
	{
		readonly IFormulaExtractor formulaExtractor;

		public RateGenerator(IFormulaExtractor formulaExtractor)
		{
			Argument.NotNull(formulaExtractor, nameof(formulaExtractor));

			this.formulaExtractor = formulaExtractor;
		}

		public IEnumerable<RefCusRate> Convert(IGroupedRateRecord rateRecord, IEnumerable<string> applicableTradeGroups,
			IEnumerable<IRawMeasureExclusionRecord> measureExclusionRecords, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords)
		{
			Argument.NotNull(rateRecord, nameof(rateRecord));
			Argument.NotNull(applicableTradeGroups, nameof(applicableTradeGroups));
			var applicabilities = new List<RefCusApplicability>();

			foreach (var applicableTradeGroup in applicableTradeGroups)
			{
				RefCusExcludedTradeGroup[] excludedTradeGroups = null;
				if (measureExclusionRecords != null && measureExclusionRecords.Any())
				{
					excludedTradeGroups = measureExclusionRecords.Where(x => x.TradeGroup == applicableTradeGroup).Select(
						x => new RefCusExcludedTradeGroup
						{
							ZZC_ZZA_NKTradeGroup = x.ExcludedTradeGroup,
						}).ToArray();
				}

				var applicability = new RefCusApplicability
				{
					ZZT_AdditionalCode = rateRecord.AdditionalCode,
					ZZT_OrderNumber = rateRecord.OrderNumber,
					ZZT_StartDate = rateRecord.StartDate,
					ZZT_EndDate = rateRecord.EndDate,
					ZZT_ZZA_NKTradeGroup = applicableTradeGroup,
					RefCusExcludedTradeGroups = excludedTradeGroups
				};

				applicabilities.Add(applicability);
			}

			var rawRate = string.IsNullOrEmpty(rateRecord.Rate) ? "0" : rateRecord.Rate;
			var formulaExtractionResults = formulaExtractor.GetFormula(rawRate, rateRecord.RateCode, rateRecord.ReductionIndicator);
			var rates = new List<RefCusRate>();

			//apply formula in case action code 07 is present (containing CertificateTypeCode) in measure condition.
			var actionCode07RateFormula = string.Empty;
			var filteredMeasureConditionRecord = measureConditionRecords?.FirstOrDefault(
						x => x.MeasureTypeId == rateRecord.MeasureTypeId
							 && ((string.IsNullOrEmpty(x.AdditionalCode) && string.IsNullOrEmpty(rateRecord.AdditionalCode)) || (x.AdditionalCode == rateRecord.AdditionalCode))
							 && ((string.IsNullOrEmpty(x.OrderNumber) && string.IsNullOrEmpty(rateRecord.OrderNumber)) || (x.OrderNumber == rateRecord.OrderNumber))
							 && x.StartDate == rateRecord.StartDate
							 && x.EndDate == rateRecord.EndDate
							 && !string.IsNullOrEmpty(x.CertificateTypeCode)
							 && new[] { "07" }.Contains(x.MeasureAction));
			if (filteredMeasureConditionRecord != null)
			{
				actionCode07RateFormula += $@"if(has(""CERT"", ""{filteredMeasureConditionRecord.CertificateTypeCode}""),0,";
			}

			foreach (var formulaExtractionResult in formulaExtractionResults)
			{
				var rateFormula = !string.IsNullOrEmpty(actionCode07RateFormula) ? actionCode07RateFormula + formulaExtractionResult.Formula + ")" : formulaExtractionResult.Formula;
				var rate = new RefCusRate
				{
					RefCusApplicabilities = applicabilities.ToArray(),

					ZZ2_StartDate = rateRecord.StartDate,
					ZZ2_EndDate = rateRecord.EndDate,
					ZZ2_RateFormula = rateFormula,
					ZZ2_ZZS_NKPreference = rateRecord.MeasureTypeId,
					ZZ2_ZY1_NKRateCode = formulaExtractionResult.RateCode,
					ZZ2_ZY1_ZZR_NKRateType = formulaExtractionResult.RateType,
					RefCusRateUOMs = GetRefCusRateUOMRecords(rateFormula)
				};
				rates.Add(rate);
			}
			return rates;
		}

		static RefCusRateUOM[] GetRefCusRateUOMRecords(string generatedFormula)
		{
			Argument.NotNullOrEmpty(generatedFormula, nameof(generatedFormula));
			return generatedFormula == "0" ? null : RateUOMGenerator.GenerateRateUomRecords(generatedFormula);
		}
	}
}
