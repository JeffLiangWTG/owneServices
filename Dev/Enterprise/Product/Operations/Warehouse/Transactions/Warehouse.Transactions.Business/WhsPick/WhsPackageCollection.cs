using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackageCollection : ActiveBusinessObjectCollection<PkgPackage>
	{
		public WhsPackageCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new WhsPackageCollectionFetchStrategy(this);
	}
}
