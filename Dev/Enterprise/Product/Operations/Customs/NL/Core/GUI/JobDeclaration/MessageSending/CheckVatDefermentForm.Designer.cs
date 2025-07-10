
namespace Enterprise.Customs.NL.GUI
{
	partial class CheckVatDefermentForm
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
			this.PaymentPartyEoriNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VATPartyTaxNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ButtonSubmit = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// PaymentPartyEoriNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PaymentPartyEoriNumberTextBox, "PaymentPartyEORINumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).PaymentPartyEORINumber)));
			this.PaymentPartyEoriNumberTextBox.CaptionResourceString = null;
			this.PaymentPartyEoriNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 13, true);
			this.PaymentPartyEoriNumberTextBox.Name = "PaymentPartyEoriNumberTextBox";
			this.PaymentPartyEoriNumberTextBox.ReadOnly = true;
			this.PaymentPartyEoriNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PaymentPartyEoriNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.PaymentPartyEoriNumberTextBox.TabIndex = 3;
			this.PaymentPartyEoriNumberTextBox.TabStop = false;
			// 
			// VATPartyTaxNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VATPartyTaxNumberTextBox, "VATPartyTaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).VATPartyTaxNumber)));
			this.VATPartyTaxNumberTextBox.CaptionResourceString = null;
			this.VATPartyTaxNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 55, true);
			this.VATPartyTaxNumberTextBox.Name = "VATPartyTaxNumberTextBox";
			this.VATPartyTaxNumberTextBox.ReadOnly = true;
			this.VATPartyTaxNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.VATPartyTaxNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.VATPartyTaxNumberTextBox.TabIndex = 4;
			this.VATPartyTaxNumberTextBox.TabStop = false;
			// 
			// ButtonSubmit
			// 
			this.ButtonSubmit.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("1cab0ac7-c973-47ac-a85a-8251d4e83a7c", "Submit to Customs");
			this.ButtonSubmit.IsCaptionOverridden = false;
			this.ButtonSubmit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 91, true);
			this.ButtonSubmit.Name = "ButtonSubmit";
			this.ButtonSubmit.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSubmit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.ButtonSubmit.TabIndex = 1;
			this.ButtonSubmit.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonSubmit.ToolTipCaption = null;
			this.ButtonSubmit.UseVisualStyleBackColor = true;
			this.ButtonSubmit.Click += new System.EventHandler(this.SubmitButton_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("e0900a08-fa37-477f-bda3-b2ca236af2ee", "Cancel");
			this.ButtonCancel.IsCaptionOverridden = false;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 91, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonCancel.TabIndex = 2;
			this.ButtonCancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// CheckVatDefermentForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("a3cbfe4b-e9f9-489f-8b70-29381fbd50a5", "VAT Deferment (article 23 license)");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 144, true);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonSubmit);
			this.Controls.Add(this.VATPartyTaxNumberTextBox);
			this.Controls.Add(this.PaymentPartyEoriNumberTextBox);
			this.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 179, true);
			this.Name = "CheckVatDefermentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PaymentPartyEoriNumberTextBox, 0);
			this.Controls.SetChildIndex(this.VATPartyTaxNumberTextBox, 0);
			this.Controls.SetChildIndex(this.ButtonSubmit, 0);
			this.Controls.SetChildIndex(this.ButtonCancel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox PaymentPartyEoriNumberTextBox;
		private ZArchitecture.ZTextBox VATPartyTaxNumberTextBox;
		private ZArchitecture.GUI.ZButton ButtonSubmit;
		private ZArchitecture.GUI.ZButton ButtonCancel;
	}
}
