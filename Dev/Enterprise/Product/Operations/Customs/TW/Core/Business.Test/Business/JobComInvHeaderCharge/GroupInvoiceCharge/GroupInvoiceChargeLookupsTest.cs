using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GroupInvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			GroupInvoiceCharge parent = Factory.New<GroupInvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
