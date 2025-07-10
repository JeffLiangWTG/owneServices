using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlobalCreditGroupHelperTest : TestCaseWithFactory
	{
		public void TestGetGlobalExchangeRate()
		{
			AssertEquals(1m, GlobalCreditGroupHelper.GetGlobalExchangeRate(Factory, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));

			var currency1 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var exchangeRate1 = currency1.ExchangeRates.AddNew();
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			exchangeRate1.RE_SellRate = 1.2m;
			exchangeRate1.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
			exchangeRate1.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));
			Factory.Save();

			AssertEquals(1.2m, GlobalCreditGroupHelper.GetGlobalExchangeRate(Factory, "USD"));

			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var currency2 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var exchangeRate2 = currency2.ExchangeRates.AddNew();
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.PeriodEndRate;
			exchangeRate2.RE_SellRate = 1.5m;
			exchangeRate2.RE_StartDate = ZDateTime.Today;
			AssertEquals(exchangeRate2.RE_StartDate.Date, periodHelper.CurrentPeriod.AM_EndDate.Date);
			AssertEquals(exchangeRate2.RE_ExpiryDate.Date, periodHelper.CurrentPeriod.AM_EndDate.Date);
			Factory.Save();

			AssertEquals(1.5m, GlobalCreditGroupHelper.GetGlobalExchangeRate(Factory, "EUR"));
		}
	}
}
