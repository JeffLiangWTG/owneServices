using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefVesselZZ)]
	public class RefVesselZZCollection : ActiveBusinessObjectCollection<RefVesselZZ>
	{
		public RefVesselZZCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}
		public RefVesselZZCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefVesselZZCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
