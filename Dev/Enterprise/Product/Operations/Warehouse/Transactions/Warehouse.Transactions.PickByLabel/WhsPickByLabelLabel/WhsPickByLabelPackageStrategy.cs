using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration.Packing;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class WhsPickByLabelPackageStrategy : IWhsPackageStrategy
	{
		public IPackageActionStrategy GetPackageActionStrategy(IWhsOrder order, IPkgPackage package)
		{
			IPackageActionStrategy result;

			var whsOrder = (WhsOrder)order;
			var pkgPackage = (PkgPackage)package;

			if (!whsOrder.IsPackageAssignedToPickByLabelJob(pkgPackage))
			{
				result = null;
			}
			else
			{
				var reasonMessage = string.Empty;
				var restrictedPickByLabelActions = PackageAction.None;
				if (pkgPackage.CannotDeleteAndPackUnpackPackage())
				{
					restrictedPickByLabelActions |= PackageAction.Delete | PackageAction.PackUnpack;
					reasonMessage = Res.GetString("14f7b334-12f4-486f-8816-58e95d858971", "Cannot modify the selected {0} because it has Pick By Label Job and packed items that are not all picked.", pkgPackage.KP_F3_NKPackType);
				}

				result = new PackageActionStrategy(pkgPackage, restrictedPickByLabelActions, reasonMessage);
			}

			return result;
		}

		public void OnPackageDelete(IWhsOrder orderInterface, IPkgPackage packageInterface)
		{
			var order = (WhsOrder)orderInterface;
			var package = (PkgPackage)packageInterface;
			if (order.IsPackageAssignedToPickByLabelJob(package))
			{
				var queryLabelLabel = new ZQuery();
				queryLabelLabel.AddToFilter(WhsPickByLabelLabelSchema.WTL_KP_Package, SQLComparisonOperator.Equal, package.PK);

				var labelLabels = order.Factory.Load<WhsPickByLabelLabel>(queryLabelLabel);
				labelLabels.ForEach(t => t.Delete());
			}
		}
	}
}
