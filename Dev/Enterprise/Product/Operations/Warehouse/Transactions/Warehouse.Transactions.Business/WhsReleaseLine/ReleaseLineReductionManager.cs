using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class ReleaseLineReductionManager
	{
		public static ActionResult ReduceStock(WhsReleaseLine releaseLine, ZDecimal quantityToReduce, IPickedStockAdjustersFactory pickedStockAdjustersFactory, ReduceStockReason reduceStockReason)
		{
			Argument.NotNull(releaseLine, nameof(releaseLine));
			Argument.NotNull(quantityToReduce, nameof(quantityToReduce));
			Argument.NotNull(pickedStockAdjustersFactory, nameof(pickedStockAdjustersFactory));

			var factory = new BusinessObjectFactory();
			var order = factory.Load<WhsOrder>(releaseLine.OrderPK);
			var pickedStockAdjuster = pickedStockAdjustersFactory.GetNewAdjuster(order.Warehouse, order.Client, reduceStockReason);
			pickedStockAdjuster.LinkToDocket(order);
			var result = ReduceStockCore(releaseLine, quantityToReduce, pickedStockAdjuster, pickedStockAdjustersFactory.GetDescription(reduceStockReason), factory);

			if (result.IsSuccess)
			{
				var (adjusterPreSaveResult, pickCriticalChangesVersionID) = AdjusterSave(pickedStockAdjuster, order, factory);
				result = adjusterPreSaveResult;

				if (result.IsSuccess)
				{
					releaseLine.Pick.OrderedInventories.GetOrderedInventoryForLine(releaseLine.OrderLine).ClearPickLinesCache();
					ChangePickVersionOfPickRow(releaseLine.Pick, pickCriticalChangesVersionID);
					ResetOrderLine(releaseLine.OrderLine);
				}
			}

			return result;
		}

		public static ActionResult ReduceStock(IEnumerable<WhsReleaseLine> releaseLines, IPickedStockAdjustersFactory pickedStockAdjustersFactory, ReduceStockReason reduceStockReason)
		{
			Argument.NotNull(releaseLines, nameof(releaseLines));
			Argument.NotNull(pickedStockAdjustersFactory, nameof(pickedStockAdjustersFactory));

			var result = IsOkToReduceStock(releaseLines, pickedStockAdjustersFactory, reduceStockReason);

			if (result.IsSuccess)
			{
				var firstReleaseLine = releaseLines.First();
				var pick = firstReleaseLine.Pick;
				var factory = new BusinessObjectFactory();

				var order = factory.Load<WhsOrder>(firstReleaseLine.OrderPK);
				var pickedStockAdjuster = pickedStockAdjustersFactory.GetNewAdjuster(order.Warehouse, order.Client, reduceStockReason);
				pickedStockAdjuster.LinkToDocket(order);

				foreach (var releaseLine in releaseLines)
				{
					var items = releaseLine.ParentCollection.GetPackableItems(releaseLine).OfType<IReducibleItem>().Where(item => item.CanBeReduced).ToArray();
					var quantityToReduce = Math.Min(items.Sum(i => i.Quantity), (releaseLine.Quantity - releaseLine.GetPackedQty()));
					result = ReduceStockCore(releaseLine, quantityToReduce, pickedStockAdjuster, pickedStockAdjustersFactory.GetDescription(reduceStockReason), factory);
					if (!result.IsSuccess)
					{
						break;
					}
				}

				if (result.IsSuccess)
				{
					var (adjusterPreSaveResult, pickCriticalChangesVersionID) = AdjusterSave(pickedStockAdjuster, order, factory);
					result = adjusterPreSaveResult;

					if (result.IsSuccess)
					{
						pick.OrderedInventories.GetOrderedInventoryForLine(firstReleaseLine.OrderLine).ClearPickLinesCache();
						ChangePickVersionOfPickRow(firstReleaseLine.Pick, pickCriticalChangesVersionID);
						ResetOrderLine(firstReleaseLine.OrderLine);
					}
				}
			}

			return result;
		}

		public static ActionResult ReduceStock(IEnumerable<WhsOrderLine> orderLines, IPickedStockAdjustersFactory pickedStockAdjustersFactory, ReduceStockReason reduceStockReason)
		{
			Argument.NotNull(orderLines, nameof(orderLines));
			Argument.NotNull(pickedStockAdjustersFactory, nameof(pickedStockAdjustersFactory));

			var result = IsOkToReduceStock(orderLines, pickedStockAdjustersFactory, reduceStockReason);

			if (result.IsSuccess)
			{
				var factory = new BusinessObjectFactory();
				var firstOrderLine = orderLines.First();
				var pick = firstOrderLine.PickLines[0].Pick;
				var order = factory.Load<WhsOrder>(firstOrderLine.WE_WD);
				var pickedStockAdjuster = pickedStockAdjustersFactory.GetNewAdjuster(order.Warehouse, order.Client, reduceStockReason);
				pickedStockAdjuster.LinkToDocket(order);

				foreach (var orderLine in orderLines)
				{
					var releaseLines = orderLine.ReleaseLines;

					foreach (WhsReleaseLine releaseLine in releaseLines)
					{
						var items = releaseLine.ParentCollection.GetPackableItems(releaseLine).OfType<IReducibleItem>().Where(item => item.CanBeReduced).ToArray();
						var quantityToReduce = Math.Min(items.Sum(i => i.Quantity), (releaseLine.Quantity - releaseLine.GetPackedQty()));
						result = ReduceStockCore(releaseLine, quantityToReduce, pickedStockAdjuster, pickedStockAdjustersFactory.GetDescription(reduceStockReason), factory);
						if (!result.IsSuccess)
						{
							break;
						}
					}

					if (result.IsSuccess)
					{
						pick.OrderedInventories.GetOrderedInventoryForLine(orderLine).ClearPickLinesCache();
						ResetOrderLine(orderLine);
					}
				}

				if (result.IsSuccess)
				{
					var (adjusterPreSaveResult, pickCriticalChangesVersionID) = AdjusterSave(pickedStockAdjuster, order, factory);
					result = adjusterPreSaveResult;

					if (result.IsSuccess)
					{
						ChangePickVersionOfPickRow(pick, pickCriticalChangesVersionID);
					}
				}
			}

			return result;
		}

		static ActionResult IsOkToReduceStock(WhsReleaseLine releaseLine, ZDecimal quantityToAdjust, ZDecimal pickLineQuantity, IMultilingualString functionDescription)
		{
			var result = releaseLine.IsValidToReduceQuantityMet(functionDescription);
			if (result.IsSuccess)
			{
				if (quantityToAdjust <= 0)
				{
					result = ActionResult.Failure(Res.GetString("71231BC2-6F7F-4518-A9DC-10FC0368BDA1", "Quantity must be a positive value."));
				}
				else if (quantityToAdjust > releaseLine.Quantity)
				{
					result = ActionResult.Failure(Res.GetString("C6B9F180-2DE1-4069-9CC9-808D682916BD", "Quantity must not be greater that Quantity Met."));
				}
				else if (releaseLine.Quantity - quantityToAdjust < releaseLine.GetPackedQty())
				{
					result = ActionResult.Failure(Res.GetString("0F81A086-93F5-440F-B54F-A7F919FC8543", "Not enough units are unpacked. Unpack goods first."));
				}
				else if (pickLineQuantity < quantityToAdjust)
				{
					var message = releaseLine.IsBOMProductPickedOnSalesOrder
						? Res.GetString("c69d8469-1ac6-4936-a733-50c4366f2181", "Quantity ({0}) cannot be greater than Quantity of Kits Picked ({1}).", quantityToAdjust, pickLineQuantity)
						: Res.GetString("9DDEC493-EF58-4339-9E4B-316D80BEC7CD", "Quantity ({0}) cannot be greater than Quantity Picked ({1}).", quantityToAdjust, pickLineQuantity);

					result = ActionResult.Failure(message);
				}
			}

			return result;
		}

		static ActionResult IsOkToReduceStock(IEnumerable<WhsReleaseLine> releaseLines, IPickedStockAdjustersFactory pickedStockAdjustersFactory, ReduceStockReason reduceStockReason)
		{
			var result = ActionResult.Failure(Res.GetString("8575dc31-2edb-44ba-9888-2faa20b0daad", "No Release Line to reduce stock."));

			foreach (var releaseLine in releaseLines)
			{
				var items = releaseLine.ParentCollection.GetPackableItems(releaseLine).OfType<IReducibleItem>().Where(item => item.CanBeReduced).ToArray();
				var quantity = items.Sum(i => i.Quantity);
				result = IsOkToReduceStock(releaseLine, quantity, quantity, pickedStockAdjustersFactory.GetDescription(reduceStockReason));

				if (!result.IsSuccess)
				{
					break;
				}
			}

			return result;
		}

		static ActionResult IsOkToReduceStock(IEnumerable<WhsOrderLine> orderLines, IPickedStockAdjustersFactory pickedStockAdjustersFactory, ReduceStockReason reduceStockReason)
		{
			var result = ActionResult.Failure(Res.GetString("ed1d15f0-925a-4c76-8722-4558a1a7a409", "No Order Lines were selected for reduction."));

			foreach (var orderLine in orderLines)
			{
				var releaseLines = orderLine.ReleaseLines;

				if (releaseLines.Count > 0)
				{
					result = IsOkToReduceStock(releaseLines.Cast<WhsReleaseLine>(), pickedStockAdjustersFactory, reduceStockReason);

					if (!result.IsSuccess)
					{
						break;
					}
				}
				else
				{
					result = ActionResult.Failure(Res.GetString("a416309a-fb4e-4e0e-b65b-e12d94f0e4ba", "One or more Order Lines have no Release Line."));
					break;
				}
			}

			return result;
		}

		static ActionResult ReduceStockCore(WhsReleaseLine releaseLine, ZDecimal quantityToReduce, IPickedStockAdjuster pickedStockAdjuster, IMultilingualString reduceStockReason, BusinessObjectFactory factory)
		{
			var items = releaseLine.ParentCollection.GetPackableItems(releaseLine).OfType<IReducibleItem>().Where(item => item.CanBeReduced).ToArray();
			var result = IsOkToReduceStock(releaseLine, quantityToReduce, items.Sum(i => i.Quantity), reduceStockReason);

			if (result.IsSuccess)
			{
				result = WhsReleaseLine.AdjustOutInventoryAndSaveInOtherFactory(releaseLine.OrderLinePK, WhsReleaseLineCollection.GetKey(releaseLine), quantityToReduce, pickedStockAdjuster, reduceStockReason, factory);
			}

			return result;
		}

		internal static (ActionResult Result, ZGuid? PickCriticalChangesVersionID) AdjusterSave(IPickedStockAdjuster pickedStockAdjuster, WhsOrder order, BusinessObjectFactory factory)
		{
			Argument.NotNull(pickedStockAdjuster, nameof(pickedStockAdjuster));
			Argument.NotNull(order, nameof(order));
			Argument.NotNull(factory, nameof(factory));

			ZGuid? pickCriticalChangesVersionID = null;
			var result = pickedStockAdjuster.PrepareForSaving();
			if (result.IsSuccess)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
				pickCriticalChangesVersionID = order.Pick.WP_CriticalChangesVersionID;
			}

			return (result, pickCriticalChangesVersionID);
		}

		static void ResetOrderLine(WhsPickableDocketLine orderLine)
		{
			orderLine.PickLines.ForEach(pl => pl.HasChanges = false); // this hack is required as other factory will mark collection in this factory as Changed because of the deletion of RCAs
			orderLine.HasChanges = false;
			orderLine.ClearReleaseLines();
		}

		static void ChangePickVersionOfPickRow(WhsPick pick, ZGuid? pickVersionID)
		{
			var pickRow = ((IBusinessObjectInternals)pick).Row;
			pickRow[WhsPickSchema.Constants.WP_CriticalChangesVersionID] = pickVersionID.Value.ToGuid();
			pickRow.AcceptChanges();
		}
	}
}
