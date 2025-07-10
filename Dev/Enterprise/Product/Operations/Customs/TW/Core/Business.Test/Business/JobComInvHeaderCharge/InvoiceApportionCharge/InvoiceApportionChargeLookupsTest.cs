using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			InvoiceApportionCharge parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
