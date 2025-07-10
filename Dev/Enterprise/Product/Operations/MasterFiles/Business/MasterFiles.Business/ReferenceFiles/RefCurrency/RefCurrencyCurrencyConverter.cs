using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCurrencyCurrencyConverter : CurrencyConverter
	{
		public RefCurrencyCurrencyConverter(BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, int maximumDaysToFallback)
			: this(GlbCompany.CurrentCompany, factory, dateForRate, rateType, maximumDaysToFallback)
		{
		}

		public RefCurrencyCurrencyConverter(GlbCompany company, BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, int maximumDaysToFallback)
			: base(company, factory, dateForRate, rateType)
		{
			base.maximumDaysToFallback = maximumDaysToFallback;
		}

		public RefCurrencyCurrencyConverter(BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, bool roundToTargetCurrencyDecimals)
			: base(GlbCompany.CurrentCompany, factory, dateForRate, rateType, roundToTargetCurrencyDecimals)
		{
		}

		public RefCurrencyCurrencyConverter(GlbCompany company, BusinessObjectFactory factory) : base(company, factory) { }

		public RefCurrencyCurrencyConverter(GlbCompany company, BusinessObjectFactory factory, bool roundToTargetCurrencyDecimals) : base(company, factory, roundToTargetCurrencyDecimals) { }

		public RefCurrencyCurrencyConverter(BusinessObjectFactory factory) : base(factory) { }

		public RefCurrencyCurrencyConverter(BusinessObjectFactory factory, bool roundToTargetCurrencyDecimals) : base(factory, roundToTargetCurrencyDecimals) { }

		public override ZDecimal GetExchangeRate(ICurrency currency)
		{
			ZDateTime dontCareDateFallBack;
			return GetExchangeRate(currency, out dontCareDateFallBack);
		}

		protected override ZDateTime DateForRateCore
		{
			get { return base.DateForRateCore; }
			set
			{
				base.DateForRateCore = value;
				RefreshCachedExchangeRates();
			}
		}

		protected override ExchangeRateType RateTypeCore
		{
			get { return base.RateTypeCore; }
			set
			{
				base.RateTypeCore = value;
				RefreshCachedExchangeRates();
			}
		}

		CachedProperty<Dictionary<string, ExchangeRate>> CachedRates
		{
			get
			{
				if (cachedRates == null)
				{
					cachedRates = new CachedProperty<Dictionary<string, ExchangeRate>>(Factory, GetRates);
				}
				return cachedRates;
			}
		}
#if DEBUG
		internal
#endif
		CachedProperty<Dictionary<string, ExchangeRate>> cachedRates;

		Dictionary<string, ExchangeRate> GetRates()
		{
			return new Dictionary<string, ExchangeRate>();  // Believe me it looks shite, but it actually does the trick :(
		}

#if DEBUG
		internal
#endif
			struct ExchangeRate
		{
			public ExchangeRate(ZDateTime date, ZDecimal rate)
			{
				this.Date = date;
				this.Rate = rate;
			}
			public readonly ZDateTime Date;
			public readonly ZDecimal Rate;
		}

		protected void RefreshCachedExchangeRates()
		{
			cachedRates = null;
		}

		protected internal void RefreshCachedExchangeRatesfIfNeeded()
		{
			if (NeedToRefreshCachedExchangeRates)
			{
				RefreshCachedExchangeRates();
			}
		}

		protected virtual bool NeedToRefreshCachedExchangeRates
		{
			get { return false; }
		}

		public override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate)
		{
			ZDecimal result;
			if (currency == null)
			{
				result = 0m;
				foundRateDate = DateForRate;
			}
			else if (LocalCurrency != null && currency.PK == LocalCurrency.PK)
			{
				result = 1m;
				foundRateDate = DateForRate;
			}
			else
			{
				RefreshCachedExchangeRatesfIfNeeded();

				ExchangeRate cachedValue;
				if (currency != null && CachedRates.Value.TryGetValue(currency.Code, out cachedValue))
				{
#if DEBUG
					HasFoundCachedValue = true;
#endif
					foundRateDate = cachedValue.Date;
					result = cachedValue.Rate;
				}
				else
				{
#if DEBUG
					HasFoundCachedValue = false;
#endif
					RefExchangeRate refExchangeRate = GetExchangeRateObject(currency);
					if (refExchangeRate != null)
					{
						result = refExchangeRate.RE_SellRate;
						if (DateForRate.Date.IsValid)
						{
							foundRateDate = DateForRate.Date;
							while (foundRateDate > refExchangeRate.RE_ExpiryDate && foundRateDate >= DateForRate.Date.AddDays(-MaximumDaysToFallback))
							{
								foundRateDate = foundRateDate.AddDays(-1);
							}
						}
						else
						{
							foundRateDate = ZDateTime.Empty;
						}
					}
					else
					{
						result = 0m;
						foundRateDate = ZDateTime.Empty;
					}

					if (currency != null)
					{
						CachedRates.Value[currency.Code] = new ExchangeRate(foundRateDate, result);
					}
				}
			}

			return result;
		}

		public ZDecimal GetExchangeRate(ICurrency currency, ICurrency destinationCurrency)
		{
			ZDecimal result;
			if (currency == null)
			{
				result = 0m;
			}
			else if (destinationCurrency != null && currency.PK == destinationCurrency.PK)
			{
				result = 1m;
			}
			else
			{
				var currencyEx = GetExchangeRate(currency);
				var destEx = GetExchangeRate(destinationCurrency);
				if (destEx != 0m)
				{
					result = currencyEx / destEx;
				}
				else
				{
					result = 0m;
				}
			}

			return result;
		}

		#region Implementation

