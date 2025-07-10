namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportFromCSVForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.SelectFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StartButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.label2 = new Enterprise.ZArchitecture.ZLabel();
			this.OutputListBox = new CargoWise.Windows.UI.KListBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyLogToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownloadTemplateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			// 
			// SelectFileButton
			// 
			this.SelectFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectFileButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportFromCSVForm|c711ef01-6fdf-43a6-9566-0a402b315347", "Browse...");
			this.SelectFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 35, true);
			this.SelectFileButton.Name = "SelectFileButton";
			this.SelectFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.SelectFileButton.TabIndex = 1;
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FileNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportFromCSVForm|bdc9779b-2f01-43d4-9062-52d3ac8a7716", "Location of the Data File");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FileNameTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 36, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 20, true);
			this.FileNameTextBox.TabIndex = 0;
			// 
			// StartButton
			// 
			this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.StartButton.BackColor = System.Drawing.Color.MediumSeaGreen;
			this.StartButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportFromCSVForm|9281f1fe-b7a0-414f-b78d-bbae0f30bffb", "Start Import");
			this.StartButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 488, true);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.StartButton.TabIndex = 6;
			this.StartButton.UseVisualStyleBackColor = false;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 488, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 20, true);
			this.ProgressBar.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportFromCSVForm|e7521883-85f3-4e43-b144-e7ce7a9cdcb1", "Import Log:");
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 64, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 16, true);
			this.label2.TabIndex = 3;
			// 
			// OutputListBox
			// 
			this.OutputListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputListBox.Font = new System.Drawing.Font(Enterprise.ZArchitecture.Core.OFont.NormalFontName, 8F);
			this.OutputListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 84, true);
			this.OutputListBox.Name = "OutputListBox";
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 394, true);
			this.OutputListBox.TabIndex = 4;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportFromCSVForm|a6944bec-222e-43b1-9ea7-665fed6e86b3", "Close");
			this.CloseButton.Enabled = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 527, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Visible = false;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyLogToClipboardButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportFromCSVForm|5fe2f134-eae3-4a33-a5bc-22c473eaa509", "Copy Log to Clipboard");
			this.CopyLogToClipboardButton.Enabled = false;
			this.CopyLogToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 526, true);
			this.CopyLogToClipboardButton.Name = "CopyLogToClipboardButton";
			this.CopyLogToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.CopyLogToClipboardButton.TabIndex = 8;
			this.CopyLogToClipboardButton.Visible = false;
			this.CopyLogToClipboardButton.Click += new System.EventHandler(this.CopyLogToClipboardButton_Click);
			// 
			// DownloadTemplateButton
			// 
			this.DownloadTemplateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DownloadTemplateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5890d9cb-1eff-4551-9d05-fa3c98778277", "Download Template");
			this.DownloadTemplateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 35, true);
			this.DownloadTemplateButton.Name = "DownloadTemplateButton";
			this.DownloadTemplateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 22, true);
			this.DownloadTemplateButton.TabIndex = 2;
			this.DownloadTemplateButton.Click += new System.EventHandler(this.DownloadTemplateButton_Click);
			// 
			// ImportFromCSVForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportFromCSVForm|fd056f7b-fd8d-4a94-9fec-07a8c776fd49", "Import CSV Data");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 551, true);
			this.Controls.Add(this.DownloadTemplateButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.CopyLogToClipboardButton);
			this.Controls.Add(this.OutputListBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.StartButton);
			this.Controls.Add(this.SelectFileButton);
			this.Controls.Add(this.FileNameTextBox);
			this.Controls.Add(this.ProgressBar);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 580, true);
			this.Name = "ImportFromCSVForm";
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.DownloadTemplateButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton SelectFileButton;
		internal protected Enterprise.ZArchitecture.ZTextBox FileNameTextBox;
		internal protected CargoWise.Windows.UI.KProgressBar ProgressBar;
		internal protected Enterprise.ZArchitecture.GUI.ZButton StartButton;
		protected Enterprise.ZArchitecture.ZLabel label2;
		internal protected CargoWise.Windows.UI.KListBox OutputListBox;
		internal protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal protected Enterprise.ZArchitecture.GUI.ZButton CopyLogToClipboardButton;
		internal protected Enterprise.ZArchitecture.GUI.ZButton DownloadTemplateButton;
	}
}
