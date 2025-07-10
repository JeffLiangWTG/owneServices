namespace Enterprise.MasterFiles.GUI
{
	public partial class ForwarderUserControl
	{

		#region Component Designer generated code

		ForwarderDetailsUserControl forwarderDetailsUserControl1;
		ProfitShareUserControl profitShareUserControl1;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl ForwarderTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage GeneralDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ProfitShareTabPage;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ForwarderTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.GeneralDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.forwarderDetailsUserControl1 = new Enterprise.MasterFiles.GUI.ForwarderDetailsUserControl();
			this.ProfitShareTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.profitShareUserControl1 = new Enterprise.MasterFiles.GUI.ProfitShareUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ForwarderTabControl.SuspendLayout();
			this.GeneralDetailsTabPage.SuspendLayout();
			this.ProfitShareTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ForwarderTabControl
			// 
			this.ForwarderTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ForwarderTabControl.Controls.Add(this.GeneralDetailsTabPage);
			this.ForwarderTabControl.Controls.Add(this.ProfitShareTabPage);
			this.ForwarderTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForwarderTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.ForwarderTabControl.Name = "ForwarderTabControl";
			this.ForwarderTabControl.SelectedIndex = 0;
			this.ForwarderTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 520, true);
			this.ForwarderTabControl.TabIndex = 4;
			// 
			// GeneralDetailsTabPage
			// 
			this.GeneralDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ForwarderUserControl|bb4b42a8-528c-41be-aac1-7536d92adf5c", "Details");
			this.GeneralDetailsTabPage.Controls.Add(this.forwarderDetailsUserControl1);
			this.GeneralDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GeneralDetailsTabPage.Name = "GeneralDetailsTabPage";
			this.GeneralDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.GeneralDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 493, true);
			this.GeneralDetailsTabPage.TabIndex = 0;
			// 
			// forwarderDetailsUserControl1
			// 
			this.BindingSource.SetBindingMember(this.forwarderDetailsUserControl1, ".");
			this.forwarderDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.forwarderDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.forwarderDetailsUserControl1.Name = "forwarderDetailsUserControl1";
			this.forwarderDetailsUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.forwarderDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 483, true);
			this.forwarderDetailsUserControl1.TabIndex = 0;
			// 
			// ProfitShareTabPage
			// 
			this.ProfitShareTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ForwarderUserControl|ee57a3af-e762-412c-bb72-35c98167282f", "Profit Share Agreements");
			this.ProfitShareTabPage.Controls.Add(this.profitShareUserControl1);
			this.ProfitShareTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ProfitShareTabPage.Name = "ProfitShareTabPage";
			this.ProfitShareTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ProfitShareTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 493, true);
			this.ProfitShareTabPage.TabIndex = 1;
			// 
			// profitShareUserControl1
			// 
			this.BindingSource.SetBindingMember(this.profitShareUserControl1, ".");
			this.profitShareUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.profitShareUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.profitShareUserControl1.Name = "profitShareUserControl1";
			this.profitShareUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 483, true);
			this.profitShareUserControl1.TabIndex = 0;
			// 
			// ForwarderUserControl
			// 
			this.Controls.Add(this.ForwarderTabControl);
			this.IsModifyForwarder = true;
			this.IsModifyForwarderDetails = true;
			this.IsModifyForwarderProfitShare = true;
			this.Name = "ForwarderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 544, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.ForwarderTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ForwarderTabControl.ResumeLayout(false);
			this.GeneralDetailsTabPage.ResumeLayout(false);
			this.ProfitShareTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
