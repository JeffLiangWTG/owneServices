using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobDocsAndCartageFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobDocsAndCartageFetchStrategy(JobDocsAndCartage docsAndCartage)
			: base(docsAndCartage)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(JobOrderItemSchema.JT_JP, BusinessObject.PK);
			Factory.AddFetchHint(JobServiceSchema.ES_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobRequiredDocumentSchema.EQ_ParentID, BusinessObject.PK);
		}
	}
}
