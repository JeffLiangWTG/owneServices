using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAirlineBranchAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOAA_APAirlineAccountNumber()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsAirLine = true;

			var branchAccount = org.OrgAirlineBranchAccounts.AddNew();
			branchAccount.OAA_GB_Branch = GlbBranch.CurrentBranch.PK;

			branchAccount.OAA_APAirlineAccountNumber = ZString.Empty;
			AssertHasErrors(branchAccount.OAA_APAirlineAccountNumberInfo);

			branchAccount.OAA_APAirlineAccountNumber = "11111";
			AssertNoErrors(branchAccount.OAA_APAirlineAccountNumberInfo);
		}

		public void TestOAA_GB_Branch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsAirLine = true;

			var branchAccount = org.OrgAirlineBranchAccounts.AddNew();
			branchAccount.OAA_GB_Branch = GlbBranch.CurrentBranch.PK;
			branchAccount.OAA_APAirlineAccountNumber = "11111";
			AssertNoErrors(branchAccount.OAA_GB_BranchInfo);

			var branchAccount2 = org.OrgAirlineBranchAccounts.AddNew();
			branchAccount2.OAA_GB_Branch = GlbBranch.CurrentBranch.PK;
			branchAccount2.OAA_APAirlineAccountNumber = "22222";
			AssertHasError(branchAccount2.OAA_GB_BranchInfo, "The Branch has been duplicated and must be unique.");

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_BranchName = "TestBranch";
			branch2.GB_Code = "TBR";
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			branchAccount2.OAA_GB_Branch = branch2.PK;
			AssertNoErrors(branchAccount2.OAA_GB_BranchInfo);
		}
	}
}
