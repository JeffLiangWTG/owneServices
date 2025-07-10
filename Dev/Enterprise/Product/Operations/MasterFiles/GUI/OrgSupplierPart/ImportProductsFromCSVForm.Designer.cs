namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportProductsFromCSVForm : DataLoaderForm
	{
		private Enterprise.ZArchitecture.GUI.ZGroupBox UpdateRecsThatExistGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton UpdateYesRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton UpdateNoRadioButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox UseLegacyCodesGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton LegacyCodesYesRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton LegacyCodesNoRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SetAuditStatusGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton SetAuditStatusYesRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton SetAuditStatusNoRadioButton;

		new void InitializeComponent()
		{
			this.UpdateRecsThatExistGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UpdateYesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.UpdateNoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.UseLegacyCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LegacyCodesYesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.LegacyCodesNoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SetAuditStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SetAuditStatusYesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SetAuditStatusNoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UpdateRecsThatExistGroupBox.SuspendLayout();
			this.UseLegacyCodesGroupBox.SuspendLayout();
			this.SetAuditStatusGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SelectFileButton
			// 
			this.SelectFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 35, true);
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 18, true);
			// 
			// ProgressBar
			//
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 20, true);
			this.ProgressBar.TabIndex = 8;
			// 
			// StartButton
			// 
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 488, true);
			this.StartButton.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 121, true);
			this.label2.TabIndex = 6;
			// 
			// OutputListBox
			// 
			this.OutputListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 144, true);
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 298, true);
			this.OutputListBox.TabIndex = 7;
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 527, true);
			this.CloseButton.ReadOnly = false;
			this.CloseButton.TabIndex = 12;
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 526, true);
			this.CopyLogToClipboardButton.ReadOnly = false;
			this.CopyLogToClipboardButton.TabIndex = 11;
			// 
			// DownloadTemplateButton
			// 
			this.DownloadTemplateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 35, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 28, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// UpdateRecsThatExistGroupBox
			// 
			this.UpdateRecsThatExistGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|0f13b4d3-3005-4a1b-b75d-11a31d3c5680", "Update Products that Already Exist");
			this.UpdateRecsThatExistGroupBox.Controls.Add(this.UpdateYesRadioButton);
			this.UpdateRecsThatExistGroupBox.Controls.Add(this.UpdateNoRadioButton);
			this.UpdateRecsThatExistGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 64, true);
			this.UpdateRecsThatExistGroupBox.Name = "UpdateRecsThatExistGroupBox";
			this.UpdateRecsThatExistGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 48, true);
			this.UpdateRecsThatExistGroupBox.TabIndex = 3;
			this.UpdateRecsThatExistGroupBox.TabStop = false;
			// 
			// UpdateYesRadioButton
			// 
			this.UpdateYesRadioButton.AutoCheck = false;
			this.UpdateYesRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|8c5ed1a5-17c9-4e31-aa2a-7408f24341cb", "Yes");
			this.UpdateYesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpdateYesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 23, true);
			this.UpdateYesRadioButton.Name = "UpdateYesRadioButton";
			this.UpdateYesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.UpdateYesRadioButton.TabIndex = 1;
			// 
			// UpdateNoRadioButton
			// 
			this.UpdateNoRadioButton.AutoCheck = false;
			this.UpdateNoRadioButton.Checked = true;
			this.UpdateNoRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|4cc4f8d6-1338-497c-9669-6732b19e4973", "No");
			this.UpdateNoRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpdateNoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 23, true);
			this.UpdateNoRadioButton.Name = "UpdateNoRadioButton";
			this.UpdateNoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.UpdateNoRadioButton.TabIndex = 0;
			this.UpdateNoRadioButton.TabStop = true;
			// 
			// UseLegacyCodesGroupBox
			// 
			this.UseLegacyCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|f164d4ff-6cd4-43cf-b603-b5b3f3a62dab", "Use Legacy Codes for Organization Matching");
			this.UseLegacyCodesGroupBox.Controls.Add(this.LegacyCodesYesRadioButton);
			this.UseLegacyCodesGroupBox.Controls.Add(LegacyCodesNoRadioButton);
			this.UseLegacyCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 64, true);
			this.UseLegacyCodesGroupBox.Name = "UseLegacyCodesGroupBox";
			this.UseLegacyCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 48, true);
			this.UseLegacyCodesGroupBox.TabIndex = 4;
			this.UseLegacyCodesGroupBox.TabStop = false;
			// 
			// LegacyCodesYesRadioButton
			// 
			this.LegacyCodesYesRadioButton.AutoCheck = false;
			this.LegacyCodesYesRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|6b1ecae9-4741-4283-aee2-1bbd7af8c13f", "Yes");
			this.LegacyCodesYesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LegacyCodesYesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 23, true);
			this.LegacyCodesYesRadioButton.Name = "LegacyCodesYesRadioButton";
			this.LegacyCodesYesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.LegacyCodesYesRadioButton.TabIndex = 1;
			// 
			// LegacyCodesNoRadioButton
			// 
			this.LegacyCodesNoRadioButton.AutoCheck = false;
			this.LegacyCodesNoRadioButton.Checked = true;
			this.LegacyCodesNoRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|4a982563-e17a-4781-956c-0cf651d79ce0", "No");
			this.LegacyCodesNoRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LegacyCodesNoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 23, true);
			this.LegacyCodesNoRadioButton.Name = "LegacyCodesNoRadioButton";
			this.LegacyCodesNoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.LegacyCodesNoRadioButton.TabIndex = 0;
			this.LegacyCodesNoRadioButton.TabStop = true;
			// 
			// SetAuditStatusGroupBox
			//
			this.SetAuditStatusGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|f5bc9b7f-8f97-4467-b0ef-09a08096b818", "Set Product to Audited Status");
			this.SetAuditStatusGroupBox.Controls.Add(this.SetAuditStatusYesRadioButton);
			this.SetAuditStatusGroupBox.Controls.Add(this.SetAuditStatusNoRadioButton);
			this.SetAuditStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(566, 64, true);
			this.SetAuditStatusGroupBox.Name = "SetAuditStatusGroupBox";
			this.SetAuditStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 48, true);
			this.SetAuditStatusGroupBox.TabIndex = 5;
			this.SetAuditStatusGroupBox.TabStop = false;
			// 
			// SetAuditStatusYesRadioButton
			// 
			this.SetAuditStatusYesRadioButton.AutoCheck = false;
			this.SetAuditStatusYesRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|a3e150f9-a6ba-4f80-96f5-74654fb927bf", "Yes");
			this.SetAuditStatusYesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 23, true);
			this.SetAuditStatusYesRadioButton.Name = "SetAuditStatusYesRadioButton";
			this.SetAuditStatusYesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.SetAuditStatusYesRadioButton.TabIndex = 1;
			this.SetAuditStatusYesRadioButton.CheckedChanged += SetAuditStatusYesRadioButton_CheckedChanged;
			// 
			// SetAuditStatusNoRadioButton
			// 
			this.SetAuditStatusNoRadioButton.AutoCheck = false;
			this.SetAuditStatusNoRadioButton.Checked = true;
			this.SetAuditStatusNoRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|9e49dc52-7430-4e6c-9ca9-223286923583", "No");
			this.SetAuditStatusNoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 23, true);
			this.SetAuditStatusNoRadioButton.Name = "SetAuditStatusNoRadioButton";
			this.SetAuditStatusNoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.SetAuditStatusNoRadioButton.TabIndex = 0;
			this.SetAuditStatusNoRadioButton.TabStop = true;
			// 
			// ImportProductsFromCSVForm
			// 
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportProductsFromCSVForm|fd056f7b-fd8d-4a94-9fec-07a8c776fd49", "Import Product Data");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 551, true);
			this.Controls.Add(this.UseLegacyCodesGroupBox);
			this.Controls.Add(this.UpdateRecsThatExistGroupBox);
			this.Controls.Add(this.SetAuditStatusGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 580, true);
			this.Name = "ImportProductsFromCSVForm";
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			this.Controls.SetChildIndex(this.UpdateRecsThatExistGroupBox, 0);
			this.Controls.SetChildIndex(this.UseLegacyCodesGroupBox, 0);
			this.Controls.SetChildIndex(this.SetAuditStatusGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UpdateRecsThatExistGroupBox.ResumeLayout(false);
			this.UseLegacyCodesGroupBox.ResumeLayout(false);
			this.SetAuditStatusGroupBox.ResumeLayout(false);
			this.SetAuditStatusGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
