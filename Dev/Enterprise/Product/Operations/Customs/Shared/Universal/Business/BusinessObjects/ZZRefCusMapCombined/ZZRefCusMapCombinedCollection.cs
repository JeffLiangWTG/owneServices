using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal
{
	[ModuleID(ModuleId.ZZRefCusMap)]
	public class ZZRefCusMapCombinedCollection : BusinessObjectCollection<ZZRefCusMapCombined>
	{
		public ZZRefCusMapCombinedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
