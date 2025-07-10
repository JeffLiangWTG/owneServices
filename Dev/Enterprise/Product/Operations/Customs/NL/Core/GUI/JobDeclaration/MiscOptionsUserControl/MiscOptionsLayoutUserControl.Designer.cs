namespace Enterprise.Customs.NL.GUI
{
	partial class MiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PaymentPartyEORINumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VATPartyTaxNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupportingInformationUserControl = new Enterprise.Customs.NL.GUI.SupportingInformationControl();
			this.PaymentSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.CustomsAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingInformationUserControl.SuspendLayout();
			this.PaymentSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// PaymentPartyEORINumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PaymentPartyEORINumberTextBox, "PaymentPartyEORINumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).PaymentPartyEORINumber)));
			this.PaymentPartyEORINumberTextBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("NLMiscOptionsUserControl|57137487-CB1F-4924-9300-804B4DE30191", "Approval Defer No.");
			this.PaymentPartyEORINumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 12, true);
			this.PaymentPartyEORINumberTextBox.Name = "PaymentPartyEORINumberTextBox";
			this.PaymentPartyEORINumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PaymentPartyEORINumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.PaymentPartyEORINumberTextBox.TabIndex = 24;
			// 
			// VATPartyTaxNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VATPartyTaxNumberTextBox, "VATPartyTaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).VATPartyTaxNumber)));
			this.VATPartyTaxNumberTextBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("NLMiscOptionsUserControl|48D83460-CA40-495B-AF1E-11695DD2E3BC", "Approval VAT No.");
			this.VATPartyTaxNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 38, true);
			this.VATPartyTaxNumberTextBox.Name = "VATPartyTaxNumberTextBox";
			this.VATPartyTaxNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.VATPartyTaxNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.VATPartyTaxNumberTextBox.TabIndex = 25;
			// 
			// SupportingInformationUserControl
			// 
			this.SupportingInformationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingInformationUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)))));
			this.SupportingInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 111, true);
			this.SupportingInformationUserControl.Name = "SupportingInformationUserControl";
			this.SupportingInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 394, true);
			this.SupportingInformationUserControl.TabIndex = 26;
			// 
			// PaymentSeparatorUserControl
			// 
			this.PaymentSeparatorUserControl.AllowDrop = true;
			this.PaymentSeparatorUserControl.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("33C7FFCD-F77B-4B52-8E2C-4B068B44F055", "Payment");
			this.PaymentSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 90, true);
			this.PaymentSeparatorUserControl.Name = "PaymentSeparatorUserControl";
			this.PaymentSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.PaymentSeparatorUserControl.TabIndex = 23;
			// 
			// CustomsAccountTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsAccountTextBox, "CustomsAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsAccount)));
			this.CustomsAccountTextBox.CaptionResourceString = null;
			this.CustomsAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 64, true);
			this.CustomsAccountTextBox.Name = "CustomsAccountTextBox";
			this.CustomsAccountTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CustomsAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.CustomsAccountTextBox.TabIndex = 22;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsAccountTextBox);
			this.Controls.Add(this.VATPartyTaxNumberTextBox);
			this.Controls.Add(this.PaymentPartyEORINumberTextBox);
			this.Controls.Add(this.PaymentSeparatorUserControl);
			this.Controls.Add(this.SupportingInformationUserControl);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 537, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingInformationUserControl.ResumeLayout(true);
			this.SupportingInformationUserControl.PerformLayout();
			this.PaymentSeparatorUserControl.ResumeLayout(true);
			this.PaymentSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.SeparatorUserControl PaymentSeparatorUserControl;
		internal Enterprise.ZArchitecture.ZTextBox PaymentPartyEORINumberTextBox;
		internal Enterprise.ZArchitecture.ZTextBox VATPartyTaxNumberTextBox;
		internal SupportingInformationControl SupportingInformationUserControl;
		internal ZArchitecture.ZTextBox CustomsAccountTextBox;
	}
}
