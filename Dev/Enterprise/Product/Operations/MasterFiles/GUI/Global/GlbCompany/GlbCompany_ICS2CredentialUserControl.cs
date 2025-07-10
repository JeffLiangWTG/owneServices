using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCompany_ICS2CredentialUserControl : ZUserControl
	{
		public GlbCompany_ICS2CredentialUserControl()
		{
			InitializeComponent();
		}

		GlbCompanyWrapper glbCompanyWrapper => DataSource as GlbCompanyWrapper;

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataLoaded;
			CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataCleared;
			CertificateLoaderUserControl.OnDataView += OnCertificateDataView;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var password = glbCompanyWrapper?.GlbCompanyCredentialICS2;
			if (password != null)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
				CertificateLoaderUserControl.SetFileData(password.GP_Certificate);
			}
		}

		void CertificateLoaderUserControl_DataLoaded(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();

			var password = glbCompanyWrapper?.GetGlbExternalPasswordOrCreateNew<GlbCompanyCredentialICS2>(PasswordTypesList.Codes.IC2);
			if (password != null)
			{
				password.GP_Certificate = loadedData;
				ResetDataBinding();
			}
		}

		void CertificateLoaderUserControl_DataCleared(object sender, EventArgs e)
		{
			glbCompanyWrapper?.GlbCompanyCredentialICS2?.Delete();
			ResetDataBinding();
		}

		void OnCertificateDataView(object sender, EventArgs e)
		{
			var password = glbCompanyWrapper?.GlbCompanyCredentialICS2;
			if (password != null)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
			}
		}

		void ResetDataBinding()
		{
			var dataSource = DataSource;
			SetDataBinding(null, null);
			SetDataBinding(dataSource, null);
		}

		#endregion
	}
}
