namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class CREReplaceForm
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

		public ZArchitecture.ZTextBox ChangeCancelReasonTextBox;

		#region Windows Form Designer generated code


		new void InitializeComponent()
		{
			this.ChangeCancelReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalInformationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 175, true);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 175, true);
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.Controls.Add(this.ChangeCancelReasonTextBox);
			this.AdditionalInformationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("B976578F-4113-482B-A8D4-7FDE9493F387", "Change Reason");
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 12, true);
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 157, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 204, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 24, true);
			// 
			// ChangeCancelReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeCancelReasonTextBox, "AM_AdditionalStatementText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_AdditionalStatementText)));
			this.ChangeCancelReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ChangeCancelReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ChangeCancelReasonTextBox.Multiline = true;
			this.ChangeCancelReasonTextBox.Name = "ChangeCancelReasonTextBox";
			this.ChangeCancelReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ChangeCancelReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 135, true);
			this.ChangeCancelReasonTextBox.TabIndex = 0;
			// 
			// CREReplaceForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 228, true);
			this.Name = "CREReplaceForm";
			this.AdditionalInformationGroupBox.ResumeLayout(false);
			this.AdditionalInformationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
