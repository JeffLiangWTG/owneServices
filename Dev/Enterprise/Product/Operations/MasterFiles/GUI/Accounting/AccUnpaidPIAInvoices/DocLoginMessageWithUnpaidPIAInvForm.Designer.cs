namespace Enterprise.MasterFiles.GUI
{
	public partial class DocLoginMessageWithPIAInvForm
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZPanel bottomPanelInner;
			this.VewUnPaidPIAInvButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.YesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			bottomPanelInner = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanelInner.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BottomPanelInner
			// 
			bottomPanelInner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			bottomPanelInner.AutoSize = true;
			bottomPanelInner.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			bottomPanelInner.Controls.Add(this.VewUnPaidPIAInvButton);
			bottomPanelInner.Controls.Add(this.NoButton);
			bottomPanelInner.Controls.Add(this.YesButton);
			bottomPanelInner.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			bottomPanelInner.Name = "BottomPanelInner";
			bottomPanelInner.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 33, true);
			bottomPanelInner.TabIndex = 0;
			// 
			// VewUnPaidPIAInvButton
			// 
			this.VewUnPaidPIAInvButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocLoginMessageWithPIAInvForm|e58338e3-6fc9-4621-a713-be1de8634f3f", "&View unpaid PIA invoices");
			this.VewUnPaidPIAInvButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 7, true);
			this.VewUnPaidPIAInvButton.Name = "VewUnPaidPIAInvButton";
			this.VewUnPaidPIAInvButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 23, true);
			this.VewUnPaidPIAInvButton.TabIndex = 2;
			this.VewUnPaidPIAInvButton.Click += new System.EventHandler(this.VewUnPaidPIAInvButton_Click);
			// 
			// NoButton
			// 
			this.NoButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocLoginMessageWithPIAInvForm|52fbfaca-cb50-4940-ad02-e00702f9918f", "&No");
			this.NoButton.DialogResult = System.Windows.Forms.DialogResult.No;
			this.NoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 7, true);
			this.NoButton.Name = "NoButton";
			this.NoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NoButton.TabIndex = 1;
			// 
			// YesButton
			// 
			this.YesButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocLoginMessageWithPIAInvForm|d6938663-da96-4cf0-84cf-fa9d8988cb4e", "&Yes");
			this.YesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.YesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 7, true);
			this.YesButton.Name = "YesButton";
			this.YesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.YesButton.TabIndex = 0;
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocLoginMessageWithPIAInvForm|f6b116fd-b562-4d00-b037-386f6eb2d89a", "Design Message.");
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.MessageLabel.TabIndex = 0;
			this.MessageLabel.SizeChanged += new System.EventHandler(this.MessageLabel_SizeChanged);
			// 
			// BottomPanel
			// 
			this.BottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BottomPanel.Controls.Add(bottomPanelInner);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 45, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// DocLoginMessageWithPIAInvForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 96, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocLoginMessageWithPIAInvForm|0f28c10c-a447-45e1-89c5-5a5e7b8e1af3", "Select Orders");
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.MessageLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 122, true);
			this.Name = "DocLoginMessageWithPIAInvForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Load += new System.EventHandler(this.DocLoginMessageWithPIAInvForm_Load);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanelInner.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.GUI.ZButton VewUnPaidPIAInvButton;
		private Enterprise.ZArchitecture.GUI.ZButton NoButton;
		private Enterprise.ZArchitecture.GUI.ZButton YesButton;
	}
}
