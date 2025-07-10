namespace Enterprise.Freight.Agency.GUI
{
	partial class PortAuthorityFilterDialog
	{
		new void InitializeComponent()
		{
			this.warningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.filterControl = new Enterprise.Freight.Agency.GUI.PortAuthorityFilterControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.filterControl.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 359, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortAuthority);
			// 
			// warningLabel
			// 
			this.warningLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.warningLabel.ForeColor = System.Drawing.Color.Red;
			this.warningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 3, 16, 3, true);
			this.warningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 48, true);
			this.warningLabel.TabIndex = 0;
			this.warningLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// filterControl
			// 
			this.filterControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.filterControl, ".");
			this.filterControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.filterControl.Name = "filterControl";
			this.filterControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 2, 16, 2, true);
			this.filterControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 311, true);
			this.filterControl.TabIndex = 4;
			this.filterControl.SendClicked += new System.EventHandler(this.filterControl_SendClicked);
			this.filterControl.CancelClicked += new System.EventHandler(this.filterControl_CancelClicked);
			// 
			// PortAuthorityFilterDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityFilterDialog|6723e376-3394-4620-b1d8-f5cb1918bde8", "Port Authority Messaging");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 383, true);
			this.Controls.Add(this.filterControl);
			this.Controls.Add(this.warningLabel);
			this.DataSourceAssemblyName = "Enterprise.Freight.Agency.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortAuthority);
			this.DataSourceTypeName = "Enterprise.Freight.Agency.Business.PortAuthorityFilter";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 410, true);
			this.Name = "PortAuthorityFilterDialog";
			this.Controls.SetChildIndex(this.warningLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.filterControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.filterControl.ResumeLayout(true);
			this.filterControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);

		}
		private PortAuthorityFilterControl filterControl;
		private Enterprise.ZArchitecture.ZLabel warningLabel;
	}
}
