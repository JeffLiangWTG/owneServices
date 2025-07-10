namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	partial class TWBillCalculationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.zGroupBoxCustomsValuation = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OtherDeductionsConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.OtherValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.InsuranceValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.FreightValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.GoodsValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.CustomsValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.ExchangeRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutiesTaxesAndFeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGridTaxs = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBoxCustomsValuation.SuspendLayout();
			this.OtherDeductionsConvertToLocalCurrencyControl.SuspendLayout();
			this.OtherValueConvertToLocalCurrencyControl.SuspendLayout();
			this.InsuranceValueConvertToLocalCurrencyControl.SuspendLayout();
			this.FreightValueConvertToLocalCurrencyControl.SuspendLayout();
			this.GoodsValueConvertToLocalCurrencyControl.SuspendLayout();
			this.CustomsValueConvertToLocalCurrencyControl.SuspendLayout();
			this.DutiesTaxesAndFeesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridTaxs)).BeginInit();
			this.zGridTaxs.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill);
			// 
			// zGroupBoxCustomsValuation
			// 
			this.zGroupBoxCustomsValuation.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("5715AC5C-0E57-4D18-9ABC-1AAE8D4D3FA0", "Customs Valuation");
			this.zGroupBoxCustomsValuation.Controls.Add(this.OtherDeductionsConvertToLocalCurrencyControl);
			this.zGroupBoxCustomsValuation.Controls.Add(this.OtherValueConvertToLocalCurrencyControl);
			this.zGroupBoxCustomsValuation.Controls.Add(this.InsuranceValueConvertToLocalCurrencyControl);
			this.zGroupBoxCustomsValuation.Controls.Add(this.FreightValueConvertToLocalCurrencyControl);
			this.zGroupBoxCustomsValuation.Controls.Add(this.GoodsValueConvertToLocalCurrencyControl);
			this.zGroupBoxCustomsValuation.Controls.Add(this.CustomsValueConvertToLocalCurrencyControl);
			this.zGroupBoxCustomsValuation.Controls.Add(this.ExchangeRateCalcEdit);
			this.zGroupBoxCustomsValuation.Dock = System.Windows.Forms.DockStyle.Left;
			this.zGroupBoxCustomsValuation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBoxCustomsValuation.Name = "zGroupBoxCustomsValuation";
			this.zGroupBoxCustomsValuation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 417, true);
			this.zGroupBoxCustomsValuation.TabIndex = 3;
			this.zGroupBoxCustomsValuation.TabStop = false;
			// 
			// OtherDeductionsConvertToLocalCurrencyControl
			// 
			this.OtherDeductionsConvertToLocalCurrencyControl.AllowDrop = true;
			this.OtherDeductionsConvertToLocalCurrencyControl.BindToAmount = "ABL_OtherDeductions";
			this.OtherDeductionsConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKOtherDeductionsCurrency";
			this.OtherDeductionsConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OtherDeductionsConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 132, true);
			this.OtherDeductionsConvertToLocalCurrencyControl.Name = "OtherDeductionsConvertToLocalCurrencyControl";
			this.OtherDeductionsConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.OtherDeductionsConvertToLocalCurrencyControl.TabIndex = 5;
			// 
			// OtherValueConvertToLocalCurrencyControl
			// 
			this.OtherValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.OtherValueConvertToLocalCurrencyControl.BindToAmount = "ABL_OtherValue";
			this.OtherValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKOtherValueCurrency";
			this.OtherValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OtherValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 106, true);
			this.OtherValueConvertToLocalCurrencyControl.Name = "OtherValueConvertToLocalCurrencyControl";
			this.OtherValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.OtherValueConvertToLocalCurrencyControl.TabIndex = 4;
			// 
			// InsuranceValueConvertToLocalCurrencyControl
			// 
			this.InsuranceValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.InsuranceValueConvertToLocalCurrencyControl.BindToAmount = "ABL_InsuranceValue";
			this.InsuranceValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKInsuranceValueCurrency";
			this.InsuranceValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InsuranceValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 79, true);
			this.InsuranceValueConvertToLocalCurrencyControl.Name = "InsuranceValueConvertToLocalCurrencyControl";
			this.InsuranceValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.InsuranceValueConvertToLocalCurrencyControl.TabIndex = 3;
			// 
			// FreightValueConvertToLocalCurrencyControl
			// 
			this.FreightValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.FreightValueConvertToLocalCurrencyControl.BindToAmount = "ABL_FreightValue";
			this.FreightValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKFreightValueCurrency";
			this.FreightValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.FreightValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 53, true);
			this.FreightValueConvertToLocalCurrencyControl.Name = "FreightValueConvertToLocalCurrencyControl";
			this.FreightValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.FreightValueConvertToLocalCurrencyControl.TabIndex = 2;
			// 
			// GoodsValueConvertToLocalCurrencyControl
			// 
			this.GoodsValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.GoodsValueConvertToLocalCurrencyControl.BindToAmount = "ABL_GoodsValue";
			this.GoodsValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKGoodsValueCurrency";
			this.GoodsValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.GoodsValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 26, true);
			this.GoodsValueConvertToLocalCurrencyControl.Name = "GoodsValueConvertToLocalCurrencyControl";
			this.GoodsValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.GoodsValueConvertToLocalCurrencyControl.TabIndex = 1;
			// 
			// CustomsValueConvertToLocalCurrencyControl
			// 
			this.CustomsValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.CustomsValueConvertToLocalCurrencyControl.BindToAmount = "ABL_CustomsValue";
			this.CustomsValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKCustomsValueCurrency";
			this.CustomsValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.CustomsValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 158, true);
			this.CustomsValueConvertToLocalCurrencyControl.Name = "CustomsValueConvertToLocalCurrencyControl";
			this.CustomsValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.CustomsValueConvertToLocalCurrencyControl.TabIndex = 6;
			// 
			// ExchangeRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExchangeRateCalcEdit, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ExchangeRate)));
			this.ExchangeRateCalcEdit.DecimalPlaces = 2;
			this.ExchangeRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 185, true);
			this.ExchangeRateCalcEdit.Name = "ExchangeRateCalcEdit";
			this.ExchangeRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 17, true);
			this.ExchangeRateCalcEdit.TabIndex = 7;
			this.ExchangeRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExchangeRateCalcEdit.TrackDisposedAccess = true;
			// 
			// DutiesTaxesAndFeesGroupBox
			// 
			this.DutiesTaxesAndFeesGroupBox.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("a1382213-3a54-4e9e-b07a-f834116069da", "Duties, Taxes and Fees");
			this.DutiesTaxesAndFeesGroupBox.Controls.Add(this.zGridTaxs);
			this.DutiesTaxesAndFeesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DutiesTaxesAndFeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 0, true);
			this.DutiesTaxesAndFeesGroupBox.Name = "DutiesTaxesAndFeesGroupBox";
			this.DutiesTaxesAndFeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 417, true);
			this.DutiesTaxesAndFeesGroupBox.TabIndex = 4;
			this.DutiesTaxesAndFeesGroupBox.TabStop = false;
			// 
			// zGridTaxs
			// 
			this.zGridTaxs.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGridTaxs, "AsycudaTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).AsycudaTaxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_MethodOfPayment)));
			this.zGridTaxs.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AET_ChargeType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AET_ChargeAmount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "AET_MethodOfPayment";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			this.zGridTaxs.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGridTaxs.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGridTaxs.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGridTaxs.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGridTaxs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGridTaxs.GridId = "4572bb85-9604-4f83-a3e7-4df905d6c02e";
			this.zGridTaxs.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridTaxs.LayoutKey = "zGrid1";
			this.zGridTaxs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.zGridTaxs.Name = "zGridTaxs";
			this.zGridTaxs.ReadOnly = true;
			this.zGridTaxs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 400, true);
			this.zGridTaxs.TabIndex = 9;
			// 
			// TWBillCalculationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DutiesTaxesAndFeesGroupBox);
			this.Controls.Add(this.zGroupBoxCustomsValuation);
			this.Name = "TWBillCalculationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1069, 417, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBoxCustomsValuation.ResumeLayout(false);
			this.zGroupBoxCustomsValuation.PerformLayout();
			this.OtherDeductionsConvertToLocalCurrencyControl.ResumeLayout(true);
			this.OtherDeductionsConvertToLocalCurrencyControl.PerformLayout();
			this.OtherValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.OtherValueConvertToLocalCurrencyControl.PerformLayout();
			this.InsuranceValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.InsuranceValueConvertToLocalCurrencyControl.PerformLayout();
			this.FreightValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.FreightValueConvertToLocalCurrencyControl.PerformLayout();
			this.GoodsValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.GoodsValueConvertToLocalCurrencyControl.PerformLayout();
			this.CustomsValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.CustomsValueConvertToLocalCurrencyControl.PerformLayout();
			this.DutiesTaxesAndFeesGroupBox.ResumeLayout(false);
			this.DutiesTaxesAndFeesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridTaxs)).EndInit();
			this.zGridTaxs.ResumeLayout(false);
			this.zGridTaxs.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBoxCustomsValuation;
		private ZArchitecture.GUI.ZGroupBox DutiesTaxesAndFeesGroupBox;
		private ZArchitecture.ZCalcEdit ExchangeRateCalcEdit;
		private ZArchitecture.ZGrid zGridTaxs;
		internal Customs.GUI.ConvertToLocalCurrencyControl CustomsValueConvertToLocalCurrencyControl;
		internal Customs.GUI.ConvertToLocalCurrencyControl GoodsValueConvertToLocalCurrencyControl;
		internal Customs.GUI.ConvertToLocalCurrencyControl OtherValueConvertToLocalCurrencyControl;
		internal Customs.GUI.ConvertToLocalCurrencyControl InsuranceValueConvertToLocalCurrencyControl;
		internal Customs.GUI.ConvertToLocalCurrencyControl FreightValueConvertToLocalCurrencyControl;
		internal Customs.GUI.ConvertToLocalCurrencyControl OtherDeductionsConvertToLocalCurrencyControl;
	}
}
