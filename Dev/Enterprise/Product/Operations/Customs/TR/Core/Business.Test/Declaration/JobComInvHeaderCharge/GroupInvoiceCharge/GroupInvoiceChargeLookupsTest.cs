namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class GroupInvoiceChargeLookupsTest : EU.Business.Declaration.Testing.GroupInvoiceChargeLookupsTest
	{
		public new void TestParent()
		{
			var parent = Factory.New<GroupInvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
