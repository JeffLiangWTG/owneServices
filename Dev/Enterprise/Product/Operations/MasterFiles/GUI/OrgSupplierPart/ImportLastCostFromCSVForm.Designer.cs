namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportLastCostFromCSVForm : DataLoaderForm
	{
		Enterprise.ZArchitecture.GUI.ZGroupBox UseLegacyCodesGroupBox;
		Enterprise.ZArchitecture.GUI.ZRadioButton LegacyCodesYesRadioButton;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZRadioButton legacyCodesNoRadioButton;
			this.UseLegacyCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LegacyCodesYesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			legacyCodesNoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UseLegacyCodesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 117, true);
			// 
			// OutputListBox
			// 
			this.OutputListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 136, true);
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 329, true);
			// 
			// CloseButton
			// 
			this.CloseButton.ReadOnly = false;
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.ReadOnly = false;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			// 
			// LegacyCodesNoRadioButton
			// 
			legacyCodesNoRadioButton.AutoCheck = false;
			legacyCodesNoRadioButton.Checked = true;
			legacyCodesNoRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportLastCostFromCSVForm|7d4dc22b-90a4-4f1f-a5c6-e7b8dc357c6c", "No");
			legacyCodesNoRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			legacyCodesNoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 23, true);
			legacyCodesNoRadioButton.Name = "LegacyCodesNoRadioButton";
			legacyCodesNoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			legacyCodesNoRadioButton.TabIndex = 1;
			legacyCodesNoRadioButton.TabStop = true;
			// 
			// UseLegacyCodesGroupBox
			// 
			this.UseLegacyCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportLastCostFromCSVForm|ff82ad6d-1a0f-4a80-8c9c-bd32585b4774", "Use Legacy Codes for Organization Matching");
			this.UseLegacyCodesGroupBox.Controls.Add(this.LegacyCodesYesRadioButton);
			this.UseLegacyCodesGroupBox.Controls.Add(legacyCodesNoRadioButton);
			this.UseLegacyCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 62, true);
			this.UseLegacyCodesGroupBox.Name = "UseLegacyCodesGroupBox";
			this.UseLegacyCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 48, true);
			this.UseLegacyCodesGroupBox.TabIndex = 42;
			this.UseLegacyCodesGroupBox.TabStop = false;
			// 
			// LegacyCodesYesRadioButton
			// 
			this.LegacyCodesYesRadioButton.AutoCheck = false;
			this.LegacyCodesYesRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportLastCostFromCSVForm|71827780-a822-4147-a07f-fc48b231d70d", "Yes");
			this.LegacyCodesYesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LegacyCodesYesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 23, true);
			this.LegacyCodesYesRadioButton.Name = "LegacyCodesYesRadioButton";
			this.LegacyCodesYesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.LegacyCodesYesRadioButton.TabIndex = 2;
			// 
			// ImportLastCostFromCSVForm
			// 
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportLastCostFromCSVForm|fd056f7b-fd8d-4a94-9fec-07a8c776fd49", "Import Products Last Cost Data");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 551, true);
			this.Controls.Add(this.UseLegacyCodesGroupBox);
			this.Name = "ImportLastCostFromCSVForm";
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			this.Controls.SetChildIndex(this.UseLegacyCodesGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UseLegacyCodesGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
