using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Module
{
	public class ProjectFlattenedCollection : NonPersistentBusinessObjectCollection<ProjectFlattened>
	{
		public ProjectFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProjectFlattened();
		}
	}
}
