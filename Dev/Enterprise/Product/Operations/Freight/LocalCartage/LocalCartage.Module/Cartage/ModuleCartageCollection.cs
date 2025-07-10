using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	[ModuleID(ModuleId.Cartage)]
	public class ModuleCartageCollection : BusinessObjectCollection<CommonCartage>
	{
		public ModuleCartageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
