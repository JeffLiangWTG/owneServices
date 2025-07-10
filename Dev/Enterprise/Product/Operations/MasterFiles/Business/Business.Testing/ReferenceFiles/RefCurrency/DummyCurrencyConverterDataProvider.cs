using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyCurrencyConverterDataProvider : ICurrencyConverterDataProvider
	{
		public DummyCurrencyConverterDataProvider()
		{
		}

		public DummyCurrencyConverterDataProvider(ZDateTime dateOfValuation, ExchangeRateType rateType, int maximumDaysToFallback)
		{
			DateOfValuationExposed = dateOfValuation;
			RateTypeExposed = rateType;
			MaximumDaysToFallbackExposed = maximumDaysToFallback;
		}

		public ZDateTime DateOfValuationExposed;
		public ZDateTime DateOfValuation
		{
			get
			{
				return DateOfValuationExposed;
			}
		}

		public ExchangeRateType RateTypeExposed;
		public ExchangeRateType RateType
		{
			get
			{
				return RateTypeExposed;
			}
		}

		public int MaximumDaysToFallbackExposed;
		public int MaximumDaysToFallback
		{
			get
			{
				return MaximumDaysToFallbackExposed;
			}
		}

		public GlbCompany CompanyExposed;
		public GlbCompany Company
		{
			get { return CompanyExposed ?? GlbCompany.CurrentCompany; }
		}

		public ZString LocalCurrencyCodeOverrideExposed = ZString.Empty;
		public ZString LocalCurrencyCodeOverride
		{
			get { return LocalCurrencyCodeOverrideExposed; }
		}

		public ZBool? IsReciprocalOverrideExposed;
		public ZBool? IsReciprocalOverride
		{
			get { return IsReciprocalOverrideExposed; }
		}
	}
}
