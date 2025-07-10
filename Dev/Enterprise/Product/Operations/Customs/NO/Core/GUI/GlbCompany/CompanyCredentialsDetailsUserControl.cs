using System;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

partial class CompanyCredentialsDetailsUserControl : ZUserControl
{
	public CompanyCredentialsDetailsUserControl()
	{
		InitializeComponent();
	}

	GlbCompanyWrapper glbCompanyWrapper => DataSource as GlbCompanyWrapper;

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataLoaded;
		CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataLoaded;
		CertificateLoaderUserControl.OnDataView += OnCertificateDataView;
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		var credential = glbCompanyWrapper?.Credential;
		if (credential != null)
		{
			CertificateLoaderUserControl.CertificatePassword = credential.CurrentDecryptedCertificatePassphrase;
			CertificateLoaderUserControl.SetFileData(credential.GP_Certificate);
		}
	}

	#region Event Handlers

	void CertificateLoaderUserControl_DataLoaded(object sender, EventArgs e)
	{
		var credential = glbCompanyWrapper?.Credential;
		if (credential != null)
		{
			credential.GP_Certificate = CertificateLoaderUserControl.FileDataAsBinary();
		}
	}

	void OnCertificateDataView(object sender, EventArgs e)
	{
		var credential = glbCompanyWrapper?.Credential;
		if (credential != null)
		{
			CertificateLoaderUserControl.CertificatePassword = credential.CurrentDecryptedCertificatePassphrase;
		}
	}

	#endregion
}
