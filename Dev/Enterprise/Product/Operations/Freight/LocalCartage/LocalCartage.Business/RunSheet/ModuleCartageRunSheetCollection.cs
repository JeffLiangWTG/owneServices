using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Business
{
	[ModuleID(ModuleId.CartageWorkSheet)]
	public class ModuleCartageRunSheetCollection : BusinessObjectCollection<CommonWorkSheet>
	{
		public ModuleCartageRunSheetCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
