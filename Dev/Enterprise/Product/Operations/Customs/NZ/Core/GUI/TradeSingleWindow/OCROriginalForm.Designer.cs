namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class OCROriginalForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.FreeTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManualProcessingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManualProcessingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalInformationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManualProcessingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 215, true);
			this.OKButton.TabIndex = 2;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 215, true);
			this.Cancel_Button.TabIndex = 3;
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.Controls.Add(this.FreeTextTextBox);
			this.AdditionalInformationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7CFE38E4-76C7-4A9C-998F-EB0BCA543EEE", "Additional Information");
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 101, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 242, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// FreeTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.FreeTextTextBox, "AM_FreeText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_FreeText)));
			this.FreeTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FreeTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FreeTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FreeTextTextBox.Multiline = true;
			this.FreeTextTextBox.Name = "FreeTextTextBox";
			this.FreeTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.FreeTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 82, true);
			this.FreeTextTextBox.TabIndex = 0;
			// 
			// ManualProcessingGroupBox
			// 
			this.ManualProcessingGroupBox.Controls.Add(this.ManualProcessingTextBox);
			this.ManualProcessingGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("97FAE0E8-2838-4697-A83F-6C255BB7E574", "Manual Processing Request Reason: Enter to Request Manual Processing");
			this.ManualProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 101, true);
			this.ManualProcessingGroupBox.Name = "ManualProcessingGroupBox";
			this.ManualProcessingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 101, true);
			this.ManualProcessingGroupBox.TabIndex = 1;
			this.ManualProcessingGroupBox.TabStop = false;
			// 
			// ManualProcessingTextBox
			// 
			this.BindingSource.SetBindingMember(this.ManualProcessingTextBox, "AM_OverrideText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_OverrideText)));
			this.ManualProcessingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ManualProcessingTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManualProcessingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ManualProcessingTextBox.Multiline = true;
			this.ManualProcessingTextBox.Name = "ManualProcessingTextBox";
			this.ManualProcessingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ManualProcessingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 82, true);
			this.ManualProcessingTextBox.TabIndex = 0;
			// 
			// OCROriginalForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 266, true);
			this.Controls.Add(this.ManualProcessingGroupBox);
			this.Name = "OCROriginalForm";
			this.Controls.SetChildIndex(this.AdditionalInformationGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ManualProcessingGroupBox, 0);
			this.AdditionalInformationGroupBox.ResumeLayout(false);
			this.AdditionalInformationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManualProcessingGroupBox.ResumeLayout(false);
			this.ManualProcessingGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion


		internal Enterprise.ZArchitecture.ZTextBox FreeTextTextBox;
		protected ZArchitecture.GUI.ZGroupBox ManualProcessingGroupBox;
		internal ZArchitecture.ZTextBox ManualProcessingTextBox;
		
	}
}
