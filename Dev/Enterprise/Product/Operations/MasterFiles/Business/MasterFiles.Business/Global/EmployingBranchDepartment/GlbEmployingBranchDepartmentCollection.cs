using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmployingBranchDepartmentCollection : ActiveBusinessObjectCollection<GlbEmployingBranchDepartment>
	{
		public GlbEmployingBranchDepartmentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbEmployingBranchDepartmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbEmployingBranchDepartmentCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
