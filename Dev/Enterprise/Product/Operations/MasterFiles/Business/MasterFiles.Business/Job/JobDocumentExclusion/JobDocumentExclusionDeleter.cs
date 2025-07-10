using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentExclusionDeleter : DeleteChecker
	{
		public override void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
			base.BeforeSuccessfulDelete(businessObject);

			if (MightHaveRelatedJobDocumentExclusions(businessObject))
			{
				foreach (var relatedJobDocumentExclusion in FindRelatedJobDocumentExclusions(businessObject))
				{
					relatedJobDocumentExclusion.Delete();
				}
			}
		}

		public override void AddFetchHint(BusinessObject businessObject)
		{
			if (MightHaveRelatedJobDocumentExclusions(businessObject))
			{
				businessObject.Factory.AddFetchHint(JobDocumentExclusionSchema.JDE_ParentID, businessObject.PK);
			}
		}

		JobDocumentExclusion[] FindRelatedJobDocumentExclusions(BusinessObject businessObject)
		{
			var query = new ZQuery(JobDocumentExclusionSchema.JDE_ParentID, businessObject.PK);
			return businessObject.Factory.Load<JobDocumentExclusion>(query);
		}

		public override DeleteDetails DeleteDetails(BusinessObject businessObject)
			=> new DeleteDetails.Allow();

		bool MightHaveRelatedJobDocumentExclusions(BusinessObject businessObject)
			=> businessObject.IsInDatabase && businessObject is IDocumentSupportable;
	}
}
