namespace Enterprise.Customs.GUI
{
	partial class InvoiceImporterForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Enterprise.ZArchitecture.GUI.ZCheckBox SkipUnkSupplierCheckBox;

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.SkipUnkSupplierCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LogGroupBox.SuspendLayout();
			this.RowStatisticsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 32, true);
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			// 
			// LogGroupBox
			// 
			this.LogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.LogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 280, true);
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 20, true);
			// 
			// RowsProcessedTitle
			// 
			this.RowsProcessedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.RowsProcessedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			// 
			// RowsProcessedLabel
			// 
			this.RowsProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 8, true);
			this.RowsProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			// 
			// RowsExcludedLabel
			// 
			this.RowsExcludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 56, true);
			this.RowsExcludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			// 
			// ProcessButton
			// 
			this.ProcessButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.ProcessButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(645, 32, true);
			this.ProcessButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			// 
			// ProgressBar
			// 
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 16, true);
			// 
			// RowsIncludedLabel
			// 
			this.RowsIncludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 32, true);
			this.RowsIncludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			// 
			// RowsIncludedTitle
			// 
			this.RowsIncludedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.RowsIncludedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			// 
			// RowStatisticsPanel
			// 
			this.RowStatisticsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 261, true);
			// 
			// LogListBox
			// 
			this.LogListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 251, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 392, true);
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 60, true);
			this.ProgressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			// 
			// RowsExcludedTitle
			// 
			this.RowsExcludedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.RowsExcludedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 419, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 26, true);
			// 
			// SkipUnkSupplierCheckBox
			// 
			this.SkipUnkSupplierCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SkipUnkSupplierCheckBox.AutoSize = true;
			this.SkipUnkSupplierCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceImporterForm|6262c983-c22a-469b-b604-48bcd118d192", "Skip Supplier with unknown code");
			this.SkipUnkSupplierCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SkipUnkSupplierCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 398, true);
			this.SkipUnkSupplierCheckBox.Name = "SkipUnkSupplierCheckBox";
			this.SkipUnkSupplierCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SkipUnkSupplierCheckBox.TabIndex = 6;
			// 
			// InvoiceImporterForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 445, true);
			this.ControlBox = true;
			this.Controls.Add(this.SkipUnkSupplierCheckBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 421, true);
			this.Name = "InvoiceImporterForm";
			this.Controls.SetChildIndex(this.ProcessButton, 0);
			this.Controls.SetChildIndex(this.ProgressLabel, 0);
			this.Controls.SetChildIndex(this.SkipUnkSupplierCheckBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.BrowseButton, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.LogGroupBox, 0);
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.LogGroupBox.ResumeLayout(false);
			this.RowStatisticsPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
