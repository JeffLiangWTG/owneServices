using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Business
{
	[ModuleID(ModuleId.Cartage)]
	public class CommonCartageCollection : ActiveBusinessObjectCollection<CommonCartage>
	{
		public CommonCartageCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CommonCartageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
