using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIssuerList()
		{
			CombineAssertions(() =>
			{
				var list = lookup.IssuerList;
				AssertEquals(0, list.Count);
				AssertSame(list, lookup.IssuerList);
			});
		}

		public void TestCodeList()
		{
			CombineAssertions(() =>
			{
				var list = lookup.CodeList;
				AssertEquals(0, list.Count);
				AssertSame(list, lookup.IssuerList);
			});
		}

		public void TestAccountList()
		{
			CombineAssertions(() =>
			{
				var list = lookup.AccountList;
				AssertEquals(0, list.Count);
				AssertSame(list, lookup.AccountList);
			});
		}

		public void TestAccountTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookup.AccountTypeList;
				AssertEquals(0, list.Count);
				AssertSame(list, lookup.AccountTypeList);
			});
		}

		public void TestReportingPeriodList()
		{
			CombineAssertions(() =>
			{
				var list = lookup.ReportingPeriodList;
				AssertEquals(0, list.Count);
				AssertSame(list, lookup.ReportingPeriodList);
			});
		}

		protected override void SetUp()
		{
			lookup = new OrgCusAccountLookups(Factory.NewWithValidTestData<OrgCusAccount>());
		}
		OrgCusAccountLookups lookup;
	}
}
