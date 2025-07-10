namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class TSWCancelForm
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

		internal ZArchitecture.ZTextBox ChangeCancelReasonTextBox;

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.ChangeCancelReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
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
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 346, true);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 346, true);
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.Controls.Add(this.ChangeCancelReasonTextBox);
			this.AdditionalInformationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8AFE08E7-2B94-48FD-91B2-609997EC94D9", "Cancel Reason");
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 175, true);
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 157, true);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.ChangeCancelReasonTextBox, 0);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.FreeTextTextBox, 0);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.ManualProcessingTextBox, 0);
			this.AdditionalInformationGroupBox.Controls.SetChildIndex(this.ManualProcessingLabel, 0);
			// 
			// FreeTextTextBox
			// 
			this.FreeTextTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FreeTextTextBox.Dock = System.Windows.Forms.DockStyle.None;
			this.FreeTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 161, true);
			this.FreeTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 125, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 142, true);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 136, true);
			// 
			// zGrid1
			// 
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 120, true);
			// 
			// ManualProcessingLabel
			// 
			this.ManualProcessingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 146, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 375, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 24, true);
			// 
			// ChangeCancelReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeCancelReasonTextBox, "AM_AdditionalStatementText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_AdditionalStatementText)));
			this.ChangeCancelReasonTextBox.CaptionResourceString = null;
			this.ChangeCancelReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ChangeCancelReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 16, true);
			this.ChangeCancelReasonTextBox.Multiline = true;
			this.ChangeCancelReasonTextBox.Name = "ChangeCancelReasonTextBox";
			this.ChangeCancelReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ChangeCancelReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 128, true);
			this.ChangeCancelReasonTextBox.TabIndex = 0;
			// 
			// TSWCancelForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 399, true);
			this.Name = "TSWCancelForm";
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
	}
}
