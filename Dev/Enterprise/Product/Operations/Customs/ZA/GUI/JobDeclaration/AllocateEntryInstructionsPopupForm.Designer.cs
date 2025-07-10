namespace Enterprise.Customs.ZA.GUI
{
	partial class AllocateEntryInstructionsPopupForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
            this.AlreadyLinkedConfirmationCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.IgnoreRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.OverwriteRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.InformationLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 237, true);
            this.MainStatusBar.ShowPanels = false;
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 24, true);
            this.MainStatusBar.Visible = false;
            // 
            // AlreadyLinkedConfirmationCaptionLabel
            // 
            this.AlreadyLinkedConfirmationCaptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AlreadyLinkedConfirmationCaptionLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("EFEC5CFE-CF39-40C0-A0AE-110774DE0685", "Some Invoice Lines are already linked to an Entry Instruction");
            this.AlreadyLinkedConfirmationCaptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.AlreadyLinkedConfirmationCaptionLabel.IsFontBold = true;
            this.AlreadyLinkedConfirmationCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 125, true);
            this.AlreadyLinkedConfirmationCaptionLabel.Name = "AlreadyLinkedConfirmationCaptionLabel";
            this.AlreadyLinkedConfirmationCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 23, true);
            this.AlreadyLinkedConfirmationCaptionLabel.TabIndex = 0;
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("260A31A3-5BFC-4341-B486-54CBD79FA2E3", "Cancel");
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 206, true);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.ToolTipCaption = null;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("B72CC96A-C9D1-4E6E-A0DF-CC8739124842", "OK");
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Enabled = false;
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 206, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.OKButton.TabIndex = 3;
            this.OKButton.ToolTipCaption = null;
            // 
            // IgnoreRadioButton
            // 
            this.IgnoreRadioButton.AutoCheck = false;
            this.IgnoreRadioButton.AutoSize = true;
            this.IgnoreRadioButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("89C790E0-7B1B-4D19-B7DF-904820642B6C", "Ignore lines already linked to Entry Instructions");
            this.IgnoreRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 158, true);
            this.IgnoreRadioButton.Name = "IgnoreRadioButton";
            this.IgnoreRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 16, true);
            this.IgnoreRadioButton.TabIndex = 1;
            this.IgnoreRadioButton.TabStop = true;
            this.IgnoreRadioButton.UseVisualStyleBackColor = true;
            this.IgnoreRadioButton.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // OverwriteRadioButton
            // 
            this.OverwriteRadioButton.AutoCheck = false;
            this.OverwriteRadioButton.AutoSize = true;
            this.OverwriteRadioButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("B5FBC92D-E1C4-4012-9F35-CE28966A6ED1", "Overwrite lines already linked to Entry Instructions");
            this.OverwriteRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 180, true);
            this.OverwriteRadioButton.Name = "OverwriteRadioButton";
            this.OverwriteRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 16, true);
            this.OverwriteRadioButton.TabIndex = 2;
            this.OverwriteRadioButton.TabStop = true;
            this.OverwriteRadioButton.UseVisualStyleBackColor = true;
            this.OverwriteRadioButton.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // InformationLabel
            // 
            this.InformationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InformationLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("feb00559-4c6f-4f6b-9370-d4f080c4eece", "This process will allocate invoice lines to Entry Instructions based on whether the invoice lines are dutiable or not. Dutiable lines are invoice lines that have a Standard Duty rate. Dutiable lines will be allocated to the first Entry Instruction that has CPC code of 40 (Clearance of goods in a customs warehouse under the \'Warehousing\' procedure) all other goods will be allocated to the first Entry Instruction that has a CPC of 11 (Clearance of goods for Home Use, and free circulation). If there are no Entry Instructions with the necessary CPC code, an Entry Instruction will be automatically created with the relevant CPC");
            this.InformationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.InformationLabel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.InformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
            this.InformationLabel.Name = "InformationLabel";
            this.InformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 116, true);
            this.InformationLabel.TabIndex = 5;
            this.InformationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AllocateEntryInstructionsPopupForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 261, true);
            this.ControlBox = false;
            this.Controls.Add(this.InformationLabel);
            this.Controls.Add(this.OverwriteRadioButton);
            this.Controls.Add(this.IgnoreRadioButton);
            this.Controls.Add(this.AlreadyLinkedConfirmationCaptionLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.Name = "AllocateEntryInstructionsPopupForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.OKButton, 0);
            this.Controls.SetChildIndex(this.CancelButton, 0);
            this.Controls.SetChildIndex(this.AlreadyLinkedConfirmationCaptionLabel, 0);
            this.Controls.SetChildIndex(this.IgnoreRadioButton, 0);
            this.Controls.SetChildIndex(this.OverwriteRadioButton, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.InformationLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel AlreadyLinkedConfirmationCaptionLabel;
		internal new ZArchitecture.GUI.ZButton CancelButton;
		internal ZArchitecture.GUI.ZButton OKButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton IgnoreRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton OverwriteRadioButton;
		internal ZArchitecture.ZLabel InformationLabel;
	}
}
