namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportOrganisationsFromCSVForm
	{

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.OrgCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GenerateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NewCodesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ExistingCodesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrgCodesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ProgressBar
			// 
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 493, true);
			// 
			// StartButton
			// 
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 493, true);
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 130, true);
			// 
			// OutputListBox
			// 
			this.OutputListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 152, true);
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 316, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 528, true);
			this.CloseButton.ReadOnly = false;
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 527, true);
			this.CopyLogToClipboardButton.ReadOnly = false;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			// 
			// OrgCodesGroupBox
			// 
			this.OrgCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportOrganisationsFromCSVForm|da8f5d7b-0c21-40df-8dea-d18c688d2c40", "Organization Codes");
			this.OrgCodesGroupBox.Controls.Add(this.GenerateLabel);
			this.OrgCodesGroupBox.Controls.Add(this.NewCodesRadioButton);
			this.OrgCodesGroupBox.Controls.Add(this.ExistingCodesRadioButton);
			this.OrgCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 64, true);
			this.OrgCodesGroupBox.Name = "OrgCodesGroupBox";
			this.OrgCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 63, true);
			this.OrgCodesGroupBox.TabIndex = 31;
			this.OrgCodesGroupBox.TabStop = false;
			// 
			// GenerateLabel
			// 
			this.GenerateLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportOrganisationsFromCSVForm|77374903-9f6e-4dd6-aa0f-2c4104c73451", "(Recommended)");
			this.GenerateLabel.ForeColor = System.Drawing.SystemColors.Highlight;
			this.GenerateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 41, true);
			this.GenerateLabel.Name = "GenerateLabel";
			this.GenerateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 18, true);
			this.GenerateLabel.TabIndex = 3;
			// 
			// NewCodesRadioButton
			// 
			this.NewCodesRadioButton.AutoCheck = false;
			this.NewCodesRadioButton.Checked = true;
			this.NewCodesRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportOrganisationsFromCSVForm|232424e6-c697-43df-9992-49fd85305639", "Generate New");
			this.NewCodesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewCodesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 20, true);
			this.NewCodesRadioButton.Name = "NewCodesRadioButton";
			this.NewCodesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.NewCodesRadioButton.TabIndex = 2;
			this.NewCodesRadioButton.TabStop = true;
			// 
			// ExistingCodesRadioButton
			// 
			this.ExistingCodesRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ExistingCodesRadioButton.AutoCheck = false;
			this.ExistingCodesRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportOrganisationsFromCSVForm|6b094b55-fc18-47cd-a62e-4c30cdf994d7", "Retain Existing");
			this.ExistingCodesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExistingCodesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.ExistingCodesRadioButton.Name = "ExistingCodesRadioButton";
			this.ExistingCodesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.ExistingCodesRadioButton.TabIndex = 1;
			// 
			// ImportOrganisationsFromCSVForm
			// 
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportOrganisationsFromCSVForm|fd056f7b-fd8d-4a94-9fec-07a8c776fd49", "Import Organization Data");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 551, true);
			this.Controls.Add(this.OrgCodesGroupBox);
			this.Name = "ImportOrganisationsFromCSVForm";
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			this.Controls.SetChildIndex(this.OrgCodesGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrgCodesGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		#region Auto-Generated

		private Enterprise.ZArchitecture.GUI.ZGroupBox OrgCodesGroupBox;
		private Enterprise.ZArchitecture.ZLabel GenerateLabel;
		private Enterprise.ZArchitecture.GUI.ZRadioButton NewCodesRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ExistingCodesRadioButton;

		#endregion

	}
}
