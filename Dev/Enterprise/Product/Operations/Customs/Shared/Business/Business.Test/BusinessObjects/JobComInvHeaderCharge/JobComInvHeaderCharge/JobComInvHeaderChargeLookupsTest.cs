using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvHeaderChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeDistributeBy()
		{
			JobComInvHeaderChargeLookups lookup = new JobComInvHeaderChargeLookups(Factory.New<BaseInvoiceCharge>());
			AssertEquals("ChargeDistributionBy", typeof(ChargeDistributeByList), lookup.ChargeDistributionBy.GetType());
		}

		public void TestApportionmentTypeList()
		{
			JobComInvHeaderChargeLookups lookup = new JobComInvHeaderChargeLookups(Factory.New<BaseInvoiceCharge>());
			AssertEquals("ChargeDistributionBy", typeof(ApportionmentTypeList), lookup.ApportionmentTypeList.GetType());
		}

		public void TestExchangeRateTypeList()
		{
			var charge = Factory.New<BaseInvoiceCharge>();
			AssertEquals(Factory.GetCachedValue<ChargeExchangeRateTypeList>(), charge.Lookups.ExchangeRateTypeList);
		}
	}
}
