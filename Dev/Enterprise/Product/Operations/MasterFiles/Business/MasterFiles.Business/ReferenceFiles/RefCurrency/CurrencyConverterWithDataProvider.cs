
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CurrencyConverterWithDataProvider : RefCurrencyCurrencyConverter
	{
		public CurrencyConverterWithDataProvider(BusinessObjectFactory factory, ICurrencyConverterDataProvider dataProvider)
			: base(dataProvider.Company, factory)
		{
			this.DataProvider = dataProvider;
		}

		public CurrencyConverterWithDataProvider(BusinessObjectFactory factory, ICurrencyConverterDataProvider dataProvider, bool roundToTargetCurrencyDecimals = true)
			: base(dataProvider.Company, factory, roundToTargetCurrencyDecimals)
		{
			this.DataProvider = dataProvider;
		}

		protected readonly ICurrencyConverterDataProvider DataProvider;

		protected sealed override ZDateTime DateForRateCore
		{
			get { return DataProvider.DateOfValuation; }
			set { }
		}

		protected sealed override ExchangeRateType RateTypeCore
		{
			get { return DataProvider.RateType; }
			set { }
		}

		public override int MaximumDaysToFallback
		{
			get { return DataProvider.MaximumDaysToFallback; }
		}

		ZDateTime DateForRateUsedLastTime;
		ExchangeRateType RateTypeUsedLastTime;

		protected override bool NeedToRefreshCachedExchangeRates
		{
			get
			{
				bool result = base.NeedToRefreshCachedExchangeRates ||
					DateForRateUsedLastTime != DataProvider.DateOfValuation ||
					RateTypeUsedLastTime != DataProvider.RateType;

				DateForRateUsedLastTime = DateForRate;
				RateTypeUsedLastTime = RateType;

				return result;
			}
		}

		public sealed override ZString LocalCurrencyCodeOverride
		{
			get { return DataProvider.LocalCurrencyCodeOverride; }
		}

		public sealed override ZBool? IsReciprocalOverride
		{
			get { return DataProvider.IsReciprocalOverride; }
		}

		protected override ZBool IsConverterValidCore
		{
			get
			{
				var dataProvider = DataProvider as EnterpriseBusinessObject;
				return dataProvider == null || !dataProvider.IsDeleted;
			}
		}
	}
}
