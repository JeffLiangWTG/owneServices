using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public static class ExtensionHelper
	{
		public static RefCusCondition[] GetTariffConditions(this RefCusTariffSchema tariffSchema)
		{
			var conditions = tariffSchema.Conditions;
			if (conditions != null)
			{
				var country = tariffSchema.Country;
				var list = new List<RefCusCondition>();
				foreach (var condition in conditions)
				{
					var startDate = condition.StartDate == DateTime.MinValue ? tariffSchema.StartDate : condition.StartDate.FixedStartDate();
					var endDate = condition.EndDate == DateTime.MinValue ? tariffSchema.EndDate : condition.EndDate.FixedEndDate();
					list.Add(new RefCusCondition
					{
						ZX1_StartDate = startDate,
						ZX1_EndDate = endDate,
						ZX1_ZX2_NKConditionType = condition.ConditionType,
						ZX1_Severity = condition.Severity,
						ZX1_ZX2_ZZZ_NKDataGrouping = country,
						ZX1_IsImport = condition.IsImport == "Y",
						ZX1_IsExport = condition.IsExport == "Y",
						ZX1_ZZZ_NKDataGrouping = country,
						RefCusApplicabilities = condition.GetCusApplicabilities(startDate, endDate, country),
						RefCusConditionValues = condition.GetTariffConditionValues()
					});
				}
				return list.ToArray();
			}
			return null;
		}

		public static RefCusConditionValue[] GetTariffConditionValues(this RefCusConditionSchema conditionSchema)
		{
			var conditionValues = conditionSchema.ConditionValues;
			if (conditionValues != null)
			{
				var list = new List<RefCusConditionValue>();
				foreach(var conditionValue in conditionValues)
				{
					list.Add(new RefCusConditionValue
					{
						ZX3_ZX4_NKValueType = conditionValue.ConditionValueType,
						ZX3_Value = conditionValue.ConditionValue
					});
				}
				return list.ToArray();
			}
			return null;
		}

		public static RefCusTariffAttribute[] GetTariffAttributes(this RefCusTariffSchema tariffSchema)
		{
			var attributes = tariffSchema.Attributes;
			if (attributes != null)
			{
				var list = new List<RefCusTariffAttribute>();
				foreach (var attr in attributes)
				{
					list.Add(new RefCusTariffAttribute
					{
						ZZ3_Name = attr.Type,
						ZZ3_Value = attr.Value
					});
				}
				return list.ToArray();
			}
			return null;
		}

		public static RefCusRate[] GetCusRates(this RefCusTariffSchema tariffSchema)
		{
			var rates = tariffSchema.Rates;
			if (rates != null)
			{
				var country = tariffSchema.Country;
				var list = new List<RefCusRate>();
				foreach (var rate in rates)
				{
					var startDate = rate.StartDate == DateTime.MinValue ? tariffSchema.StartDate : rate.StartDate.FixedStartDate();
					var endDate = rate.EndDate == DateTime.MinValue ? tariffSchema.EndDate : rate.EndDate.FixedEndDate();
					list.Add(new RefCusRate
					{
						ZZ2_ZY1_NKRateCode = rate.RateCode,
						ZZ2_ZY1_ZZR_NKRateType = rate.RateType,
						ZZ2_StartDate = startDate,
						ZZ2_EndDate = endDate,
						ZZ2_ZZZ_NKDataGrouping = country,
						ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = country,
						ZZ2_RateFormula = rate.RateFormula,
						RefCusApplicabilities = rate.GetCusApplicabilities(startDate, endDate, country)
					});
				}
				return list.ToArray();
			}
			return null;
		}

		public static RefCusApplicability[] GetCusApplicabilities(this RefCusConditionSchema conditionSchema, DateTime parentStartDate, DateTime parentEndDate, string country)
		{
			return GetCusApplicabilities(conditionSchema.Applicabilities, parentStartDate, parentEndDate, country);
		}

		public static RefCusApplicability[] GetCusApplicabilities(this RefCusRateSchema rateSchema, DateTime parentStartDate, DateTime parentEndDate, string country)
		{
			return GetCusApplicabilities(rateSchema.Applicabilities, parentStartDate, parentEndDate, country);
		}

		static RefCusApplicability[] GetCusApplicabilities(IEnumerable<RefCusApplicabilitySchema> applicabilities, DateTime parentStartDate, DateTime parentEndDate, string country)
		{
			if (applicabilities != null)
			{
				var list = new List<RefCusApplicability>();
				foreach (var applicability in applicabilities)
				{
					var startDate = applicability.StartDate == DateTime.MinValue ? parentStartDate : applicability.StartDate.FixedStartDate();
					var endDate = applicability.EndDate == DateTime.MinValue ? parentEndDate : applicability.EndDate.FixedEndDate();

					list.Add(new RefCusApplicability
					{
						ZZT_ZZA_NKTradeGroup = applicability.TradeGroup,
						ZZT_StartDate = startDate,
						ZZT_EndDate = endDate,
						ZZT_ZZA_ZZZ_NKDataGrouping = country,
						RefCusExcludedTradeGroups = applicability.GetExcludedTradeGroups(country, applicability.TradeGroup)
					});
				}
				return list.ToArray();
			}
			return null;
		}

		public static RefCusExcludedTradeGroup[] GetExcludedTradeGroups(this RefCusApplicabilitySchema applicabilitySchema, string country, string tradeGroup)
		{
			var exclusions = applicabilitySchema.Exclusions;
			if (exclusions != null)
			{
				if (tradeGroup.Trim().Equals(Constants.AllCountries, StringComparison.OrdinalIgnoreCase))
				{
					exclusions = exclusions.Append(Constants.USCountryCode);
				}

				var list = new List<RefCusExcludedTradeGroup>();
				foreach (var exclusion in exclusions.Distinct())
				{
					list.Add(new RefCusExcludedTradeGroup
					{
						ZZC_ZZA_NKTradeGroup = exclusion,
						ZZC_ZZA_ZZZ_NKDataGrouping = country
					});
				}
				return list.ToArray();
			}
			else
			{
				return null;
			}
		}

		public static RefCusTariffRelationship[] GetCusTariffRelationships(this RefCusTariffSchema tariffSchema)
		{
			var parentTariffs = tariffSchema.ParentTariffs;
			if (parentTariffs != null)
			{
				var tariffList = new List<string>();
				var list = new List<RefCusTariffRelationship>();
				foreach (var parentTariff in parentTariffs)
				{
					var tariff = parentTariff.Replace(".", "");
					if (!tariffList.Contains(tariff))
					{
						list.Add(new RefCusTariffRelationship
						{
							ZZH_ZZI_NKTariffType = tariffSchema.TariffType,
							ZZH_TariffCode = tariff,
							ZZH_ZZI_ZZZ_NKDataGrouping = tariffSchema.Country
						});
					}
					tariffList.Add(tariff);
				}
				return list.ToArray();
			}
			return null;
		}
	}
}
