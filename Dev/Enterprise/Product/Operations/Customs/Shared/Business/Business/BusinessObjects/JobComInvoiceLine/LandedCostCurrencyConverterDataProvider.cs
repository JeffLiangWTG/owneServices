using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class LandedCostCurrencyConverterDataProvider : ICurrencyConverterDataProviderWithFixedExRates
	{
		public LandedCostCurrencyConverterDataProvider(BaseJobComInvoiceHeader invoice)
		{
			this.invoice = invoice;
		}

		readonly BaseJobComInvoiceHeader invoice;

		#region ICurrencyConverterDataProviderWithFixedExRates Members

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate
		{
			get { return invoice.LandedCostingExRateFallBackToJobExRate; }
		}

		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode
		{
			get { return invoice.Invoice_Currency != null ? invoice.Invoice_Currency.RX_Code : ZString.Empty; }
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return ((ICurrencyConverterDataProvider)invoice).DateOfValuation; }
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return ((ICurrencyConverterDataProvider)invoice).MaximumDaysToFallback; }
		}

		ZArchitecture.Core.ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return ((ICurrencyConverterDataProvider)invoice).RateType; }
		}

		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return ((ICurrencyConverterDataProvider)invoice).Company; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return ((ICurrencyConverterDataProvider)invoice).LocalCurrencyCodeOverride; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return ((ICurrencyConverterDataProvider)invoice).IsReciprocalOverride; }
		}

		#endregion
	}
}
