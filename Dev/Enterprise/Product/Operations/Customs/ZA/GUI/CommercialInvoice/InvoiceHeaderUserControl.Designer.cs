
namespace Enterprise.Customs.ZA.GUI.CommercialInvoice
{
	public partial class InvoiceHeaderUserControl
	{
		void InitializeComponent()
		{
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// JZ_IncoTermDropDownEdit
			// 
			this.JZ_IncoTermDropDownEdit.TabIndex = 5;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.TabIndex = 6;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			// 
			// InvoiceHeaderUserControl
			// 
			this.Name = "InvoiceHeaderUserControl";
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

	}
}
