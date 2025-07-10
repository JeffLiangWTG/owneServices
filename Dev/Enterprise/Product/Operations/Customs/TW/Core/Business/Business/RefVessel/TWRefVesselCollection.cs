using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.Business
{
	[ModuleID(ModuleId.RefVessel)]
	public class TWRefVesselCollection : BusinessObjectCollection<TWRefVessel>
	{
		public TWRefVesselCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
