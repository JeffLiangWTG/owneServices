using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	static class PickLineUpdater
	{
		#region PackPickLineForAttributeNeutral

		internal static void PackPickLineForAttributeNeutral(WhsPickLine pickLine, WhsReleaseLine releaseLine, PkgPackage packageToPackInto)
		{
			Argument.NotNull(pickLine, nameof(pickLine));
			Argument.NotNull(releaseLine, nameof(releaseLine));
			Argument.NotNull(packageToPackInto, nameof(packageToPackInto));

			if (pickLine.WZ_Units != 1m)
			{
				throw new InvalidOperationException("Must pack Serial Numbers in Units of 1.");
			}

			if (!pickLine.IsUnpacked(pickLine.Factory))
			{
				throw new InvalidOperationException("Pick Line to Pack for Attribute Neutral should be unpacked.");
			}

			packageToPackInto.Pack(pickLine, releaseLine);
		}

		#endregion

		#region ConfirmPickLinesPickedQty

		internal static IEnumerable<Guid> ConfirmPickLinesPickedQty(
			WhsPickLine[] lines,
			PickingInfo pickingInfo,
			GlbStaff picker,
			PickLinesToPickedPackTypeInfo[] pickLinesToPickedPackTypes,
			PkgPackage packageToPackInto,
			bool createPackagesForPickedPacks)
		{
			var pickedQty = pickingInfo.PickedQty;
			var isPicking = !pickingInfo.ShouldSplit || pickedQty > 0m;
			var nonPickedLines = isPicking ? lines.Where(l => !l.IsPickedFromPutawayLocation).ToArray() : Array.Empty<WhsPickLine>();

			var releaseCapturedInfos = pickLinesToPickedPackTypes.SelectMany(l => l.PickedPackTypes.SelectMany(p => p.ReleaseCapturedInfos)).ToArray();
			ValidateParametersBeforePickReleaseCaptureOrPack(nonPickedLines, pickedQty, isPicking, picker, releaseCapturedInfos);

			var pickLinesQty = nonPickedLines.Sum(l => l.WZ_Units);
			var pickableDocketLines = nonPickedLines.Select(l => l.DocketLine).Cast<WhsPickableDocketLine>().ToArray();

			// we only need the pick when we are actually picking some units
			var pick = isPicking
				? pickableDocketLines[0].PickableDocket.Pick
				: null;

			var pickWasShorted = pickedQty < pickLinesQty;
			var pickLinesSortedByUnits = new Queue<WhsPickLine>(nonPickedLines.OrderByDescending(l => l.WZ_Units));
			var shortedOrderLinePKs = new HashSet<Guid>(pickLinesSortedByUnits.Count);
			var shortedPickByBOMKitLines = new HashSet<WhsOrderLine>();

			// we need to store how the packages were previously packed to know how to re-pack them, this is to allow release-capture and trolley picking to work.
			var pickLinePacking = GetPackedInfo(picker.Factory, nonPickedLines, pick?.IsWorkOrderPick ?? false);

			var shortedInventories = new HashSet<WhsInventoryView>();
			var pickDateTime = ZDateTimeOffset.Now;
			IReadOnlyList<WhsPickLine> pickedPickLines = AllocateConfirmedQtyAlongPicklines(pickLinesSortedByUnits, shortedInventories, pick, picker, pickedQty, pickingInfo.IsVerifiedNonEmpty, pickingInfo.ShouldSplit, shortedOrderLinePKs, shortedPickByBOMKitLines).ToArray();

			if (pickWasShorted)
			{
				// Allocated Pack Types could split PickLines 
				// UpdateAllocatedPackTypes returns all pick lines
				pickedPickLines = UpdateAllocatedPackTypes(pick, pickedPickLines);
			}

			var newlySplitPickLines = new Dictionary<WhsPickLine, List<WhsPickLine>>();
			using (pick?.SuspendBuildingAllReleaseLinesForPick()) // we don't want to build release lines for the whole pick, just the order lines we are looking at
			{
				DeleteUnpickedPickLinesThatWereShorted(picker, shortedInventories, pickLinesSortedByUnits, pickingInfo, shortedOrderLinePKs, shortedPickByBOMKitLines);

				// lazy because this needs to be evaluated after release captured attribs are captured and so that the result is cached
				var parentOrderLines = isPicking && !pick.IsWorkOrderPick
					? pickableDocketLines.Where(l => l.WE_WE_ParentDocketLine.IsValid).DistinctBy(l => l.WE_WE_ParentDocketLine).Select(l => l.ParentLine) : Enumerable.Empty<WhsPickableDocketLine>();
				var allReleaseLines = new Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>>(() => PackageHelper.GetReleaseLinesByKey(pickableDocketLines.Distinct().Union(parentOrderLines).ToArray()));
				IReadOnlyCollection<WhsPickLine> newOrUpdatedKitPickLines = null;
				if (isPicking)
				{
					newOrUpdatedKitPickLines = RecreatePickByBOMReceiveLine(pick, shortedPickByBOMKitLines.ToArray(), allReleaseLines, isShorting: true);
				}

				var reducedOrSplitPickLine = pickedPickLines.SingleOrDefault(pl => pl.WZ_UnitsInfo.HasChanges) // first look for the shorted pick line (Units were reduced), there can only be one
					?? nonPickedLines.Where(pl => !pl.IsDeleted && pl.WZ_UnitsInfo.HasChanges && !pickedPickLines.Contains(pl)).SingleOrDefault(); // if shorted pick line does not exist, look for the original pick line that was split, there can only be one

				if (reducedOrSplitPickLine != null)
				{
					UpdateDivotsFromReducedOrSplitPickLine(reducedOrSplitPickLine);
				}

				if (createPackagesForPickedPacks && packageToPackInto == null)
				{
					AutoCreateAndPackPackages.CreateAndPackIntoPickedPackages(
						pick,
						pickedPickLines,
						pickLinesToPickedPackTypes,
						releaseCapturedInfos.Length > 0,
						pickLinePacking.Select(pickLinePairs => pickLinePairs.Item1.PK).ToHashSet(),
						allReleaseLines,
						newlySplitPickLines);
				}
				else
				{
					if (isPicking)
					{
						var pickLinesToReleaseCapturedGroupingInfos = pickLinesToPickedPackTypes
							.Select(pl => (pl.PickLinePKs, pl.PickedPackTypes.SelectMany(pack => pack.ReleaseCapturedInfos)))
							.ToArray();

						ReleaseCaptureAndPack(pick, pickedPickLines, releaseCapturedInfos.Length > 0, pickLinesToReleaseCapturedGroupingInfos, pickLinePacking, allReleaseLines, packageToPackInto, reducedOrSplitPickLine, newlySplitPickLines, newOrUpdatedKitPickLines);
					}

					if (packageToPackInto != null)
					{
						// this is for pick and pack, pack into package provided by RF
						PackIntoSuppliedPackage(pickedPickLines, packageToPackInto, allReleaseLines, newlySplitPickLines);
					}
				}
			}

			if (shortedInventories.Count > 0)
			{
				CreateCycleCountTasksForShortedLocations(picker.Factory, pick.WP_WW_Whs, shortedInventories);
			}
			SetPickDateToPickLines(pick, pickedPickLines.Concat(newlySplitPickLines.SelectMany(kv => kv.Value)).ToArray(), pickDateTime);

			return shortedOrderLinePKs;
		}

		static void ValidateParametersBeforePickReleaseCaptureOrPack(WhsPickLine[] lines, ZDecimal pickedQty, bool isPicking, GlbStaff picker, IEnumerable<WhsReleaseCapturedInfo> releaseCapturedInfos)
		{
			Argument.NotNull(picker, nameof(picker));

			if (isPicking && lines.Length == 0)
			{
				throw new ArgumentException("Lines array must not be empty.");
			}

			if (lines.Any(l => l.AssignedTo != null && l.AssignedTo != picker))
			{
				throw new InvalidOperationException("Unable to update pick line. Pick line is assigned to another operator.");
			}

			var pickLinesQty = lines.Sum(l => l.WZ_Units);
			if (pickLinesQty < pickedQty)
			{
				throw new ArgumentException("Confirm Qty is greater than Requested Qty.");
			}

			if (isPicking && releaseCapturedInfos.Any() && releaseCapturedInfos.Sum(r => r.Quantity) != pickedQty)
			{
				throw new ArgumentException("Sum for release captured attributes does not match Confirm Qty.");
			}
		}

		#region GetPackedInfo

		static IReadOnlyCollection<(WhsPickLine, IReadOnlyCollection<PackedInfo>)> GetPackedInfo(BusinessObjectFactory factory, WhsPickLine[] nonPickedLines, bool isWorkOrderPick)
		{
			var pickLineWithPackedInfoPairs = new List<(WhsPickLine, IReadOnlyCollection<PackedInfo>)>();
			var pickLinePKs = nonPickedLines.Select(pl => pl.PK).ToArray();

			if (pickLinePKs.Length > 0)
			{
				var (kitPickLinePKs, kitPickLines) = !isWorkOrderPick ? GetKitPickLinesForPackedInfo(factory, nonPickedLines) : (Array.Empty<ZGuid>(), Array.Empty<WhsPickLine>());
				var packedPackages = GetPackagesForPackableItems(factory, pickLinePKs.Append(kitPickLinePKs).ToArray());

				foreach (var pickLine in nonPickedLines.Append(kitPickLines))
				{
					var packableItem = (IPackableItem)pickLine;
					var packedInfos = new List<PackedInfo>();
					if (packedPackages.TryGetValue(pickLine.PK, out var package))
					{
						packedInfos.Add(new PackedInfo(packableItem.Key, package, pickLine.DocketLine.WE_WD, pickLine.PK, packableItem.Quantity));
					}

					if (packedInfos.Count > 0)
					{
						pickLineWithPackedInfoPairs.Add((pickLine, packedInfos));
					}
				}
			}

			return pickLineWithPackedInfoPairs.ToArray();
		}

		static (ZGuid[] KitPickLinePKs, WhsPickLine[] KitPickLines) GetKitPickLinesForPackedInfo(BusinessObjectFactory factory, WhsPickLine[] nonPickedLines)
		{
			var kitPickLinePKs = Array.Empty<ZGuid>();
			var kitPickLines = Array.Empty<WhsPickLine>();
			var componentOrderLines = nonPickedLines.Select(pl => pl.DocketLine).Where(l => l.WE_WE_ParentDocketLine.IsValid);
			var kitOrderLinePKs = componentOrderLines.Select(l => l.WE_WE_ParentDocketLine).ToArray();
			if (kitOrderLinePKs.Length > 0)
			{
				var rawSql = @"
SELECT
	ParentPickLine.WZ_PK
FROM
	dbo.WhsPickLine ParentPickLine
	JOIN dbo.WhsDocketLine ParentOrderLine ON ParentOrderLine.WE_PK = ParentPickLine.WZ_WE_TransactionLine
	JOIN dbo.WhsDocketLine KitReceiveLine ON KitReceiveLine.WE_PK = ParentPickLine.WZ_WE_InventoryLine
	JOIN dbo.WhsDocket KitReceive ON KitReceive.WD_PK = KitReceiveLine.WE_WD
WHERE
	ParentOrderLine.WE_PK IN (SELECT VALUE FROM @KitOrderLinePKs)
	AND KitReceive.WD_WP_ParentPickForReceive IS NOT NULL
";

				var pks = new DynamicBusinessObjectCollection(factory);
				var tvp = ZSqlParameter.New("@KitOrderLinePKs", kitOrderLinePKs, WhsPickLineSchema.PK, isTableValued: true);
				pks.Load(rawSql, new[] { tvp });
				kitPickLinePKs = pks.Select(r => (ZGuid)r[WhsPickLineSchema.PK]).ToArray();
				kitPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, kitPickLinePKs));
			}

			return (kitPickLinePKs, kitPickLines);
		}

		static Dictionary<ZGuid, PkgPackage> GetPackagesForPackableItems(BusinessObjectFactory factory, ZGuid[] packableItemPKs)
		{
			var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, packableItemPKs);
			var divots = factory.Load<PkgPackageItemDivot>(query);

			return divots.ToDictionary(p => p.KI_ParentID, p => p.ParentPackage);
		}

		sealed class PackedInfo
		{
			public PackedInfo(GroupingKey key, PkgPackage package, ZGuid orderPK, ZGuid pk, ZDecimal qty)
			{
				Key = Argument.NotNull(key, nameof(key));
				Package = Argument.NotNull(package, nameof(package));
				OrderPK = Argument.NotNull(orderPK, nameof(orderPK));
				PK = pk;
				Qty = qty;
			}

			public ZDecimal Qty { get; }
			public PkgPackage Package { get; }
			public ZGuid OrderPK { get; }
			public ZGuid PK { get; }
			public GroupingKey Key { get; }
		}

		#endregion

		#region AllocateConfirmedQtyAlongPicklines

		static IEnumerable<WhsPickLine> AllocateConfirmedQtyAlongPicklines(
			Queue<WhsPickLine> pickLinesSortedByUnits,
			HashSet<WhsInventoryView> shortedInventories,
			WhsPick pick,
			GlbStaff picker,
			ZDecimal confirmedQty,
			bool isVerfiedNonEmpty,
			bool shouldSplitPickLine,
			HashSet<Guid> shortedOrderLinePKs,
			HashSet<WhsOrderLine> shortedPickByBOMKitLines)
		{
			while (confirmedQty > 0)
			{
				var pickLineToConfirm = pickLinesSortedByUnits.Dequeue();
				WhsPickLine pickLinePicked;

				var originalPickLineUnits = pickLineToConfirm.WZ_Units;
				if (originalPickLineUnits <= confirmedQty)
				{
					// pick full pickline
					pickLinePicked = pickLineToConfirm;
				}
				else if (!shouldSplitPickLine)
				{
					// Reduce WD_UnitsSent in order using notifier
					var reductionQuantity = Math.Min(0, 0 - (originalPickLineUnits - confirmedQty));
					ReleaseLinesOnPickManager.NotifyChange(pickLineToConfirm.Factory, pickLineToConfirm, reductionQuantity);

					// short pick line.
					pickLinePicked = pickLineToConfirm;
					pickLinePicked.WZ_Units = confirmedQty; // reduce quantity to remaining confirmed qty.

					CreateWhsPickShortLine(
						picker,
						pickLinePicked,
						originalPickLineUnits - confirmedQty);

					ShortPickInventoryLine(shortedInventories, new HashSet<WhsDocketLine>(new [] { pickLinePicked.InventoryLine }));
					shortedOrderLinePKs.Add(pickLinePicked.WZ_WE_TransactionLine.ToGuid());

					var orderLine = (WhsPickableDocketLine)pickLinePicked.DocketLine;
					if (orderLine.IsComponentLineOnSalesOrder)
					{
						shortedPickByBOMKitLines.Add((WhsOrderLine)orderLine.ParentLine);
					}
				}
				else
				{
					// Split off the picked quantity and pick the split pickline. This is done when the Pick is either Suspended or the Package
					// in Pick & Pack is closed, we don't pick the original pick line so that RF does not need to be sent an update of new pick lines.
					pickLinePicked = pickLineToConfirm.Split(confirmedQty);
				}

				pickLinePicked.WZ_GS_NKAssignedTo = picker.GS_Code;
				pickLinePicked.WZ_VerifiedEmpty = isVerfiedNonEmpty ? "N" : "Y";
				confirmedQty -= pickLinePicked.WZ_Units;

				yield return pickLinePicked;
			}
		}

		#endregion

		#region UpdateAllocatedPackTypes

		static IReadOnlyList<WhsPickLine> UpdateAllocatedPackTypes(WhsPick pick, IReadOnlyList<WhsPickLine> pickedPickLines)
		{
			IReadOnlyList<WhsPickLine> allPickLines = new List<WhsPickLine>(pickedPickLines);
			if (pick.IsPickByUOMEnabled && pickedPickLines.Any())
			{
				// when the pick is shorted the Pack Types may no longer match the allocations and so it needs to be rebuilt (Rebuilding will retain Picker Details).
				allPickLines = PickLinePackAssigner.AssignPackTypes(pickedPickLines, pick.ForceWhsPickUOMTypeAllocation);
			}

			return allPickLines;
		}

		#endregion

		#region DeleteUnpickedPickLinesThatWereShorted

		static void DeleteUnpickedPickLinesThatWereShorted(
			GlbStaff picker,
			HashSet<WhsInventoryView> shortedInventories,
			Queue<WhsPickLine> pickLinesSortedByUnits,
			PickingInfo pickingInfo,
			HashSet<Guid> shortedOrderLinePKs,
			HashSet<WhsOrderLine> shortedPickByBOMKitLines)
		{
			var factory = picker.Factory;
			// Delete divots and pick lines for everything that was not picked.
			// We don't delete the lines if we are splitting off PickLines as we are not shorting the pick.
			if (!pickingInfo.ShouldSplit && pickLinesSortedByUnits.Count > 0)
			{
				var linesForDeletedPickLines = pickLinesSortedByUnits.Select(pl => (WhsPickableDocketLine)pl.DocketLine).Distinct().ToArray();
				var releaseLines = PackageHelper.GetReleaseLinesByKey(linesForDeletedPickLines);
				if (releaseLines.Count > 0)
				{
					var allPackableItems = pickLinesSortedByUnits.Select(pl => pl.PK);
					var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, allPackableItems);
					var divots = factory.Load<PkgPackageItemDivot>(query);

					// delete all previously packed divots, they will be replaced later
					foreach (var divot in divots)
					{
						var parentPackage = divot.ParentPackage;
						divot.DeleteForRepacking(releaseLines[divot.PackedItem.Key]);
						DeletePickByLabelIfShortPickMakesPackageEmpty(parentPackage);
					}
				}

				// clear release lines cache for later use
				foreach (var orderLine in linesForDeletedPickLines)
				{
					orderLine.ClearReleaseLines();
					shortedOrderLinePKs.Add(orderLine.PK.ToGuid());

					if (orderLine.IsComponentLineOnSalesOrder)
					{
						shortedPickByBOMKitLines.Add((WhsOrderLine)orderLine.ParentLine);
					}
				}

				var inventoryLines = new HashSet<WhsDocketLine>(pickLinesSortedByUnits.Count);

				// delete pick lines that were not picked and mark inventory short picked
				foreach (var pickLine in pickLinesSortedByUnits)
				{
					// Reduce WD_UnitsSent in order using notifier
					ReleaseLinesOnPickManager.NotifyChange(pickLine.Factory, pickLine, (0 - pickLine.WZ_Units));

					CreateWhsPickShortLine(picker, pickLine, pickLine.WZ_Units);

					var inventoryLine = pickLine.InventoryLine;
					inventoryLines.Add(inventoryLine);

					pickLine.Delete();
				}

				ShortPickInventoryLine(shortedInventories, inventoryLines);
			}
		}

		static void DeletePickByLabelIfShortPickMakesPackageEmpty(PkgPackage package)
		{
			if (!package.GetPickLines().Any())
			{
				WhsPickByLabelHelper.GetActivePickByLabelByPackage(package)?.Delete();
			}
		}

		static void CreateWhsPickShortLine(
			GlbStaff staff,
			WhsPickLine pickLine,
			decimal qtyShorted)
		{
			if (qtyShorted > 0)
			{
				var result = staff.Factory.New<WhsPickShortLine>();
				result.WZS_WE_InventoryLine = pickLine.WZ_WE_InventoryLine;
				result.WZS_WE_TransactionLine = pickLine.WZ_WE_TransactionLine;
				result.WZS_ShortUnits = qtyShorted;
				result.WZS_GS_NKShortedBy = staff.GS_Code;
				result.WZS_ShortedDateTimeUtc = ZDateTime.UtcNow;
			}
		}

		internal static IReadOnlyCollection<WhsPickLine> RecreatePickByBOMReceiveLine(WhsPick pick, WhsOrderLine[] updatedPickByBOMKitLines, Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines, bool isShorting)
			=> updatedPickByBOMKitLines.Length > 0 ? RecreatePickByBOMReceiveLineCore(pick, updatedPickByBOMKitLines, allReleaseLines, isShorting) : new List<WhsPickLine>();

		static IReadOnlyCollection<WhsPickLine> RecreatePickByBOMReceiveLineCore(WhsPick pick, WhsOrderLine[] updatedPickByBOMKitLines, Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines, bool isShorting)
		{
			var factory = pick.Factory;
			var receiveMap = new Dictionary<(ZGuid, ZGuid), WhsReceive>();
			var clientPKOnOrders = new Dictionary<ZGuid, ZGuid>();
			var bomParts = new Dictionary<ZGuid, Dictionary<(ZGuid, ZString), OrgPartBOM>>();
			var newOrUpdatedPickLines = new Dictionary<(ZGuid, ZGuid), List<WhsPickLine>>();
			var releaseLinesByPickLinePK = new Dictionary<ZGuid, WhsReleaseLine>();
			var allKitPickLinesPKsToUnpack = new List<ZGuid>();

			foreach (var kitLine in updatedPickByBOMKitLines)
			{
				var kitQtyFromComponents = kitLine.TotalPickLineQuantityFromComponents;
				if (isShorting || kitQtyFromComponents > 0)
				{
					if (!bomParts.TryGetValue(kitLine.WE_OP, out var bomPartsByKit))
					{
						bomPartsByKit = kitLine.SupplierPart.BillOfMaterials.Cast<OrgPartBOM>().ToDictionary(b => (b.OE_OP_Component, b.OE_F3_NKPackType));
						bomParts[kitLine.WE_OP] = bomPartsByKit;
					}

					if (!clientPKOnOrders.TryGetValue(kitLine.WE_WD, out var clientPK))
					{
						clientPK = kitLine.PickableDocket.WD_OH_Client;
						clientPKOnOrders[kitLine.WE_WD] = clientPK;
					}

					var (newOrUpdatedPickLine, kitPickLinesPKsToUnpack) = CreateOrUpdateReceiveLineForKit(bomPartsByKit, allReleaseLines, kitLine, clientPK, kitQtyFromComponents, releaseLinesByPickLinePK);
					allKitPickLinesPKsToUnpack.AddRange(kitPickLinesPKsToUnpack);
					if (newOrUpdatedPickLine != null)
					{
						if (!newOrUpdatedPickLines.TryGetValue((kitLine.WE_WD, kitLine.WE_OP), out var pickLines))
						{
							newOrUpdatedPickLines[(kitLine.WE_WD, kitLine.WE_OP)] = pickLines = new List<WhsPickLine>();
						}
						pickLines.Add(newOrUpdatedPickLine);
					}

					kitLine.ClearReleaseLines();
				}
			}

			DeletePackageDivotsForRepacking(factory, allKitPickLinesPKsToUnpack, releaseLinesByPickLinePK);

			return pick.IsPickByUOMEnabled ? UpdateAllocatedPackTypesForKits(pick, newOrUpdatedPickLines) : newOrUpdatedPickLines.Values.SelectMany(pl => pl).ToList();

			(WhsPickLine NewOrUpdatedKitPickLine, IEnumerable<ZGuid> KitPickLinesPKsToUnpack) CreateOrUpdateReceiveLineForKit(
				Dictionary<(ZGuid, ZString), OrgPartBOM> bomPartsByKit,
				Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
				WhsOrderLine kitLine,
				ZGuid clientPK,
				decimal kitQtyFromComponents,
				Dictionary<ZGuid, WhsReleaseLine> releaseLinesByPickLinePK)
			{
				var factory = pick.Factory;
				var componentLines = kitLine.ChildComponentLines;
				var kitQtyChanged = false;
				var newReceiveLineCreated = false;
				WhsReceiveLine kitInventoryLine = null;
				foreach (var componentLine in componentLines)
				{
					var bomPart = bomPartsByKit[(componentLine.WE_OP, componentLine.WE_F3_NKPackType)];
					var componentQtyCouldBeUsed = kitQtyFromComponents * bomPart.OE_ComponentQty;

					var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, componentLine.PK);
					var link = factory.LoadTop1<WhsBOMInventoryPivot>(linkQuery);

					if (link == null)
					{
						if (kitInventoryLine == null && !isShorting)
						{
							kitInventoryLine = CreateReceiveLine(clientPK, kitLine.WE_OP, kitQtyFromComponents);
							newReceiveLineCreated = true;
						}

						if (kitInventoryLine != null)
						{
							CreateLink(kitInventoryLine.PK, componentLine.PK, componentQtyCouldBeUsed);
						}
					}
					else
					{
						kitInventoryLine ??= (WhsReceiveLine)link.InventoryLine;
						if (componentQtyCouldBeUsed != link.WIP_ComponentQuantity)
						{
							kitQtyChanged = true;
							link.Delete();

							if (componentQtyCouldBeUsed > 0)
							{
								CreateLink(kitInventoryLine.PK, componentLine.PK, componentQtyCouldBeUsed);
							}
						}
					}
				}

				return UpdateReceiveLineAndPickLine(kitQtyChanged, newReceiveLineCreated, kitLine, kitInventoryLine, kitQtyFromComponents, allReleaseLines, releaseLinesByPickLinePK);
			}

			WhsReceiveLine CreateReceiveLine(ZGuid clientPK, ZGuid supplierPartPK, decimal quantity)
			{
				var factory = pick.Factory;
				if (!receiveMap.TryGetValue((pick.PK, clientPK), out var receive))
				{
					var receiveQuery = new ZQuery(WhsDocketSchema.WD_OH_Client, clientPK);
					receiveQuery.AddToFilter(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK);
					receive = factory.LoadTop1<WhsReceive>(receiveQuery);
					receiveMap[(pick.PK, clientPK)] = receive;
				}
				return WhsPickByBOMHelper.NewKitReceiveLine(factory, receive.PK, supplierPartPK, quantity);
			}

			(WhsPickLine NewOrUpdatedKitPickLine, IEnumerable<ZGuid> KitPickLinesPKsToUnpack) UpdateReceiveLineAndPickLine(
				bool kitQtyChanged,
				bool newReceiveLineCreated,
				WhsOrderLine kitLine,
				WhsReceiveLine kitInventoryLine,
				decimal kitQtyFromComponents,
				Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
				Dictionary<ZGuid, WhsReleaseLine> releaseLinesByPickLinePK)
			{
				WhsPickLine newOrUpdatedPickLine = null;
				var kitPickLinesPKsToUnpack = new List<ZGuid>();
				var factory = kitLine.Factory;

				if (newReceiveLineCreated)
				{
					newOrUpdatedPickLine = factory.New<WhsPickLine>();
					newOrUpdatedPickLine.WZ_Units = kitQtyFromComponents;
					newOrUpdatedPickLine.WZ_WE_InventoryLine = kitInventoryLine.PK;
					newOrUpdatedPickLine.WZ_WE_TransactionLine = kitLine.PK;
					// no need to set WZ_F3_NKAllocatedPackType here as pick.IsPickByUOMEnabled will be cached correctly during allocation
				}
				else if (kitQtyChanged)
				{
					var kitPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitInventoryLine.PK));
					if (kitQtyFromComponents > 0)
					{
						newOrUpdatedPickLine = kitPickLines[0];
						newOrUpdatedPickLine.WZ_Units = kitQtyFromComponents;

						kitInventoryLine.WE_ClientOrderedUnits = kitQtyFromComponents;
						kitInventoryLine.WE_StockOnHand = kitQtyFromComponents;
					}

					foreach (var kitPickLine in kitPickLines)
					{
						kitPickLinesPKsToUnpack.Add(kitPickLine.PK);
						releaseLinesByPickLinePK[kitPickLine.PK] = allReleaseLines.Value[((IPackableItem)kitPickLine).Key];
						if (newOrUpdatedPickLine == null || kitPickLine.PK != newOrUpdatedPickLine.PK)
						{
							kitPickLine.Delete();
						}
					}

					if (kitQtyFromComponents == 0)
					{
						kitInventoryLine.Delete();
					}
				}
				return (newOrUpdatedPickLine, kitPickLinesPKsToUnpack);
			}

			void CreateLink(ZGuid receiveLinePK, ZGuid componentLinePK, decimal qty)
			{
				var newLink = factory.New<WhsBOMInventoryPivot>();
				newLink.WIP_ComponentQuantity = qty;
				newLink.WIP_WE_ComponentLine = componentLinePK;
				newLink.WIP_WE_InventoryLine = receiveLinePK;
			}
		}

		static void DeletePackageDivotsForRepacking(BusinessObjectFactory factory, IEnumerable<ZGuid> allKitPickLinesPKsToUnpack, Dictionary<ZGuid, WhsReleaseLine> releaseLinesByPickLinePK)
		{
			var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, allKitPickLinesPKsToUnpack);
			var divots = factory.Load<PkgPackageItemDivot>(query);

			foreach (var divot in divots)
			{
				var releaseLine = releaseLinesByPickLinePK[divot.KI_ParentID];
				divot.DeleteForRepacking(releaseLine);
			}
		}

		static IReadOnlyCollection<WhsPickLine> UpdateAllocatedPackTypesForKits(WhsPick pick, Dictionary<(ZGuid, ZGuid), List<WhsPickLine>> newOrUpdatedPickLines)
		{
			var result = new List<WhsPickLine>();
			if (newOrUpdatedPickLines.Count > 0)
			{
				foreach (var pickLines in newOrUpdatedPickLines.Values)
				{
					result.AddRange(PickLinePackAssigner.AssignPackTypes(pickLines, pick.ForceWhsPickUOMTypeAllocation));
				}
			}
			return result;
		}

		#endregion

		#region UpdateDivotsFromReducedOrSplitPickLine

		static void UpdateDivotsFromReducedOrSplitPickLine(WhsPickLine reducedOrSplitPickLine)
		{
			var orderLine = (WhsPickableDocketLine)reducedOrSplitPickLine.DocketLine;
			var releaseLines = PackageHelper.GetReleaseLinesByKey(new[] { orderLine });
			if (releaseLines.Count > 0)
			{
				// delete previous packing
				var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, new[] { reducedOrSplitPickLine.PK });
				var divots = orderLine.Factory.Load<PkgPackageItemDivot>(query);
				foreach (var divot in divots)
				{
					IPackableItem packableItem = reducedOrSplitPickLine;
					divot.DeleteForRepacking(releaseLines[packableItem.Key]);
				}
			}

			// clear release lines for later use
			orderLine.ClearReleaseLines();
		}

		#endregion

		#region ShortPickInventoryLine

		static void ShortPickInventoryLine(
			HashSet<WhsInventoryView> shortedInventories,
			HashSet<WhsDocketLine> inventoryLines)
		{
			var shortedInventoryLines = new HashSet<WhsDocketLine>();
			foreach (var inventoryLine in inventoryLines)
			{
				var factory = inventoryLine.Factory;

				var referenceInventory = inventoryLine.Inventory[0];
				var inventoriesToShort = factory.Load<WhsInventoryView>(GetInventoriesToShortQuery(referenceInventory));

				foreach (var inventory in inventoriesToShort)
				{
					var line = inventory.InDocketLine;
					if (line.HeldCodeToChangeTo != InventoryHoldCodes.Codes.ShortPicked
						&& line.HeldCodeChangeQuantity != line.AvailableToPickQuantity)
					{
						line.HeldCodeToChangeTo = InventoryHoldCodes.Codes.ShortPicked;
						line.HeldCodeChangeQuantity = line.AvailableToTransferQuantity;
						shortedInventoryLines.Add(line);
					}

					if (inventory.WI_TotalUnits > 0)
					{
						shortedInventories.Add(inventory);
					}
				}
			}
			shortedInventoryLines.ForEach(sl => sl.ChangeInventoryHeldCode(true));
		}

		static ZQuery GetInventoriesToShortQuery(WhsInventoryView inventory)
		{
			var inventoryToShortQuery = new ZDBOnlySubQuery(typeof(WhsInventoryView), WhsInventoryViewSchema.PK);
			inventoryToShortQuery.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, inventory.WI_OH_Client);
			inventoryToShortQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, inventory.WI_WL);
			inventoryToShortQuery.AddToFilter(WhsInventoryViewSchema.WI_OP, inventory.WI_OP);
			inventoryToShortQuery.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.Equal, string.Empty);

			var palletsWithProductQuery = new ZDBOnlySubQuery(typeof(WhsInventoryView), WhsInventoryViewSchema.WI_PalletID);
			palletsWithProductQuery.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, inventory.WI_OH_Client);
			palletsWithProductQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, inventory.WI_WL);
			palletsWithProductQuery.AddToFilter(WhsInventoryViewSchema.WI_OP, inventory.WI_OP);
			palletsWithProductQuery.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, string.Empty);

			var inventoryForPalletsQuery = new ZDBOnlySubQuery(typeof(WhsInventoryView), WhsInventoryViewSchema.PK);
			inventoryForPalletsQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, inventory.WI_WL);
			inventoryForPalletsQuery.AddSubQuery(WhsInventoryViewSchema.WI_PalletID, palletsWithProductQuery, JoinCondition.And);

			inventoryToShortQuery.AddAsUnionQuery(inventoryForPalletsQuery, true);

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddToFilter(WhsInventoryViewSchema.WI_InventoryStatus, SQLComparisonOperator.NotEqual, InventoryStatus.Codes.Staged);
			query.AddSubQuery(inventoryToShortQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region ReleaseCaptureAndPack

		static void ReleaseCaptureAndPack(
			WhsPick pick,
			IReadOnlyList<WhsPickLine> pickedPickLines,
			bool anyReleaseCapturedValues,
			(Guid[], IEnumerable<WhsReleaseCapturedInfo>)[] pickLinesToReleaseCapturedGroupingInfos,
			IReadOnlyCollection<(WhsPickLine, IReadOnlyCollection<PackedInfo>)> pickLinePacking,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
			PkgPackage packageToPackInto,
			WhsPickLine reducedOrSplitPickLine,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines,
			IReadOnlyCollection<WhsPickLine> newOrUpdatedKitPickLines)
		{
			// we do not currently support release capturing for work orders. the first IF statement is a quick fix, and we have a WI for a proper fix
			// note we should never mix picklines for both orders and work orders, but do .All() just in case.
			var isReleaseCapturingAnOrder = pickedPickLines.All(pl => pl.DocketLine is WhsOrderLine);
			if (isReleaseCapturingAnOrder)
			{
				if (anyReleaseCapturedValues)
				{
					UpdatePickLinesWithReleaseCapturedAttribs(pickedPickLines, pickLinesToReleaseCapturedGroupingInfos, reducedOrSplitPickLine, newlySplitPickLines);
					var pickLinesIncludingSplit = pickedPickLines.Concat(newlySplitPickLines.SelectMany(kv => kv.Value)).ToArray();

					// this is only for packing pre-packed (e.g cartonised) things not for pick and pack
					if (packageToPackInto == null)
					{
						var unAssignedPackQuantities = PackPickLinesAsOriginallyPackedWithReleaseCapture(pickLinePacking, allReleaseLines, newlySplitPickLines);
						PackUnAssignedQuantities(unAssignedPackQuantities, allReleaseLines, pickLinesIncludingSplit, newOrUpdatedKitPickLines);
					}

					ValidateReleaseLinesAfterReleaseCapturedAttribsUpdate(pickLinesIncludingSplit);
				}
				else
				{
					ClearReleaseCapturedAttribsForMultiOrderPick(pick, pickedPickLines);

					// this is only for packing pre-packed (e.g cartonised) things not for pick and pack
					if (packageToPackInto == null)
					{
						var unAssignedPackQuantities = PackPickLinesAsOriginallyPackedWithoutReleaseCapture(pickLinePacking, allReleaseLines);
						PackUnAssignedQuantities(unAssignedPackQuantities, allReleaseLines, pickedPickLines, newOrUpdatedKitPickLines);
					}
				}
			}
		}

		static void ClearReleaseCapturedAttribsForMultiOrderPick(WhsPick pick, IReadOnlyList<WhsPickLine> pickedPickLines)
		{
			// quick fix for multi-order release capturing
			if (pick.Orders.IsCountMoreThan(1))
			{
				pickedPickLines
				.Where(line => line.HasReleaseCapturedAttribs)
				.ForEach(line => line.ClearReleaseCapturedAttributes());
			}
		}

		#region UpdatePickLinesWithReleaseCapturedAttribs

		static void UpdatePickLinesWithReleaseCapturedAttribs(
			IReadOnlyList<WhsPickLine> lines,
			(Guid[] PickLinePKs, IEnumerable<WhsReleaseCapturedInfo> ReleaseCapturedGroupingInfos)[] pickLinesToReleaseCapturedGroupingInfos,
			WhsPickLine reducedOrSplitPickLine,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			var pickLinesLookup = lines.ToDictionary(line => line.PK.ToGuid());
			foreach (var pickLinesToReleaseCapturedGroupingInfo in pickLinesToReleaseCapturedGroupingInfos)
			{
				var pickLinePKs = pickLinesToReleaseCapturedGroupingInfo.PickLinePKs.ToHashSet();
				var pickLines = pickLinePKs
					.Where(pk => pickLinesLookup.ContainsKey(pk))
					.Select(pk => pickLinesLookup[pk])
					.ToList();

				if (reducedOrSplitPickLine != null && pickLinePKs.Contains(reducedOrSplitPickLine.PK.ToGuid()))
				{
					var allMappedPickLines = pickLinesToReleaseCapturedGroupingInfos.SelectMany(p => p.PickLinePKs).ToHashSet();
					var unmappedPickLines = lines.Where(p => !allMappedPickLines.Contains(p.PK.ToGuid()));
					pickLines.AddRange(unmappedPickLines);
				}

				UpdateReleaseCapturedAttribs(pickLinesToReleaseCapturedGroupingInfo.ReleaseCapturedGroupingInfos, pickLines, newlySplitPickLines);
			}
		}

		#region UpdateReleaseCapturedAttribs

		public static void UpdateReleaseCapturedAttribs(
			IEnumerable<WhsReleaseCapturedInfo> releaseCapturedInfosForPickLines,
			IEnumerable<WhsPickLine> pickLines,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			var groupedReleaseCapturedInfosForPickLines = new Dictionary<ZString, (WhsReleaseCapturedInfo Info, ZDecimal Quantity)>();
			foreach (var info in releaseCapturedInfosForPickLines)
			{
				var key = CreateKey(info);
				if (groupedReleaseCapturedInfosForPickLines.TryGetValue(key, out var groupedInfo))
				{
					groupedReleaseCapturedInfosForPickLines[key] = (info, groupedInfo.Quantity + info.Quantity);
				}
				else
				{
					groupedReleaseCapturedInfosForPickLines[key] = (info, info.Quantity);
				}
			}

			foreach (var pickLine in pickLines.OrderByDescending(l => l.WZ_Units))
			{
				// need to overwrite existing release captured attributes when coming from RF.
				if (pickLine.HasReleaseCapturedAttribs)
				{
					pickLine.ClearReleaseCapturedAttributes();
				}

				var unReleaseCapturedQty = pickLine.WZ_Units;
				var infosToRemove = new List<ZString>();
				var infosToUpdate = new List<(ZString, WhsReleaseCapturedInfo, ZDecimal)>();

				foreach (var groupedInfo in groupedReleaseCapturedInfosForPickLines)
				{
					var info = groupedInfo.Value.Info;
					var qtyToReleaseCapture = groupedInfo.Value.Quantity;

					if (unReleaseCapturedQty > qtyToReleaseCapture)
					{
						infosToRemove.Add(groupedInfo.Key);
						var newPickLine = pickLine.Split(qtyToReleaseCapture);
						newPickLine.SetReleaseCapturedAttributes(info.Attribute1, info.Attribute2, info.Attribute3, info.SerialNumber);
						AddNewSplitPickLine(newlySplitPickLines, pickLine, newPickLine);

						unReleaseCapturedQty -= qtyToReleaseCapture;
					}
					else
					{
						if (unReleaseCapturedQty == qtyToReleaseCapture)
						{
							infosToRemove.Add(groupedInfo.Key);
						}
						else
						{
							infosToUpdate.Add((groupedInfo.Key, info, qtyToReleaseCapture - unReleaseCapturedQty));
						}

						pickLine.SetReleaseCapturedAttributes(info.Attribute1, info.Attribute2, info.Attribute3, info.SerialNumber);

						break;
					}
				}

				foreach (var infoToRemove in infosToRemove)
				{
					groupedReleaseCapturedInfosForPickLines.Remove(infoToRemove);
				}

				foreach (var (key, info, newQty) in infosToUpdate)
				{
					groupedReleaseCapturedInfosForPickLines[key] = (info, newQty);
				}
			}

			string CreateKey(WhsReleaseCapturedInfo info) => string.Join("|", new[] { info.Attribute1.ToUpper(), info.Attribute2.ToUpper(), info.Attribute3.ToUpper(), info.SerialNumber.ToUpper() });
		}

		public static void AddNewSplitPickLine(Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines, WhsPickLine pickLine, WhsPickLine splitPickLine)
		{
			if (splitPickLine != null)
			{
				if (newlySplitPickLines.TryGetValue(pickLine, out var splitLines))
				{
					splitLines.Add(splitPickLine);
				}
				else
				{
					newlySplitPickLines[pickLine] = new List<WhsPickLine> { splitPickLine };
				}
			}
		}

		#endregion

		#region ValidateReleaseLinesAfterReleaseCapturedAttribsUpdate

		static void ValidateReleaseLinesAfterReleaseCapturedAttribsUpdate(IReadOnlyList<WhsPickLine> pickLines)
		{
			var releaseLines = pickLines.SelectMany(pickLine => ((WhsPickableDocketLine)pickLine.DocketLine).ReleaseLines).Cast<WhsReleaseLine>().Distinct();
			foreach (var releaseLine in releaseLines)
			{
				releaseLine.Validation.ValidateAll();
				if (releaseLine.HasErrors)
				{
					throw new InvalidOperationException("Invalid release line resulted from release captured attributes update.");
				}
			}
		}

		#endregion

		#endregion

		#region PackPickLinesAsOriginallyPacked

		#region PackPickLinesAsOriginallyPackedWithReleaseCapture

		static IReadOnlyDictionary<(PkgPackage Package, ZGuid OrderPK), decimal> PackPickLinesAsOriginallyPackedWithReleaseCapture(
			IReadOnlyCollection<(WhsPickLine pickLine, IReadOnlyCollection<PackedInfo> packedInfos)> pickLinePacking,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			var unAssignedPackQuantities = new Dictionary<(PkgPackage, ZGuid), decimal>();
			foreach (var pickLineGroup in pickLinePacking)
			{
				var pickLine = pickLineGroup.pickLine;

				// a pick line may have been packed into multiple pick lines via its release captured attributes
				var packageGroups = pickLineGroup.packedInfos.GroupBy(o => (o.Package, o.OrderPK));
				if (!pickLine.IsDeleted) // pickline was deleted if it was shorted to 0 units
				{
					foreach (var packageGroup in packageGroups)
					{
						RepackPickLine(packageGroup, pickLine, allReleaseLines.Value, unAssignedPackQuantities, newlySplitPickLines);
					}
				}
				// if pickline was deleted through shorting we still want to pack the package with the remaining picklines if possible
				else
				{
					UpdateUnAssignedPackQuantities(unAssignedPackQuantities, packageGroups);
				}
			}

			return unAssignedPackQuantities;
		}

		static void RepackPickLine(
			IGrouping<(PkgPackage Package, ZGuid OrderPK), PackedInfo> packageGroup,
			WhsPickLine pickLine,
			IReadOnlyDictionary<GroupingKey, WhsReleaseLine> allReleaseLines,
			Dictionary<(PkgPackage, ZGuid), decimal> unAssignedPackQuantities,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			IPackableItem packableItem = pickLine;
			var releaseLine = allReleaseLines[packableItem.Key];

			DeleteExistingDivots(packageGroup, pickLine.PK, releaseLine);

			var packedQty = packageGroup.Sum(o => o.Qty);

			// attempt to pack the pick line fully
			var package = packageGroup.Key.Package;
			var pickLines = GetPickLinesIncludingNewlySplit(pickLine, newlySplitPickLines);

			// pack the original pickline and all the split lines from this pickline
			foreach (var line in pickLines)
			{
				package.Pack(line, allReleaseLines[line.Key]);
			}

			// if there was any amount shorted, we store that qty to be potentially fulfilled through packing by other picklines
			UpdateUnAssignedPackQuantities(unAssignedPackQuantities, packageGroup.Key, packedQty - pickLines.Sum(l => l.Quantity));
		}

		static IEnumerable<IPackableItem> GetPickLinesIncludingNewlySplit(WhsPickLine pickLine, Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			var pickLinesIncludingNewlySplit = new[] { pickLine };
			if (newlySplitPickLines.TryGetValue(pickLine, out var newLines))
			{
				pickLinesIncludingNewlySplit = pickLinesIncludingNewlySplit.Concat(newLines).ToArray();
			}
			return pickLinesIncludingNewlySplit;
		}

		#endregion

		#region PackPickLinesAsOriginallyPackedWithoutReleaseCapture

		static IReadOnlyDictionary<(PkgPackage Package, ZGuid OrderPK), decimal> PackPickLinesAsOriginallyPackedWithoutReleaseCapture(
			IReadOnlyCollection<(WhsPickLine Pickline, IReadOnlyCollection<PackedInfo> PackedInfos)> pickLinePacking,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines)
		{
			var unAssignedPackQuantities = new Dictionary<(PkgPackage, ZGuid), decimal>();

			foreach (var pickLineGroup in pickLinePacking)
			{
				var pickLine = pickLineGroup.Pickline;

				// a pick line may have been packed into multiple pick lines via its release captured attributes
				var packageGroups = pickLineGroup.PackedInfos.GroupBy(o => (o.Package, o.OrderPK));
				if (!pickLine.IsDeleted) // pickline was deleted if it was shorted to 0 units
				{
					// for each previously packed pickLine we need to clear the existing divots and re-pack what was actually picked
					foreach (var packageGroup in packageGroups)
					{
						IPackableItem packableItem = pickLine;
						var releaseLine = allReleaseLines.Value[packableItem.Key];
						RepackPickLine(pickLine, packageGroup, releaseLine, unAssignedPackQuantities);
					}
				}
				// if pickline was deleted through shorting we still want to pack the package with the remaining picklines if possible
				else
				{
					UpdateUnAssignedPackQuantities(unAssignedPackQuantities, packageGroups);
				}
			}

			return unAssignedPackQuantities;
		}

		static void RepackPickLine(
			WhsPickLine pickLine,
			IGrouping<(PkgPackage Package, ZGuid), PackedInfo> packageGroup,
			WhsReleaseLine releaseLine,
			Dictionary<(PkgPackage Package, ZGuid), decimal> unAssignedPackQuantities)
		{
			IPackableItem packableItem = pickLine;
			DeleteExistingDivots(packageGroup, pickLine.PK, releaseLine);

			var package = packageGroup.Key.Package;
			var qtyToPack = packageGroup.Sum(q => q.Qty); // this amount was previously packed, we now need to repack that amount
			if (packableItem.Quantity > 0m)
			{
				qtyToPack -= PackPickLine(package, pickLine, qtyToPack, releaseLine).QtyPacked;
			}

			// if pickline was shorted then we need to pack as much as possible
			var qtyToPackRemaining = qtyToPack;
			UpdateUnAssignedPackQuantities(unAssignedPackQuantities, packageGroup.Key, qtyToPackRemaining);
		}

		static (decimal QtyPacked, WhsPickLine SplitItem) PackPickLine(PkgPackage package, WhsPickLine pickLine, decimal qtyToPack, WhsReleaseLine releaseLine)
		{
			var qtyPacked = 0m;
			WhsPickLine splitItem = null;
			var itemQuantity = pickLine.WZ_Units;
			if (itemQuantity <= qtyToPack)
			{
				package.Pack(pickLine, releaseLine);
				qtyPacked = itemQuantity;
			}
			else
			{
				splitItem = pickLine.Split(qtyToPack);
				package.Pack(splitItem, releaseLine);
				qtyPacked = qtyToPack;
			}

			return (qtyPacked, splitItem);
		}

		#endregion

		#region DeleteExistingDivots

		static void DeleteExistingDivots(IGrouping<(PkgPackage Package, ZGuid), PackedInfo> packageGroup, ZGuid pickLinePK, WhsReleaseLine releaseLine)
		{
			var package = packageGroup.Key.Package;

			foreach (var divot in package.PackedItemDivots.ToArray())
			{
				if (divot.KI_ParentID == pickLinePK || packageGroup.Any(o => o.PK == divot.KI_ParentID))
				{
					divot.DeleteForRepacking(releaseLine);
				}
			}
		}

		#endregion

		#region UpdateUnAssignedPackQuantities

		static void UpdateUnAssignedPackQuantities(Dictionary<(PkgPackage, ZGuid), decimal> unAssignedPackQuantities, IEnumerable<IGrouping<(PkgPackage, ZGuid), PackedInfo>> packageGroups)
		{
			foreach (var packageGroup in packageGroups)
			{
				var packedQty = packageGroup.Sum(o => o.Qty);
				UpdateUnAssignedPackQuantities(unAssignedPackQuantities, packageGroup.Key, packedQty);
			}
		}

		static void UpdateUnAssignedPackQuantities(Dictionary<(PkgPackage, ZGuid), decimal> unAssignedPackQuantities, (PkgPackage, ZGuid) key, decimal unPackedQty)
		{
			if (unPackedQty > 0m)
			{
				decimal result;
				if (unAssignedPackQuantities.TryGetValue(key, out result))
				{
					unAssignedPackQuantities[key] = result + unPackedQty;
				}
				else
				{
					unAssignedPackQuantities[key] = unPackedQty;
				}
			}
		}

		#endregion

		#endregion

		#region PackUnAssignedQuantities

		static void PackUnAssignedQuantities(
			IReadOnlyCollection<KeyValuePair<(PkgPackage, ZGuid), decimal>> unAssignedPackQuantities,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
			IEnumerable<WhsPickLine> pickedPickLines,
			IReadOnlyCollection<WhsPickLine> newOrUpdatedKitPickLines)
		{
			// For picklines that were deleted or shorted we want to still pack as many of the remaining picklines as possible.
			// This is so that if an amount of the product was packed, we can try to fulfill the original packed qty.
			if (unAssignedPackQuantities.Count > 0)
			{
				var unPackedPickLinesLookup = pickedPickLines.Append(newOrUpdatedKitPickLines.ToArray())
					.Where(i => i.WZ_Units > 0m && i.IsUnpacked(i.Factory)).ToLookup(pl => pl.DocketLine.WE_WD);
				foreach (var unAssignedPackQty in unAssignedPackQuantities)
				{
					var (package, orderPK) = unAssignedPackQty.Key;
					var qtyToPack = unAssignedPackQty.Value;

					var unPackedPickLines = unPackedPickLinesLookup[orderPK];
					foreach (var pickLine in unPackedPickLines.OrderBy(pl => pl.WZ_Units))
					{
						// Component Pick Lines from Pick by BOM don't have Release Lines.
						if (allReleaseLines.Value.TryGetValue(((IPackableItem)pickLine).Key, out var releaseLine))
						{
							qtyToPack -= PackPickLine(package, pickLine, qtyToPack, releaseLine).QtyPacked;
						}

						if (qtyToPack == 0m)
						{
							break;
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region PackIntoSuppliedPackage

		static void PackIntoSuppliedPackage(
			IReadOnlyList<WhsPickLine> pickedPickLines,
			PkgPackage packageToPackInto,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			// pick and pack wants everything that was picked to be packed into the package supplied by RF
			if (pickedPickLines.Count > 0)
			{
				if (pickedPickLines.Any(l => l.WZ_Units > maxValueForKIPackedQty))
				{
					throw new ArgumentException($"Packing failed, cannot pack more than {maxValueForKIPackedQty} units.");
				}

				var packableItems = pickedPickLines.Concat(newlySplitPickLines.SelectMany(kv => kv.Value)).ToArray();
				var packableItemsByPK = packableItems.Cast<IPackableItem>().ToDictionary(i => i.PK);

				// delete any divots that exist for the picklines on the current package
				foreach (var divot in packageToPackInto.PackedItemDivots.ToArray())
				{
					if (packableItemsByPK.TryGetValue(divot.KI_ParentID, out var pickLine))
					{
						divot.DeleteForRepacking(allReleaseLines.Value[pickLine.Key]);
					}
				}

				PackPickLinesIntoPackage(allReleaseLines.Value, packableItems, packageToPackInto);
			}
		}

		static readonly ZDecimal maxValueForKIPackedQty = (decimal)Math.Pow(10, PkgPackageItemDivotSchema.KI_PackedQty.Precision - PkgPackageItemDivotSchema.KI_PackedQty.Scale) - 1;

		#region PackPickLinesIntoPackage

		static void PackPickLinesIntoPackage(IReadOnlyDictionary<GroupingKey, WhsReleaseLine> allReleaseLines, IReadOnlyList<WhsPickLine> pickLinesToPack, PkgPackage packageToPackInto)
		{
			foreach (IPackableItem pickLine in pickLinesToPack.OrderByDescending(pl => pl.WZ_Units))
			{
				if (pickLine.IsUnpacked(packageToPackInto.Factory))
				{
					packageToPackInto.Pack(pickLine, allReleaseLines[pickLine.Key]);
				}
			}
		}

		#endregion

		#endregion

		#region SavePickLines

		static void SetPickDateToPickLines(WhsPick pick, IReadOnlyList<WhsPickLine> pickedPickLines, ZDateTimeOffset pickDateTime)
		{
			if (pickedPickLines.Count > 0)
			{
				using (pick?.SuspendPickPercentageRecalculation())
				{
					pickedPickLines.ForEach(pickLine => pickLine.WZ_PickedDateTime = pickDateTime);
				}
			}
		}

		#endregion

		#region CreateCycleCountTasksForShortedLocations

		static void CreateCycleCountTasksForShortedLocations(BusinessObjectFactory factory, ZGuid whsPK, HashSet<WhsInventoryView> shortedInventories)
		{
			var allClients = shortedInventories.Select(i => i.WI_OH_Client).Distinct();

			var query = new ZQuery(WhsClientPickPackParamsByWhsSchema.WPP_WW_Warehouse, whsPK);
			query.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, allClients);
			query.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_WSH_SalesChannel, null);
			query.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_CycleCountOnShort, true);

			var allCCOnShortParams = factory.Load<WhsClientPickPackParamsByWhs>(query);
			var clientsWithShortCCEnabled = allCCOnShortParams.Select(pr => pr.WPP_OH_Client).ToHashSet();

			var locationPKsToShort = new List<ZGuid>();
			foreach (var group in shortedInventories.GroupBy(i => i.WI_WL))
			{
				if (clientsWithShortCCEnabled.Overlaps(group.Select(i => i.WI_OH_Client).Distinct()))
				{
					locationPKsToShort.Add(group.Key);
				}
			}

			if (locationPKsToShort.Count > 0)
			{
				var cycleCountTaskCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>();
				cycleCountTaskCreator.CreateCycleCountLocations(factory, locationPKsToShort, priority: 1);
			}
		}

		#endregion

		#endregion

		#region ClosePackage

		internal static void ClosePackage(PkgPackage package, WebServiceResponse response)
		{
			if (response.ValidateShouldNotBeNull(package, nameof(package)))
			{
				if (package.IsClosed)
				{
					response.LogBusinessValidationError(Res.GetString("23e2292b-8735-452c-a1ca-c7e3d5bd13c6", "Package with ID '{0}' is already closed.", package.KP_PackageID));
				}
				else
				{
					package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				}
			}
		}

		internal static void ClosePackageWithPrinter(PkgPackage package, string printerName, WebServiceResponse response)
		{
			if (response.ValidateShouldNotBeNull(package, nameof(package)))
			{
				package.PrinterUsedToPrintLabel = printerName;
				ClosePackage(package, response);
			}
		}

		#endregion
	}
}
