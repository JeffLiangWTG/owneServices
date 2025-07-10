using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class JobRequiredDocumentFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobRequiredDocumentFetchStrategy(JobRequiredDocument document)
			: base(document)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			AddFetchHintOnJobRequiredDocumentAddInfo();
		}

		void AddFetchHintOnJobRequiredDocumentAddInfo()
		{
			Factory.AddFetchHint(JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument, BusinessObject.PK);
		}
	}
}
