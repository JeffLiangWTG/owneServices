namespace Enterprise.Customs.NL.GUI
{
	partial class CheckGuaranteeAmountForm
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
			this.PayerIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
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
			this.BindingSource.SetBindingMember(this.PayerIDTextBox, "PaymentPartyEORINumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).PaymentPartyEORINumber)));
			this.PayerIDTextBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("826B5274-2461-42F7-B9D7-900174CC54DF", "Payer ID");
			this.PayerIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 13, true);
			this.PayerIDTextBox.Name = "PayerIDTextBox";
			this.PayerIDTextBox.ReadOnly = true;
			this.PayerIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PayerIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.PayerIDTextBox.TabIndex = 3;
			this.PayerIDTextBox.TabStop = false;
			// 
			// ButtonSubmit
			// 
			this.ButtonSubmit.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("11C71FA5-724B-4628-B6D0-3B5DB505A11A", "Submit to Customs");
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
			this.ButtonCancel.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("16D95671-7147-4851-A026-960503C33FBD", "Cancel");
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
			// CheckGuaranteeAmountForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("F595915C-8038-44CA-81FB-D390C3D77AE5", "Customs Guarantee Check");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 144, true);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonSubmit);
			this.Controls.Add(this.PayerIDTextBox);
			this.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 179, true);
			this.Name = "CheckGuaranteeAmountForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PayerIDTextBox, 0);
			this.Controls.SetChildIndex(this.ButtonSubmit, 0);
			this.Controls.SetChildIndex(this.ButtonCancel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox PayerIDTextBox;
		private ZArchitecture.GUI.ZButton ButtonSubmit;
		private ZArchitecture.GUI.ZButton ButtonCancel;
	}
}
