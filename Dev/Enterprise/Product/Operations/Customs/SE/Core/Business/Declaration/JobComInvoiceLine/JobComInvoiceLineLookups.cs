namespace Enterprise.Customs.SE.Business.Declaration
{
	public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
	}
}
