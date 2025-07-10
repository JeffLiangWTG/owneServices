namespace Enterprise.MasterFiles.GUI
{
	public partial class CompetitorUserControl
	{

		#region Component Designer generated code

		protected Enterprise.ZArchitecture.GUI.ZTabPage CIDetailsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage CIProfileTabPage;
		internal protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl CompetitorTabControl;
		private CompetitorDetailsUserControl competitorDetailsUserControl1;
		private CompetitorTradeProfileUserControl competitorTradeProfileUserControl1;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.CompetitorTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.CIDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.competitorDetailsUserControl1 = new Enterprise.MasterFiles.GUI.CompetitorDetailsUserControl();
			this.CIProfileTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.competitorTradeProfileUserControl1 = new Enterprise.MasterFiles.GUI.CompetitorTradeProfileUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompetitorTabControl.SuspendLayout();
			this.CIDetailsTabPage.SuspendLayout();
			this.CIProfileTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CompetitorTabControl
			// 
			this.CompetitorTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CompetitorTabControl.Controls.Add(this.CIDetailsTabPage);
			this.CompetitorTabControl.Controls.Add(this.CIProfileTabPage);
			this.CompetitorTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompetitorTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.CompetitorTabControl.Name = "CompetitorTabControl";
			this.CompetitorTabControl.SelectedIndex = 0;
			this.CompetitorTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 535, true);
			this.CompetitorTabControl.TabIndex = 0;
			// 
			// CIDetailsTabPage
			// 
			this.CIDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorUserControl|5ed5b857-47e4-4866-ade9-d1625df08677", "Competitor Details");
			this.CIDetailsTabPage.Controls.Add(this.competitorDetailsUserControl1);
			this.CIDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CIDetailsTabPage.Name = "CIDetailsTabPage";
			this.CIDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CIDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 508, true);
			this.CIDetailsTabPage.TabIndex = 0;
			// 
			// competitorDetailsUserControl1
			// 
			this.BindingSource.SetBindingMember(this.competitorDetailsUserControl1, ".");
			this.competitorDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.competitorDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.competitorDetailsUserControl1.Name = "competitorDetailsUserControl1";
			this.competitorDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 498, true);
			this.competitorDetailsUserControl1.TabIndex = 0;
			// 
			// CIProfileTabPage
			// 
			this.CIProfileTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorUserControl|95f222aa-f075-42d2-922c-ba87afb47cc1", "Trade Profile");
			this.CIProfileTabPage.Controls.Add(this.competitorTradeProfileUserControl1);
			this.CIProfileTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CIProfileTabPage.Name = "CIProfileTabPage";
			this.CIProfileTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CIProfileTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 508, true);
			this.CIProfileTabPage.TabIndex = 1;
			// 
			// competitorTradeProfileUserControl1
			// 
			this.BindingSource.SetBindingMember(this.competitorTradeProfileUserControl1, ".");
			this.competitorTradeProfileUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.competitorTradeProfileUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.competitorTradeProfileUserControl1.Name = "competitorTradeProfileUserControl1";
			this.competitorTradeProfileUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 498, true);
			this.competitorTradeProfileUserControl1.TabIndex = 0;
			// 
			// CompetitorUserControl
			// 
			this.Controls.Add(this.CompetitorTabControl);
			this.IsModifyCompetitor = true;
			this.Name = "CompetitorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 559, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.CompetitorTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompetitorTabControl.ResumeLayout(false);
			this.CIDetailsTabPage.ResumeLayout(false);
			this.CIProfileTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
