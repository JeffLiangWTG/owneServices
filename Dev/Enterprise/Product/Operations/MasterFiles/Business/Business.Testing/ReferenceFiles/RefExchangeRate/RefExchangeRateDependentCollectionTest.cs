using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefExchangeRateDependentCollection))]
	sealed class RefExchangeRateDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<RefExchangeRateDependentCollection>
	{
		public void TestDefaultValuesForNewChild()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZUB";
			var item = currency.ExchangeRates.AddNew();
			AssertEquals(1m, item.RE_SellRate);
			AssertEquals("SEL", item.RE_ExRateType);
		}

		public void TestRelationshipFilter()
		{
			var company1 = Factory.New<GlbCompany>();
			var currency = Factory.New<RefCurrency>();

			currency.RX_Code = "ZUB";

			var item = Factory.New<RefExchangeRate>();
			item.RE_RX_NKExCurrency = "ZUB";
			item.RE_GC = company1.PK;

			var item2 = Factory.New<RefExchangeRate>();
			item2.RE_RX_NKExCurrency = "ZUB";
			item2.RE_GC = GlbCompany.CurrentCompany.PK;

			AssertCollectionContains(item2, currency.ExchangeRates);
			AssertCollectionNotContains(item, currency.ExchangeRates);
		}

		protected override RefExchangeRateDependentCollection GetCollectionToTest()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZUB";
			return new RefExchangeRateDependentCollection(currency);
		}
	}
}
