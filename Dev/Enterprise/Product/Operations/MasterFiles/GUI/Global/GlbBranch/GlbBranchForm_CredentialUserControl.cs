using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbBranchForm_CredentialUserControl : ZUserControl
	{
		public GlbBranchForm_CredentialUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataChanged;
			CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CertificateLoaderUserControl.DataLoaded -= CertificateLoaderUserControl_DataChanged;
				CertificateLoaderUserControl.DataCleared -= CertificateLoaderUserControl_DataChanged;
			}
			base.Dispose(disposing);
		}

		#region Event Handlers

		void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();
			(BranchCredentialsGrid.ListManager.GetCurrent() as GlbExternalPassword).GP_Certificate = loadedData;
		}

		void BranchCredentialsGrid_AfterBind(object sender, EventArgs e)
		{
			if (BranchCredentialsGrid.ListManager != null)
			{
				BranchCredentialsGrid.ListManager.CurrentChanged += BranchCredentialsGrid_CurrentChanged;
				BranchCredentialsGrid_CurrentChanged(BranchCredentialsGrid.ListManager, EventArgs.Empty);
			}
		}

		void BranchCredentialsGrid_CurrentChanged(object sender, EventArgs e)
		{
			try
			{
				CertificateLoaderUserControl.SetFileData((BranchCredentialsGrid.ListManager.GetCurrent() as GlbExternalPassword).GP_Certificate);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CertificateLoaderUserControl.SetFileData(ZBlob.Empty);
			}

			CheckCertLoaderVisibility();
		}

		void BranchCredentialsGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			CheckCertLoaderVisibility();
		}

		void BranchCredentialsGrid_Leave(object sender, EventArgs e)
		{
			try
			{
				CertificateLoaderUserControl.CertificatePassword = (BranchCredentialsGrid.ListManager.GetCurrent() as GlbExternalPassword).CurrentDecryptedCertificatePassphrase;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CertificateLoaderUserControl.CertificatePassword = ZString.Empty;
			}
		}

		void CheckCertLoaderVisibility()
		{
			var isVisible = false;

			if (BranchCredentialsGrid.ListManager != null)
			{
				if (BranchCredentialsGrid.ListManager.Count > 0)
				{
					var currentCell = BranchCredentialsGrid.ListManager.GetCurrent() as GlbExternalPassword;
					isVisible = (currentCell.GP_MailBoxID != ZString.Empty
						|| currentCell.CurrentDecryptedCertificatePassphrase != ZString.Empty);
				}
			}

			CertificateLoaderUserControl.Visible = isVisible;
		}

		#endregion
	}
}
