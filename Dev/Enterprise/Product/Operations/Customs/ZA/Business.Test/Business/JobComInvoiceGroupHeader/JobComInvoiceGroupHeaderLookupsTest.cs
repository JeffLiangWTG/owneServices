using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGroupHeader()
		{
			AssertEquals(lookup.GroupHeader, groupHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			lookup = new JobComInvoiceGroupHeaderLookups(groupHeader);
		}
		JobComInvoiceGroupHeaderLookups lookup;
		JobComInvoiceGroupHeader groupHeader;
	}
}
