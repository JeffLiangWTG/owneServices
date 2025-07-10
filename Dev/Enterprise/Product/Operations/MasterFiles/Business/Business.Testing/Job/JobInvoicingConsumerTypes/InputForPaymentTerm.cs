namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InputForPaymentTerm
	{
		public string IncoTerm { get; set; }
		public string CurrentInvoiceType { get; set; }
		public AccChargeCode ChargeCode { get; set; }

		public override string ToString()
		{
			return string.Format("[IncoTerm: '{0}', InvoiceType: '{1}', ChargeCodeGroup: '{2}']", IncoTerm, CurrentInvoiceType, ChargeCode.AC_ChargeGroup);
		}
	}
}
