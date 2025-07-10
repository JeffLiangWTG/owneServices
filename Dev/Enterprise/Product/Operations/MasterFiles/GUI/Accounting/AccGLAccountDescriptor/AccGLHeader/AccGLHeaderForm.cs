using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGLHeaderForm : ZForm
	{
		public AccGLHeaderForm()
		{
			InitializeComponent();
		}

		public AccGLHeaderForm(AccGLHeader bO)
			: base(bO)
		{
			this.IsInitializing = true;
			this.ControllerID = ControllerIDs.AccGLHeader;
			InitializeComponent();
			SetUpEventHandlers();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			AlternateChartsDissectionConfigurationTabPage.TabVisible = AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			this.IsInitializing = false;
			GLAccountDetailsGroupBox.AllowOutsideOfParent();
		}

		#region EventHandlers

		void SetUpEventHandlers()
		{
			AccGLHeaderTabControl.SelectedIndexChanged += new EventHandler(SetCompanyFilterLabelVisibility);
			AccGLHeaderTabControl.SelectedIndexChanged += new EventHandler(SetNoDissectionConfigurationSecurityLabelVisibility);
			AccGLHeaderTabControl.SelectedIndexChanged += new EventHandler(SetNotBSHPLDissectionConfigurationLabelVisibility);
			AccGLHeaderTabControl.SelectedIndexChanged += new EventHandler(SetNoDissectionConfigurationLabelVisibility);
			AccGLHeaderTabControl.SelectedIndexChanged += new EventHandler(SetOverlapsForControls);
		}

		void AG_CodeBoundTextEdit_KeyPress(object sender, KeyPressEventArgs e)
		{
			char ch = e.KeyChar;
			if (!(char.IsDigit(ch) || ch == '.' || char.IsControl(ch)))
			{
				e.Handled = true;
			}
		}

		void SetOverlapsForControls(object sender, EventArgs e)
		{
			if (CompanyFilterLabel != null || AlternateChartsDissectionConfigurationGrid != null)
			{
				if (CompanyFilterLabel != null)
				{
					CompaniesGrid.AllowOutsideOfParent();
					CompanyFilterLabel.AllowOutsideOfParent();
					CompanyFilterLabel.AllowOverlap(CompaniesGrid);
					CompanyFilterLabel.AllowOverlap(AG_DescriptionBoundText);
					CompanyFilterLabel.AllowOverlap(AG_CodeBoundText);
				}

				if (AlternateChartsDissectionConfigurationGrid != null)
				{
					NoDissectionConfigurationSecurityLabel.AllowOverlap(AlternateChartsDissectionConfigurationGrid);
					AlternateChartsDissectionConfigurationGrid.AllowOutsideOfParent();
					NoDissectionConfigurationSecurityLabel.AllowOutsideOfParent();
					NoDissectionConfigurationSecurityLabel.AllowOverlap(AlternateChartsDissectionConfigurationGrid);
					NotBSHPLDissectionConfigurationLabel.AllowOutsideOfParent();
					NotBSHPLDissectionConfigurationLabel.AllowOverlap(AlternateChartsDissectionConfigurationGrid);
					NoDissectionConfigurationLabel.AllowOutsideOfParent();
					NoDissectionConfigurationLabel.AllowOverlap(AlternateChartsDissectionConfigurationGrid);
				}

				AccGLHeaderTabControl.SelectedIndexChanged -= new EventHandler(SetOverlapsForControls);
			}
		}

		void SetCompanyFilterLabelVisibility(object sender, EventArgs e)
		{
			if (CompanyFilterLabel != null)
			{
				CompanyFilterLabel.Visible = AG_IsGlobalBoundCheckBox.Checked;
			}
		}

		void WarnIfIsGlobalChanged(object sender, EventArgs e)
		{
			if (!this.IsInitializing && AG_IsGlobalBoundCheckBox.Checked)
			{
				if (Globals.Message.Show(Res.GetString("221970ac-464d-4a6a-ae32-74d0a6aacb5b", "Setting this GL Account as a Global GL Account will remove all companies from the Companies Tab. Are you sure you want to proceed?"), Res.GetString("73c47dc6-967b-4360-8c32-b7a24362f0a1", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
				{
					AG_IsGlobalBoundCheckBox.Checked = false;
				}
			}
		}

		void SetNoDissectionConfigurationSecurityLabelVisibility(object sender, EventArgs e)
		{
			if (NoDissectionConfigurationSecurityLabel != null)
			{
				NoDissectionConfigurationSecurityLabel.Visible = !Env.Security.GLAccountsDissectionConfigurationEdit.IsAllowed;
			}
		}

		void SetNotBSHPLDissectionConfigurationLabelVisibility(object sender, EventArgs e)
		{
			var supportAccountType = new List<string>() { AccountType.BalanceSheetAccount, AccountType.ProfitAndLossAccount };
			if (NotBSHPLDissectionConfigurationLabel != null)
			{
				NotBSHPLDissectionConfigurationLabel.Visible = !supportAccountType.Contains(AG_AccountTypeBoundDropEdit.CodeBox.Text);
			}
		}

		void SetNoDissectionConfigurationLabelVisibility(object sender, EventArgs e)
		{
			var glHeader = BusinessEntity as AccGLHeader;
			if (NoDissectionConfigurationLabel != null)
			{
				NoDissectionConfigurationLabel.Visible = !glHeader?.IsAllowedToHaveAttributes() ?? false;
			}
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
			AccGLHeaderTabControl.SelectedIndexChanged -= new EventHandler(SetCompanyFilterLabelVisibility);
			AccGLHeaderTabControl.SelectedIndexChanged -= new EventHandler(SetNoDissectionConfigurationSecurityLabelVisibility);
			AccGLHeaderTabControl.SelectedIndexChanged -= new EventHandler(SetNotBSHPLDissectionConfigurationLabelVisibility);
			AccGLHeaderTabControl.SelectedIndexChanged -= new EventHandler(SetNoDissectionConfigurationLabelVisibility);
		}

		#endregion
	}
}
