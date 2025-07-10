using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbStaff)]
	public class GlbStaffAndResourceCollection : ActiveBusinessObjectCollection<GlbStaff>
	{
		public GlbStaffAndResourceCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public GlbStaffAndResourceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffAndResourceCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
