using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCountryStates)]
	public class RefCountryStatesCollection : ActiveBusinessObjectCollection<RefCountryStates>
	{
		public RefCountryStatesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefCountryStatesCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
