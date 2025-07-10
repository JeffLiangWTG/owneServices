using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCurrencyConverterWithTestExchangeRates : CurrencyConverterWithDataProvider
	{
		public NZCurrencyConverterWithTestExchangeRates(JobDeclaration declaration)
			: base(declaration.Factory, declaration)
		{
			this.declaration = declaration;
		}

		#region GetExchangeRate
		public override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate)
		{
			ZDecimal result = GetExchangeRate(currency);
			foundRateDate = (result == 0) ? ZDateTime.Empty : DateForRate;
			return result;
		}

		public override ZDecimal GetExchangeRate(ICurrency currency)
		{
			ZDecimal result = 0;
			if (currency != null)
			{
				if (currency.PK == LocalCurrency.PK)
				{
					result = 1;
				}
				else if (IsInTestMode)
				{
					result = GetTestExchangeRate(currency.Code);
				}
				else if (DateForRate.IsValid)
				{
					result = GetRealExchangeRate(currency);
				}
			}
			return result;
		}
		#endregion

		#region Implementation

		protected JobDeclaration declaration;

		#region IsInTestMode
		protected bool IsInTestMode
		{
			get
			{
				bool result = false;
#if DEBUG
				if (Globals.IsTest)
				{
					if (declaration != null)
					{
						result = declaration.IsInTestMode;
					}
				}
#endif
				return result;
			}
		}
		#endregion

		#region GetTestExchangeRate
		protected ZDecimal GetTestExchangeRate(string currencyCode)
		{
			if (TestExchangeRates.ContainsKey(currencyCode))
			{
				return (decimal)TestExchangeRates[currencyCode];
			}
			else
			{
				return 0.00m;
			}
		}

		protected Hashtable TestExchangeRates
		{
			get
			{
				if (fTestExchangeRates == null)
				{
					fTestExchangeRates = new Hashtable();
					fTestExchangeRates.Add("AUD", 0.90m);
					fTestExchangeRates.Add("CAD", 0.81m);
					fTestExchangeRates.Add("CHF", 0.78m);
					fTestExchangeRates.Add("CLP", 394.10m);
					fTestExchangeRates.Add("CNY", 4.97m);
					fTestExchangeRates.Add("DKK", 3.75m);
					fTestExchangeRates.Add("EGP", 3.67m);
					fTestExchangeRates.Add("EUR", 0.51m);
					fTestExchangeRates.Add("FJD", 1.09m);
					fTestExchangeRates.Add("GBP", 0.35m);
					fTestExchangeRates.Add("HKD", 4.61m);
					fTestExchangeRates.Add("ILS", 2.65m);
					fTestExchangeRates.Add("INR", 26.74m);
					fTestExchangeRates.Add("JMD", 35.39m);
					fTestExchangeRates.Add("JPY", 65.72m);
					fTestExchangeRates.Add("KRW", 690.88m);
					fTestExchangeRates.Add("LKR", 56.10m);
					fTestExchangeRates.Add("MXN", 6.61m);
					fTestExchangeRates.Add("NOK", 4.15m);
					fTestExchangeRates.Add("NZD", 1.00m);
					fTestExchangeRates.Add("PGK", 1.90m);
					fTestExchangeRates.Add("PHP", 32.25m);
					fTestExchangeRates.Add("PKR", 34.03m);
					fTestExchangeRates.Add("SEK", 4.56m);
					fTestExchangeRates.Add("SGD", 1.03m);
					fTestExchangeRates.Add("THB", 23.30m);
					fTestExchangeRates.Add("TOP", 1.25m);
					fTestExchangeRates.Add("TRL", 833791.50m);
					fTestExchangeRates.Add("TWD", 20.26m);
					fTestExchangeRates.Add("USD", 0.64m);
					fTestExchangeRates.Add("WST", 1.72m);
					fTestExchangeRates.Add("XPF", 59.69m);
					fTestExchangeRates.Add("ZAR", 4.09m);
				}
				return fTestExchangeRates;
			}
		}
		Hashtable fTestExchangeRates;
		#endregion

		#region GetRealExchangeRate
		protected ZDecimal GetRealExchangeRate(ICurrency currency)
		{
			ZDecimal result = 0.00m;
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, currency.Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, Env.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Enterprise.Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualTo, DateForRate);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, DateForRate);

			RefExchangeRate exchangeRate = LoadAndGetExRate(sQLFilter);

			if (exchangeRate != null)
			{
				result = exchangeRate.RE_SellRate;
			}
			return result;
		}

		protected new RefExchangeRate LoadAndGetExRate(ZQuery sqlFilter)
		{
			RefExchangeRateCollection rates = new RefExchangeRateCollection(Factory);
			rates.AdditionalFilter = sqlFilter;

			RefExchangeRate result = null;
			if (rates.Count >= 1)
			{
				result = rates[rates.Count - 1];
			}
			return result;
		}
		#endregion

		#endregion
	}
}
