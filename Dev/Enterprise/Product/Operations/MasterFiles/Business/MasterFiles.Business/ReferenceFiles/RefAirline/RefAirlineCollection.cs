using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefAirline)]
	public class RefAirlineCollection : ActiveBusinessObjectCollection<RefAirline>
	{
		public RefAirlineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefAirlineCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
