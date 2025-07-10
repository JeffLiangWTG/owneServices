namespace Enterprise.MarketingManager.GUI
{
	partial class ProspectiveTradeProfileForm
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
		public new void InitializeComponent()
		{
			this.SaveButtonUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.salesHeaderListControl = new Enterprise.MarketingManager.GUI.SalesHeaderListControl();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SaveButtonUserControl.SuspendLayout();
			this.salesHeaderListControl.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 641, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ISalesValueAssociatedEntity);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.AllowDrop = true;
			this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(944, 5, true);
			this.SaveButtonUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.SaveButtonUserControl.Name = "SaveButtonUserControl";
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.SaveButtonUserControl.TabIndex = 2;
			// 
			// salesHeaderListControl
			// 
			this.salesHeaderListControl.AllowDrop = true;
			this.salesHeaderListControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.salesHeaderListControl, ".");
			this.salesHeaderListControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesHeaderListControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.salesHeaderListControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.salesHeaderListControl.Name = "salesHeaderListControl";
			this.salesHeaderListControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 607, true);
			this.salesHeaderListControl.TabIndex = 1;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.SaveButtonUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 607, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 34, true);
			this.bottomPanel.TabIndex = 4;
			// 
			// ProspectiveTradeProfileForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 665, true);
			this.Controls.Add(this.salesHeaderListControl);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ISalesValueAssociatedEntity);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 600, true);
			this.Name = "ProspectiveTradeProfileForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.salesHeaderListControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			this.salesHeaderListControl.ResumeLayout(true);
			this.salesHeaderListControl.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl SaveButtonUserControl;
		protected Enterprise.MarketingManager.GUI.SalesHeaderListControl salesHeaderListControl;
		protected ZArchitecture.GUI.ZPanel bottomPanel;
	}
}
