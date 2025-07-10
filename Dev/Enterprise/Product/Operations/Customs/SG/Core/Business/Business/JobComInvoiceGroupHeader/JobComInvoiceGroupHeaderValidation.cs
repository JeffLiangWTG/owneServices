namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceGroupHeaderValidation : Customs.Business.JobComInvoiceGroupHeaderValidation
	{
		public JobComInvoiceGroupHeaderValidation(JobComInvoiceGroupHeader groupHeader)
			: base(groupHeader)
		{
		}

		public new JobComInvoiceGroupHeader InvoiceGroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.InvoiceGroupHeader; }
		}

		protected new JobComInvoiceGroupHeader Parent
		{
			get { return (JobComInvoiceGroupHeader)base.Parent; }
		}
	}
}
