using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Module
{
	public class WorkItemFlattenedCollection : NonPersistentBusinessObjectCollection<WorkItemFlattened>
	{
		public WorkItemFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WorkItemFlattened();
		}
	}
}
