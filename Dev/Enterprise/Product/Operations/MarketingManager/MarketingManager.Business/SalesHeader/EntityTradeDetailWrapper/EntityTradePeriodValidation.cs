using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class EntityTradePeriodValidation : OrgTradePeriodValidation
	{
		public EntityTradePeriodValidation(EntityTradePeriod parent) : base(parent)
		{
		}

		new EntityTradePeriod Parent => base.Parent as EntityTradePeriod;

		protected override void CheckPAS_RX_NKCurrency()
		{
			base.CheckPAS_RX_NKCurrency();

			if (!Parent.PAS_RX_NKCurrencyInfo.HasErrors())
			{
				var entitySales = Parent.TradeDetail.Parent as EntitySalesWrapper;
				var missingExchangeRates = entitySales != null ?
					entitySales.RevenueCalculator.GetMissingExchangeRatesToConvertTo(Parent.Currency) :
					Enumerable.Empty<RefCurrency>();

				if (missingExchangeRates.Any())
				{
					var dateForExchangeRate = entitySales.RevenueCalculator.DateForExchangeRate;
					var companyForExchangeRate = entitySales.RevenueCalculator.CompanyForExchangeRate;
					var message = EntitySalesWrapperValidation.GetNoExchangeRateFoundMessage(
						dateForExchangeRate,
						companyForExchangeRate,
						missingExchangeRates.Select(x => x.RX_Code.ToString()));

					Parent.PAS_RX_NKCurrencyInfo.AddWarning(message);
				}
			}
		}
	}
}
