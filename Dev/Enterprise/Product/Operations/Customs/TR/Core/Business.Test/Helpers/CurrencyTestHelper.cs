using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	public static class CurrencyTestHelper
	{
		public static void SetExchangeRate(RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var rateCode = GetRateCode(rateType);
			var exchangeRate = FindExchangeRate(currency, effectiveDate, rateCode);
			if (exchangeRate == null)
			{
				exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = rateCode;
			}
			exchangeRate.RE_SellRate = rate;
		}

		public static void RemoveExchangeRate(RefCurrency currency, ZDateTime effectiveDate, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var rateCode = GetRateCode(rateType);
			var exchangeRate = FindExchangeRate(currency, effectiveDate, rateCode);
			if (exchangeRate != null)
			{
				exchangeRate.Delete();
			}
		}

		public static void SetExchangeRate(string currency, ZDecimal rate, ZDateTime effectiveDate, BusinessObjectFactory factory, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var refCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			SetExchangeRate(refCurrency, rate, effectiveDate, rateType);
		}

		public static void RemoveExchangeRate(string currency, ZDateTime effectiveDate, BusinessObjectFactory factory, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var refCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			RemoveExchangeRate(refCurrency, effectiveDate, rateType);
		}

		public static string GetRateCode(ExchangeRateType rateType)
		{
			return rateType == ExchangeRateType.Customs ? Core.Constants.ExchangeRateTypes.Code.CustomsRate : Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
		}

		public static RefExchangeRate FindExchangeRate(RefCurrency currency, ZDateTime effectiveDate, ZString rateCode)
		{
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, currency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, rateCode);

			return currency.Factory.LoadTop1<RefExchangeRate>(filter);
		}
	}
}
