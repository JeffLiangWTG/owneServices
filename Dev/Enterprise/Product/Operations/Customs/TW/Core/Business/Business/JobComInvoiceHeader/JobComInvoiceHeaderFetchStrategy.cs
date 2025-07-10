using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
	{
		public JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JZ_OH_Buyer);
			Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JZ_OH_Supplier);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
		}
	}
}
