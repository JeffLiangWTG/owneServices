using Enterprise.Integration.Packing;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderPackageStrategy : IWhsPackageStrategy
	{
		public IPackageActionStrategy GetPackageActionStrategy(IWhsOrder order, IPkgPackage package)
		{
			IPackageActionStrategy result = null;

			var pkgPackage = (PkgPackage)package;
			if (pkgPackage.IsInDatabase && pkgPackage.HasLoadedPivot())
			{
				var restrictedActions = PackageAction.Delete | PackageAction.PackUnpack;
				var reasonMessage = Res.GetString("6a085809-224d-4bc5-889e-4c169b6e4e9e", "Cannot delete or unpack a loaded package.");
				result = new PackageActionStrategy(pkgPackage, restrictedActions, reasonMessage);
			}

			return result;
		}

		public void OnPackageDelete(IWhsOrder order, IPkgPackage package)
		{
		}
	}
}
