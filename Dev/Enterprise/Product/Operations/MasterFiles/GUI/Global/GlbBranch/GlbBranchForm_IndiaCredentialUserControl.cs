
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbBranchForm_IndiaCredentialUserControl : UserAndClientCredentialsUserControl
	{
		public GlbBranchForm_IndiaCredentialUserControl()
		{
		}

		protected override UserAndClientCredentials GetDataSource()
		{
			if (BindingSource.DataSource is GlbBranchCredentialsForIndia wrapper)
			{
				return wrapper;
			}
			else if (BindingSource.DataSource is GlbBranch branch)
			{
				return branch.BranchCredentialsIndia;
			}
			else
			{
				return base.GetDataSource();
			}
		}
	}
}
