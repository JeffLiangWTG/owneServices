using System;
using CargoWise.Types;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public partial class LicensingCertificateUserControl : ZUserControl
	{
		public LicensingCertificateUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataChanged;
			CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataChanged;
			CertificateLoaderUserControl.OnDataView += OnCertificateDataView;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CertificateLoaderUserControl.DataLoaded -= CertificateLoaderUserControl_DataChanged;
				CertificateLoaderUserControl.DataCleared -= CertificateLoaderUserControl_DataChanged;
				CertificateLoaderUserControl.OnDataView -= OnCertificateDataView;
			}
			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (((TWGlbCompanyWrapper)DataSource)?.LicensingCertificate is GlbCompanyCredential password)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
				CertificateLoaderUserControl.SetFileData(password.GP_Certificate);
			}
		}

		#region Event Handlers

		internal void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			if (((TWGlbCompanyWrapper)DataSource)?.LicensingCertificate is GlbCompanyCredential password)
			{
				password.GP_Certificate = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();
			}
		}

		void OnCertificateDataView(object sender, EventArgs e)
		{
			if (((TWGlbCompanyWrapper)DataSource)?.LicensingCertificate is GlbCompanyCredential password)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
			}
		}

		#endregion
	}
}
