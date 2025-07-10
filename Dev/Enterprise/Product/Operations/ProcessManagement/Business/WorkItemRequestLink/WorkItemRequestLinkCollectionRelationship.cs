using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	class WorkItemRequestLinkCollectionRelationship : CollectionRelationship
	{
		public WorkItemRequestLinkCollectionRelationship(BusinessObject master)
			: base(typeof(WorkItemRequestLink), GetRelatedActivitiesQuery(master))
		{
		}

		internal static ZQuery GetRelatedActivitiesQuery(BusinessObject businessObject)
		{
			return businessObject is WorkItem
				? new ZQuery(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, businessObject.PK)
				: new ZQuery(WorkItemRequestLinkSchema.WKL_WKR_Request, businessObject.PK);
		}
	}
}
