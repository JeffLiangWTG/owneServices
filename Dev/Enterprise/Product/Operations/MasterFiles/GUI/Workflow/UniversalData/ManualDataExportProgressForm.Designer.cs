namespace Enterprise.MasterFiles.GUI
{
	partial class ManualDataExportProgressForm
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
			//DisposeCore();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zChildTitle = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseLogo = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.notificationsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 256, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UniversalData.ManualDataExport);
			// 
			// closeButton
			// 
			this.closeButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ManualDataExportForm|b096f692-aa32-446f-b73d-814e70b5a2cb", "Close", "Close the form.");
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 229, true);
			this.closeButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.closeButton.TabIndex = 10;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer1.Panel1.Controls.Add(this.zChildTitle);
			this.splitContainer1.Panel1.Controls.Add(this.EnterpriseLogo);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.notificationsTextBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 223, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(39);
			this.splitContainer1.SplitterWidth = 1;
			this.splitContainer1.TabIndex = 8;
			// 
			// zChildTitle
			// 
			this.zChildTitle.Text = Enterprise.MasterFiles.GUI.Res.GetString("DA2FF143-99D5-4029-924D-3F17778C876C", "Send XML {0}", universalXMLTypeName);
			this.zChildTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 4, true);
			this.zChildTitle.Name = "zChildTitle";
			this.zChildTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 32, true);
			this.zChildTitle.TabIndex = 3;
			// 
			// EnterpriseLogo
			// 
			this.EnterpriseLogo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.EnterpriseLogo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.EnterpriseLogo.Name = "EnterpriseLogo";
			this.EnterpriseLogo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.EnterpriseLogo.TabIndex = 2;
			this.EnterpriseLogo.TabStop = false;
			// 
			// notificationsTextBox
			// 
			this.notificationsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e21c84d5-c063-48a9-ac12-cc93e7b5ac2c", "Notifications");
			this.notificationsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.notificationsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 0, true);
			this.notificationsTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.notificationsTextBox.Multiline = true;
			this.notificationsTextBox.Name = "notificationsTextBox";
			this.notificationsTextBox.ReadOnly = true;
			this.notificationsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.notificationsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 181, true);
			this.notificationsTextBox.TabIndex = 8;
			// 
			// sendButton
			// 
			this.sendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.sendButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("77938439-D89C-4A42-9867-8D6AFB05FFD3", "Send");
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 229, true);
			this.sendButton.Name = "sendButton";
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.sendButton.TabIndex = 9;
			this.sendButton.UseVisualStyleBackColor = true;
			// 
			// ManualDataExportProgressProgressForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 280, true);
			this.Controls.Add(this.sendButton);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.closeButton);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UniversalData.ManualDataExport);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ManualDataExportProgressForm";			
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.sendButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.GUI.ZButton closeButton;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private Enterprise.ZArchitecture.GUI.ZPictureBox EnterpriseLogo;
		private ZArchitecture.ZTextBox notificationsTextBox;
		private ZArchitecture.ZLabel zChildTitle;
		private ZArchitecture.GUI.ZButton sendButton;

	}
}
