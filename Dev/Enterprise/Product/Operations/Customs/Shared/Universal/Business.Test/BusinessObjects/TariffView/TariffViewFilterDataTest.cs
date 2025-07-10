using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	public class TariffViewFilterDataTest : TestCaseWithFactory
	{
		public void TestRatesApplyToCountry() => CombineAssertions(() =>
		{
			var filterData = new TariffViewFilterData("X", ZDate.Empty);
			AssertEquals("X", filterData.RatesApplyToCountry);
			filterData = new(ZString.Empty, ZDate.Empty);
			AssertEquals(ZString.Empty, filterData.RatesApplyToCountry);
		});

		public void TestEffectiveDate() => CombineAssertions(() =>
		{
			var filterData = new TariffViewFilterData(ZString.Empty, ZDate.BrettsBirthday);
			AssertEquals(ZDate.BrettsBirthday, filterData.EffectiveDate);
			filterData = new(ZString.Empty, ZDate.Empty);
			AssertEquals(ZDate.Empty, filterData.EffectiveDate);
			filterData = new(ZString.Empty, null);
			AssertEquals(null, filterData.EffectiveDate);
		});
	}
}
