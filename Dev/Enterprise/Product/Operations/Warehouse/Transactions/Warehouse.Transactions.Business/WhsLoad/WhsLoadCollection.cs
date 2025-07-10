using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsLoad)]
	public class WhsLoadCollection : ActiveBusinessObjectCollection<WhsLoad>
	{
		public WhsLoadCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
