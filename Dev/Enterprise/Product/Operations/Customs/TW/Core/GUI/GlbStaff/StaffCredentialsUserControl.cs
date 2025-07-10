using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class StaffCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
	{
		public StaffCredentialsUserControl()
		{
			InitializeComponent();
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(CertificateLoaderUserControl, new SuppressFormsLocalizedTestAttribute());
#endif
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

		GlbExternalPassword CurrentRow => SubscriptionsGrid.ListManager.GetCurrent() as GlbExternalPassword;

		internal void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();
			CurrentRow.GP_Certificate = loadedData;
		}

		void SubscriptionsGrid_AfterBind(object sender, EventArgs e)
		{
			if (SubscriptionsGrid.ListManager != null)
			{
				SubscriptionsGrid.ListManager.CurrentChanged += SubscriptionsGrid_CurrentChanged;
				SubscriptionsGrid_CurrentChanged(SubscriptionsGrid.ListManager, EventArgs.Empty);
				RefreshCertLoaderPassword();
				SubscriptionsGrid_CurrentCellChanged(SubscriptionsGrid.ListManager, EventArgs.Empty);
			}
		}

		void SubscriptionsGrid_CurrentChanged(object sender, EventArgs e)
		{
			try
			{
				CertificateLoaderUserControl.SetFileData(CurrentRow.GP_Certificate);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CertificateLoaderUserControl.SetFileData(ZBlob.Empty);
			}

			SetCertLoaderVisibility();
			RefreshCertLoaderPassword();
		}

		void SubscriptionsGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			SetCertLoaderVisibility();
		}

		void SubscriptionsGrid_Leave(object sender, EventArgs e)
		{
			RefreshCertLoaderPassword();
		}

		void SetCertLoaderVisibility()
		{
			if (SubscriptionsGrid.ListManager != null)
			{
				var isVisible = false;
				var hasAnyRows = SubscriptionsGrid.ListManager.Count > 0;
				if (hasAnyRows)
				{
					var row = CurrentRow;
					isVisible = (!row.GP_MailBoxID.IsEmpty || !row.GP_UserID.IsEmpty || !row.CurrentDecryptedCertificatePassphrase.IsEmpty);
				}

				CertificateLoaderUserControl.Visible = isVisible;
			}
		}

		void RefreshCertLoaderPassword()
		{
			try
			{
				CertificateLoaderUserControl.CertificatePassword = CurrentRow.CurrentDecryptedCertificatePassphrase;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CertificateLoaderUserControl.CertificatePassword = ZString.Empty;
			}
		}

		#endregion
	}
}
