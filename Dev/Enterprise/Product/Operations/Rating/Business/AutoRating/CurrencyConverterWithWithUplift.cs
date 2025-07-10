using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class CurrencyConverterWithWithUplift
	{
		public CurrencyConverterWithWithUplift(BusinessObjectFactory factory, AutoRateInfo master, IJobExRateCurrencyConverter parentConverter)
		{
			Master = master;
			Factory = factory;
			ParentConverter = parentConverter ?? NonOrgSpecificExRateCurrencyConverter.Default(factory);
		}

		public ZDecimal Convert(ZDecimal amount, ZString sourceCurrencyCode, ZString destinationCurrencyCode)
		{
			var rate = GetExchangeRate(sourceCurrencyCode, destinationCurrencyCode);
			ZDecimal result = 0m;
			if (rate != 0)
			{
				if (Company.GC_IsReciprocal)
				{
					result = amount * rate;
				}
				else
				{
					result = amount / rate;
				}
			}
			var destinationCurrency = GetCurrencyFromCode(destinationCurrencyCode);
			if (destinationCurrency != null)
			{
				result = Utilities.Round(result, destinationCurrency.Decimals);
			}

			return result;
		}

		public ZDecimal GetExchangeRate(ZString sourceCurrencyCode, ZString destinationCurrencyCode)
		{
			return GetExchangeRate(sourceCurrencyCode, destinationCurrencyCode, true);
		}

		public ZDecimal GetExchangeRate(ZString sourceCurrencyCode, ZString destinationCurrencyCode, bool includeCFXInRatingCalculation)
		{
			ZDecimal rate = 0m;

			if (sourceCurrencyCode == destinationCurrencyCode)
			{
				rate = 1m;
			}
			else
			{
				var sourceCurrency = GetCurrencyFromCode(sourceCurrencyCode);
				var destinationCurrency = GetCurrencyFromCode(destinationCurrencyCode);
				var localCurrency = Company.LocalCurrency;
				if (sourceCurrency != null && destinationCurrency != null && localCurrency != null)
				{
					ZDecimal rate1 = 1m;
					if (sourceCurrency.PK != localCurrency.PK)
					{
						ParentConverter.RateType = ExchangeRateType.Sell;
						rate1 = ParentConverter.GetExchangeRate(sourceCurrency, ZGuid.Empty, CostSell.Revenue);
					}

					ZDecimal rate2 = 1m;
					if (destinationCurrency.PK != localCurrency.PK)
					{
						ParentConverter.RateType = ExchangeRateType.Buy;
						rate2 = ParentConverter.GetExchangeRate(destinationCurrency, ZGuid.Empty, CostSell.Cost);
					}

					rate = rate2 != 0m ? rate1 / rate2 : 0m;
					if (includeCFXInRatingCalculation)
					{
						if (Company.GC_IsReciprocal)
						{
							rate *= (100m + CFX) / 100m; // increase exchange rate by uplift (increase for conversion to local currency)
						}
						else
						{
							rate *= (100m - CFX) / 100m; // reduce exchange rate by uplift (reduce for conversion to local currency)
						}
					}
				}
			}

			return rate;
		}

		#region Implementation

		readonly AutoRateInfo Master;
		readonly BusinessObjectFactory Factory;
		readonly IJobExRateCurrencyConverter ParentConverter;

		GlbCompany Company
		{
			get { return Master != null ? Master.Company : GlbCompany.CurrentCompany; }
		}

		ZDecimal CFX
		{
			get { return Master != null ? Master.CFX : (ZDecimal)0m; }
		}

		RefCurrency GetCurrencyFromCode(ZString currencyCode)
		{
			return RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
		}

		#endregion
	}
}

