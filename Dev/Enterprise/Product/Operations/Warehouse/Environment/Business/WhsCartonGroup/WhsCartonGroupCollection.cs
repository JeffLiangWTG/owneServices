using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsCartonGroup)]
	public class WhsCartonGroupCollection : ActiveBusinessObjectCollection<WhsCartonGroup>, IWhsCartonGroupCollection
	{
		public WhsCartonGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
