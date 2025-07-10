namespace Enterprise.MasterFiles.GUI
{
	public partial class ExternalValidationProgressForm
	{

		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.ZLabel LabelStatus;
		Enterprise.ZArchitecture.GUI.ZButton buttonCancel;
		Enterprise.ZArchitecture.GUI.ZButton buttonClose;
		System.Windows.Forms.Timer closeTimer;
		System.Windows.Forms.Timer labelRefreshTimer;
		int labelRefreshTicks = 0;
		System.ComponentModel.IContainer components;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.LabelStatus = new Enterprise.ZArchitecture.ZLabel();
			this.buttonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonClose = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeTimer = new System.Windows.Forms.Timer(this.components);
			this.labelRefreshTimer = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 150, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(294);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(294);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// LabelStatus
			// 
			this.LabelStatus.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExternalValidationProgressForm|d96584d2-5e5a-4e39-8453-ce8dd9252beb", "Performing external validation of organization XXX...");
			this.LabelStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.LabelStatus.Name = "LabelStatus";
			this.LabelStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 103, true);
			this.LabelStatus.TabIndex = 2;
			this.LabelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonCancel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExternalValidationProgressForm|b5ddd6ac-7536-43b4-aff1-961142b01a82", "Cancel");
			this.buttonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 115, true);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.buttonClose.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExternalValidationProgressForm|4a2efce1-84d3-4cb2-86fc-d712186cc386", "Close");
			this.buttonClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 115, true);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.buttonClose.TabIndex = 4;
			this.buttonClose.Visible = false;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
			// 
			// closeTimer
			// 
			this.closeTimer.Interval = 2000;
			this.closeTimer.Tick += new System.EventHandler(this.closeTimer_OnTick);
			// 
			// labelRefreshTimer
			// 
			this.labelRefreshTimer.Interval = 1000;
			this.labelRefreshTimer.Tick += new System.EventHandler(this.labelRefreshTimer_OnTick);
			// 
			// ExternalValidationProgressForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExternalValidationProgressForm|39023CAF-33A6-4948-B541-D0D7F1E59504", "Organization Validation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 150, true);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.LabelStatus);
			this.Controls.Add(this.buttonCancel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ExternalValidationProgressForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Load += new System.EventHandler(this.ExternalValidationProgressForm_Load);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LabelStatus, 0);
			this.Controls.SetChildIndex(this.buttonClose, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
