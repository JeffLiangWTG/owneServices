using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemRequestLinkCollection : PivotBusinessObjectCollection<WorkItemRequestLink>
	{
		public WorkItemRequestLinkCollection(BusinessObject master)
			: base(master, new WorkItemRequestLinkCollectionRelationship(master), includeChildren: master is WorkRequest, includeParents: master is WorkItem)
		{
		}
	}
}
