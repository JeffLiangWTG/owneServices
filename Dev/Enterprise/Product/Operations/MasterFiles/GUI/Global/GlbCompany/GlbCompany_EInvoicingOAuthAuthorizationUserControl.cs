using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCompany_EInvoicingOAuthAuthorizationUserControl : ZUserControl
	{
		public GlbCompany_EInvoicingOAuthAuthorizationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (BindingSource.DataSource is GlbCompany company)
			{
				EInvoiceCredentialsProvider = ObjectFactory.Get<ICountryComplianceFactory>()?.GetICountryComplianceInfoBase(company.GC_RN_NKCountryCode) as IEInvoiceCredentialsProvider;
				OAuthData = EInvoiceCredentialsProvider?.CreateOAuthData(company);
			}
		}

		#region Event Handlers

		void AuthorizeButton_Click(object sender, EventArgs e)
		{
			var launchUrl = EInvoiceCredentialsProvider?.GetAuthorizationURL(OAuthData.Company) ?? "";
			WebUrlLauncher.Launch(launchUrl);

			EInvoiceCredentialsProvider?.CreateOrUpdateEInvoicingCredential(OAuthData.Company);
			ReloadData();
		}

		void ReloadButton_Click(object sender, EventArgs e)
		{
			ReloadData();
		}

		#endregion

		void ReloadData()
		{
			OAuthData = EInvoiceCredentialsProvider?.CreateOAuthData(OAuthData.Company);
			TokenManagementGrid.SetDataBinding(OAuthData, "OAuthCredentials");
		}

		EInvoiceOAuthData OAuthData
		{
			get => oAuthData;
			set
			{
				oAuthData = value;
				TokenManagementGrid.SetDataBinding(OAuthData, "OAuthCredentials");
			}
		}
		EInvoiceOAuthData oAuthData;

		IEInvoiceCredentialsProvider EInvoiceCredentialsProvider { get; set; }
	}
}
