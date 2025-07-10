namespace Enterprise.Rating.GUI
{
	public partial class WizardForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private new void InitializeComponent()
		{
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.nextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.backButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.divider = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.pageContainerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.finishButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.pageHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.stepDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.stepTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.wizardContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.wizardNavigationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.pageHeaderPanel.SuspendLayout();
			this.wizardContentPanel.SuspendLayout();
			this.wizardNavigationPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 388, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 24, true);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 26, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// nextButton
			// 
			this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.nextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 26, true);
			this.nextButton.Name = "nextButton";
			this.nextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.nextButton.TabIndex = 1;
			this.nextButton.UseVisualStyleBackColor = true;
			// 
			// backButton
			// 
			this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.backButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 26, true);
			this.backButton.Name = "backButton";
			this.backButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.backButton.TabIndex = 0;
			this.backButton.UseVisualStyleBackColor = true;
			// 
			// divider
			// 
			this.divider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.divider, false);
			this.divider.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.divider.Name = "divider";
			this.divider.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 15, true);
			this.divider.TabIndex = 3;
			this.divider.TabStop = false;
			// 
			// pageContainerPanel
			// 
			this.pageContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 55, true);
			this.pageContainerPanel.Name = "pageContainerPanel";
			this.pageContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 273, true);
			this.pageContainerPanel.TabIndex = 4;
			// 
			// finishButton
			// 
			this.finishButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.finishButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 26, true);
			this.finishButton.Name = "finishButton";
			this.finishButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.finishButton.TabIndex = 2;
			this.finishButton.UseVisualStyleBackColor = true;
			// 
			// pageHeaderPanel
			// 
			this.pageHeaderPanel.Controls.Add(this.stepDescriptionLabel);
			this.pageHeaderPanel.Controls.Add(this.stepTitleLabel);
			this.pageHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.pageHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pageHeaderPanel.Name = "pageHeaderPanel";
			this.pageHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 55, true);
			this.pageHeaderPanel.TabIndex = 0;
			// 
			// stepDescriptionLabel
			// 
			this.stepDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.stepDescriptionLabel, false);
			this.stepDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 27, true);
			this.stepDescriptionLabel.Name = "stepDescriptionLabel";
			this.stepDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 23, true);
			this.stepDescriptionLabel.TabIndex = 1;
			// 
			// stepTitleLabel
			// 
			this.stepTitleLabel.BackColor = System.Drawing.Color.Transparent;
			this.stepTitleLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.stepTitleLabel, false);
			this.stepTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.stepTitleLabel.Name = "stepTitleLabel";
			this.stepTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 23, true);
			this.stepTitleLabel.TabIndex = 0;
			// 
			// wizardContentPanel
			// 
			this.wizardContentPanel.Controls.Add(this.pageContainerPanel);
			this.wizardContentPanel.Controls.Add(this.pageHeaderPanel);
			this.wizardContentPanel.Controls.Add(this.wizardNavigationPanel);
			this.wizardContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wizardContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.wizardContentPanel.Name = "wizardContentPanel";
			this.wizardContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 388, true);
			this.wizardContentPanel.TabIndex = 7;
			// 
			// wizardNavigationPanel
			// 
			this.wizardNavigationPanel.Controls.Add(this.divider);
			this.wizardNavigationPanel.Controls.Add(this.cancelButton);
			this.wizardNavigationPanel.Controls.Add(this.finishButton);
			this.wizardNavigationPanel.Controls.Add(this.nextButton);
			this.wizardNavigationPanel.Controls.Add(this.backButton);
			this.wizardNavigationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.wizardNavigationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.wizardNavigationPanel.Name = "wizardNavigationPanel";
			this.wizardNavigationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 60, true);
			this.wizardNavigationPanel.TabIndex = 7;
			// 
			// WizardForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 412, true);
			this.Controls.Add(this.wizardContentPanel);
			this.MinimizeBox = false;
			this.Name = "WizardForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WizardForm_FormClosing);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.wizardContentPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.pageHeaderPanel.ResumeLayout(false);
			this.wizardContentPanel.ResumeLayout(false);
			this.wizardNavigationPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.GUI.ZButton nextButton;
		private Enterprise.ZArchitecture.GUI.ZButton backButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox divider;
		private Enterprise.ZArchitecture.GUI.ZPanel pageContainerPanel;
		private Enterprise.ZArchitecture.GUI.ZButton finishButton;
		private Enterprise.ZArchitecture.GUI.ZPanel pageHeaderPanel;
		private Enterprise.ZArchitecture.ZLabel stepDescriptionLabel;
		private Enterprise.ZArchitecture.ZLabel stepTitleLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel wizardContentPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel wizardNavigationPanel;
	}
}
