using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	[ModuleID(ModuleId.CartageLeg)]
	public class ModuleCartageLegCollection : BusinessObjectCollection<CommonCartageLeg>
	{
		public ModuleCartageLegCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
