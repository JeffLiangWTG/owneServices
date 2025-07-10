using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ExchangeRateWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected internal const string BuyExRateType = "BUY";
		protected internal const string SellExRateType = "SEL";

		public abstract class Schema
		{
			public const string CurrencyNK = "CurrencyNK";
			public const string BuyStartDate = "BuyStartDate";
			public const string BuyExpiryDate = "BuyExpiryDate";
			public const string BuyRate = "BuyRate";
			public const string SellStartDate = "SellStartDate";
			public const string SellExpiryDate = "SellExpiryDate";
			public const string SellRate = "SellRate";
			public const string ExchangeRateDecimalPlaces = "ExchangeRateDecimalPlaces";
		}

		public ExchangeRateWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Business Object Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCurrency();
			ValidateBuyStartDate();
			ValidateBuyExpiryDate();
			ValidateSellStartDate();
			ValidateSellExpiryDate();
			ValidateBuyRate();
			ValidateSellRate();
		}

		public override void Delete()
		{
			base.Delete();
			if (fBuyExchangeRateBizO != null)
			{
				fBuyExchangeRateBizO.Delete();
			}
			if (fSellExchangeRateBizO != null)
			{
				fSellExchangeRateBizO.Delete();
			}
		}

		#endregion

		#region Proxy Properties

		#region CurrencyNK
		[List("Currencies")]
		[MaxLength(RefCurrency.Schema.RX_CodeMaxLength)]
		public ZString CurrencyNK
		{
			get
			{
				return BuyExchangeRateBizO.RE_RX_NKExCurrency;
			}
			set
			{
				BuyExchangeRateBizO.RE_RX_NKExCurrency = value;
				SellExchangeRateBizO.RE_RX_NKExCurrency = value;
				if (!CurrencyNK.IsEmpty)
				{
					PullOutExistingRates(value);
				}
				CurrencyNKInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateCurrency();
				}
			}
		}

		void PullOutExistingRates(ZString curCurrency)
		{
			ZQuery buyRateFilter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, curCurrency);
			buyRateFilter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC,
				SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			buyRateFilter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType,
				SQLComparisonOperator.Equal, BuyExRateType);
			buyRateFilter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.PK,
				SQLComparisonOperator.NotEqual, BuyExchangeRateBizO.PK);
			buyRateFilter.OrderBy = RefExchangeRateSchema.RE_ExpiryDate.Name + " DESC";
			RefExchangeRate latestBuy = (RefExchangeRate)Factory.LoadTop1(typeof(RefExchangeRate),
				buyRateFilter);

			ZQuery sellRateFilter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, curCurrency);
			sellRateFilter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC,
				SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			sellRateFilter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType,
				SQLComparisonOperator.Equal, SellExRateType);
			sellRateFilter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.PK,
				SQLComparisonOperator.NotEqual, SellExchangeRateBizO.PK);
			sellRateFilter.OrderBy = RefExchangeRateSchema.RE_ExpiryDate.Name + " DESC";
			RefExchangeRate latestSell = (RefExchangeRate)Factory.LoadTop1(typeof(RefExchangeRate),
				sellRateFilter);

			// pulling out the dates
			if (latestBuy != null && latestBuy.RE_ExpiryDate.IsValid)
			{
				ZDateTime defaultDate = latestBuy.RE_ExpiryDate.AddDays(1);
				BuyStartDate = defaultDate;
				BuyExpiryDate = defaultDate;
			}
			if (latestSell != null && latestSell.RE_ExpiryDate.IsValid)
			{
				ZDateTime defaultDate = latestSell.RE_ExpiryDate.AddDays(1);
				SellStartDate = defaultDate;
				SellExpiryDate = defaultDate;
			}

			// pulling out the rates
			if (latestBuy != null && latestSell != null)
			{
				BuyRate = latestBuy.RE_SellRate;
				SellRate = latestSell.RE_SellRate;
			}
			else if (latestBuy != null && latestSell == null)
			{
				BuyRate = latestBuy.RE_SellRate;
				SellRate = latestBuy.RE_SellRate;
			}
			else if (latestBuy == null && latestSell == null)
			{
				BuyRate = 0M;
				BuyRateInfo.ClearAllNotifications();
				SellRate = 0M;
				SellRateInfo.ClearAllNotifications();
			}
		}

		public ZPropertyInfo CurrencyNKInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CurrencyNK); }
		}

		#endregion

		#region BuyStartDate

		public ZDateTime BuyStartDate
		{
			get
			{
				return BuyExchangeRateBizO.RE_StartDate;
			}
			set
			{
				BuyExchangeRateBizO.RE_StartDate = value;

				if (value != ZDateTime.Invalid && value != ZDateTime.Empty)
				{
					if (BuyExpiryDate == ZDateTime.Empty)
					{
						BuyExpiryDate = value;
					}
					if (SellStartDate == ZDateTime.Empty || SellStartDate == DateTime.MinValue)
					{
						SellStartDate = value;
					}
					if (SellExpiryDate == ZDateTime.Empty)
					{
						SellExpiryDate = value;
					}
				}

				BuyStartDateInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateBuyStartDate();
				}
			}
		}

		public ZPropertyInfo BuyStartDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.BuyStartDate); }
		}

		#endregion

		#region BuyExpiryDate

		[BusinessObjectTestExclude()]   // testing a property of RefExchangeRate that is excluded 
										// from BusinessObjectTestCase already
		public ZDateTime BuyExpiryDate
		{
			get
			{
				return BuyExchangeRateBizO.RE_ExpiryDate;
			}
			set
			{
				BuyExchangeRateBizO.RE_ExpiryDate = value;

				if (SellExpiryDate == ZDateTime.Empty && value != ZDateTime.Invalid
					&& value != ZDateTime.Invalid)
				{
					SellExpiryDate = value;
				}
				BuyExpiryDateInfo.RefreshBinding();
				ValidateBuyExpiryDate();
			}
		}

		public ZPropertyInfo BuyExpiryDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.BuyExpiryDate); }
		}

		#endregion

		#region BuyRate

		public ZDecimal BuyRate
		{
			get
			{
				return BuyExchangeRateBizO.RE_SellRate;
			}
			set
			{
				BuyExchangeRateBizO.RE_SellRate = value;
				if (SellRate == 0M && BuyRate.IsValid && BuyRate > 0M)
				{
					SellRate = value;
				}
				BuyRateInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateBuyRate();
				}
			}
		}

		public ZPropertyInfo BuyRateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.BuyRate); }
		}

		#endregion

		#region SellStartDate

		[BusinessObjectTestExclude()]
		public ZDateTime SellStartDate
		{
			get
			{
				return SellExchangeRateBizO.RE_StartDate;
			}
			set
			{
				SellExchangeRateBizO.RE_StartDate = value;
				SellStartDateInfo.RefreshBinding();
				ValidateSellStartDate();
			}
		}

		public ZPropertyInfo SellStartDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.SellStartDate); }
		}

		#endregion

		#region SellExpiryDate

		[BusinessObjectTestExclude()]
		public ZDateTime SellExpiryDate
		{
			get
			{
				return SellExchangeRateBizO.RE_ExpiryDate;
			}
			set
			{
				SellExchangeRateBizO.RE_ExpiryDate = value;
				SellExpiryDateInfo.RefreshBinding();
				ValidateSellExpiryDate();
			}
		}

		public ZPropertyInfo SellExpiryDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.SellExpiryDate); }
		}

		#endregion

		#region SellRate

		public ZDecimal SellRate
		{
			get
			{
				return SellExchangeRateBizO.RE_SellRate;
			}
			set
			{
				SellExchangeRateBizO.RE_SellRate = value;
				SellRateInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateSellRate();
				}
			}
		}

		public ZPropertyInfo SellRateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.SellRate); }
		}

		#endregion

		#endregion

		#region List Properties

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					ZQuery localCurrencyFilter = new ZQuery(RefCurrencySchema.PK,
						SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.LocalCurrency.PK);
					fCurrencies = new RefCurrencyCollection(Factory, localCurrencyFilter);
				}
				return fCurrencies;
			}
		}
		protected RefCurrencyCollection fCurrencies;

		#endregion

		ZQuery GetRateQuery(string exRateType, RefExchangeRate exchangeRate)
		{
			var result = ZQuery.NoResultQuery;

			if (exRateType == BuyExRateType && BuyStartDate != ZDateTime.Empty && BuyExpiryDate != ZDateTime.Empty)
			{
				result = GetRateQueryCore(exRateType, exchangeRate, BuyStartDate, BuyExpiryDate);
			}

			if (exRateType == SellExRateType && SellStartDate != ZDateTime.Empty && SellExpiryDate != ZDateTime.Empty)
			{
				result = GetRateQueryCore(exRateType, exchangeRate, SellStartDate, SellExpiryDate);
			}

			return result;
		}

		ZQuery GetRateQueryCore(string exRateType, RefExchangeRate exchangeRate, ZDateTime startDate, ZDateTime expiryDate)
		{
			var lastMonth = startDate.AddMonths(-1);
			var startDateOfLastMonth = new ZDateTime(lastMonth.Year, lastMonth.Month, 1);

			var nextMonth = expiryDate.AddMonths(1);
			var expiryDateOfNextMonth = new ZDateTime(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));

			if (!startDateOfLastMonth.IsValidSmallDateTime || !expiryDateOfNextMonth.IsValidSmallDateTime || startDateOfLastMonth > expiryDateOfNextMonth)
			{
				return ZQuery.NoResultQuery;
			}

			var result = new ZQuery(RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, exRateType);
			result.AddToFilter(RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			result.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, CurrencyNK);
			result.AddToFilter(RefExchangeRateSchema.PK, SQLComparisonOperator.NotEqual, exchangeRate.PK);
			result.AddToFilter(RefExchangeRateSchema.RE_OH_Client, SQLComparisonOperator.Equal, DBNull.Value);
			result.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualTo, expiryDateOfNextMonth);
			result.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDateOfLastMonth);
			return result;
		}

		#region MatchingBuyExchangeRates

		public RefExchangeRateCollection MatchingBuyExchangeRates
		{
			get
			{
				if (fMatchingBuyExchangeRates == null)
				{
					fMatchingBuyExchangeRates = new RefExchangeRateCollection(Factory, GetRateQuery(BuyExRateType, BuyExchangeRateBizO));
				}
				return fMatchingBuyExchangeRates;
			}
		}
		protected RefExchangeRateCollection fMatchingBuyExchangeRates;

		#endregion

		#region MatchingSellExchangeRates

		public RefExchangeRateCollection MatchingSellExchangeRates
		{
			get
			{
				if (fMatchingSellExchangeRates == null)
				{
					fMatchingSellExchangeRates = new RefExchangeRateCollection(Factory, GetRateQuery(SellExRateType, SellExchangeRateBizO));
				}
				return fMatchingSellExchangeRates;
			}
		}
		protected RefExchangeRateCollection fMatchingSellExchangeRates;

		#endregion

		#endregion

		#region Validation

		#region ValidateCurrency

		public void ValidateCurrency()
		{
			CurrencyNKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CurrencyNKInfo);
			ListValidation.ErrorIfInvalidCode(CurrencyNKInfo);
			RefCurrency currencyBizO = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyNK);
			if (currencyBizO != null && !currencyBizO.RX_IsActive)
			{
				CurrencyNKInfo.AddError(Res.GetString("6057d944-d3bd-4a45-a9ea-ec74b73bc95e", "This currency is not active"));
			}
		}

		#endregion

		#region ValidateBuyStartDate

		public void ValidateBuyStartDate()
		{
			BuyStartDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(BuyStartDateInfo);
			TypeValidation.CheckValidSmallDateTime(BuyStartDateInfo);
			MandatoryValidation.CheckEntered(BuyStartDateInfo);

			if (BuyStartDate.IsValid && !BuyStartDate.IsEmpty)
			{
				ValidateBuyExpiryDate();
			}
		}

		#endregion

		#region ValidateBuyExpiryDate

		public void ValidateBuyExpiryDate()
		{
			if (IsInDatabase && !HasChanges)
			{
				return;
			}

			BuyExpiryDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(BuyExpiryDateInfo);
			TypeValidation.CheckValidSmallDateTime(BuyExpiryDateInfo);
			MandatoryValidation.CheckEntered(BuyExpiryDateInfo);

			// if both dates are entered check that start date is before expiry date
			if (BuyExpiryDate.IsValid && !BuyExpiryDate.IsEmpty && BuyStartDate.IsValid && !BuyStartDate.IsEmpty)
			{
				fMatchingBuyExchangeRates = null;

				if (BuyStartDate > BuyExpiryDate)
				{
					BuyExpiryDateInfo.AddError(Res.GetString("a86e8d3c-d1ff-4a04-9507-cb407ab6d0b3", "Start date must be before Expiry date"));
					return;
				}

				// check for a direct overlap of Sell Expiry Date with Sell Exchange Rates for current currency
				var filterResult = MatchingBuyExchangeRates.FirstOrDefault(x => BuyStartDate >= x.RE_StartDate && BuyStartDate <= x.RE_ExpiryDate);
				if (filterResult != null)
				{
					BuyExpiryDateInfo.AddError(Res.GetString("31f752e5-fe36-4a09-b819-267db935b31d", "Entered start date overlaps with another exchange rate with the following dates:\r\nStart Date: {0}\r\nExpiry date: {1}", filterResult.RE_StartDate.ToShortDateString(), filterResult.RE_ExpiryDate.ToShortDateString()));
					return;
				}

				// check for direct overlap of Sell Start Date with Sell Exchange Rates for current currency
				filterResult = MatchingBuyExchangeRates.FirstOrDefault(x => BuyExpiryDate >= x.RE_StartDate && BuyExpiryDate <= x.RE_ExpiryDate);
				if (filterResult != null)
				{
					BuyExpiryDateInfo.AddError(Res.GetString("a15c41ff-5db3-4520-a969-e142e70d6935", "Entered expiry date overlaps with another exchange rate with the following dates:\r\nStart Date: {0}\r\nExpiry date: {1}", filterResult.RE_StartDate.ToShortDateString(), filterResult.RE_ExpiryDate.ToShortDateString()));
					return;
				}

				// check for an enclosing overlap
				filterResult = MatchingBuyExchangeRates.FirstOrDefault(x => (BuyStartDate <= x.RE_StartDate && BuyExpiryDate >= x.RE_ExpiryDate)
				|| (BuyStartDate >= x.RE_StartDate && BuyExpiryDate <= x.RE_ExpiryDate));
				if (filterResult != null)
				{
					BuyExpiryDateInfo.AddError(Res.GetString("48df4344-779c-462c-8c59-cb0fbab76d12", "Entered date overlaps with another exchange rate with the following dates:\r\nStart Date: {0}\r\nExpiry date: {1}", filterResult.RE_StartDate.ToShortDateString(), filterResult.RE_ExpiryDate.ToShortDateString()));
				}
			}
		}

		#endregion

		#region ValidateSellStartDate

		public void ValidateSellStartDate()
		{
			SellStartDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(SellStartDateInfo);
			TypeValidation.CheckValidSmallDateTime(SellStartDateInfo);
			MandatoryValidation.CheckEntered(SellStartDateInfo);

			if (SellStartDate.IsValid && !SellStartDate.IsEmpty)
			{
				ValidateSellExpiryDate();
			}
		}

		#endregion

		#region ValidateSellExpiryDate

		public void ValidateSellExpiryDate()
		{
			if (IsInDatabase && !HasChanges)
			{
				return;
			}

			SellExpiryDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(SellExpiryDateInfo);
			TypeValidation.CheckValidSmallDateTime(SellExpiryDateInfo);
			MandatoryValidation.CheckEntered(SellExpiryDateInfo);

			// if both dates are entered check that start date is before expiry date
			if (SellStartDate.IsValid && !SellStartDate.IsEmpty && SellExpiryDate.IsValid && !SellExpiryDate.IsEmpty)
			{
				fMatchingSellExchangeRates = null;

				if (SellStartDate > SellExpiryDate)
				{
					SellExpiryDateInfo.AddError(Res.GetString("503def23-e90b-46a4-a4f4-c02b408aafbf", "Start date should be before expiry date"));
					return;
				}

				// check for a direct overlap of Sell Expiry Date with Sell Exchange Rates for current currency
				var filterResult = MatchingSellExchangeRates.FirstOrDefault(x => SellStartDate >= x.RE_StartDate && SellStartDate <= x.RE_ExpiryDate);
				if (filterResult != null)
				{
					SellExpiryDateInfo.AddError(Res.GetString("d83b798d-a2c2-41e6-80c6-4e6389b8469e", "Entered expiry date overlaps with another exchange rate with the following dates:\r\nStart Date: {0}\r\nExpiry Date: {1}", filterResult.RE_StartDate.ToShortDateString(), filterResult.RE_ExpiryDate.ToShortDateString()));
					return;
				}

				// check for direct overlap of Sell Start Date with Sell Exchange Rates for current currency
				filterResult = MatchingSellExchangeRates.FirstOrDefault(x => SellExpiryDate >= x.RE_StartDate && SellExpiryDate <= x.RE_ExpiryDate);
				if (filterResult != null)
				{
					SellExpiryDateInfo.AddError(Res.GetString("cbae5064-8df0-437b-b4e5-059dab6c7cf1", "Entered start date overlaps with another exchange rate with the following dates:\r\nStart Date: {0}\r\nExpiry Date: {1}", filterResult.RE_StartDate.ToShortDateString(), filterResult.RE_ExpiryDate.ToShortDateString()));
					return;
				}

				// check for an enclosing overlap
				filterResult = MatchingSellExchangeRates.FirstOrDefault(x => (SellStartDate <= x.RE_StartDate && SellExpiryDate >= x.RE_ExpiryDate)
				|| (SellStartDate >= x.RE_StartDate && SellExpiryDate <= x.RE_ExpiryDate));
				if (filterResult != null)
				{
					SellExpiryDateInfo.AddError(Res.GetString("70a8935f-cfc5-472f-8c6e-6a2358dc111f", "Entered date overlaps with another exchange rate with the following dates: \r\nStart Date: {0}\r\nExpiry Date: {1}", filterResult.RE_StartDate.ToShortDateString(), filterResult.RE_ExpiryDate.ToShortDateString()));
				}
			}
		}

		#endregion

		#region ValidateBuyRate

		public void ValidateBuyRate()
		{
			BuyRateInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(BuyRateInfo, 18, 9);
			MandatoryValidation.CheckEntered(BuyRateInfo);
			if (BuyRate < 0)
			{
				BuyRateInfo.AddError(Res.GetString("0abddc24-dd2f-4bcf-9a9b-71e88cc56303", "Buy Rate must be positive."));
			}
		}

		#endregion

		#region ValidateSellRate

		public void ValidateSellRate()
		{
			SellRateInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(SellRateInfo, 18, 9);
			MandatoryValidation.CheckEntered(SellRateInfo);
			if (SellRate < 0)
			{
				SellRateInfo.AddError(Res.GetString("5f0f355c-b9de-4a69-82b4-6a2e363602d4", "Sell rate must be positive"));
			}
		}

		#endregion

		#endregion

		#region Implementation

		#region BuyExchangeRateBizO

		public RefExchangeRate BuyExchangeRateBizO
		{
			get
			{
				if (fBuyExchangeRateBizO == null)
				{
					fBuyExchangeRateBizO = Factory.New<RefExchangeRate>();
					using (fBuyExchangeRateBizO.GetValidationSuspender())
					using (fBuyExchangeRateBizO.SuspendSettingHasChanges())
					{
						fBuyExchangeRateBizO.RE_ExRateType = BuyExRateType;
						fBuyExchangeRateBizO.RE_SellRate = 0;
						fBuyExchangeRateBizO.RE_StartDate = ZDateTime.Empty;
						RegisterEditableChildObject(fBuyExchangeRateBizO);
					}
				}
				return fBuyExchangeRateBizO;
			}
		}
		protected internal RefExchangeRate fBuyExchangeRateBizO;

		#endregion

		#region SellExchangeRateBizO

		public RefExchangeRate SellExchangeRateBizO
		{
			get
			{
				if (fSellExchangeRateBizO == null)
				{
					fSellExchangeRateBizO = Factory.New<RefExchangeRate>();
					using (fSellExchangeRateBizO.GetValidationSuspender())
					using (fSellExchangeRateBizO.SuspendSettingHasChanges())
					{
						fSellExchangeRateBizO.RE_ExRateType = SellExRateType;
						fSellExchangeRateBizO.RE_SellRate = 0;
						fSellExchangeRateBizO.RE_StartDate = ZDateTime.Empty;
						RegisterEditableChildObject(fSellExchangeRateBizO);
					}
				}
				return fSellExchangeRateBizO;
			}
		}
		protected internal RefExchangeRate fSellExchangeRateBizO;

		#endregion

		#endregion
	}
}
