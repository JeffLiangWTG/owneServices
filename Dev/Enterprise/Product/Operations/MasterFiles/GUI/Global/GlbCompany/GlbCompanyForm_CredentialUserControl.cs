using System;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCompanyForm_CredentialUserControl : GlbBranchForm_CredentialUserControl
	{
		public GlbCompanyForm_CredentialUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				if (CredentialsDataSource.CredentialSettings is IGridControlSettings gridSettings)
				{
					BranchCredentialsGrid.RemoveFromAvailableColumns(gridSettings.HiddenColumns);
				}
			}
		}

		CombinedEInvoicingCertificateCollection CredentialsDataSource =>
#if DEBUG
			(DataSource is CombinedEInvoicingCertificateCollection)
				? DataSource as CombinedEInvoicingCertificateCollection
				:
#endif
				((GlbCompany)DataSource).EInvoicingCertificateCredentials;
	}
}
