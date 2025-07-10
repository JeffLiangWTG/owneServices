using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefFacility)]
	public class RefFacilityCollection : ActiveBusinessObjectCollection<RefFacility>
	{
		public RefFacilityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefFacilityCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
