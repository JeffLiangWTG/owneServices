namespace Enterprise.Freight.GUI
{
	partial class PortCallRequestForm
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

		protected new void InitializeComponent()
		{
			this.bottomPanel = new CargoWise.Windows.UI.KPanel();
			this.filterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			cancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			okBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 629, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.OnlineSailingSchedules.PortCall.PortCallManager);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(cancelBtn);
			this.bottomPanel.Controls.Add(okBtn);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 594, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 35, true);
			this.bottomPanel.TabIndex = 2;
			// 
			// cancelBtn
			// 
			cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			cancelBtn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("09910a3a-f3a9-4f50-8326-29f4002bfd4c", "Cancel");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(cancelBtn, false);
			cancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(761, 6, true);
			cancelBtn.Name = "cancelBtn";
			cancelBtn.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			cancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			cancelBtn.TabIndex = 4;
			cancelBtn.Text = "Cancel";
			cancelBtn.ToolTipCaption = null;
			cancelBtn.UseVisualStyleBackColor = true;
			cancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// okBtn
			// 
			okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			okBtn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("a9066580-4fe4-423c-9097-ab00e8f5e7b9", "OK");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(okBtn, false);
			okBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(841, 6, true);
			okBtn.Name = "okBtn";
			okBtn.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			okBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			okBtn.TabIndex = 5;
			okBtn.Text = "OK";
			okBtn.ToolTipCaption = null;
			okBtn.UseVisualStyleBackColor = true;
			okBtn.Click += new System.EventHandler(this.OkBtn_Click);
			// 
			// filterPanel
			// 
			this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 594, true);
			this.filterPanel.TabIndex = 4;
			// 
			// PortCallRequestForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("2a9a8da4-46f7-4375-b7f0-efb0a3163d36", "Port Call Search");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 653, true);
			this.Controls.Add(this.filterPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.OnlineSailingSchedules.PortCall.PortCallManager);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 692, true);
			this.Name = "PortCallRequestForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.filterPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private CargoWise.Windows.UI.KPanel bottomPanel;
		private ZArchitecture.GUI.ZPanel filterPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton cancelBtn;
		protected Enterprise.ZArchitecture.GUI.ZButton okBtn;
	}
}
