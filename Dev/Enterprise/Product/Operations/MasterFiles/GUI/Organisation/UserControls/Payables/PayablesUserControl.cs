using System;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	public partial class PayablesUserControl : OrganisationSecurityContainerControl
	{
		public PayablesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				ShowCreditReportPanel();
			}
		}

		void ShowCreditReportPanel()
		{
			if (CreditReportHelper.CreditReportEnabled && OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.Value.Count > 0)
			{
				PayablesDetailsContainer.Panel1Collapsed = false;
				creditReportControl.SetDataBinding(Organisation, string.Empty);
			}
			else
			{
				PayablesDetailsContainer.Panel1Collapsed = true;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			OB_APExcludeFromPaymentReportsCheckBox.Visible = AreComplianceReportsEnabledInAustralia();
			OB_APPrintContractorFormCheckBox.Visible = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.UnitedStates || AreComplianceReportsEnabledInAustralia();

			if (AreComplianceReportsEnabledInAustralia())
			{
				//CaptionResourceString for US companies is set in the designer code
				OB_APPrintContractorFormCheckBox.CaptionResourceString = Res.GetData("0e7d666e-172c-401a-8a7d-e342f6cb50b8", "Include in TPAR Reporting",
					"This flag identifies AP organizations that your login company may need to report on what was paid to them within a reporting period by lodging TPAR (Taxable Payment Annual Report).");
			}

			if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				OB_APVATConfigDropEdit.Visible = false;
				OB_APVATConfigLabel.Visible = false;
			}

			if (Env.Security.OrgPayablesViewAccountDetails.IsAllowed)
			{
				AccountDetailsGrid.Visible = true;
				lblNotAllowedToSeeAccDetails.Visible = false;
			}
			else
			{
				AccountDetailsGrid.Visible = false;
				lblNotAllowedToSeeAccDetails.Visible = true;
				lblNotAllowedToSeeAccDetails.Text = Res.GetString("1fcd1e85-2ae4-420b-a1d7-aa0c0e7690d5", "You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: \r\n\r\nMaintain > Reference Files  > Organization > View Payables > View Account Details");
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
			{
				OB_APCreateVATComplianceDocumentOnPostingDropEdit.Visible = false;
				OB_APCreateVATComplianceDocumentOnPostingLabel.Visible = false;
			}

			if (!ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().HasAnyActiveAccTaxConfiguration(Organisation.Factory, Organisation.CompanyData.Company, LedgerTypes.AccountsPayable))
			{
				taxConfigurationTabPage.Dispose();
			}
		}

		bool AreComplianceReportsEnabledInAustralia() => GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia && AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.Value;

		#region Tax Configuration Template

		void TaxConfigurationTabPage_InitializeTab(object sender, EventArgs e)
		{
			if (Organisation != null && Organisation.CompanyData != null)
			{
				Organisation.CompanyData.OB_OCT_APTaxTemplateInfo.ValueChanged += CompanyData_APTaxTemplateInfo_ValueChanged;
			}

			var securityProvider = Organisation?.SecurityProvider;
			redefaultFromTemplateButton.Enabled = securityProvider?.HasModifyPayablesTaxConfigurationTemplateSecurity ?? false;
			accOrgTaxConfigurationGrid.ReadOnly = (!securityProvider?.HasModifyPayablesTaxConfigurationGridSecurity) ?? true;
		}

		TaxConfigurationTemplateGUIHelper TaxConfigurationTemplateGUIHelper => taxConfigurationTemplateGUIHelper ?? (taxConfigurationTemplateGUIHelper = new TaxConfigurationTemplateGUIHelper(Organisation?.CompanyData, false));
		TaxConfigurationTemplateGUIHelper taxConfigurationTemplateGUIHelper;

		void CompanyData_APTaxTemplateInfo_ValueChanged(object sender, EventArgs e)
		{
			TaxConfigurationTemplateGUIHelper.OrgTaxConfigurationTemplateChanged();
		}

		void RedefaultFromTemplateButton_Click(object sender, EventArgs e)
		{
			TaxConfigurationTemplateGUIHelper.RedefaultOrgTaxConfigurationFormTemplate();
		}

		#endregion

		#region Binding

		OrgHeader Organisation
		{
			get { return (OrgHeader)CurrentDataItem; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnsubscribeEventHandlers();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void UnsubscribeEventHandlers()
		{
			if (Organisation != null && Organisation.CompanyData != null)
			{
				Organisation.CompanyData.OB_OCT_APTaxTemplateInfo.ValueChanged -= CompanyData_APTaxTemplateInfo_ValueChanged;
			}
		}

		#endregion

		#region Test
#if DEBUG

		void OM_RX_APDefaultCurrencyBoundGuidFindBox_Load(object sender, EventArgs e)
		{
		}

#endif
		#endregion
	}
}
