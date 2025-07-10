using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccEPaymentStaffTokenLookupsTest : BusinessObjectLookupsTestCase
	{
		protected AccEPaymentStaffTokenLookups Lookups => lookups ?? (lookups = new AccEPaymentStaffTokenLookups(Factory.New<AccEPaymentStaffToken>()));
		AccEPaymentStaffTokenLookups lookups;

		public void TestStatusCodeList()
		{
			AssertEquals(4, Lookups.StatusCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "NAT", "PND", "ATH", "ERR" }, Lookups.StatusCodeList.GetAllCodes());
		}

		public void TestScopeList()
		{
			AssertEquals(3, Lookups.ScopeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ofxrates", "payments", "users" }, Lookups.ScopeList.GetAllCodes());
		}
	}
}
