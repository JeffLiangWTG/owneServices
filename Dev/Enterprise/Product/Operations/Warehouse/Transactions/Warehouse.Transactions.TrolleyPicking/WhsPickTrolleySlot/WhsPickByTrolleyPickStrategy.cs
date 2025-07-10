using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsPickByTrolleyPickStrategy : IWhsPickByTrolleyPickStrategy
	{
		public void OnFinalised(BusinessObjectFactory factory, IWhsPick pick)
		{
		}

		public bool IsOrderActionAllowed(IWhsOrder order, PickOrderAction action, out ErrorNotification reasonNotAllowed)
		{
			var isOrderAssociatedWithTrolleyJob = IsOrderAssociatedWithTrolleyJob(order);
			switch (action)
			{
				case PickOrderAction.DetachOrder when isOrderAssociatedWithTrolleyJob:
				case PickOrderAction.AttachOrder when isOrderAssociatedWithTrolleyJob:
					reasonNotAllowed = new ErrorNotification(OrderErrorTypes.CannotPerformThisOperationBecauseOrderHasPackagesAssignedToTrolleyJob);
					return false;
				default:
					reasonNotAllowed = null;
					return true;
			}
		}

		bool IsOrderAssociatedWithTrolleyJob(IWhsOrder order)
		{
			var result = false;
			if (order != null)
			{
				result = HasAnyPackageAssignedToATrolleyJob(order);
			}
			return result;
		}

		#region HasAnyPackageAssignedToATrolleyJob

		public bool HasAnyPackageAssignedToATrolleyJob(IWhsOrder order)
		{
			var whsOrder = (WhsOrder)order;
			return GetTrolleyJobForPackage(whsOrder, null, false) != null;
		}

		public bool IsPackageAssignedToTrolleyJob(WhsOrder order, PkgPackage package)
		{
			return GetTrolleyJobForPackage(order, package, false) != null;
		}

		IWhsPickTrolleyJob GetTrolleyJobForPackage(WhsOrder order, PkgPackage package, bool activeTrolleysOnly = true)
		{
			var packagesPKs = package == null ? order.PackageJob?.Packages.Select(p => p.PK) : new ZGuid[] { package.PK };
			IWhsPickTrolleyJob result = null;
			if (packagesPKs != null)
			{
				var packageIsInDatabase = package == null || package.IsInDatabase;

				var slotQuery = new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, packagesPKs) { FetchOnlyFromLocalCache = !packageIsInDatabase };
				var slots = order.Factory.Load<IWhsPickTrolleySlot>(slotQuery);
				var queryTrolleyJob = new ZQuery(WhsPickTrolleyJobSchema.PK, slots.Select(s => s.WTS_WTJ_TrolleyJob).Distinct()) { FetchOnlyFromLocalCache = !packageIsInDatabase };

				if (activeTrolleysOnly)
				{
					queryTrolleyJob.AddToFilter(WhsPickTrolleyJobSchema.WTJ_Status, SQLComparisonOperator.NotEqual, "FIN");
				}

				result = order.Factory.LoadTop1<IWhsPickTrolleyJob>(queryTrolleyJob);
			}
			return result;
		}

		#endregion
	}
}
