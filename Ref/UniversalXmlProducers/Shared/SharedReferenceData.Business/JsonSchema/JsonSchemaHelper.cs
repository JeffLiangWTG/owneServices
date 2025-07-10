using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	static class JsonSchemaHelper
	{
		public static RefCusCodeListAttribute[] GetCodeListAttributes(this RefCusCodeListSchema codeListSchema)
		{
			var attributes = codeListSchema.Attributes;
			if (attributes != null)
			{
				var list = new List<RefCusCodeListAttribute>();
				foreach (var attr in attributes)
				{
					list.Add(new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = attr.Type,
						ZZE_Value = attr.Value
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
				var list = new List<RefCusRate>();
				foreach (var rate in rates)
				{
					var startDate = (rate.StartDate == DateTime.MinValue) ? tariffSchema.StartDate : rate.StartDate.FixedStartDate();
					var endDate = (rate.EndDate == DateTime.MinValue) ? tariffSchema.EndDate : rate.EndDate.FixedEndDate();
					list.Add(new RefCusRate
					{
						ZZ2_ZY1_NKRateCode = rate.RateCode,
						ZZ2_ZY1_ZZR_NKRateType = rate.RateType,
						ZZ2_StartDate = startDate,
						ZZ2_EndDate = endDate,
						ZZ2_ZZZ_NKDataGrouping = tariffSchema.Country,
						ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = tariffSchema.Country,
						ZZ2_RateFormula = rate.RateFormula,
						RefCusApplicabilities = rate.GetCusApplicabilitys(startDate, endDate, tariffSchema.Country)
					});
				}
				return list.ToArray();
			}
			return null;
		}

		public static RefCusApplicability[] GetCusApplicabilitys(this RefCusRateSchema rateSchema, DateTime parentStartDate, DateTime parentToDate, string country)
		{
			var applicabilitys = rateSchema.Applicabilitys;
			if (applicabilitys != null)
			{
				var list = new List<RefCusApplicability>();
				foreach (var applicability in applicabilitys)
				{
					var startDate = (applicability.StartDate == DateTime.MinValue) ? parentStartDate : applicability.StartDate.FixedStartDate();
					var endDate = (applicability.EndDate == DateTime.MinValue) ? parentToDate : applicability.EndDate.FixedEndDate();

					list.Add(new RefCusApplicability
					{
						ZZT_ZZA_NKTradeGroup = applicability.TradeGroup,
						ZZT_StartDate = startDate,
						ZZT_EndDate = endDate,
						ZZT_ZZA_ZZZ_NKDataGrouping = country,
						RefCusExcludedTradeGroups = applicability.GetExcludedTradeGroups(country)
					});
				}
				return list.ToArray();
			}
			return null;
		}

		public static RefCusExcludedTradeGroup[] GetExcludedTradeGroups(this RefCusApplicabilitySchema applicabilitySchema, string country)
		{
			var exclusions = applicabilitySchema.Exclusions;
			if (exclusions != null)
			{
				var list = new List<RefCusExcludedTradeGroup>();
				foreach (var exclusion in applicabilitySchema.Exclusions)
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

