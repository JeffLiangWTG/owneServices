using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class OutboundDockDoorTransferCreator : IOutboundDockDoorTransferCreator
	{
		public WhsTransferLine CreateOutboundDockDoorTransfer(WhsPickLine pickLine)
		{
			Argument.NotNull(pickLine, nameof(pickLine));
			WhsTransferLine transferLine = null;

			if (pickLine.IsPickedInMemory)
			{
				var pickableDocketLine = pickLine.DocketLine as WhsPickableDocketLine;
				var pickableDocket = pickableDocketLine?.PickableDocket;
				var pick = pickableDocket?.Pick;
				if (pick != null && !pick.IsFinalised && !IsPickedComponentLineOnSalesOrder(pickableDocketLine))
				{
					var transfer = GetOrCreateTransfer(pickableDocket, pick);

					var inventoryLine = pickLine.InventoryLine;
					transferLine = transfer.Lines.AddNew();

					// suspend validation purely for performance
					using (transferLine.GetValidationSuspender())
					{
						transferLine.WE_OP = inventoryLine.WE_OP;
						transferLine.WE_F3_NKPackType = pickableDocketLine.WE_F3_NKPackType; // take pack type from order line
						transferLine.SetDocketLineFromInventory(inventoryLine, ExcludeFromCopy.CustomAttribs | ExcludeFromCopy.PackType | ExcludeFromCopy.Qty);

						transferLine.WE_TransactionQuantity = pickLine.WZ_Units;
						transferLine.WE_GS_NKPutawayBy = pickLine.WZ_GS_NKAssignedTo;

						transferLine.WE_WL = GetDestinationLocation(pick, pickableDocket, pickableDocketLine);

						var palletId = pickLine.Inventory.WI_PalletID;
						if (!palletId.IsEmpty)
						{
							var fullyPickedPalletIDs = GetFullyPickedPalletIDCache(pick);
							if (fullyPickedPalletIDs.Contains(palletId))
							{
								transferLine.WE_PalletID = palletId;
							}
						}
					}

					var originalInventoryLinePK = pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid
						? pickLine.WZ_WE_OriginalPickedInventoryLine
						: pickLine.WZ_WE_InventoryLine;

					UpdatePickLines(pickLine, originalInventoryLinePK, transferLine, pickableDocketLine.PK);

					transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer;
					transferLine.CreateInTransitInventory();

					// need to update the Picker Details on the Available Inventory
					pick.ClearPickLinesCacheForInventory(pickableDocketLine, originalInventoryLinePK);

					if (transferLine.WE_CurrentInventoryStatus != InventoryStatus.Codes.InTransit)
					{
						throw new InvalidOperationException("This should never happen.");
					}
				}
			}

			return transferLine;

			bool IsPickedComponentLineOnSalesOrder(WhsPickableDocketLine pickableDocketLine)
			{
				return pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid && pickableDocketLine.IsComponentLineOnSalesOrder;
			}
		}

		#region CacheIsPalletFullyPicked

		static HashSet<string> GetFullyPickedPalletIDCache(WhsPick pick)
		{
			return pick.Factory.GetCachedValue(FormattableString.Invariant($"OutboundDockDoorTransferCreator|FullyPickedPalletIDCache|{pick.PK}"), () => CacheFullyPickedPalletID(pick), CacheStalenessPolicy.StaleBeforeFactorySavingTransaction); // Key used in Factory Cache
		}

		static HashSet<string> CacheFullyPickedPalletID(WhsPick pick)
		{
			var factory = pick.Factory;
			var palletIDCache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var relatedInventory = pick.GetAllPickLines().Where(pl => pl.IsPickedInMemory).Select(pl => pl.Inventory).Where(i => !i.WI_PalletID.IsEmpty).Distinct();
			if (relatedInventory.Any())
			{
				var fullyPickedPalletIDList = GetFullyPickedPalletIDFromMemory(factory, pick.WP_WW_Whs, relatedInventory);
				if (fullyPickedPalletIDList.Any())
				{
					var notFullyPickedPalletIDList = GetUnFullyPickedPalletIDFromDB(factory, pick.WP_WW_Whs, relatedInventory.Select(i => i.PK).ToArray(), fullyPickedPalletIDList.ToArray());
					foreach (var palletID in Enumerable.Except(fullyPickedPalletIDList, notFullyPickedPalletIDList, StringComparer.OrdinalIgnoreCase))
					{
						palletIDCache.Add(palletID);
					}
				}
			}

			return palletIDCache;
		}

		static IEnumerable<string> GetUnFullyPickedPalletIDFromDB(BusinessObjectFactory factory, ZGuid whsPK, ZGuid[] inventoryPKsInMemory, string[] fullyPickedPalletIDs)
		{
			var sql = FormattableString.Invariant($@"
SELECT 
	DISTINCT WI_PalletID
FROM
	dbo.WhsInventoryView
WHERE
	WI_WW_WHS = @WhsPK AND
	WI_TotalUnits > 0 AND
	WI_PK NOT IN (SELECT value FROM @InventoryPKs) AND
	WI_PalletID IN (SELECT value FROM @PalletIDs) AND
	WI_PalletID <> ''");

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WhsPK", whsPK, WhsInventoryViewSchema.WI_WW_Whs);
			sqlParams.Add(ZSqlParameter.New("@InventoryPKs", inventoryPKsInMemory, WhsInventoryViewSchema.PK, true));
			sqlParams.Add(ZSqlParameter.New("@PalletIDs", fullyPickedPalletIDs, WhsInventoryViewSchema.WI_PalletID, true));

			var palletIDCollection = new DynamicBusinessObjectCollection(factory);
			palletIDCollection.Load(sql, sqlParams);

			return palletIDCollection.Select(p => ((ZString)p[WhsInventoryViewSchema.Constants.WI_PalletID]).ToString());
		}

		static IEnumerable<string> GetFullyPickedPalletIDFromMemory(BusinessObjectFactory factory, ZGuid whsPK, IEnumerable<WhsInventoryView> relatedInventory)
		{
			var query = new ZQuery(WhsInventoryViewSchema.WI_PalletID, relatedInventory.Select(i => i.WI_PalletID).Distinct()) { FetchOnlyFromLocalCache = true };
			var inventoryByPalletIDsInMemory = factory.Load<WhsInventoryView>(query);

			return inventoryByPalletIDsInMemory.Where(i => i.WI_WW_Whs == whsPK).GroupBy(i => i.WI_PalletID.ToUpper()).Where(g => g.All(i => i.WI_TotalUnits == 0)).Select(g => g.Key.ToString());
		}

		#endregion

		static WhsTransfer GetOrCreateTransfer(WhsPickableDocket pickableDocket, WhsPick pick)
		{
			var query = new ZQuery(WhsDocketSchema.WD_OH_Client, pickableDocket.WD_OH_Client);
			query.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketLineStatus.Codes.Finalised);

			var inTransitTransfer = pick.Transfers.Find(query).FirstOrDefault();
			if (inTransitTransfer == null)
			{
				inTransitTransfer = pick.Transfers.AddNew();
				inTransitTransfer.WD_OH_Client = pickableDocket.WD_OH_Client;
				inTransitTransfer.WD_WW_Whs = pickableDocket.WD_WW_Whs;
				inTransitTransfer.WD_ExternalReference = pick.WP_PickNo;
			}

			return inTransitTransfer;
		}

		static ZGuid GetDestinationLocation(WhsPick pick, WhsPickableDocket pickableDocket, WhsPickableDocketLine pickableDocketLine)
		{
			var result = ZGuid.Empty;

			if (pick.IsWorkOrderPick)
			{
				result = WorkOrderStagingLocationHelper.GetStagingLocationForWorkOrderLine(pickableDocket.Warehouse, (WhsComponentOrderLine)pickableDocketLine);
			}
			else
			{
				result = pick.DockDoorPK;
			}

			return result;
		}

		static void UpdatePickLines(WhsPickLine pickLine, ZGuid originalInventoryLinePK, WhsTransferLine transferLine, ZGuid originalOrderLinePK)
		{
			// don't want to clone the Originally Picked InventoryLine as it should only be on the Last PickLine attached to the OrderLine.
			var excludedColumns = new[] { WhsPickLineSchema.Constants.WZ_WE_TransactionLine, WhsPickLineSchema.Constants.WZ_WE_OriginalPickedInventoryLine, WhsPickLineSchema.Constants.WZ_ReleaseCapturedPartAttrib1, WhsPickLineSchema.Constants.WZ_ReleaseCapturedPartAttrib2, WhsPickLineSchema.Constants.WZ_ReleaseCapturedPartAttrib3, WhsPickLineSchema.Constants.WZ_ReleaseCapturedSerialNumber };
			var newPickLine = (WhsPickLine)pickLine.Clone(new BusinessObjectCloneArgs(excludedColumns));
			newPickLine.WZ_WE_TransactionLine = transferLine.PK;
			newPickLine.WZ_WE_OriginalOrderLine = originalOrderLinePK;

			using (((IWhsPickLineInternals)pickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			}

			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = originalInventoryLinePK;
			pickLine.WZ_GS_NKAssignedTo = "";
			pickLine.WZ_P9_Task = ZGuid.Empty;

			if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				CopySerialNumber(pickLine, newPickLine);
			}
		}

		static void CopySerialNumber(WhsPickLine pickLine, WhsPickLine newPickLine)
		{
			var query = new ZQuery(WhsSerialNumberPivotSchema.WSV_ParentID, newPickLine.WZ_WE_InventoryLine);
			query.AddToFilter(WhsSerialNumberPivotSchema.WSV_WZ_PickingLine, pickLine.PK);
			var factory = pickLine.Factory;

			var pivots = factory.Load<WhsSerialNumberPivot>(query);
			foreach (var pivot in pivots)
			{
				CopySerialNumberPivotToTransfer();

				pivot.WSV_WZ_PickingLine = newPickLine.PK;

				void CopySerialNumberPivotToTransfer()
				{
					var pivotTransfer = (WhsSerialNumberPivot)pivot.Clone();
					pivotTransfer.WSV_ParentID = pickLine.WZ_WE_InventoryLine;
					pivotTransfer.WSV_WZ_PickingLine = pickLine.PK;
				}
			}
		}
	}
}
