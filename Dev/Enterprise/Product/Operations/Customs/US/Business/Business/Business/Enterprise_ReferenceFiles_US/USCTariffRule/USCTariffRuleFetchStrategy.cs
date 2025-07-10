using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCTariffRuleFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public USCTariffRuleFetchStrategy(USCTariffRule tariffRule)
			: base(tariffRule)
		{
		}

		USCTariffRule TariffRule
		{
			get { return (USCTariffRule)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(USCTariffRuleExceptionSchema.U2_U1, BusinessObject.PK);
			Factory.AddFetchHint(USCRuleSecondaryTariffSchema.U3_U1, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			if (!TariffRule.U1_Tariff.IsEmpty)
			{
				Factory.AddFetchHint(USCTariffSchema.Instance, new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, TariffRule.U1_Tariff));

				if (TariffRule.U1_RuleCode == TariffRuleList.Codes.EligibleForSecondaryTariffNumbers)
				{
					Factory.AddFetchHint(USCTariffSchema.UE_Tariff, TariffRule.U1_Tariff);
				}

				if (!TariffRule.U1_TariffTo.IsEmpty)
				{
					Factory.AddFetchHint(USCTariffSchema.Instance, new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, TariffRule.U1_TariffTo));
				}

				var duplicateQuery = USCTariffRule.Loader.GetTariffQuery(TariffRule.U1_Tariff, TariffRule.EffectiveTariffTo);
				duplicateQuery.AddToFilter(USCTariffRuleSchema.U1_RuleCode, TariffRule.U1_RuleCode);
				if (!duplicateQuery.IsNoResultQuery)
				{
					Factory.AddFetchHint(USCTariffRuleSchema.Instance, duplicateQuery);
				}
			}
		}
	}
}
