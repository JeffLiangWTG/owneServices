using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class InvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestZAChargeDistributionByList()
		{
			var testCharge = Factory.NewWithValidTestData<InvoiceCharge>();
			AssertEquals(1, testCharge.Lookups.ChargeDistributionBy.Count);
			AssertEquals(ChargeDistributeByList.Codes.Value, testCharge.Lookups.ChargeDistributionBy[0].Code);
		}
	}
}
