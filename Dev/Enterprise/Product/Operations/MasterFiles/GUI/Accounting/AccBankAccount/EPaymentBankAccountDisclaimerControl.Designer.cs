
namespace Enterprise.MasterFiles.GUI
{
	partial class EPaymentBankAccountDisclaimerControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DisclaimerMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LearnMoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DisclaimerMessageLabel
			// 
			this.DisclaimerMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DisclaimerMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 13, true);
			this.DisclaimerMessageLabel.Name = "DisclaimerMessageLabel";
			this.DisclaimerMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 171, true);
			this.DisclaimerMessageLabel.TabIndex = 0;
			this.DisclaimerMessageLabel.Text = "zLabel1";
			// 
			// LearnMoreButton
			// 
			this.LearnMoreButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("83acf3ba-61d6-4587-b1af-051cfa06ceaf", "Learn More");
			this.LearnMoreButton.IsCaptionOverridden = false;
			this.LearnMoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 198, true);
			this.LearnMoreButton.Name = "LearnMoreButton";
			this.LearnMoreButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LearnMoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.LearnMoreButton.TabIndex = 1;
			this.LearnMoreButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LearnMoreButton.ToolTipCaption = null;
			this.LearnMoreButton.UseVisualStyleBackColor = true;
			this.LearnMoreButton.Click += new System.EventHandler(this.LearnMoreButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a0df4ede-c690-4c04-aaee-860ef36a962c", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 198, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// EPaymentBankAccountDisclaimerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.LearnMoreButton);
			this.Controls.Add(this.DisclaimerMessageLabel);
			this.Name = "EPaymentBankAccountDisclaimerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel DisclaimerMessageLabel;
		private ZArchitecture.GUI.ZButton LearnMoreButton;
		private ZArchitecture.GUI.ZButton OKButton;
	}
}
