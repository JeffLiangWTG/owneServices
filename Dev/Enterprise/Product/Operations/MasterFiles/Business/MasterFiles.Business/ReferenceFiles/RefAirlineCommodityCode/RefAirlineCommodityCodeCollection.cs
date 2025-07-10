using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefAirlineCommodityCode)]
	public class RefAirlineCommodityCodeCollection : ActiveBusinessObjectCollection<RefAirlineCommodityCode>
	{
		public RefAirlineCommodityCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefAirlineCommodityCodeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
