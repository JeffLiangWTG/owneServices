using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PartiallyReplenishedPickSplitter : IPartiallyReplenishedPickSplitter
	{
		public WhsPick SplitOrdersFromPartiallyReplenishedPick(WhsPick pickToProcess)
		{
			Argument.NotNull(pickToProcess, nameof(pickToProcess));

			WhsPick newPick = null;
			if (pickToProcess.WP_IsAwaitingReplenishment
				&& pickToProcess.Orders.Count > 1
				&& pickToProcess.Transfers.Count == 0
				&& !pickToProcess.IsWorkOrderPick)
			{
				newPick = SplitPartiallyReplenishedPick(pickToProcess);
				if (newPick != null)
				{
					pickToProcess.Orders.Load();
					newPick.Orders.Load();
					newPick.PickOrdersWithoutAutoAllocate(saveFactory: false);
				}
			}

			return newPick;
		}

		#region SplitPartiallyReplenishedPicks

		WhsPick SplitPartiallyReplenishedPick(WhsPick pick)
		{
			WhsPick resultPick = null;
			var splitParamsDictionary = BuildShouldSplitLookup(pick);
			var ordersToCheck = pick.Orders.Cast<WhsOrder>().Where(o => splitParamsDictionary[(o.WD_OH_Client, o.WD_WW_Whs, o.WD_WSH_SalesChannel)]).ToArray();
			if (ordersToCheck.Length > 0)
			{
				var ordersToSplit = ordersToCheck.Where(o => OrderIsNotAwaitingReplenishment(o, pick)).ToArray();
				if (ordersToSplit.Length > 0)
				{
					resultPick = SplitOrdersFromPick(pick, ordersToSplit);
				}
			}
			return resultPick;
		}

		static Dictionary<(ZGuid WD_OH_Client, ZGuid WD_WW_Whs, ZGuid WD_WSH_SalesChannel), ZBool> BuildShouldSplitLookup(WhsPick partiallyReplenishedPick)
		{
			return IEnumerableExtensions.DistinctBy(partiallyReplenishedPick.Orders.Cast<WhsOrder>(), o => (o.WD_OH_Client, o.WD_WW_Whs, o.WD_WSH_SalesChannel))
				.ToDictionary(
					order => (order.WD_OH_Client, order.WD_WW_Whs, order.WD_WSH_SalesChannel),
					order => order.ClientPickingParams?.WPP_SplitOrdersFromPartiallyReplenishedPicks ?? false);
		}

		static bool OrderIsNotAwaitingReplenishment(WhsOrder order, WhsPick pick)
		{
			var result = true;
			foreach (var line in order.GetLinesToPick().Cast<WhsOrderLine>())
			{
				if (line.WE_ShortfallQuantityCached > 0m && pick.IsLineInventoryAwaitingReplenishment(line))
				{
					result = false;
					break;
				}
			}

			return result;
		}

		WhsPick SplitOrdersFromPick(WhsPick oldPick, IEnumerable<WhsOrder> ordersToSplit)
		{
			var newPick = (WhsPick)oldPick.Clone();
			foreach (var order in ordersToSplit)
			{
				order.WD_WP = newPick.PK;
			}

			oldPick.ReCalculatePercentageComplete();
			newPick.ReCalculatePercentageComplete();

			return newPick;
		}

		#endregion
	}
}
