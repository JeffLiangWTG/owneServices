using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDeliveryDeleter : DeleteChecker
	{
		public override void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
			base.BeforeSuccessfulDelete(businessObject);

			if (MightHaveRelatedJobDocumentDeliveries(businessObject))
			{
				foreach (var relatedJobDocumentDelivery in FindRelatedJobDocumentDeliveries(businessObject))
				{
					relatedJobDocumentDelivery.Delete();
				}
			}
		}

		public override void AddFetchHint(BusinessObject businessObject)
		{
			if (MightHaveRelatedJobDocumentDeliveries(businessObject))
			{
				businessObject.Factory.AddFetchHint(JobDocumentDeliverySchema.JDC_ParentID, businessObject.PK);
			}
		}

		JobDocumentDelivery[] FindRelatedJobDocumentDeliveries(BusinessObject businessObject)
		{
			var query = new ZQuery(JobDocumentDeliverySchema.JDC_ParentID, businessObject.PK);
			return businessObject.Factory.Load<JobDocumentDelivery>(query);
		}

		public override DeleteDetails DeleteDetails(BusinessObject businessObject)
			=> new DeleteDetails.Allow();

		bool MightHaveRelatedJobDocumentDeliveries(BusinessObject businessObject)
			=> businessObject.IsInDatabase && businessObject is IDocumentSupportable;
	}
}