#if DEBUG
		internal bool HasFoundCachedValue;
#endif

		protected string RateCode
		{
			get
			{
				switch (RateType)
				{
					case ZArchitecture.Core.ExchangeRateType.Buy:
						return Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate;
					case ZArchitecture.Core.ExchangeRateType.Sell:
						return Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;
					case ZArchitecture.Core.ExchangeRateType.Customs:
						return Enterprise.Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					case ZArchitecture.Core.ExchangeRateType.CustomsSecondary:
						return Enterprise.Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
					case ZArchitecture.Core.ExchangeRateType.CustomsMeasureEURExRate:
						return Enterprise.Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
					case ZArchitecture.Core.ExchangeRateType.IATA:
						return Enterprise.Core.Constants.ExchangeRateTypes.Code.IATARate;
					default:
						return "BUY";
				}
			}
		}

		public RefExchangeRate GetExchangeRateObject(ICurrency currency)
		{
			RefExchangeRate result = null;
			ZDateTime dateForRate = DateForRate;
			if (currency != null && dateForRate.IsValid)
			{
				ZQuery sQLFilter = new ZQuery();
				sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, currency.Code);
				sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, Company.PK);
				sQLFilter.AddToFilter(RateTypeFilter);
				sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, ZDateTime.GetValidSmallDateTime(dateForRate.Date.AddDays(1)));
				sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.GetValidSmallDateTime(dateForRate.Date.AddDays(-MaximumDaysToFallback)));
				sQLFilter.AddToFilter(RefExchangeRateSchema.RE_OH_Client, null);
				if (RateType == ZArchitecture.Core.ExchangeRateType.All)
				{
					result = LoadAndGetExRate(sQLFilter);
				}
				else
				{
					sQLFilter.OrderBy = RefExchangeRateSchema.RE_StartDate.Name + " DESC";
					result = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
				}
			}
			return result;
		}

		/// <summary>
		/// If the rate type is 'All', will get a rate in the order of Customs -> Sell -> Buy type.
		/// </summary>
		/// <param name="sqlFilter"></param>
		/// <returns></returns>
		protected RefExchangeRate LoadAndGetExRate(ZQuery sqlFilter)
		{
			RefExchangeRateCollection rates = new RefExchangeRateCollection(Factory);
			rates.AdditionalFilter = sqlFilter;
			rates.ApplySort(new ExRateTypeComparer(DateForRate));

			RefExchangeRate result = null;
			if (rates.Count >= 1)
			{
				result = rates[rates.Count - 1];
			}
			return result;
		}

		/// <summary>
		/// Rate Types : Customs > Sell > Buy
		/// </summary>
		protected class ExRateTypeComparer : IComparer
		{
			public ExRateTypeComparer(ZDateTime dateForRate)
			{
				this.DateForRate = dateForRate;
			}

			readonly ZDateTime DateForRate;

			public override bool Equals(object obj)
			{
				return obj.GetType() == GetType();
			}

			public override int GetHashCode()
			{
				return 1;
			}

			#region Compare

			public int Compare(object x, object y)
			{
				RefExchangeRate rate1 = x as RefExchangeRate;
				RefExchangeRate rate2 = y as RefExchangeRate;

				int result = 0;

				if (DateForRate.Date.IsValid)
				{
					ZDateTime foundRateDate1 = DateForRate.Date;
					ZDateTime foundRateDate2 = DateForRate.Date;

					while (foundRateDate1 > rate1.RE_ExpiryDate)
					{
						foundRateDate1 = foundRateDate1.AddDays(-1);
					}
					while (foundRateDate2 > rate2.RE_ExpiryDate)
					{
						foundRateDate2 = foundRateDate2.AddDays(-1);
					}
					result = foundRateDate1.CompareTo(foundRateDate2);
				}

				if (result == 0)
				{
					if (rate1.RE_ExRateType == rate2.RE_ExRateType)
					{
						result = rate1.RE_StartDate.CompareTo(rate2.RE_StartDate);
					}
					else if (rate1.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate
						|| (rate1.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate && rate2.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.BuyRate))
					{
						result = 1;
					}
					else if (rate2.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate
						|| (rate2.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate && rate1.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.BuyRate))
					{
						result = -1;
					}
				}

				return result;
			}

			#endregion
		}

		protected ZQuery RateTypeFilter
		{
			get
			{
				ZQuery result = new ZQuery();
				if (RateType == ZArchitecture.Core.ExchangeRateType.All)
				{
					result.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate);
					result.AddToFilter(JoinCondition.Or, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate);
					result.AddToFilter(JoinCondition.Or, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, Enterprise.Core.Constants.ExchangeRateTypes.Code.CustomsRate);
				}
				else
				{
					result.AddToFilter(RefExchangeRateSchema.RE_ExRateType, RateCode);
				}
				return result;
			}
		}

		#endregion
	}
}
