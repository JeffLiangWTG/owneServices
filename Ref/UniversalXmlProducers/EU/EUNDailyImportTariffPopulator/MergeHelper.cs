using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public static class MergeHelper
	{
		public static void MergeRates(RefCusTariff dailyTariff, IEnumerable<RefCusRate> rates)
		{
			foreach (var rate in rates)
			{
				var matchedIndex = IndexOfMatchedObj(dailyTariff.RefCusRates, rate, RateMatch);

				if (matchedIndex < 0)
				{
					dailyTariff.RefCusRates = dailyTariff.RefCusRates.Append(rate).ToArray();
					continue;
				}

				var matchedRate = dailyTariff.RefCusRates[matchedIndex];
				if (matchedRate.RefCusApplicabilities == null || matchedRate.RefCusApplicabilities.Length != 1)
				{
					throw new NotSupportedException($"This monthly rate has 0 or more than 1 applicabilities. TariffCode[{dailyTariff.ZZ1_TariffCode}], RateCode[{matchedRate.ZZ2_ZY1_NKRateCode}]");
				}
				if (rate.RefCusApplicabilities == null || rate.RefCusApplicabilities.Length != 1)
				{
					throw new NotSupportedException($"This daily rate has 0 or more than 1 applicabilities. TariffCode[{dailyTariff.ZZ1_TariffCode}], RateCode[{rate.ZZ2_ZY1_NKRateCode}]");
				}
				if (!ApplicabilityMatch(matchedRate.RefCusApplicabilities[0], rate.RefCusApplicabilities[0]))
				{
					dailyTariff.RefCusRates = dailyTariff.RefCusRates.Append(rate).ToArray();
				}
				else
				{
					dailyTariff.RefCusRates[matchedIndex] = rate;
				}
			}
		}

		public static void MergeConditions(RefCusTariff dailyTariff, IEnumerable<RefCusCondition> conditions)
		{
			foreach (var condition in conditions)
			{
				var matchIndex = IndexOfMatchedObj(dailyTariff.RefCusConditions, condition, ConditionMatch);

				if (matchIndex < 0)
				{
					dailyTariff.RefCusConditions = dailyTariff.RefCusConditions.Append(condition).ToArray();
					continue;
				}

				var matchedCondition = dailyTariff.RefCusConditions[matchIndex];
				if (matchedCondition.RefCusApplicabilities == null || matchedCondition.RefCusApplicabilities.Length != 1)
				{
					throw new NotSupportedException($"This monthly condition has 0 or more than 1 applicabilities. TariffCode[{dailyTariff.ZZ1_TariffCode}], ConditionType[{matchedCondition.ZX1_ZX2_NKConditionType}]");
				}
				if (condition.RefCusApplicabilities == null || condition.RefCusApplicabilities.Length != 1)
				{
					throw new NotSupportedException($"This daily condition has 0 or more than 1 applicabilities. TariffCode[{dailyTariff.ZZ1_TariffCode}], ConditionType[{condition.ZX1_ZX2_NKConditionType}]");
				}
				if (!ApplicabilityMatch(matchedCondition.RefCusApplicabilities[0], condition.RefCusApplicabilities[0]))
				{
					dailyTariff.RefCusConditions = dailyTariff.RefCusConditions.Append(condition).ToArray();
				}
				else
				{
					dailyTariff.RefCusConditions[matchIndex] = condition;
				}
			}
		}

		static bool RateMatch(RefCusRate rate1, RefCusRate rate2)
		{
			return rate1.ZZ2_ZY1_NKRateCode == rate2.ZZ2_ZY1_NKRateCode
					&& rate1.ZZ2_ZY1_ZZR_NKRateType == rate2.ZZ2_ZY1_ZZR_NKRateType
					&& rate1.ZZ2_ZZS_NKPreference == rate2.ZZ2_ZZS_NKPreference
					&& rate1.ZZ2_ZZS_ZZZ_NKDataGrouping == rate2.ZZ2_ZZS_ZZZ_NKDataGrouping
					&& rate1.ZZ2_StartDate == rate2.ZZ2_StartDate;
		}

		static bool ConditionMatch(RefCusCondition condition1, RefCusCondition condition2)
		{
			return condition1.ZX1_Comment == condition2.ZX1_Comment
					&& condition1.ZX1_ZX2_NKConditionType == condition2.ZX1_ZX2_NKConditionType
					&& condition1.ZX1_ZZS_NKPreference == condition2.ZX1_ZZS_NKPreference
					&& condition1.ZX1_ZZS_ZZZ_NKDataGrouping == condition2.ZX1_ZZS_ZZZ_NKDataGrouping
					&& condition1.ZX1_StartDate == condition2.ZX1_StartDate;
		}

		static bool ApplicabilityMatch(RefCusApplicability app1, RefCusApplicability app2)
		{
			return app1.ZZT_AdditionalCode == app2.ZZT_AdditionalCode
					&& app1.ZZT_OrderNumber == app2.ZZT_OrderNumber
					&& app1.ZZT_ZZA_NKTradeGroup == app2.ZZT_ZZA_NKTradeGroup
					&& app1.ZZT_StartDate == app2.ZZT_StartDate;
		}

		static int IndexOfMatchedObj<T>(T[] objs, T targetObj, Func<T, T, bool> isMatch)
		{
			for (int i = 0; i < objs.Length; i++)
			{
				if (isMatch(objs[i], targetObj))
				{
					return i;
				}
			}
			return -1;
		}

		public static void InsertTariffUOMIfNotExists(RefCusTariff dailyTariff, IEnumerable<RefCusTariffUOM> tariffUOMs)
		{
			foreach(var uom in tariffUOMs)
			{
				var existsInDaily = dailyTariff.RefCusTariffUOMs.Any(x => x.ZZ8_Type == uom.ZZ8_Type && x.ZZ8_UOM == uom.ZZ8_UOM && x.ZZ8_ZZA_NKTradeGroup == uom.ZZ8_ZZA_NKTradeGroup);
				if (!existsInDaily)
				{
					dailyTariff.RefCusTariffUOMs = dailyTariff.RefCusTariffUOMs.Append(uom).ToArray();
				}
			}
		}
	}
}
