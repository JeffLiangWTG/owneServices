using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ReceivablesConfigurationUserControl : OrganisationContainerControl
	{
		public ReceivablesConfigurationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered)
				{
					OB_ARVATConfigDropEdit.Visible = false;
					OB_ARVATConfigLabel.Visible = false;
					OM_ARDontShowTaxOnDocsBoundCheckEdit.Visible = false;
					OB_ARGoodsOwnershipLabel.Visible = false;
					OB_ARGoodsOwnershipDropEdit.Visible = false;
				}

				if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Italy)
				{
					OM_ARVATSplitPaymentApplicableBoundCheckEdit.Visible = false;
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
				{
					OB_ARCreateVATComplianceDocumentOnPostingLabel.Visible = false;
					OB_ARCreateVATComplianceDocumentOnPostingDropEdit.Visible = false;
				}

				if (!Env.Security.OrgReceivablesModifySurchargeConfiguration.IsAllowedWithConstraint())
				{
					ApplicableSurchargesEditButton.Enabled = false;
				}
			}
		}

		internal void ApplicableSurchargesEditButtonButton_Click(object sender, EventArgs e)
		{
			var bizo = this.CurrentDataItem as OrgHeader;
			var surchargeCodes = new AccSurchargeConfigurationCollection(bizo.Factory, GlbCompany.CurrentCompany.PK, true);
			surchargeCodes.Load();
			surchargeCodes.ApplicableSurchargesAsString = bizo.CompanyData.OB_ARApplicableSurcharges;
			var dialogResult = AccSurchargeConfigurationCollectionForm.ShowDialog(surchargeCodes);
			if (dialogResult == System.Windows.Forms.DialogResult.OK)
			{
				bizo.CompanyData.OB_ARApplicableSurcharges = surchargeCodes.ApplicableSurchargesAsString;
			}
		}

		#region Binding

		ZCheckBox OverridePayToAccountCheckBox;
		ZPanel MainPanel;
		ZCodeFindBox OB_RX_NKAPDefltCurrencyCodeFindBox;
		ZGroupBox CompanyDataGroupBox;
		ZCheckBox AllowMultiCurrencyPaymentCheckBox;

		internal ZPanel ApplicableSurchargesPanel;
		internal ZTextBox ApplicableSurchargesTextBox;
		internal ZButton ApplicableSurchargesEditButton;

		#endregion
	}
}
