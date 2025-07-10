using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables
{
	public partial class CreditControlAndSettlementControl : OrganisationContainerControl
	{
		public CreditControlAndSettlementControl()
		{
			InitializeComponent();

			SetDataSourceBinding(CreditOnHoldLabel, "IsVisibleForBinding", "CompanyData+OB_AROnCreditHold");
			SetDataSourceBinding(CreditNotApprovedLabel, "IsVisibleForBinding", "CompanyData+OB_ARCreditLimitNotApprovedLabelIsVisible");
			SetDataSourceBinding(NoCreditLimitSetLabel, "IsVisibleForBinding", "CompanyData+OB_NoCreditLimitSetLabelIsVisible");
			SetDataSourceBinding(GlobalCreditOnHoldLabel, "IsVisibleForBinding", "CompanyData+OB_OnARGlobalCreditHoldLabelIsVisible");
			SetDataSourceBinding(GlobalCreditApprovedLabel, "IsVisibleForBinding", "CompanyData+OB_ARGlobalCreditApprovedLabelIsVisible");

			if (!DesignModeFinder.IsDesigning)
			{
				if (!Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed)
				{
					GlobalTabPage.SetupSecurity(Env.Security.OrgReceivablesGlobalCreditControl);
					GlobalCreditGroupBox.Visible = false;
				}

				CreditCardDetailsGroupBox.Visible = Env.Security.OrgReceivablesViewCreditCardDetails.IsAllowed;
				this.GlobalCreditApprovedLabel.CaptionResourceString = this.GlobalCreditApprovedLabel.CaptionResourceString.Format(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms);
				this.CreditOnHoldLabel.CaptionResourceString = this.CreditOnHoldLabel.CaptionResourceString.Format(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms);
				this.GlobalCreditOnHoldLabel.CaptionResourceString = this.GlobalCreditOnHoldLabel.CaptionResourceString.Format(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms);
				this.CreditNotApprovedLabel.CaptionResourceString = this.CreditNotApprovedLabel.CaptionResourceString.Format(OrganisationRegistry.Instance.PreApprovalTerms.Value);
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			var header = BindingSource.DataSource as OrgHeader;
			if (header != null)
			{
				if (CreditReportHelper.CreditReportEnabled)
				{
					if (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.Value.Count > 0)
					{
						LocalSplitContainer.Panel1Collapsed = false;
						var creditReportControl = new CreditReportUserControl();
						creditReportControl.SetDataBinding(header, string.Empty);
						LocalSplitContainer.Panel1.Controls.Add(creditReportControl);
					}
					else
					{
						LocalSplitContainer.Panel1Collapsed = true;
					}

					CreditControlPanel.Dock = DockStyle.None;
					ARCreditReportDetailsGroupBox.Visible = true;
				}
				else
				{
					LocalSplitContainer.Panel1Collapsed = true;

					CreditControlPanel.Dock = DockStyle.Fill;
					ARCreditReportDetailsGroupBox.Visible = false;
				}
			}

			base.OnAfterFirstBinding(e);
		}
	}
}
