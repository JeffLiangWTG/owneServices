namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceHeaderValidatorClass : InvoiceHeaderValidation
	{
		public InvoiceHeaderValidatorClass(BaseJobComInvoiceHeader invoice) : base(invoice)
		{
		}

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges
		{
			get { return InvoiceHeaderValidation.TypeOfValidationForMissingMandatoryChargesForIncoterm.Error; }
		}
	}
}
