
namespace Enterprise.MasterFiles.GUI
{
	partial class SalesTeamForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.salesTeamDetailsControl = new Enterprise.MasterFiles.GUI.SalesTeamDetailsControl();
			this.CommissionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.salesTeamCommissionControl = new Enterprise.MasterFiles.GUI.SalesTeamCommissionControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommissionTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.CommissionTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 505, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CommissionTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.salesTeamDetailsControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 478, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 478, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 505, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.SalesTeam);
			// 
			// salesTeamDetailsControl
			// 
			this.salesTeamDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.salesTeamDetailsControl, ".");
			this.salesTeamDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesTeamDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.salesTeamDetailsControl.Name = "salesTeamDetailsControl";
			this.salesTeamDetailsControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.salesTeamDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 478, true);
			this.salesTeamDetailsControl.TabIndex = 0;
			// 
			// CommissionTabPage
			// 
			this.CommissionTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CommissionTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("86580676-e4ba-4ec7-aa13-f3d9d0f047ea", "Commission");
			this.CommissionTabPage.Controls.Add(this.salesTeamCommissionControl);
			this.CommissionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CommissionTabPage.Name = "CommissionTabPage";
			this.CommissionTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CommissionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 478, true);
			this.CommissionTabPage.TabIndex = 3;
			// 
			// salesTeamCommissionControl
			// 
			this.salesTeamCommissionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.salesTeamCommissionControl, ".");
			this.salesTeamCommissionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesTeamCommissionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.salesTeamCommissionControl.Name = "salesTeamCommissionControl";
			this.salesTeamCommissionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 472, true);
			this.salesTeamCommissionControl.TabIndex = 0;
			// 
			// SalesTeamForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cfc8df08-8570-4e34-bfd7-bbf033a1790f", "Sales Team");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 561, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.SalesTeam);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 600, true);
			this.Name = "SalesTeamForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommissionTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private SalesTeamDetailsControl salesTeamDetailsControl;
		protected ZArchitecture.GUI.ZTabPage CommissionTabPage;
		private SalesTeamCommissionControl salesTeamCommissionControl;
	}
}
