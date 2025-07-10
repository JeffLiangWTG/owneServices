using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class PLExRateHelperTest : TestCaseWithFactory
{
	public void TestDecimalPlacesForSellRate() => CombineAssertions(() =>
	{
		var exRateHelper = new PLExRateHelper();
		AssertEquals("The IDR or VND should accept 9 decimal places", 9, exRateHelper.DecimalPlacesForSellRate(Core.Constants.CurrencyCodes.Indonesia));
		AssertEquals("Accept 6 decimal places if the currency are not IDR or VND", 6, exRateHelper.DecimalPlacesForSellRate(Core.Constants.CurrencyCodes.India));
	});
}
