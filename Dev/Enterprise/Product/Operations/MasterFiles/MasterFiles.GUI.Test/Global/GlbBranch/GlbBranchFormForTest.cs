using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbBranchFormForTest : GlbBranchForm
	{
		public GlbBranchFormForTest(GlbBranch branch) : base(branch)
		{
		}

		public void RunDelete()
		{
			Delete();
		}
	}
}
