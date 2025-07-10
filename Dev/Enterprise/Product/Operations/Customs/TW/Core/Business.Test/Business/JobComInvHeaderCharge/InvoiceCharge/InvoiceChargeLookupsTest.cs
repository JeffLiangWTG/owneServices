using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
