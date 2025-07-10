namespace Enterprise.Rating.GUI
{
	partial class GRIUpdateForm
	{
		protected override void Dispose(bool IsNotFinalizing)
		{
			if (IsNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(IsNotFinalizing);
		}

		new void InitializeComponent()
		{
			CargoWise.Windows.UI.KPanel mainPanel;
			CargoWise.Windows.UI.KPanel bottomPanel;
			this.previewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.updateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			mainPanel = new CargoWise.Windows.UI.KPanel();
			bodyControl = new Enterprise.Rating.GUI.GRIUpdateControl();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			mainPanel.SuspendLayout();
			bodyControl.SuspendLayout();
			bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 448, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 22, true);
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(232);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.RateUpdater);
			// 
			// mainPanel
			// 
			mainPanel.Controls.Add(bodyControl);
			mainPanel.Controls.Add(bottomPanel);
			mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			mainPanel.Name = "mainPanel";
			mainPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0, true);
			mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 448, true);
			mainPanel.TabIndex = 10;
			// 
			// bodyControl
			// 
			bodyControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(bodyControl, ".");
			bodyControl.Dock = System.Windows.Forms.DockStyle.Fill;
			bodyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			bodyControl.Name = "bodyControl";
			bodyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 419, true);
			bodyControl.TabIndex = 10;
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(this.previewButton);
			bottomPanel.Controls.Add(this.updateButton);
			bottomPanel.Controls.Add(this.cancelButtonX);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 419, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 29, true);
			bottomPanel.TabIndex = 9;
			// 
			// previewButton
			// 
			this.previewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.previewButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("GRIUpdateForm|1cda9d90-fe23-474a-85fa-6ebfa54a863d", "Preview");
			this.previewButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.previewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 3, true);
			this.previewButton.Name = "previewButton";
			this.previewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.previewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.previewButton.TabIndex = 5;
			this.previewButton.ToolTipCaption = null;
			this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("GRIUpdateForm|060486a3-2ca6-4965-a52d-7c3cd96e9730", "Send Updates");
			this.updateButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.updateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 3, true);
			this.updateButton.Name = "updateButton";
			this.updateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.updateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.updateButton.TabIndex = 6;
			this.updateButton.ToolTipCaption = null;
			this.updateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// cancelButtonX
			// 
			this.cancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButtonX.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("GRIUpdateForm|69fab51a-cf25-4b49-b728-31e1bad23588", "Cancel");
			this.cancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 3, true);
			this.cancelButtonX.Name = "cancelButtonX";
			this.cancelButtonX.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.cancelButtonX.TabIndex = 7;
			this.cancelButtonX.ToolTipCaption = null;
			this.cancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// GRIUpdateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("430f1ab8-df88-400a-b553-4a9731aeff4a", "Rate Update Notification");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 470, true);
			this.Controls.Add(mainPanel);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(Enterprise.Rating.Business.RateUpdater);
			this.DataSourceTypeName = "Enterprise.Rating.Business.RateUpdater";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 300, true);
			this.Name = "GRIUpdateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(mainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			mainPanel.ResumeLayout(false);
			mainPanel.PerformLayout();
			bodyControl.ResumeLayout(true);
			bodyControl.PerformLayout();
			bottomPanel.ResumeLayout(false);
			bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private System.ComponentModel.Container components = null;
		private ZArchitecture.GUI.ZButton updateButton;
		private ZArchitecture.GUI.ZButton cancelButtonX;
		internal ZArchitecture.GUI.ZButton previewButton;
		internal Enterprise.Rating.GUI.GRIUpdateControl bodyControl;
	}
}
