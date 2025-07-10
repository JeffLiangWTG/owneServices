using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsAdHocServiceJob)]
	public class WhsAdHocServiceJobCollection : ActiveBusinessObjectCollection<WhsAdHocServiceJob>
	{
		public WhsAdHocServiceJobCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
