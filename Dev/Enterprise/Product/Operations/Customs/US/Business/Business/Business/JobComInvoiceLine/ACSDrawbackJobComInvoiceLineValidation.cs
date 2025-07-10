namespace Enterprise.Customs.US.Business
{
	public class ACSDrawbackJobComInvoiceLineValidation : CommonDrawbackJobComInvoiceLineValidation
	{
		public ACSDrawbackJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override bool IsTariffMandatory
		{
			get { return false; }
		}
	}
}
