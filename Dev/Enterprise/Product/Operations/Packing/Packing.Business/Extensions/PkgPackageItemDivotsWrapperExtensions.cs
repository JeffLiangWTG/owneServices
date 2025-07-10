using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Packing.Business
{
	public static class PkgPackageItemDivotsWrapperExtensions
	{
		public static bool IsOnePackedItem(this IEnumerable<PkgPackageItemDivotsWrapper> packedItems)
		{
			return packedItems.GroupedPackedItems().Count == 1;
		}

		public static ILookup<GroupedPackedItemsKey, PkgPackageItemDivotsWrapper> GroupedPackedItems(this IEnumerable<PkgPackageItemDivotsWrapper> packedItems)
		{
			return packedItems.ToLookup(p => new GroupedPackedItemsKey(p));
		}
	}
}
