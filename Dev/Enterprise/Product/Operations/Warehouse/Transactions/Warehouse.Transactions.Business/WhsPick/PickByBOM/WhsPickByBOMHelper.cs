using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsPickByBOMHelper
	{
		#region NewKitReceiveLine

		public static WhsReceiveLine NewKitReceiveLine(BusinessObjectFactory factory, ZGuid receivePK, ZGuid supplierPartPK, decimal quantity)
		{
			var kitInventoryLine = factory.New<WhsReceiveLine>();
			kitInventoryLine.WE_WD = receivePK;
			kitInventoryLine.WE_OP = supplierPartPK;
			kitInventoryLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Pending;
			kitInventoryLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;
			kitInventoryLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;
			kitInventoryLine.WE_ClientOrderedUnits = quantity;
			kitInventoryLine.WE_StockOnHand = quantity;
			return kitInventoryLine;
		}

		#endregion

		#region CreatePickByBOMTransferLinesIfNecessary

		// Tested in PutawayStockInDockDoorOrPackingStationTest.cs
		public static CreatePickByBOMTransferLineResult CreatePickByBOMTransferLinesIfNecessary(
			BusinessObjectFactory factory,
			ZGuid locationPK,
			WhsTransferLine[] inTransitLines,
			ZString rfUser,
			bool isClosingPackagesWhenPutToDockDoor = false)
		{
			var transferLinesToIgnoreWhenSettingLocation = new HashSet<WhsTransferLine>();
			var kitPackages = Enumerable.Empty<PkgPackage>();

			var transferPickLineQuery = new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, inTransitLines.Select(l => l.PK));
			var transferPickLines = factory.Load<WhsPickLine>(transferPickLineQuery);

			var orderPickLineQuery = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inTransitLines.Select(l => l.PK));
			var orderPickLines = factory.Load<WhsPickLine>(orderPickLineQuery);

			var componentOrderLineQuery = new ZQuery(WhsDocketLineSchema.PK, orderPickLines.Select(pl => pl.WZ_WE_TransactionLine));
			componentOrderLineQuery.AddToFilter(WhsDocketLineSchema.WE_WE_ParentDocketLine, SQLComparisonOperator.NotEqual, null);
			componentOrderLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Order);
			var inTransitComponentOrderLines = factory.Load<WhsOrderLine>(componentOrderLineQuery);

			if (inTransitComponentOrderLines.Length > 0)
			{
				var transferLineKitOrderLineMap = orderPickLines.ToDictionary(pl => pl.WZ_WE_InventoryLine, pl => pl.DocketLine.WE_WE_ParentDocketLine);
				var transferPickLinesByKitLinePK = transferPickLines.ToLookup(pl => transferLineKitOrderLineMap[pl.WZ_WE_TransactionLine]);
				var transferLinesByKitLinePK = inTransitLines.ToLookup(l => transferLineKitOrderLineMap[l.PK]);
				var kitOrderLines = IEnumerableExtensions.DistinctBy(inTransitComponentOrderLines, l => l.WE_WE_ParentDocketLine).Select(l => l.ParentLine).Cast<WhsOrderLine>();
				var stagedOrPuttingPickLinesByComponent = new Dictionary<ZGuid, IEnumerable<WhsPickLine>>();
				var kitPickLinesToCheckPackage = new List<WhsPickLine>();
				var splitKitPickLineMap = new Dictionary<ZGuid, (ZDecimal SplitLineUnit, WhsPickLine NewPickLine)>();

				AddFetchHints(factory, kitOrderLines);

				var bomParts = CacheBOMParts(kitOrderLines);
				var kitReceiveLinesMap = GetKitReceiveLinesGroup(factory, inTransitComponentOrderLines);

				foreach (var kitOrderLine in kitOrderLines)
				{
					var kitReceiveLines = kitReceiveLinesMap[kitOrderLine.PK];
					var pickedKitReceiveLines = kitReceiveLines.Where(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Putaway).ToArray();
					var pickedKitCount = pickedKitReceiveLines.Sum(l => l.WE_TransactionQuantity);
					var minKitQtyThatCanBeAssembled = GetMinKitQtyThatCanBeAssembled(kitOrderLine, rfUser, bomParts, stagedOrPuttingPickLinesByComponent);

					var kitQtyAssembledThisTime = minKitQtyThatCanBeAssembled - pickedKitCount;
					if (kitQtyAssembledThisTime > 0)
					{
						UpdatePickByBOMData(kitOrderLine, kitReceiveLines, kitQtyAssembledThisTime);
					}
				}

				void UpdatePickByBOMData(WhsOrderLine kitOrderLine, IEnumerable<WhsReceiveLine> kitReceiveLines, decimal kitQtyAssembledThisTime)
				{
					var putawayPickLines = transferPickLinesByKitLinePK[kitOrderLine.PK];
					var lastPickedPickLine = putawayPickLines.OrderByDescending(pl => pl.WZ_PickedDateTime).First();
					var lastPickedTime = lastPickedPickLine.WZ_PickedDateTime;
					var lastPickedLocationPK = lastPickedPickLine.DocketLine.WE_WL_TransferFrom;

					var notPickedKitReceiveLine = kitReceiveLines.Single(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Pending);
					var kitPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, notPickedKitReceiveLine.PK));
					var kitNotYetPickedQty = notPickedKitReceiveLine.WE_TransactionQuantity - kitQtyAssembledThisTime;
					if (kitNotYetPickedQty > 0m)
					{
						var newKitReceiveLine = NewKitReceiveLine(factory, notPickedKitReceiveLine.WE_WD, notPickedKitReceiveLine.WE_OP, kitNotYetPickedQty);

						notPickedKitReceiveLine.WE_ClientOrderedUnits = kitQtyAssembledThisTime;
						notPickedKitReceiveLine.WE_StockOnHand = kitQtyAssembledThisTime;

						kitPickLines = AssignQuantityAmongUnpickedLines(kitPickLines, newKitReceiveLine, kitQtyAssembledThisTime, splitKitPickLineMap);

						var bomLinks = factory.Load<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, notPickedKitReceiveLine.PK));
						bomLinks.ForEach(link => link.Delete());
						foreach (var componentLine in kitOrderLine.ChildComponentLines)
						{
							var bomPart = bomParts[new BOMPartCacheKey(componentLine.WE_OP, componentLine.WE_F3_NKPackType)];
							CreateBOMLinks(factory, componentLine.PK, notPickedKitReceiveLine.PK, BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, notPickedKitReceiveLine.WE_TransactionQuantity));
							CreateBOMLinks(factory, componentLine.PK, newKitReceiveLine.PK, BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, newKitReceiveLine.WE_TransactionQuantity));
						}
					}

					UpdateKitReceiveLineToPUT(notPickedKitReceiveLine, lastPickedLocationPK, rfUser, lastPickedTime);

					foreach (var kitPickLine in kitPickLines)
					{
						CreateKitTransferLineAndCommitIt(kitPickLine, rfUser, lastPickedTime, locationPK);
					}
					kitPickLinesToCheckPackage.AddRange(kitPickLines);

					var componentTransferLines = transferLinesByKitLinePK[kitOrderLine.PK];
					UpdateComponentTransferLines(componentTransferLines, lastPickedLocationPK);
					transferLinesToIgnoreWhenSettingLocation.UnionWith(componentTransferLines);

					UpdateComponentPickLines(kitOrderLine.ChildComponentLines.Cast<WhsOrderLine>(), stagedOrPuttingPickLinesByComponent, bomParts, rfUser, kitQtyAssembledThisTime);
				}

				if (splitKitPickLineMap.Count > 0)
				{
					RepackKitPickLines(factory, splitKitPickLineMap);
				}

				if (isClosingPackagesWhenPutToDockDoor)
				{
					kitPackages = GetPackagesFromPickLines(factory, kitPickLinesToCheckPackage);
				}
			}

			return new CreatePickByBOMTransferLineResult(transferLinesToIgnoreWhenSettingLocation, kitPackages);
		}

		static void AddFetchHints(BusinessObjectFactory factory, IEnumerable<WhsOrderLine> kitOrderLines)
		{
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, kitOrderLines.Select(l => l.PK)));
		}

		public static Dictionary<BOMPartCacheKey, OrgPartBOM> CacheBOMParts(IEnumerable<WhsOrderLine> kitOrderLines)
		{
			Argument.NotNull(kitOrderLines, nameof(kitOrderLines));

			var partsCached = new HashSet<ZGuid>();
			var bomParts = new Dictionary<BOMPartCacheKey, OrgPartBOM>();
			foreach (var kitOrderLine in kitOrderLines)
			{
				if (partsCached.Add(kitOrderLine.WE_OP))
				{
					foreach (var bomPartDef in kitOrderLine.SupplierPart.BillOfMaterials)
					{
						var key = new BOMPartCacheKey(bomPartDef.OE_OP_Component, bomPartDef.OE_F3_NKPackType);
						if (!bomParts.TryGetValue(key, out var bomPart))
						{
							bomParts[key] = bomPartDef;
						}
					}
				}
			}
			return bomParts;
		}

		static ILookup<ZGuid, WhsReceiveLine> GetKitReceiveLinesGroup(BusinessObjectFactory factory, IEnumerable<WhsOrderLine> inTransitComponentOrderLines)
		{
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, inTransitComponentOrderLines.Select(l => l.PK));
			var links = factory.Load<WhsBOMInventoryPivot>(linkQuery);
			var receiveLineKitPKMap = IEnumerableExtensions.DistinctBy(links, l => l.WIP_WE_InventoryLine).ToDictionary(l => l.WIP_WE_InventoryLine, l => l.ComponentLine.WE_WE_ParentDocketLine);

			var kitReceiveLineQuery = new ZQuery(WhsDocketLineSchema.PK, links.Select(l => l.WIP_WE_InventoryLine));
			return factory.Load<WhsReceiveLine>(kitReceiveLineQuery).ToLookup(l => receiveLineKitPKMap[l.PK]);
		}

		static int GetMinKitQtyThatCanBeAssembled(WhsOrderLine kitOrderLine, ZString rfUser, Dictionary<BOMPartCacheKey, OrgPartBOM> bomParts, Dictionary<ZGuid, IEnumerable<WhsPickLine>> stagedOrPuttingPickLinesByComponent)
		{
			ZInt? minKitQtyThatCanBeAssembled = null;
			foreach (var componentLine in kitOrderLine.ChildComponentLines)
			{
				var stagedOrPuttingLines = componentLine.PickLines.Where(l => l.IsPickedFromPutawayLocation
					&& (l.InventoryLine.IsFinalised || l.InventoryLine.WE_GS_NKPutawayBy == rfUser && l.InventoryLine.WE_DocketLineStatus == DocketLineStatus.Codes.HeldForTransfer)).ToArray();
				var stagedOrPuttingQty = stagedOrPuttingLines.Sum(l => l.WZ_Units);
				stagedOrPuttingPickLinesByComponent[componentLine.PK] = stagedOrPuttingLines;

				var bomPart = bomParts[new BOMPartCacheKey(componentLine.WE_OP, componentLine.WE_F3_NKPackType)];
				var kitQtyThatCanBeAssembled = BOMComponentQuantityHelper.GetNumberOfPossibleKitsFromComponent(bomPart, stagedOrPuttingQty);
				minKitQtyThatCanBeAssembled = minKitQtyThatCanBeAssembled == null ? kitQtyThatCanBeAssembled : Math.Min(kitQtyThatCanBeAssembled, minKitQtyThatCanBeAssembled.Value);
			}
			return minKitQtyThatCanBeAssembled.Value;
		}

		static WhsPickLine[] AssignQuantityAmongUnpickedLines(WhsPickLine[] kitPickLines, WhsReceiveLine newKitReceiveLine, decimal kitQtyToAssembled, Dictionary<ZGuid, (ZDecimal, WhsPickLine)> splitKitPickLineMap)
		{
			var pickedLines = new List<WhsPickLine>();

			foreach (var kitPickLine in kitPickLines.OrderByDescending(l => (!l.IsUnpacked(l.Factory), l.WZ_Units)))
			{
				if (kitQtyToAssembled > 0m)
				{
					pickedLines.Add(kitPickLine);

					if (kitPickLine.WZ_Units > kitQtyToAssembled)
					{
						var newPickLine = kitPickLine.Split(kitPickLine.WZ_Units - kitQtyToAssembled);
						newPickLine.WZ_WE_InventoryLine = newKitReceiveLine.PK;
						splitKitPickLineMap[kitPickLine.PK] = (kitPickLine.WZ_Units, newPickLine);
						kitQtyToAssembled = 0m;
					}
					else
					{
						kitQtyToAssembled -= kitPickLine.WZ_Units;
					}
				}
				else
				{
					kitPickLine.WZ_WE_InventoryLine = newKitReceiveLine.PK;
				}
			}
			return pickedLines.ToArray();
		}

		public static void UpdateKitReceiveLineToPUT(WhsDocketLine kitReceiveLine, ZGuid lastPickedLocationPK, ZString lastPickedBy, ZDateTimeOffset lastPickedTimeOffset)
		{
			Argument.NotNull(kitReceiveLine, nameof(kitReceiveLine));

			kitReceiveLine.WE_WL = lastPickedLocationPK;
			kitReceiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;
			kitReceiveLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;
			kitReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;
			kitReceiveLine.WE_AdjustmentArrivalDate = lastPickedTimeOffset;
			kitReceiveLine.WE_UnloadedTime = lastPickedTimeOffset;
			kitReceiveLine.WE_GS_NKUnloadedBy = lastPickedBy;
		}

		static void CreateKitTransferLineAndCommitIt(WhsPickLine kitPickLine, ZString lastPickedBy, ZDateTimeOffset lastPickedTime, ZGuid transferToLocationPK)
		{
			kitPickLine.WZ_PickedDateTime = lastPickedTime;
			kitPickLine.WZ_GS_NKAssignedTo = lastPickedBy;
			var outboundDockDoorTransferCreator = new OutboundDockDoorTransferCreator();
			var kitTransferLine = outboundDockDoorTransferCreator.CreateOutboundDockDoorTransfer(kitPickLine);
			kitTransferLine.WE_WL = transferToLocationPK;
			kitTransferLine.FinaliseDocketLine();
		}

		static void UpdateComponentTransferLines(IEnumerable<WhsTransferLine> componentTransferLines, ZGuid lastPickedLocationPK)
		{
			foreach (var componentTransferLine in componentTransferLines)
			{
				componentTransferLine.WE_WL = lastPickedLocationPK;
			}
		}

		static void UpdateComponentPickLines(
			IEnumerable<WhsOrderLine> componentOrderLines,
			Dictionary<ZGuid, IEnumerable<WhsPickLine>> stagedOrPuttingPickLines,
			Dictionary<BOMPartCacheKey, OrgPartBOM> bomParts,
			ZString lastPickedBy,
			decimal kitQtyToAssemble)
		{
			foreach (var componentOrderLine in componentOrderLines)
			{
				var bomPart = bomParts[new BOMPartCacheKey(componentOrderLine.WE_OP, componentOrderLine.WE_F3_NKPackType)];
				var componentQtyToPick = BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, kitQtyToAssemble);
				var pickLines = stagedOrPuttingPickLines[componentOrderLine.PK].OrderByDescending(l => l.WZ_Units);
				foreach (var pickLine in pickLines.Where(pl => !pl.IsPicked))
				{
					if (pickLine.WZ_Units > componentQtyToPick)
					{
						pickLine.Split(pickLine.WZ_Units - componentQtyToPick);
					}
					pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
					pickLine.WZ_GS_NKAssignedTo = lastPickedBy;

					componentQtyToPick -= pickLine.WZ_Units;
					if (componentQtyToPick <= 0)
					{
						break;
					}
				}
			}
		}

		static void CreateBOMLinks(BusinessObjectFactory factory, ZGuid componentLinkePK, ZGuid receiveLinePK, decimal quantity)
		{
			var link = factory.New<WhsBOMInventoryPivot>();
			link.WIP_WE_ComponentLine = componentLinkePK;
			link.WIP_WE_InventoryLine = receiveLinePK;
			link.WIP_ComponentQuantity = quantity;
		}

		static void RepackKitPickLines(BusinessObjectFactory factory, Dictionary<ZGuid, (ZDecimal, WhsPickLine)> splitKitPickLineMap)
		{
			var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, splitKitPickLineMap.Keys.ToArray());
			var packedKitLineDivots = factory.Load<PkgPackageItemDivot>(query);
			if (packedKitLineDivots.Length > 0)
			{
				foreach (var divot in packedKitLineDivots)
				{
					var (splitLineUnit, newPickLine) = splitKitPickLineMap[divot.KI_ParentID];
					NewDivot(divot.KI_KP_Package, divot.KI_ParentID, splitLineUnit);
					NewDivot(divot.KI_KP_Package, newPickLine.PK, newPickLine.WZ_Units);
					divot.Delete();
				}
			}

			void NewDivot(ZGuid packagePK, ZGuid pickLinePK, ZDecimal packedQty)
			{
				var newDivot = factory.New<PkgPackageItemDivot>();
				newDivot.KI_KP_Package = packagePK;
				newDivot.KI_ParentID = pickLinePK;
				newDivot.KI_PackedQty = packedQty;
				newDivot.KI_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			}
		}

		static PkgPackage[] GetPackagesFromPickLines(BusinessObjectFactory factory, List<WhsPickLine> kitPickLinesToCheckPackage)
		{
			var divotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_KP_Package);
			divotSubQuery.AddToFilter(PkgPackageItemDivotSchema.KI_ParentID, kitPickLinesToCheckPackage.Select(l => l.PK));

			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			packageQuery.AddSubQuery(divotSubQuery, JoinCondition.And);
			return factory.Load<PkgPackage>(packageQuery);
		}

		#endregion

		#region AddFetchHintsForIsPickByBOMKitPickLine

		public static void AddFetchHintsForIsPickByBOMKitPickLine(BusinessObjectFactory factory, IEnumerable<WhsPickLine> pickLines)
		{
			var inventoryPKs = pickLines.Select(pl => pl.InventoryLinePKForAvailableInventory).Distinct();

			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.PK, inventoryPKs));

			var inventorySubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			inventorySubQuery.AddToFilter(WhsDocketLineSchema.PK, inventoryPKs);

			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddSubQuery(inventorySubQuery, JoinCondition.And);
			factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);
		}

		#endregion
	}
}
