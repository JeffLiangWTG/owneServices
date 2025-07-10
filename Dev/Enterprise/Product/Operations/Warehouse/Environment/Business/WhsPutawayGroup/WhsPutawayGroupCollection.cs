using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsConfigPutawayGroup)]
	public class WhsPutawayGroupCollection : ActiveBusinessObjectCollection<WhsPutawayGroup>, IWhsPutawayGroupCollection
	{
		public WhsPutawayGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
