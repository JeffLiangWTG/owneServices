using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	sealed class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvLineRefsSchema.JG_JI, BusinessObject.PK);
			Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, BusinessObject.PK);
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(JobComInvLineRefsSchema.JG_JI, BusinessObject.PK);
			Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, BusinessObject.PK);
		}
	}
}
