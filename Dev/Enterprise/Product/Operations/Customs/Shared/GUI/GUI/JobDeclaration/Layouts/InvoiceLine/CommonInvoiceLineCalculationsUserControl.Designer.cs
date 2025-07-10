namespace Enterprise.Customs.GUI
{
    partial class CommonInvoiceLineCalculationsUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CIFConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.FOBConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.SummaryLabel = new Enterprise.ZArchitecture.ZLabel();
            this.BalanceConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.LinesEnteredConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.LinesTotalConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.CurrentInvoiceLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CIFConvertToLocalCurrencyControl.SuspendLayout();
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.SuspendLayout();
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.SuspendLayout();
            this.FOBConvertToLocalCurrencyControl.SuspendLayout();
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.SuspendLayout();
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.SuspendLayout();
            this.BalanceConvertToLocalCurrencyControl.SuspendLayout();
            this.LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
            this.LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceLine);
            // 
            // CIFConvertToLocalCurrencyControl
            // 
            this.CIFConvertToLocalCurrencyControl.AllowDrop = true;
            this.CIFConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_CIF";
            this.CIFConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.CIFConvertToLocalCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.CIFConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|A5FEC0F4-5420-413B-92A5-3C18D220EC42", "CIF", "CIF Value", "CIF value for current line item.");
            this.CIFConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 209, true);
            this.CIFConvertToLocalCurrencyControl.Name = "CIFConvertToLocalCurrencyControl";
            this.CIFConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.CIFConvertToLocalCurrencyControl.TabIndex = 0;
            // 
            // InsuranceInInvoiceCurrConvertToLocalCurrencyControl
            // 
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.AllowDrop = true;
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_InsuranceInInvoiceCurr";
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|4B23CBE6-D217-4B38-85CA-AE43446ABA93", "Insurance", "Insurance Amount", "Insurance charges for current line item.");
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 183, true);
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.Name = "InsuranceInInvoiceCurrConvertToLocalCurrencyControl";
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.TabIndex = 1;
            // 
            // FreightInInvoiceCurrConvertToLocalCurrencyControl
            // 
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.AllowDrop = true;
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_FreightInInvoiceCurr";
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|9CBBE97C-E6D9-45CC-AE32-4A099CE4BE12", "Freight", "Freight Charges for current line item.");
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 157, true);
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.Name = "FreightInInvoiceCurrConvertToLocalCurrencyControl";
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.TabIndex = 2;
            // 
            // FOBConvertToLocalCurrencyControl
            // 
            this.FOBConvertToLocalCurrencyControl.AllowDrop = true;
            this.FOBConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_FOB";
            this.FOBConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.FOBConvertToLocalCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.FOBConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|994B534B-603A-408D-8E49-110CBD2987EF", "FOB Value", "FOB value for current line item.");
            this.FOBConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 131, true);
            this.FOBConvertToLocalCurrencyControl.Name = "FOBConvertToLocalCurrencyControl";
            this.FOBConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.FOBConvertToLocalCurrencyControl.TabIndex = 3;
            // 
            // GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl
            // 
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.AllowDrop = true;
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_GSTVATAmountIncludingWHEstimate";
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.BindToUnit = "JI_RX_LocalCurrency";
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|45B12AAB-C5C6-4947-B054-58EF7449CDFE", "{0} Amount", "{0} value for current line item");
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 261, true);
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.Name = "GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl";
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.TabIndex = 4;
            // 
            // DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl
            // 
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.AllowDrop = true;
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_DutyAmountIncludingWHEstimate";
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.BindToUnit = "JI_RX_LocalCurrency";
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|163E2BD1-F1C0-4822-B785-35C08F9AFA25", "Duty", "Duty value for current line item.");
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 235, true);
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.Name = "DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl";
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.TabIndex = 5;
            // 
            // SummaryLabel
            // 
            this.SummaryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.SummaryLabel.IsFontBold = true;
            this.SummaryLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|1DB03584-8950-49EB-B17C-709DFC626A5D", "Line Calculations");
            this.SummaryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 116, true);
            this.SummaryLabel.Name = "SummaryLabel";
            this.SummaryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 12, true);
            this.SummaryLabel.TabIndex = 1;
            this.SummaryLabel.UseMnemonic = false;
            // 
            // BalanceConvertToLocalCurrencyControl
            // 
            this.BalanceConvertToLocalCurrencyControl.AllowDrop = true;
            this.BalanceConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_Balance";
            this.BalanceConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.BalanceConvertToLocalCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.BalanceConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.BalanceConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|F1833D25-6E44-4A65-AF59-317037025611", "Balance", "Remaining balance for entered lines on current invoice.");
            this.BalanceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 41, true);
            this.BalanceConvertToLocalCurrencyControl.Name = "BalanceConvertToLocalCurrencyControl";
            this.BalanceConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.BalanceConvertToLocalCurrencyControl.TabIndex = 6;
            // 
            // LinesEnteredConvertToLocalCurrencyControl
            // 
            this.LinesEnteredConvertToLocalCurrencyControl.AllowDrop = true;
            this.LinesEnteredConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_LinesEntered";
            this.LinesEnteredConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.LinesEnteredConvertToLocalCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.LinesEnteredConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|7D701219-667F-4C4A-B0E9-596BCDC90CE7", "Lines Entered", "The total amount of lines entered so far.");
            this.LinesEnteredConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.LinesEnteredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 67, true);
            this.LinesEnteredConvertToLocalCurrencyControl.Name = "LinesEnteredConvertToLocalCurrencyControl";
            this.LinesEnteredConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.LinesEnteredConvertToLocalCurrencyControl.TabIndex = 7;
            // 
            // LinesTotalConvertToLocalCurrencyControl
            // 
            this.LinesTotalConvertToLocalCurrencyControl.AllowDrop = true;
            this.LinesTotalConvertToLocalCurrencyControl.BindToAmount = "JI_Calc_LinesTotal";
            this.LinesTotalConvertToLocalCurrencyControl.BindToList = "Lookups+CurrencyList";
            this.LinesTotalConvertToLocalCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.LinesTotalConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|894333EB-C5D4-4AF2-B1DC-91E37BB09B39", "Expected", "Total Expected", "The expected line total amount.");
            this.LinesTotalConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.LinesTotalConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 93, true);
            this.LinesTotalConvertToLocalCurrencyControl.Name = "LinesTotalConvertToLocalCurrencyControl";
            this.LinesTotalConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.LinesTotalConvertToLocalCurrencyControl.TabIndex = 8;
            // 
            // CurrentInvoiceLabel
            // 
            this.CurrentInvoiceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.CurrentInvoiceLabel.IsFontBold = true;
            this.CurrentInvoiceLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|B40A5588-0B98-432E-BDD4-1B21F8231821", "Current Invoice");
            this.CurrentInvoiceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 15, true);
            this.CurrentInvoiceLabel.Name = "CurrentInvoiceLabel";
            this.CurrentInvoiceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.CurrentInvoiceLabel.TabIndex = 9;
            this.CurrentInvoiceLabel.UseMnemonic = false;
            // 
            // CommonInvoiceLineCalculationsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.SummaryLabel);
            this.Controls.Add(this.CIFConvertToLocalCurrencyControl);
            this.Controls.Add(this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl);
            this.Controls.Add(this.FreightInInvoiceCurrConvertToLocalCurrencyControl);
            this.Controls.Add(this.FOBConvertToLocalCurrencyControl);
            this.Controls.Add(this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl);
            this.Controls.Add(this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl);
            this.Controls.Add(this.BalanceConvertToLocalCurrencyControl);
            this.Controls.Add(this.LinesEnteredConvertToLocalCurrencyControl);
            this.Controls.Add(this.LinesTotalConvertToLocalCurrencyControl);
            this.Controls.Add(this.CurrentInvoiceLabel);
            this.Name = "CommonInvoiceLineCalculationsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 329, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CIFConvertToLocalCurrencyControl.ResumeLayout(true);
            this.CIFConvertToLocalCurrencyControl.PerformLayout();
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.ResumeLayout(true);
            this.InsuranceInInvoiceCurrConvertToLocalCurrencyControl.PerformLayout();
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.ResumeLayout(true);
            this.FreightInInvoiceCurrConvertToLocalCurrencyControl.PerformLayout();
            this.FOBConvertToLocalCurrencyControl.ResumeLayout(true);
            this.FOBConvertToLocalCurrencyControl.PerformLayout();
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.ResumeLayout(true);
            this.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl.PerformLayout();
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.ResumeLayout(true);
            this.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl.PerformLayout();
            this.BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
            this.BalanceConvertToLocalCurrencyControl.PerformLayout();
            this.LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
            this.LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
            this.LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
            this.LinesTotalConvertToLocalCurrencyControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal Enterprise.ZArchitecture.ZLabel CurrentInvoiceLabel;
        internal ConvertToLocalCurrencyControl BalanceConvertToLocalCurrencyControl;
        internal ConvertToLocalCurrencyControl LinesEnteredConvertToLocalCurrencyControl;
        internal ConvertToLocalCurrencyControl LinesTotalConvertToLocalCurrencyControl;
        internal Enterprise.ZArchitecture.ZLabel SummaryLabel;
        internal ConvertToLocalCurrencyControl CIFConvertToLocalCurrencyControl;
        internal ConvertToLocalCurrencyControl InsuranceInInvoiceCurrConvertToLocalCurrencyControl;
        internal ConvertToLocalCurrencyControl FreightInInvoiceCurrConvertToLocalCurrencyControl;
        internal ConvertToLocalCurrencyControl FOBConvertToLocalCurrencyControl;
        internal ConvertToLocalCurrencyControl GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl;
        internal ConvertToLocalCurrencyControl DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl;
    }
}
