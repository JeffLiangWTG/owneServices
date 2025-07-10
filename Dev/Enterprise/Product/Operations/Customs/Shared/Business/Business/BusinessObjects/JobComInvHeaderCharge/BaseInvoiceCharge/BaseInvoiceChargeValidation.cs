namespace Enterprise.Customs.Business
{
	public class BaseInvoiceChargeValidation : JobComInvHeaderChargeValidation
	{
		public BaseInvoiceChargeValidation(BaseInvoiceCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		protected new BaseInvoiceCharge Parent
		{
			get { return (BaseInvoiceCharge)base.Parent; }
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			ValidateJ7_IsDutiable();
		}

		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			ValidateIfAllInvoiceLinesHaveDistributeByField();
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			ValidateCurrency();
		}
	}
}
