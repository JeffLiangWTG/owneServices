namespace Enterprise.Customs.SE.Business.Declaration
{
	public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
	}
}
