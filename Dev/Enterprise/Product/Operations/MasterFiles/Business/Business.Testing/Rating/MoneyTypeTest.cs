using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MoneyTypeTest : TestCaseWithFactory
	{
		public void TestMoneyType()
		{
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var eur = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");

			var monetaryValues = new MoneyType();

			monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(new Money(1000m, aud)));
			monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(new Money(1400m, usd)));
			monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(new Money(3000m, aud)));
			monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(new Money(4000m, eur)));

			monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(new Money(100m, aud)));
			monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(new Money(200m, eur)));
			monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(new Money(300m, aud)));
			monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(new Money(140m, usd)));

			var converter = new TestCurrencyConverter(Factory);

			AssertEquals(14000m, monetaryValues.GetMoney(MoneyType.ValueType.GoodsValue, aud, converter).Amount);
			AssertEquals(9800m, monetaryValues.GetMoney(MoneyType.ValueType.GoodsValue, usd, converter).Amount);
			AssertEquals(1000m, monetaryValues.GetMoney(MoneyType.ValueType.InsuranceValue, aud, converter).Amount);
			AssertEquals(500m, monetaryValues.GetMoney(MoneyType.ValueType.InsuranceValue, eur, converter).Amount);
		}
	}
}
