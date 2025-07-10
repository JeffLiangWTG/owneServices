using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbBranchExtensionTest : TestCaseWithFactory
	{
		public void TestSetAsTemporaryContext()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			using (branch.SetAsTemporaryContext())
			{
				AssertEquals(GlbBranch.CurrentBranch.GB_Code, branch.GB_Code);
			}
		}
	}
}
