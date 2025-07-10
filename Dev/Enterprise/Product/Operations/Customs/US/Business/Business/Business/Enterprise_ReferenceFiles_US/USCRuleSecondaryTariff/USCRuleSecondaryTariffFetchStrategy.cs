using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCRuleSecondaryTariffFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public USCRuleSecondaryTariffFetchStrategy(USCRuleSecondaryTariff secondaryTariffRule)
			: base(secondaryTariffRule)
		{
		}

		USCRuleSecondaryTariff SecondaryTariffRule
		{
			get { return (USCRuleSecondaryTariff)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(USCRuleSecondaryTariffExceptionSchema.U4_U3, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			if (!SecondaryTariffRule.U3_TariffFrom.IsEmpty)
			{
				Factory.AddFetchHint(USCTariffSchema.Instance, new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, SecondaryTariffRule.U3_TariffFrom));

				if (!SecondaryTariffRule.U3_Tariff2.IsEmpty)
				{
					Factory.AddFetchHint(USCTariffSchema.Instance, new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, SecondaryTariffRule.U3_Tariff2));
				}

				if (!SecondaryTariffRule.U3_Tariff3.IsEmpty)
				{
					Factory.AddFetchHint(USCTariffSchema.Instance, new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, SecondaryTariffRule.U3_Tariff3));
				}

				if (!SecondaryTariffRule.U3_TariffTo.IsEmpty)
				{
					Factory.AddFetchHint(USCTariffSchema.Instance, new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, SecondaryTariffRule.U3_TariffTo));
				}
			}
		}
	}
}
