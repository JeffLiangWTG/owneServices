using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Business.FetchStrategies
{
	public class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
	{
		public JobComInvoiceHeaderFetchStrategy(ZA.Business.JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(JobComInvoiceHeaderRefsSchema.J2_JZ, BusinessObject.PK);
		}
	}
}
