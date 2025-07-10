using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsVASOrder)]
	public class WhsVASOrderCollection : ActiveBusinessObjectCollection<WhsVASOrder>
	{
		public WhsVASOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new WhsVASOrderCollectionFetchStrategy(this);
	}
}
