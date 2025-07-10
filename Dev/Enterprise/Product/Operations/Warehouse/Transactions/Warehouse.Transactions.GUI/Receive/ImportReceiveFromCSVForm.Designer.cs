namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class ImportReceiveFromCSVForm
	{
		private ZArchitecture.GUI.ZCheckBox DeliveranceCheckBox;

		new void InitializeComponent()
		{
			this.DeliveranceCheckBox = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SelectFileButton
			// 
			this.SelectFileButton.TabIndex = 2;
			// 
			// FileNameTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FileNameTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FileNameTextBox.TabIndex = 1;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 483, true);
			this.ProgressBar.TabIndex = 6;
			// 
			// StartButton
			// 
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 483, true);
			this.StartButton.TabIndex = 7;
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 68, true);
			this.label2.TabIndex = 4;
			// 
			// OutputListBox
			// 
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 355, true);
			this.OutputListBox.TabIndex = 5;
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 522, true);
			this.CloseButton.ReadOnly = false;
			this.CloseButton.TabIndex = 10;
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 521, true);
			this.CopyLogToClipboardButton.ReadOnly = false;
			this.CopyLogToClipboardButton.TabIndex = 9;
			// 
			// DownloadTemplateButton
			// 
			this.DownloadTemplateButton.TabIndex = 3;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 518, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// DeliveranceCheckBox
			// 
			this.DeliveranceCheckBox.AutoSize = true;
			this.DeliveranceCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ImportReceiveFromCSVForm|185e4ea4-ad2f-4903-a25a-f87fc3bb66fe", "Deliverance Conversion");
			this.DeliveranceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeliveranceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 11, true);
			this.DeliveranceCheckBox.Name = "DeliveranceCheckBox";
			this.DeliveranceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 17, true);
			this.DeliveranceCheckBox.TabIndex = 0;
			this.DeliveranceCheckBox.UseVisualStyleBackColor = true;
			// 
			// ImportReceiveFromCSVForm
			// 

			this.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ImportReceiveFromCSVForm|fd056f7b-fd8d-4a94-9fec-07a8c776fd49", "Import Warehouse Receive Data");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 546, true);
			this.Controls.Add(this.DeliveranceCheckBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 252, true);
			this.Name = "ImportReceiveFromCSVForm";
			this.Controls.SetChildIndex(this.DownloadTemplateButton, 0);
			this.Controls.SetChildIndex(this.DeliveranceCheckBox, 0);
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
