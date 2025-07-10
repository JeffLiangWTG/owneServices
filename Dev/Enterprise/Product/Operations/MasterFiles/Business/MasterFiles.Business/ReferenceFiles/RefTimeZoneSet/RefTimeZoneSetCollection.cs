using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefTimeZoneSet)]
	public class RefTimeZoneSetCollection : ActiveBusinessObjectCollection<RefTimeZoneSet>
	{
		public RefTimeZoneSetCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefTimeZoneSetCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
