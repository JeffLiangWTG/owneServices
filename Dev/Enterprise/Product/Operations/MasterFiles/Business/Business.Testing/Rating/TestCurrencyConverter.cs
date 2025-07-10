using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestCurrencyConverter : CurrencyConverter
	{
		public TestCurrencyConverter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override ZDecimal GetExchangeRate(ICurrency currency)
		{
			switch (currency.Code)
			{
				case "AUD":
					return 1m;

				case "USD":
					return 0.7m;

				case "EUR":
					return 0.5m;

				default:
					return 0m;
			}
		}

		public override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate)
		{
			foundRateDate = ZDateTime.Today;
			return GetExchangeRate(currency);
		}
	}
}
