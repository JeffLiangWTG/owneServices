using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.GUI.DigitalCertificateControl_p12_EInvoicing;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EInvoicingCertificateUserControl : ZUserControl
	{
		public EInvoicingCertificateUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var branch = X509CertificatesGrid.DataSource as GlbBranch;
			var companyBranches = GlbCompany.CurrentCompany.Branches;

			CertificateLoaderUserControl.AllowRegister = false;

			if (branch != null)
			{
				if (GlbCompany.CurrentCompany.Branches.Any(b => b.GB_Code == branch.GB_Code))
				{
					var countryEInvoicingSettings = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>()
						.GetCountryEInvoicingObjectFactorySettings(branch.Company?.GC_RN_NKCountryCode ?? string.Empty);
					var credentialSettings = countryEInvoicingSettings?.Credentials;

					if (credentialSettings?.IsBranchRegistrationRequired ?? false)
					{
						CertificateLoaderUserControl.LoadButtonText = Registry.GUI.DigitalCertificateLoadButtonText.Load;
						CertificateLoaderUserControl.AllowRegister = true;
					}
				}
			}
		}

#if DEBUG
		internal ZArchitecture.ZGrid X509CertificatesGrid_TestOnly => X509CertificatesGrid;

		internal void SetDataSourceForX509CertificatesGrid_TestOnly(GlbBranch dataSource)
		{
			this.X509CertificatesGrid.DataSource = dataSource;
		}

		internal DigitalCertificateControl_p12_EInvoicing CertificateLoaderUserControl_TestOnly => CertificateLoaderUserControl;
#endif

		#region Event Handlers

		bool isDoingAdd;

		void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			CopyCertificateDataToCredential();
		}

		void CertificateLoaderUserControl_DataRegistered(object sender, DataRegistrationEventArgs e)
		{
			CopyCertificateDataToCredential(e.Secret);
		}

		void CopyCertificateDataToCredential(string passphrase = "")
		{
			if (X509CertificatesGrid.ListManager == null)
			{
				return;
			}
			isDoingAdd = true;
			try
			{
				var branch = X509CertificatesGrid.DataSource as GlbBranch;
				EInvoicingCertificateCredential credentialToUpdate = null;

				var currentCredential = (EInvoicingCertificateCredential)X509CertificatesGrid.GetCurrent();
				if (currentCredential != null && currentCredential.GP_Certificate.IsEmpty)
				{
					credentialToUpdate = currentCredential;
				}
				else
				{
					credentialToUpdate = branch.EInvoicingCertificateCredentials.AddNew();
				}
				SetCredentialValue(passphrase, credentialToUpdate);
				CertificateLoaderUserControl.SetFileData(credentialToUpdate.CertificateRawData);
			}
			finally
			{
				isDoingAdd = false;
			}
		}

		void SetCredentialValue(string passphrase, EInvoicingCertificateCredential credential)
		{
			if (!passphrase.IsNullOrEmpty())
			{
				credential.CurrentDecryptedCertificatePassphrase = passphrase;
			}
			credential.GP_Certificate = CertificateLoaderUserControl.FileDataAsBinary();
		}

		void BranchCredentialsGrid_AfterBind(object sender, EventArgs e)
		{
			if (X509CertificatesGrid.ListManager != null)
			{
				X509CertificatesGrid.ListManager.CurrentChanged += BranchCredentialsGrid_CurrentChanged;
				BranchCredentialsGrid_CurrentChanged(X509CertificatesGrid.ListManager, EventArgs.Empty);

				if (X509CertificatesGrid.DataSource is GlbBranch branch
					&& branch.EInvoicingCertificateCredentials.CredentialSettings is IEInvoicingCertificateCredentialSettings certificateSettings
					&& certificateSettings.ExpiryWarningDays > 0)
				{
					CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = certificateSettings.ExpiryWarningDays;
				}
			}
		}

		void BranchCredentialsGrid_CurrentChanged(object sender, EventArgs e)
		{
			if (isDoingAdd)
			{
				return;
			}

			if (X509CertificatesGrid.ListManager.GetCurrent() is EInvoicingCertificateCredential credential)
			{
				try
				{
					CertificateLoaderUserControl.SetFileData(credential.CertificateRawData);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					CertificateLoaderUserControl.SetFileData(ZBlob.Empty);
				}
			}
		}

		void BranchCredentialsGrid_Leave(object sender, EventArgs e)
		{
			if (isDoingAdd)
			{
				return;
			}

			if (X509CertificatesGrid.ListManager.GetCurrent() is GlbExternalPassword credential)
			{
				try
				{
					CertificateLoaderUserControl.CertificatePassword = credential.CurrentDecryptedCertificatePassphrase;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					CertificateLoaderUserControl.CertificatePassword = ZString.Empty;
				}
			}
		}

		void CertificateLoaderUserControl_OnDataRegister(object sender, EventArgs e)
		{
			var branch = X509CertificatesGrid.DataSource as GlbBranch;
			var registerBO = new EInvoicingBranchRegister(branch);
			var form = new EInvoicingBranchRegisterForm(registerBO, CertificateLoaderUserControl);
			ZFormModaliser.Show(form, this.ParentForm);
		}

		#endregion
	}
}
