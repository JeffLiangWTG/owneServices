using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.GUI;

public partial class StaffCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
{
	public StaffCredentialsUserControl()
	{
		InitializeComponent();
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		SeapIdTextBox.Visible = GlbStaffWrapper?.IsNCTSPhase5 ?? false;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataLoaded;
		CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataCleared;
		CertificateLoaderUserControl.OnDataView += OnCertificateDataView;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			CertificateLoaderUserControl.DataLoaded -= CertificateLoaderUserControl_DataLoaded;
			CertificateLoaderUserControl.DataCleared -= CertificateLoaderUserControl_DataCleared;
			CertificateLoaderUserControl.OnDataView -= OnCertificateDataView;
		}

		base.Dispose(disposing);
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		var password = GlbStaffWrapper?.GlbExternalPassword;
		if (password != null)
		{
			CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
			CertificateLoaderUserControl.SetFileData(password.GP_Certificate);
		}
	}

	#region Event Handlers

	GlbStaffWrapper GlbStaffWrapper => DataSource as GlbStaffWrapper;

	void CertificateLoaderUserControl_DataLoaded(object sender, EventArgs e)
	{
		var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();

		var password = GlbStaffWrapper?.GlbExternalPassword;
		if (password != null)
		{
			password.GP_Certificate = loadedData;
		}
	}

	void CertificateLoaderUserControl_DataCleared(object sender, EventArgs e)
	{
		var password = GlbStaffWrapper?.GlbExternalPassword;
		if (password != null)
		{
			password.GP_Certificate = null;
			password.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			password.GP_PasswordStatus = ZString.Empty;
		}
	}

	void OnCertificateDataView(object sender, EventArgs e)
	{
		try
		{
			var password = GlbStaffWrapper?.GlbExternalPassword;
			if (password != null)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
			}
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			CertificateLoaderUserControl.CertificatePassword = ZString.Empty;
		}
	}

	#endregion
}
