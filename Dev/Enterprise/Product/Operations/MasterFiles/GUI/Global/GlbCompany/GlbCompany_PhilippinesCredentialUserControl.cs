using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public sealed class GlbCompany_PhilippinesCredentialUserControl : UserAndClientCredentialsUserControl
	{
		public GlbCompany_PhilippinesCredentialUserControl()
		{
		}

		protected override UserAndClientCredentials GetDataSource()
		{
			if (BindingSource.DataSource is GlbCompanyExternalPasswordForPhilippines wrapper)
			{
				return wrapper;
			}
			else if (BindingSource.DataSource is GlbCompany branch)
			{
				return branch.PhilippinesEInvoicingCredentials;
			}
			else
			{
				return base.GetDataSource();
			}
		}
	}
}
