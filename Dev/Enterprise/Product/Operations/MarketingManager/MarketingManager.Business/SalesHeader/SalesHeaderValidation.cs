using System.Linq;
using CargoWise.ComponentModel;

namespace Enterprise.MarketingManager.Business
{
	public class SalesHeaderValidation : AutoSalesHeaderValidation
	{
		public SalesHeaderValidation(AutoSalesHeader parent)
			: base(parent)
		{
		}

		public new SalesHeader Parent
		{
			get { return (SalesHeader)base.Parent; }
		}

		protected override void CheckTotalCurrencyCode()
		{
			base.CheckTotalCurrencyCode();

			if (!Parent.TotalCurrencyCodeInfo.HasErrors())
			{
				var missingExchangeRates = Parent.GetMissingExchangeRates();
				if (missingExchangeRates.Any())
				{
					var dateForExchangeRate = Parent.RevenueCalculator.DateForExchangeRate;
					var companyForExchangeRate = Parent.RevenueCalculator.CompanyForExchangeRate;
					var message = EntitySalesWrapperValidation.GetNoExchangeRateFoundMessage(
						dateForExchangeRate,
						companyForExchangeRate,
						missingExchangeRates.Select(x => x.RX_Code.ToString()));

					Parent.TotalCurrencyCodeInfo.AddWarning(message);
				}
			}
		}
	}
}
