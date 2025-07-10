using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration.Packing;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsPickByTrolleyPackageStrategy : IWhsPackageStrategy
	{
		public WhsPickByTrolleyPackageStrategy()
		{
			pickStrategy = new WhsPickByTrolleyPickStrategy();
		}

		readonly WhsPickByTrolleyPickStrategy pickStrategy;

		public IPackageActionStrategy GetPackageActionStrategy(IWhsOrder order, IPkgPackage package)
		{
			IPackageActionStrategy result;

			var whsOrder = (WhsOrder)order;
			var pkgPackage = (PkgPackage)package;

			if (!pickStrategy.IsPackageAssignedToTrolleyJob(whsOrder, pkgPackage))
			{
				result = null;
			}
			else
			{
				var reasonMessage = string.Empty;
				var restrictedTrolleyActions = PackageAction.None;
				if (pkgPackage.CannotDeleteAndPackUnpackPackage())
				{
					restrictedTrolleyActions |= PackageAction.Delete | PackageAction.PackUnpack;
					reasonMessage = Res.GetString("cee279f8-9a43-47d6-a6fd-fb7a8a1a00d0", "Cannot modify the selected {0} because it has Trolley Job and packed items that are not all picked.", pkgPackage.KP_F3_NKPackType);
				}

				result = new PackageActionStrategy(pkgPackage, restrictedTrolleyActions, reasonMessage);
			}

			return result;
		}

		public void OnPackageDelete(IWhsOrder orderInterface, IPkgPackage packageInterface)
		{
			var order = (WhsOrder)orderInterface;
			var package = (PkgPackage)packageInterface;
			if (pickStrategy.IsPackageAssignedToTrolleyJob(order, package))
			{
				var queryTrolleySlot = new ZQuery();
				queryTrolleySlot.AddToFilter(WhsPickTrolleySlotSchema.WTS_KP_Package, SQLComparisonOperator.Equal, package.PK);

				var trolleySlots = order.Factory.Load<WhsPickTrolleySlot>(queryTrolleySlot);
				trolleySlots.ForEach(t => t.Delete());
			}
		}
	}
}
