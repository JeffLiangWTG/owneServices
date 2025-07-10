namespace Enterprise.Customs.ZA.Business
{
	public class JobComInvoiceGroupHeaderLookups : Customs.Business.JobComInvoiceGroupHeaderLookups
	{
		public JobComInvoiceGroupHeaderLookups(JobComInvoiceGroupHeader parent)
			: base(parent)
		{
		}

		public JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)Parent; }
		}
	}
}
