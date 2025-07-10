namespace Enterprise.Customs.US.GUI
{
	partial class InvoiceLineChargesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ApportionedChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ChargesGrid
			// 
			this.BindingSource.SetBindingMember(this.ChargesGrid, "FilteredInvoiceLines.Charges");
			zCheckBoxColumnStyleInfo1.Caption = "Fixed Rate";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsJ7_ExchangeRateUserEnterable";
			zCheckBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("InvoiceLineCharges|46e9a629-4fba-441b-8378-487092dbbe8d", "Exchange Rate");
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Exchange Rate";
			zCalcEditColumnStyleInfo1.ColumnName = "J7_ExchangeRate";
			zCalcEditColumnStyleInfo1.Decimals = 6;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("InvoiceLineCharges|46e9a629-4fba-441b-8378-487092dbbe8d", "Exchange Rate");
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo1.Caption = "AII Charge Desc.";
			zTextBoxColumnStyleInfo1.ColumnName = "J7_ChargeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			// 
			// ApportionedChargesGrid
			// 
			this.BindingSource.SetBindingMember(this.ApportionedChargesGrid, "FilteredInvoiceLines.ApportionedCharges");
			zCheckBoxColumnStyleInfo2.Caption = "Fixed Rate";
			zCheckBoxColumnStyleInfo2.ColumnName = "IsJ7_ExchangeRateUserEnterable";
			zCheckBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("InvoiceLineCharges|66ff5627-9e7d-4f13-b3c8-5e3291b6dc36", "Exchange Rate");
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Exchange Rate";
			zCalcEditColumnStyleInfo2.ColumnName = "J7_ExchangeRate";
			zCalcEditColumnStyleInfo2.Decimals = 6;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("InvoiceLineCharges|66ff5627-9e7d-4f13-b3c8-5e3291b6dc36", "Exchange Rate");
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo2.Caption = "AII Charge Desc.";
			zTextBoxColumnStyleInfo2.ColumnName = "J7_ChargeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// InvoiceLineChargesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "InvoiceLineChargesUserControl";
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ApportionedChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
