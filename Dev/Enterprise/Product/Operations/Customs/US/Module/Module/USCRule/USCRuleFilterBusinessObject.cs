using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCRuleFilterBusinessObject : FilterStripBusinessObject
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
			result.AddTextFilter(Schema.RuleCode, USCRuleSchema.U0_Code, () => new TariffRuleList());
			var tariffFilter = new TariffProvTariffModuleFilter(Schema.Tariff, GetTariffQuery);
			tariffFilter.MaxLength = USCTariffRuleSchema.U1_Tariff.MaxLength;
			tariffFilter.Category = FilterCategories.NumbersAndReferences;
			result.AddCustomFilter(tariffFilter);
			result.AddDateFilter(Schema.EffectiveDates, GetEffectiveDateQuery);
			return result;
		}

		ZQuery GetEffectiveDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(USCRule));

			if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				var subQueryOnTariffRule = new ZDBOnlySubQuery(typeof(USCTariffRule), USCTariffRuleSchema.U1_RuleCode);

				if (date1.IsValid)
				{
					subQueryOnTariffRule.AddToFilter(USCTariffRuleSchema.U1_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date1);
				}

				if (date2.IsValid)
				{
					var dateToQuery = new ZQuery(USCTariffRuleSchema.U1_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date2);
					dateToQuery.AddToFilter(JoinCondition.Or, USCTariffRuleSchema.U1_DateTo, SQLComparisonOperator.Equal, DBNull.Value);
					subQueryOnTariffRule.AddToFilter(dateToQuery);
				}

				result.AddSubQuery(USCRuleSchema.U0_Code, subQueryOnTariffRule, JoinCondition.And);
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
			var result = new ZDBOnlyQuery(typeof(USCRule));
			var findTariffRange = USCTariffRule.Loader.GetTariffRange(tariffNumber);

			var subQueryOnTariffRule = new ZDBOnlySubQuery(typeof(USCTariffRule), USCTariffRuleSchema.U1_RuleCode);
			subQueryOnTariffRule.AddToFilter(JoinCondition.And, USCTariffRuleSchema.U1_Tariff, SQLComparisonOperator.StartsWith, tariffNumber);
			subQueryOnTariffRule.AddToFilter(findTariffRange, JoinCondition.Or);
			result.AddSubQuery(USCRuleSchema.U0_Code, subQueryOnTariffRule, JoinCondition.And);

			return result;
		}
	}
}
