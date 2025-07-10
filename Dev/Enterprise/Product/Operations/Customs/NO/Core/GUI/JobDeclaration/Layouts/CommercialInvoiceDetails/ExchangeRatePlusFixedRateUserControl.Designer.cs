namespace Enterprise.Customs.NO.GUI
{
	partial class ExchangeRatePlusFixedRateUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InvoiceCurrExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FixedCurrencyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobComInvoiceHeader);
			// 
			// InvoiceCurrExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCurrExRateCalcEdit, "JZ_InvoiceCurrExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.JobComInvoiceHeader)(null)).JZ_InvoiceCurrExRate)));
			this.InvoiceCurrExRateCalcEdit.CaptionResourceString = null;
			this.InvoiceCurrExRateCalcEdit.DecimalPlaces = 2;
			this.InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceCurrExRateCalcEdit.Name = "InvoiceCurrExRateCalcEdit";
			this.InvoiceCurrExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.InvoiceCurrExRateCalcEdit.TabIndex = 3;
			this.InvoiceCurrExRateCalcEdit.Text = "0.000000";
			this.InvoiceCurrExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InvoiceCurrExRateCalcEdit.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("29776EE7-B077-4E62-9A32-7B8FBBEF158C", "Exchange Rate");
			// 
			// FixedCurrencyCheckBox
			// 
			this.FixedCurrencyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FixedCurrencyCheckBox, "IsJZ_InvoiceCurrExRateUserEnterable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NO.Business.JobComInvoiceHeader)(null)).IsJZ_InvoiceCurrExRateUserEnterable)));
			this.FixedCurrencyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FixedCurrencyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 3, true);
			this.FixedCurrencyCheckBox.Name = "FixedCurrencyCheckBox";
			this.FixedCurrencyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 14, true);
			this.FixedCurrencyCheckBox.TabIndex = 8;
			this.FixedCurrencyCheckBox.UseVisualStyleBackColor = true;
			this.FixedCurrencyCheckBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("D611EDF5-8963-4B5A-AC06-C0BFFC199E0F", "Fixed rate?");
			// 
			// ExchangeRatePlusFixedRateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceCurrExRateCalcEdit);
			this.Controls.Add(this.FixedCurrencyCheckBox);
			this.Name = "ExchangeRatePlusFixedRateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceCurrExRateCalcEdit.ResumeLayout();
			this.InvoiceCurrExRateCalcEdit.PerformLayout();
			this.FixedCurrencyCheckBox.ResumeLayout();
			this.FixedCurrencyCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Enterprise.ZArchitecture.ZCalcEdit InvoiceCurrExRateCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox FixedCurrencyCheckBox;
	}
}
