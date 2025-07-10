using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// A tab control for A/R including Receivables and Claims/Queries
	/// </summary>
	public partial class ReceivablesUserControl : OrganisationSecurityContainerControl
	{
		public ReceivablesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				ARTabControl.PlugIns.Add(ControllerIDs.ARAccQueryClaim);
				if (!ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().HasAnyActiveAccTaxConfiguration(Organisation.Factory, Organisation.CompanyData.Company, LedgerTypes.AccountsReceivable))
				{
					taxConfigurationTabPage.Dispose();
				}
			}
		}

		#region Tax Configuration Template

		void InitTaxConfigurationTabPage()
		{
			if (Organisation != null && Organisation.CompanyData != null)
			{
				Organisation.CompanyData.OB_OCT_ARTaxTemplateInfo.ValueChanged += CompanyData_ARTaxTemplateInfo_ValueChanged;
			}

			var securityProvider = Organisation?.SecurityProvider;
			redefaultFromTemplateButton.Enabled = securityProvider?.HasModifyReceivablesTaxConfigurationTemplateSecurity ?? false;
			accOrgTaxConfigurationGrid.ReadOnly = (!securityProvider?.HasModifyReceivablesTaxConfigurationGridSecurity) ?? true;
		}

		TaxConfigurationTemplateGUIHelper TaxConfigurationTemplateGUIHelper => taxConfigurationTemplateGUIHelper ?? (taxConfigurationTemplateGUIHelper = new TaxConfigurationTemplateGUIHelper(Organisation?.CompanyData, true));
		TaxConfigurationTemplateGUIHelper taxConfigurationTemplateGUIHelper;

		void CompanyData_ARTaxTemplateInfo_ValueChanged(object sender, EventArgs e)
		{
			TaxConfigurationTemplateGUIHelper.OrgTaxConfigurationTemplateChanged();
		}

		void RedefaultFromTemplateButton_Click(object sender, EventArgs e)
		{
			TaxConfigurationTemplateGUIHelper.RedefaultOrgTaxConfigurationFormTemplate();
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Organisation != null)
			{
				InvoiceDetailsTabPage.RunWhenBindingOrFirstShown(delegate
				{
					ReceivablesInvoiceDetailsControl.SetControlsReadOnlyForSecurity(!Organisation.SecurityProvider.HasModifyReceivablesInvoiceBatchingSecurity, !Organisation.SecurityProvider.HasModifyReceivablesExchangeRatesSecurity);
					ReceivablesInvoiceDetailsControl.SetClientOverrideExchageRatesGroupBoxVisibility(Organisation.Factory.IsLocalClientExchangeRatefieldNeeded());
				});
			}
		}

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
				Organisation.CompanyData.OB_OCT_ARTaxTemplateInfo.ValueChanged -= CompanyData_ARTaxTemplateInfo_ValueChanged;
			}
		}

		#endregion
	}
}
