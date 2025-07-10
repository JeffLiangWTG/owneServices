namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class TSWReplaceForm
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
		new void InitializeComponent()
		{
			this.ChangeCancelReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReplaceReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AdditionalInformationGroupBox.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 537, true);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 537, true);
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.Controls.Add(this.ReplaceReasonLabel);
			this.AdditionalInformationGroupBox.Controls.Add(this.ChangeCancelReasonTextBox);
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 356, true);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.ChangeCancelReasonTextBox, 0);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.FreeTextTextBox, 0);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.ManualProcessingTextBox, 0);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.ManualProcessingLabel, 0);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.ReplaceReasonLabel, 0);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			// 
			// ManualProcessingTextBox
			// 
			this.ManualProcessingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 143, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 568, true);
			// 
			// ChangeCancelReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeCancelReasonTextBox, "AM_AdditionalStatementText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_AdditionalStatementText)));
			this.ChangeCancelReasonTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("353f52a4-a61d-486a-868d-61bdc31cd2a3", "", "Must be present to advise the reason for change on all Adjustment transactions");
			this.ChangeCancelReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ChangeCancelReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 261, true);
			this.ChangeCancelReasonTextBox.Multiline = true;
			this.ChangeCancelReasonTextBox.Name = "ChangeCancelReasonTextBox";
			this.ChangeCancelReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ChangeCancelReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 91, true);
			this.ChangeCancelReasonTextBox.TabIndex = 7;
			// 
			// ReplaceReasonLabel
			// 
			this.ReplaceReasonLabel.AutoSize = true;
			this.ReplaceReasonLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("534C4518-5D9E-4D05-A652-2423D1FE97AD", "Replace Reason");
			this.ReplaceReasonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ReplaceReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 245, true);
			this.ReplaceReasonLabel.Name = "ReplaceReasonLabel";
			this.ReplaceReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.ReplaceReasonLabel.TabIndex = 8;
			// 
			// TSWReplaceForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 592, true);
			this.Name = "TSWReplaceForm";
			this.Text = "TSWReplaceForm";
			this.AdditionalInformationGroupBox.ResumeLayout(false);
			this.AdditionalInformationGroupBox.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZTextBox ChangeCancelReasonTextBox;
		public ZArchitecture.ZLabel ReplaceReasonLabel;
	}
}
