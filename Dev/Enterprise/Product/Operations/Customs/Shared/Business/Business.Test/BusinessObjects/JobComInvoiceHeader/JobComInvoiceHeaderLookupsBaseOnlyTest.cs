using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsBaseOnlyTest : TestCaseWithFactory
	{
		public void TestExchangeRateTypeList()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals(Factory.GetCachedValue<ChargeExchangeRateTypeList>(), invoice.Lookups.ExchangeRateTypeList);
		}

		public void TestRelatedIndicatorList()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("RelatedIndicatorList", Factory.GetCachedValue<RelatedIndicatorList>(), invoice.Lookups.RelatedIndicatorList);
		}
	}
}
