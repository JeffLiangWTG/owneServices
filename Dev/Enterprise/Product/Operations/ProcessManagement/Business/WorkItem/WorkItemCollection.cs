using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.ProcessManagement.Business
{
	[ModuleID("WorkItem")]
	public class WorkItemCollection : ActiveBusinessObjectCollection<WorkItem>
	{
		public WorkItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WorkItemCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public WorkItemCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
