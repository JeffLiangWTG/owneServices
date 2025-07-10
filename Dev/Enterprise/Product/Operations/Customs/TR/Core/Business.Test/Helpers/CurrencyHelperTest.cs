using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Testing
{
	class CurrencyHelperTest : TestCaseWithFactory
	{
		public void TestGetExchangeRate()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "USD";
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_StartDate = new ZDateTime(2006, 5, 1);
			rate.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rate.RE_ExRateType = "CUS";
			rate.RE_SellRate = 0.5m;

			RefCurrencyCurrencyConverter converter = new RefCurrencyCurrencyConverter(Factory, new ZDateTime(2006, 5, 1), ExchangeRateType.Customs, 0);
			var actual = CurrencyHelper.GetExchangeRate(converter, "USD");
			AssertEquals(0.5m, actual);
		}
	}
}
