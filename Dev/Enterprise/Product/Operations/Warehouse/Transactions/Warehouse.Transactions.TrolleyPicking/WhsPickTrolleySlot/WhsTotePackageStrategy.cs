using Enterprise.Integration.Packing;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsTotePackageStrategy : IWhsPackageStrategy
	{
		public IPackageActionStrategy GetPackageActionStrategy(IWhsOrder order, IPkgPackage package)
		{
			IPackageActionStrategy result;

			var pkgPackage = (PkgPackage)package;

			if (!pkgPackage.IsToteOrParentIsTote())
			{
				result = null;
			}
			else
			{
				var restrictedToteActions = PackageAction.Edit;
				var reasonMessage = Res.GetString("d95c6a2d-0809-4b51-8276-7baae644474c", "Cannot edit the selected package or package contained within a Tote package.");
				if (pkgPackage.CannotDeleteAndPackUnpackPackage())
				{
					restrictedToteActions |= PackageAction.Delete | PackageAction.PackUnpack;
					reasonMessage = Res.GetString("9ee81203-2e47-4bb0-8ea0-da15dc107add", "Cannot modify or delete the selected Tote package or package contained within a Tote package that has packed items that are not all picked.");
				}

				result = new PackageActionStrategy(pkgPackage, restrictedToteActions, reasonMessage);
			}

			return result;
		}

		public void OnPackageDelete(IWhsOrder order, IPkgPackage package)
		{
		}
	}
}
