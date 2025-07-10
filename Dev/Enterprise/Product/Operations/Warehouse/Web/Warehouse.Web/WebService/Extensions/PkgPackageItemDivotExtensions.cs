using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	static class PkgPackageItemDivotExtensions
	{
		public static void DeleteForRepacking(this PkgPackageItemDivot divot, IPackableItemParent packableItemParent)
		{
			var pickLine = divot.PackedItem as WhsPickLine;
			using (pickLine?.SuspendReMerge())
			{
				// need to have the Divot Wrapper so that deleting the divot will properly update the package weight.
				PkgPackageItemDivotsWrapper.New(divot, packableItemParent).Delete();
			}
		}
	}
}
