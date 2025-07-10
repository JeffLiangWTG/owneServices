using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public static class CurrencyConverterTestHelper
	{
		#region StrategyIfExchangeRateAlreadyExists

		public enum StrategyIfExchangeRateAlreadyExists
		{
			/// <summary>
			/// Same as <see cref="UpdateRateButKeepTimePeriod"/>
			/// </summary>
			/// <remarks>
			/// One may argue that <see cref="SplitTimePeriod"/> is a better default, but this will break many existing
			/// tests. At least at the point of writing.
			/// </remarks>
			Default,

			/// <summary>
			/// Existing exchange rate will be split in order to preserve the rate for already specified time period.
			/// </summary>
			/// <remarks>
			/// For examples: <see cref="CurrencyConverterTestHelperTest.TestSetExchangeRate_StrategySplitTimePeriod"/>
			/// </remarks>
			SplitTimePeriod,

			/// <summary>
			/// Update rate for the entire time period of existing exchange rate.
			/// </summary>
			/// <remarks>
			/// For examples: <see cref="CurrencyConverterTestHelperTest.TestSetExchangeRate_StrategyUpdateRateButKeepTimePeriod"/>
			/// </remarks>
			UpdateRateButKeepTimePeriod,
		}

		static Strategies.HandleIfExchangeRateAlreadyExists StrategySelector(StrategyIfExchangeRateAlreadyExists strategy) => strategy switch
		{
			StrategyIfExchangeRateAlreadyExists.Default => Strategies.UpdateRateButKeepTimePeriod,
			StrategyIfExchangeRateAlreadyExists.SplitTimePeriod => Strategies.SplitTimePeriod,
			StrategyIfExchangeRateAlreadyExists.UpdateRateButKeepTimePeriod => Strategies.UpdateRateButKeepTimePeriod,
			_ => Strategies.UpdateRateButKeepTimePeriod,
		};

		static class Strategies
		{
			public delegate void HandleIfExchangeRateAlreadyExists(BusinessObjectFactory factory, RefExchangeRate exchangeRate, ZDecimal rate, ZDateTime startDate, ZDateTime expiryDate, ZString asPublished);

			public static void SplitTimePeriod(BusinessObjectFactory factory, RefExchangeRate exchangeRate, ZDecimal rate, ZDateTime startDate, ZDateTime expiryDate, ZString asPublished)
			{
				if (exchangeRate.RE_StartDate < startDate)
				{
					var newExchangeRate = CloneExchangeRate(factory, exchangeRate);
					newExchangeRate.RE_ExpiryDate = startDate.AddDays(-1);
					exchangeRate.RE_StartDate = startDate;
				}
				if (exchangeRate.RE_ExpiryDate.AddMinutes(1) > expiryDate)
				{
					var newExchangeRate = CloneExchangeRate(factory, exchangeRate);
					newExchangeRate.RE_StartDate = expiryDate.AddMinutes(1);
					exchangeRate.RE_ExpiryDate = expiryDate.AddDays(-1);
				}
				exchangeRate.RE_SellRate = rate;
				exchangeRate.RE_AsPublished = asPublished;
			}

			public static void UpdateRateButKeepTimePeriod(BusinessObjectFactory factory, RefExchangeRate exchangeRate, ZDecimal rate, ZDateTime startDate, ZDateTime expiryDate, ZString asPublished)
			{
				exchangeRate.RE_SellRate = rate;
				exchangeRate.RE_AsPublished = asPublished;
			}

			static RefExchangeRate CloneExchangeRate(BusinessObjectFactory factory, RefExchangeRate exchangeRate)
			{
				var newExchangeRate = factory.New<RefExchangeRate>();
				newExchangeRate.RE_RX_NKExCurrency = exchangeRate.RE_RX_NKExCurrency;
				newExchangeRate.RE_GC = exchangeRate.RE_GC;
				newExchangeRate.RE_StartDate = exchangeRate.RE_StartDate;
				newExchangeRate.RE_ExpiryDate = exchangeRate.RE_ExpiryDate;
				newExchangeRate.RE_ExRateType = exchangeRate.RE_ExRateType;
				newExchangeRate.RE_SellRate = exchangeRate.RE_SellRate;
				newExchangeRate.RE_AsPublished = exchangeRate.RE_AsPublished;
				return newExchangeRate;
			}
		}

		#endregion

		public static void SetExchangeRateWithAsPublished(BusinessObjectFactory factory, RefCurrency currency, ZDecimal rate, ZDateTime startDate, ZDateTime expiryDate, ZString rateType, ZString asPublished, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, currency.RX_Code)
				.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, startDate)
				.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, expiryDate)
				.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, rateType);

			if (factory.LoadTop1<RefExchangeRate>(filter) is { } exchangeRate)
			{
				StrategySelector(strategy)(factory, exchangeRate, rate, startDate, expiryDate, asPublished);
			}
			else
			{
				exchangeRate = factory.New<RefExchangeRate>();
				exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = startDate;
				exchangeRate.RE_ExpiryDate = expiryDate;
				exchangeRate.RE_ExRateType = rateType;
				exchangeRate.RE_SellRate = rate;
				exchangeRate.RE_AsPublished = asPublished;
			}
		}

		public static void SetExchangeRateWithAsPublished(BusinessObjectFactory factory, RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate, ZString rateType, ZString asPublished, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			var startDate = effectiveDate;
			var expiryDate = effectiveDate.AddDays(1);
			SetExchangeRateWithAsPublished(factory, currency, rate, startDate, expiryDate, rateType, asPublished, strategy);
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, RefCurrency currency, ZDecimal rate, ZDateTime startDate, ZDateTime expiryDate, ZString rateType, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			SetExchangeRateWithAsPublished(factory, currency, rate, startDate, expiryDate, rateType, asPublished: ZString.Empty, strategy: strategy);
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate, ZString rateType, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			var startDate = effectiveDate;
			var expiryDate = effectiveDate.AddDays(1);
			SetExchangeRate(factory, currency, rate, startDate, expiryDate, rateType, strategy);
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, ZString currencyCode, ZDecimal rate, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			var currency = GetCurrency(factory, currencyCode);
			SetExchangeRate(factory, currency, rate);
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, RefCurrency currency, ZDecimal rate, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			SetExchangeRate(factory, currency, rate, ZDateTime.Today, Core.Constants.ExchangeRateTypes.Code.CustomsRate, strategy);
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, ZString currencyCode, ZDecimal rate, ZDateTime effectiveDate, ZString rateType, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			var currency = GetCurrency(factory, currencyCode);
			SetExchangeRate(factory, currency, rate, effectiveDate, rateType, strategy);
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate, StrategyIfExchangeRateAlreadyExists strategy = StrategyIfExchangeRateAlreadyExists.Default)
		{
			SetExchangeRate(factory, currency, rate, effectiveDate, Core.Constants.ExchangeRateTypes.Code.CustomsRate, strategy);
		}

		public static RefCurrency GetCurrency(BusinessObjectFactory factory, ZString currencyCode)
		{
			return factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
		}
	}
}
