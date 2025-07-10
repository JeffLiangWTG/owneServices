using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class USCustomsSupplierHeaderUserControl : NonLayoutCustomsSupplierHeaderUserControl
	{
		public const string IsCIFComponent = "CIF component";

		public USCustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
			JZ_InvoiceCurrLandedCostExRateCalcEdit.Visible = false;
			JZ_CIFAmountBoundCurrencyControl.Visible = false;
			InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable, IsCIFComponent);
			ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable, IsCIFComponent);
			BaseGroupChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable, IsCIFComponent);
			JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill);
		}

		internal ZTabPage InvoiceChargesTabPageInternal => InvoiceChargesTabPage;
		internal ConvertToLocalCurrencyControl JZ_Calc_TNIBoundInvoiceCurrencyControlInternal => JZ_Calc_TNIBoundInvoiceCurrencyControl;
		internal ConvertToLocalCurrencyControl JZ_CIFAmountBoundCurrencyControlInternal => JZ_CIFAmountBoundCurrencyControl;
		internal ZCalcEdit JZ_InvoiceCurrLandedCostExRateCalcEditInternal => JZ_InvoiceCurrLandedCostExRateCalcEdit;
	}
}
