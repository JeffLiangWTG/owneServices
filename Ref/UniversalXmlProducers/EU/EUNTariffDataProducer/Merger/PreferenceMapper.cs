using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class PreferenceMapper : IPreferenceMapper
	{
		public IEnumerable<RefCusRate> SetPreferenceOnRates(IEnumerable<RefCusRate> originalRates)
		{
			Argument.NotNull(originalRates, nameof(originalRates));
			var newRates = new List<RefCusRate>();
			foreach (var originalRate in originalRates)
			{
				var measureTypeId = originalRate.ZZ2_ZZS_NKPreference;
				var preferences = MeasureTypeIdToPreferenceMapping.GetPreferenceCodes(measureTypeId, originalRate.RefCusApplicabilities.Select(x => x.ZZT_ZZA_NKTradeGroup));
				if (preferences != null)
				{
					UpdatePreferenceOnRate(newRates, originalRate, preferences);
				}
				else
				{
					originalRate.ZZ2_ZZS_NKPreference = string.Empty;
					newRates.Add(originalRate);
				}
			}

			return newRates;
		}

		public IEnumerable<RefCusCondition> SetPreferenceOnConditions(IEnumerable<RefCusCondition> originalConditions)
		{
			Argument.NotNull(originalConditions, nameof(originalConditions));
			var newConditions = new List<RefCusCondition>();
			foreach (var originalCondition in originalConditions)
			{
				var measureTypeId = originalCondition.ZX1_ZZS_NKPreference;
				var preferences = MeasureTypeIdToPreferenceMapping.GetPreferenceCodes(measureTypeId, originalCondition.RefCusApplicabilities.Select(x => x.ZZT_ZZA_NKTradeGroup));
				if (preferences != null)
				{
					UpdatePreferenceOnCondition(newConditions, originalCondition, preferences);
				}
				else
				{
					originalCondition.ZX1_ZZS_NKPreference = string.Empty;
					newConditions.Add(originalCondition);
				}
			}

			return newConditions;
		}

		static void UpdatePreferenceOnRate(ICollection<RefCusRate> rates, RefCusRate originalRate, IEnumerable<Preference> preferences)
		{
			Argument.NotNull(rates, nameof(rates));
			Argument.NotNull(originalRate, nameof(originalRate));

			if (preferences != null)
			{
				foreach (var preference in preferences)
				{
					rates.Add(new RefCusRate
					{
						ZZ2_StartDate = originalRate.ZZ2_StartDate,
						ZZ2_EndDate = originalRate.ZZ2_EndDate,
						ZZ2_RateFormula = originalRate.ZZ2_RateFormula,
						ZZ2_ZY1_NKRateCode = originalRate.ZZ2_ZY1_NKRateCode,
						ZZ2_ZY1_ZZR_NKRateType = originalRate.ZZ2_ZY1_ZZR_NKRateType,
						ZZ2_ZZS_NKPreference = preference.Code,
						ZZ2_ZZS_ZZZ_NKDataGrouping = preference.DataGrouping,
						RefCusApplicabilities = originalRate.RefCusApplicabilities,
						RefCusRateUOMs = originalRate.RefCusRateUOMs
					});
				}
			}
		}

		static void UpdatePreferenceOnCondition(ICollection<RefCusCondition> conditions, RefCusCondition originalCondition, IEnumerable<Preference> preferences)
		{
			Argument.NotNull(conditions, nameof(conditions));
			Argument.NotNull(originalCondition, nameof(originalCondition));

			if (preferences != null)
			{
				foreach (var preference in preferences)
				{
					conditions.Add(new RefCusCondition
					{
						ZX1_ZX2_NKConditionType = originalCondition.ZX1_ZX2_NKConditionType,
						ZX1_StartDate = originalCondition.ZX1_StartDate,
						ZX1_EndDate = originalCondition.ZX1_EndDate,
						ZX1_Comment = originalCondition.ZX1_Comment,
						ZX1_IsImport = originalCondition.ZX1_IsImport,
						ZX1_IsExport = originalCondition.ZX1_IsExport,
						ZX1_ZZS_NKPreference = preference.Code,
						ZX1_ZZS_ZZZ_NKDataGrouping = preference.DataGrouping,
						RefCusApplicabilities = originalCondition.RefCusApplicabilities,
						RefCusConditionValues = originalCondition.RefCusConditionValues
					});
				}
			}
		}
	}
}
