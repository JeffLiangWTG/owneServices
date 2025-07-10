using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCTariffRuleFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string RuleCode = "Rule Code";
			public const string Tariff = "Tariff";
			public const string EffectiveDates = "Effective Dates";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(Schema.RuleCode, USCTariffRuleSchema.U1_RuleCode, () => new TariffRuleList());
			var tariffFilter = new TariffProvTariffModuleFilter(Schema.Tariff, GetTariffQuery);
			tariffFilter.Category = FilterCategories.NumbersAndReferences;
			tariffFilter.MaxLength = USCTariffRuleSchema.U1_Tariff.MaxLength + 2;
			result.AddCustomFilter(tariffFilter);
			result.AddDateFilter(Schema.EffectiveDates, GetEffectiveDateQuery);
			return result;
		}

		ZQuery GetEffectiveDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				if (date1.IsValid)
				{
					result.AddToFilter(USCTariffRuleSchema.U1_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date1);
				}

				if (date2.IsValid)
				{
					var dateToQuery = new ZQuery(USCTariffRuleSchema.U1_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date2);
					dateToQuery.AddToFilter(JoinCondition.Or, USCTariffRuleSchema.U1_DateTo, SQLComparisonOperator.Equal, DBNull.Value);
					result.AddToFilter(dateToQuery);
				}
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		ZQuery GetTariffQuery(ZString tariff, ZString provTariff)
		{
			var tariffNumber = tariff.Replace(".", "").Replace(" ", "");
			var findTariffRange = USCTariffRule.Loader.GetTariffRange(tariffNumber);
			var result = new ZQuery(USCTariffRuleSchema.U1_Tariff, SQLComparisonOperator.StartsWith, tariffNumber);
			result.AddToFilter(findTariffRange, JoinCondition.Or);
			return result;
		}
	}
}
