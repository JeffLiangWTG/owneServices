namespace Enterprise.Customs.GUI
{
	partial class ProductClassificationCreationConfirmationForm
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
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.QuitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InformationLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 156, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfirmButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 149, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 23, true);
			this.ConfirmButton.TabIndex = 6;
			this.ConfirmButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D3A01A30-082A-4C4E-9125-A8A9FDC008EC", "Confirm");
			this.ConfirmButton.ToolTipCaption = null;
			this.ConfirmButton.UseVisualStyleBackColor = true;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// QuitButton
			// 
			this.QuitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.QuitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.QuitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 149, true);
			this.QuitButton.Name = "QuitButton";
			this.QuitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.QuitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 23, true);
			this.QuitButton.TabIndex = 7;
			this.QuitButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("42C47E59-317D-40B6-8FAA-BA762170FC53", "Ignore");
			this.QuitButton.ToolTipCaption = null;
			this.QuitButton.UseVisualStyleBackColor = true;
			this.QuitButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// InformationLabel
			// 
			this.InformationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InformationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 14, true);
			this.InformationLabel.Name = "InformationLabel";
			this.InformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 107, true);
			this.InformationLabel.TabIndex = 1;
			this.InformationLabel.Text = "If you are seeing this, something went wrong.";
			this.InformationLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// ProductClassificationCreationConfirmationForm
			// 
			this.AcceptButton = this.ConfirmButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.QuitButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 180, true);
			this.Controls.Add(this.InformationLabel);
			this.Controls.Add(this.QuitButton);
			this.Controls.Add(this.ConfirmButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 180, true);
			this.Name = "ProductClassificationCreationConfirmationForm";
			this.Text = "ProductClassificationCreationConfirmationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ConfirmButton, 0);
			this.Controls.SetChildIndex(this.QuitButton, 0);
			this.Controls.SetChildIndex(this.InformationLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZButton ConfirmButton;
		private ZArchitecture.GUI.ZButton QuitButton;
		private ZArchitecture.ZLabel InformationLabel;
	}
}
