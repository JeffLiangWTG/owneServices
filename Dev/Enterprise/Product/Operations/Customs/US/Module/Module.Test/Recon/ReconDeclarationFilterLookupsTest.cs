using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class ReconDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestLists()
		{
			var lookups = new ReconDeclarationFilterLookups(Factory);
			AssertEquals("Importers", typeof(ConsigneeCollection), lookups.Importers.GetType());
			AssertEquals("IssueCodeList", typeof(ReconIssueCodeList), lookups.IssueCodeList.GetType());
			AssertEquals("ReconPortsList", typeof(ReconPortsList), lookups.ReconPortsList.GetType());
			AssertEquals("PaymentTypeList", typeof(PaymentTypeList), lookups.PaymentTypeList.GetType());
			AssertEquals("StaffList", typeof(GlbStaffCollection), lookups.StaffList.GetType());
			AssertEquals("ReconMessageStatusList", typeof(ReconMessageStatusList), lookups.ReconMessageStatusList.GetType());
			AssertEquals("ReconMessageStatusList", typeof(GlbBranchCollection), lookups.BranchList.GetType());
			AssertEquals("EntrySummaryActionsList", 2, lookups.EntrySummaryActionsList.Count);
		}
	}
}
