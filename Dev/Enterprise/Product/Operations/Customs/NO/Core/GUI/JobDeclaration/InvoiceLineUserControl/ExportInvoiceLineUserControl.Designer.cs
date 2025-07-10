namespace Enterprise.Customs.NO.GUI
{
	partial class ExportInvoiceLineUserControl
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
			this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			//
			// JI_Calc_GSTConvertToLocalCurrencyControl
			//
			this.JI_Calc_GSTConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("d142201d-9630-462a-8dc8-10d7225256a0", "VAT Amount", "VAT value for current line item");
			//
			// JI_Calc_DutyConvertToLocalCurrencyControl
			//
			this.JI_Calc_DutyConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("94f91828-0976-4774-af35-b79c854b159b", "Duties", "Duties value for current line item.");
			//
			// ExportInvoiceLineUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "ExportInvoiceLineUserControl";
			this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
