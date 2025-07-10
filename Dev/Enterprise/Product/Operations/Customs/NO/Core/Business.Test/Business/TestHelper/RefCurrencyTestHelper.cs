using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	public class RefCurrencyTestHelper
	{
		BusinessObjectFactory Factory { get; }
		Customs.Business.Testing.TestHelper Helper { get; }

		public RefCurrencyTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
			Helper = new Customs.Business.Testing.TestHelper();
		}

		public RefCurrency USDCurrency => fUSDCurrency ??= GetUSDCurrency();
		RefCurrency fUSDCurrency;
		RefCurrency GetUSDCurrency()
		{
			var usd = Helper.USDCurrency;
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, 0.08m, usd);
			return usd;
		}

		void SetExchangeRate(GlbCompany company, ZString rateType, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();
			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = ZDateTime.Today.AddDays(-2);
			result.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;
		}
	}
}
