using System.Collections.Generic;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderToPackageItemNumbers
	{
		public WhsOrderToPackageItemNumbers(IPackingParent order)
		{
			Order = order;
		}

		public IPackingParent Order { get; }
		public List<KeyValuePair<PkgPackage, int>> PackageItemNumbers { get; } = new List<KeyValuePair<PkgPackage, int>>();
	}
}
