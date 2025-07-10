using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbHolidaySourceCollection : ActiveBusinessObjectCollection<GlbEmployingBranchDepartment>
	{
		public GlbHolidaySourceCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbHolidaySourceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbHolidaySourceCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
