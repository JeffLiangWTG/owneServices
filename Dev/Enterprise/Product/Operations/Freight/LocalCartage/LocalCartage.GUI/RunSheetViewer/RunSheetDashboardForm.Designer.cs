using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class RunSheetDashboardForm
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
			this.RunSheetDashboardPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RunSheetDashboardControl = new Enterprise.Freight.LocalCartage.GUI.RunSheetDashboardControl();
			this.AutoRefreshWarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RunSheetDashboardPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1014, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1904, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.RunSheetDashboard);
			// 
			// RunSheetDashboardPanel
			// 
			this.RunSheetDashboardPanel.Controls.Add(this.RunSheetDashboardControl);
			this.RunSheetDashboardPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RunSheetDashboardPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RunSheetDashboardPanel.Name = "RunSheetDashboardPanel";
			this.RunSheetDashboardPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.RunSheetDashboardPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1904, 989, true);
			this.RunSheetDashboardPanel.TabIndex = 0;
			// 
			// RunSheetDashboardControl
			// 
			this.RunSheetDashboardControl.AllowDrop = true;
			this.RunSheetDashboardControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.RunSheetDashboardControl, ".");
			this.RunSheetDashboardControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RunSheetDashboardControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.RunSheetDashboardControl.Name = "RunSheetDashboardControl";
			this.RunSheetDashboardControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1898, 989, true);
			this.RunSheetDashboardControl.TabIndex = 1;
			// 
			// AutoRefreshWarningLabel
			// 
			this.AutoRefreshWarningLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.AutoRefreshWarningLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoRefreshWarningLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AutoRefreshWarningLabel, false);
			this.AutoRefreshWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AutoRefreshWarningLabel.Name = "AutoRefreshWarningLabel";
			this.AutoRefreshWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 25, true);
			this.AutoRefreshWarningLabel.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Controls.Add(this.AutoRefreshWarningLabel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 989, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1904, 25, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("RunSheetDashboardForm|8308112c-d3d7-4729-9302-fb27bb3ad667", "Close", "Close Run Sheet Dashboard.");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1822, 1, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// RunSheetDashboardForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("WorkSheetTileForm|9b2ba819-3e92-4b66-b1d4-e5902806c29a", "Run Sheet Viewer");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1904, 1038, true);
			this.Controls.Add(this.RunSheetDashboardPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.RunSheetDashboard);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 550, true);
			this.Name = "RunSheetDashboardForm";
			this.Text = "CartageLegPlannerForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.RunSheetDashboardPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RunSheetDashboardPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZPanel RunSheetDashboardPanel;
		internal RunSheetDashboardControl RunSheetDashboardControl;
		public Enterprise.ZArchitecture.ZLabel AutoRefreshWarningLabel;
		private ZPanel BottomPanel;
		private ZButton CloseButton;
	}
}
