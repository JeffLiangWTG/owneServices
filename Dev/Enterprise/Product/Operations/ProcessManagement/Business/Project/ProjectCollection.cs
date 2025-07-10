using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.ProcessManagement.Business
{
	[ModuleID("Project")]
	public class ProjectCollection : ActiveBusinessObjectCollection<Project>
	{
		public ProjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ProjectCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public ProjectCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
