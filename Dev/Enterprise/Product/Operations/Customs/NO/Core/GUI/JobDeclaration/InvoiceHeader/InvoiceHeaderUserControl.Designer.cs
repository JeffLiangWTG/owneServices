using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class InvoiceHeaderUserControl
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
			components = new System.ComponentModel.Container();
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RightBottomPanel.SuspendLayout();
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Controls.Add(this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl);
			// 
			// JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl
			//
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.AllowDrop = true;
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.BindToAmount = "Invoices.JZ_Calc_ChargesAmount";
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.BindToList = "Lookups.CurrencyList";
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.BindToUnit = "Invoices.JZ_Calc_ChargesCurrency";
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("4d2d036c-5480-4b8a-bc11-589b598b42c8", "Charges");
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 6, true);
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.Name = "JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl";
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.TabIndex = 4;
			// 
			// InvoiceHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "InvoiceHeaderUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.ResumeLayout(false);
			this.JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected internal ConvertToLocalCurrencyControl JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl;
	}
}
