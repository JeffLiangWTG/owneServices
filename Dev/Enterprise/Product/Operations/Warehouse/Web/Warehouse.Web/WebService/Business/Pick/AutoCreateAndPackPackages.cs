using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	// Tested in PickAndPossiblyReleaseCaptureAndPossiblyPackTest.cs
	public static class AutoCreateAndPackPackages
	{
		public static void CreateAndPackIntoPickedPackages(
			WhsPick pick,
			IReadOnlyList<WhsPickLine> pickedPickLines,
			PickLinesToPickedPackTypeInfo[] pickLinesToPackageCollection,
			bool isReleaseCaptured,
			HashSet<ZGuid> previouslyPackedPickLinesPKs,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			if (pickLinesToPackageCollection.Length > 0 && !pick.IsWorkOrderPick)
			{
				var orders = pick.Orders;
				var logParentPKs = orders.Select(o => o.PK).ToList();
				pick.Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, logParentPKs));

				logParentPKs.Add(pick.PK);
				pick.Factory.AddFetchHint(StmALogSchema.Instance, new ZQuery(StmALogSchema.SL_Parent, logParentPKs));

				var palletIDs = pickLinesToPackageCollection
					.SelectMany(p => p.PickedPackTypes)
					.Select(p => p.PalletID)
					.Distinct();
				var totalPalletQuantities = LoadInventoryQuantitiesPerPallet(pick.Factory, palletIDs, pick.WP_WW_Whs);

				var orderEnablesPackValues = orders.ToDictionary(k => k.PK, value => ((WhsOrder)value).ClientPickingParams?.WPP_EnableAutoPackageCreationOnPicking ?? false);

				var (packagesToCreate, leftOverRCAs) =
					ConstructPackagesToCreate(
						pick,
						pickLinesToPackageCollection,
						isReleaseCaptured,
						previouslyPackedPickLinesPKs,
						newlySplitPickLines,
						orderEnablesPackValues,
						pickedPickLines);

				if (isReleaseCaptured)
				{
					UpdatePickLinesWithLooseRCAs(pickedPickLines, leftOverRCAs, newlySplitPickLines);
				}

				if (packagesToCreate.Count > 0)
				{
					CreatePackages(packagesToCreate, totalPalletQuantities, allReleaseLines);
					orders.Cast<WhsOrder>().ForEach(o => o.UpdatePackageDataAndWeightAndVolume()); 
				}
			}
		}

		static (Dictionary<PkgPackageJob, List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>>, List<WhsReleaseCapturedInfo>) ConstructPackagesToCreate(
			WhsPick pick,
			PickLinesToPickedPackTypeInfo[] pickLinesToPackageCollection,
			bool isReleaseCaptured,
			HashSet<ZGuid> previouslyPackedPickLinesPKs,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines,
			Dictionary<ZGuid, ZBool> orderEnablesPackValues,
			IReadOnlyList<WhsPickLine> pickedPickLines)
		{
			var packagesToCreate = new Dictionary<PkgPackageJob, List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>>();
			var releaseCaptureInfosLeftOver = new List<WhsReleaseCapturedInfo>();
			var orderLookup = GetPickLineOrders(pick.Factory, pickedPickLines);
			if (orderLookup.Count > 0)
			{
				var pickLinesLookup = pickedPickLines.ToDictionary(pl => pl.PK);
				var supportedPackTypes = new RefPackTypeCollection(pick.Factory).Select(p => p.F3_Code.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
				foreach (var pickLineToPackageType in pickLinesToPackageCollection)
				{
					var pickLinePKs = pickLineToPackageType.PickLinePKs;
					if (pickLinePKs.Length > 0 && pickLinePKs.All(pk => !previouslyPackedPickLinesPKs.Contains(pk)))
					{
						var firstOrderLine = GetOrderLine(pickLinePKs, pickLinesLookup);
						var product = firstOrderLine.Product;
						var stockKeepingUnit = product.Parent.OP_StockKeepingUnit;
						var pickedPackageTypeInfos = new List<PickedPackTypeInfo>(pickLineToPackageType.PickedPackTypes.OrderBy(p => p.UnitsPicked));
						var ordersWithPickLines = GetOrdersWithPickLines(pickLinePKs, orderLookup, pickLinesLookup);

						CreatePackagesPerOrder(
							isReleaseCaptured,
							newlySplitPickLines,
							supportedPackTypes,
							orderEnablesPackValues,
							packagesToCreate,
							product,
							stockKeepingUnit,
							pickedPackageTypeInfos,
							ordersWithPickLines);

						releaseCaptureInfosLeftOver.AddRange(pickedPackageTypeInfos.SelectMany(c => c.ReleaseCapturedInfos));
					}
				}
			}

			return (packagesToCreate, releaseCaptureInfosLeftOver);

			// Test throw Exception in PickLineUpdateTest TestCreateAndPackIntoPickedPackages_ThrowExceptionForGetOrderLine
			WhsOrderLine GetOrderLine(Guid[] pickLinePKs, Dictionary<ZGuid, WhsPickLine> pickLinesLookup)
			{
				foreach (var pk in pickLinePKs)
				{
					if (pickLinesLookup.TryGetValue(pk, out var pickLine))
					{
						return (WhsOrderLine)pickLine.DocketLine;
					}
				}
				throw new ArgumentException("Should have a matching OrderLine");
			}
		}

		static Dictionary<ZGuid, WhsOrder> GetPickLineOrders(BusinessObjectFactory factory, IReadOnlyList<WhsPickLine> pickedPickLines)
		{
			var rawQuery = $@"
SELECT
	WE_WD,
	WZ_PK
FROM
	dbo.WhsPickLine
	JOIN dbo.WhsDocketLine ON WE_PK = WZ_WE_TransactionLine
WHERE (1=1)
	AND WZ_PK IN (SELECT Value FROM @PickLinePKs)
	AND WE_WE_ParentDocketLine IS NULL";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@PickLinePKs", pickedPickLines.Select(p => p.PK).ToArray(), WhsPickLineSchema.PK, isTableValued: true)
			};

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(rawQuery, sqlParams);

			var result = collection.Select(c => (OrderPK: (ZGuid)c[WhsDocketLineSchema.WE_WD], PickLinePK: (ZGuid)c[WhsPickLineSchema.PK])).ToArray();
			var ordersQuery = new ZQuery(WhsDocketSchema.PK, result.Select(r => r.OrderPK).Distinct());
			var orders = factory.Load<WhsOrder>(ordersQuery).ToDictionary(ol => ol.PK);

			return result.ToDictionary(k => k.PickLinePK, v => orders[v.OrderPK]);
		}

		static (WhsOrder Order, List<WhsPickLine> PickLines, decimal AvailableQty)[] GetOrdersWithPickLines(
				Guid[] pickLinePKs,
				Dictionary<ZGuid, WhsOrder> orderLookup,
				Dictionary<ZGuid, WhsPickLine> pickLineLookup)
		{
			var orderWithPickLines = new Dictionary<WhsOrder, List<WhsPickLine>>();
			foreach (var pickLinePK in pickLinePKs)
			{
				if (orderLookup.TryGetValue(pickLinePK, out var orderForPickline))
				{
					var pickline = pickLineLookup[pickLinePK];
					if (!orderWithPickLines.TryGetValue(orderForPickline, out var pickLines))
					{
						orderWithPickLines[orderForPickline] = pickLines = new List<WhsPickLine>();
					}
					pickLines.Add(pickline);
				}
			}

			return
				orderWithPickLines
					.Select(o => (Order: o.Key, PickLines: o.Value, AvailableQty: o.Value.Sum(pl => pl.WZ_Units)))
					.OrderByDescending(o => o.AvailableQty)
					.ToArray();
		}

		static void CreatePackagesPerOrder(
			bool isReleaseCaptured,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines,
			HashSet<string> supportedPackTypes,
			Dictionary<ZGuid, ZBool> orderEnablesPackValues,
			Dictionary<PkgPackageJob, List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>> packagesToCreate,
			WhsProduct product,
			ZString stockKeepingUnit,
			List<PickedPackTypeInfo> pickedPackageTypeInfos,
			(WhsOrder Order, List<WhsPickLine> PickLines, decimal AvailableQty)[] ordersWithPickLines)
		{
			foreach (var (order, pickLines, availableQty) in ordersWithPickLines)
			{
				if (orderEnablesPackValues[order.PK] && pickLines.Count > 0)
				{
					var packageJob = order.Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, order.PK));
					var pkgCount = pickedPackageTypeInfos.Count;
					var qtyLeft = availableQty;
					for (var index = pkgCount - 1; index >= 0; index--)
					{
						var packageTypeInfo = pickedPackageTypeInfos[index];
						var isStockUnit = stockKeepingUnit.EqualsIgnoringCase(packageTypeInfo.PackType);
						var isSupportedPackType = supportedPackTypes.Contains(packageTypeInfo.PackType);
						if (!isStockUnit && isSupportedPackType && qtyLeft >= packageTypeInfo.UnitsPicked)
						{
							qtyLeft -= ConstructPackageInfosFromOrder(packageTypeInfo, pickLines, packageJob, isReleaseCaptured, newlySplitPickLines, product.Parent, packagesToCreate);
							pickedPackageTypeInfos.RemoveAt(index);
						}

						if (qtyLeft == 0)
						{
							break;
						}
					}
				}
			}
		}

		static decimal ConstructPackageInfosFromOrder(
			PickedPackTypeInfo packageInfo,
			List<WhsPickLine> pickLinesToPack,
			PkgPackageJob packageJob,
			bool isReleaseCaptured,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines,
			OrgSupplierPart orgSupplierPart,
			Dictionary<PkgPackageJob, List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>> packagesToCreate)
		{
			decimal qtyPacked;
			if (isReleaseCaptured)
			{
				qtyPacked = ConstructPackageInfosForRCAs(pickLinesToPack, packageJob, packageInfo, newlySplitPickLines, orgSupplierPart, packagesToCreate);
			}
			else
			{
				qtyPacked = ConstructPackageInfos(pickLinesToPack, packageJob, packageInfo, newlySplitPickLines, orgSupplierPart, packagesToCreate);
			}
			return qtyPacked;
		}

		static decimal ConstructPackageInfosForRCAs(
			List<WhsPickLine> pickLinesToPackFrom,
			PkgPackageJob packageJob,
			PickedPackTypeInfo packTypeInfo,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines,
			OrgSupplierPart orgSupplierPart,
			Dictionary<PkgPackageJob, List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>> packagesToCreate)
		{
			var newRCASplitPickLines = new Dictionary<WhsPickLine, List<WhsPickLine>>();
			PickLineUpdater.UpdateReleaseCapturedAttribs(packTypeInfo.ReleaseCapturedInfos, pickLinesToPackFrom, newRCASplitPickLines);
			var pickLinesRCAed = pickLinesToPackFrom.Concat(newRCASplitPickLines.SelectMany(kv => kv.Value)).Where(pl => pl.HasReleaseCapturedAttribs).ToList();

			UpdatePackageInfos(packageJob, packTypeInfo, orgSupplierPart, packagesToCreate, pickLinesRCAed);

			foreach (var item in newRCASplitPickLines)
			{
				if (!newlySplitPickLines.TryGetValue(item.Key, out var pickLines))
				{
					newlySplitPickLines.Add(item.Key, item.Value);
				}
				else
				{
					pickLines.AddRange(item.Value);
				}
			}
			return pickLinesRCAed.Sum(pl => pl.WZ_Units);
		}

		static void UpdatePackageInfos(
			PkgPackageJob packageJob,
			PickedPackTypeInfo packTypeInfo,
			OrgSupplierPart orgSupplierPart,
			Dictionary<PkgPackageJob, List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>> packagesToCreate,
			List<WhsPickLine> pickLines)
		{
			if (!packagesToCreate.TryGetValue(packageJob, out var pkgInfo))
			{
				packagesToCreate[packageJob] = pkgInfo = new List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>();
			}
			pkgInfo.Add((packTypeInfo, orgSupplierPart, pickLines));
		}

		static decimal ConstructPackageInfos(
			List<WhsPickLine> pickLinesToPackFrom,
			PkgPackageJob packageJob,
			PickedPackTypeInfo packTypeInfo,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines,
			OrgSupplierPart orgSupplierPart,
			Dictionary<PkgPackageJob, List<(PickedPackTypeInfo, OrgSupplierPart, List<WhsPickLine>)>> packagesToCreate)
		{
			var unitsToPack = packTypeInfo.UnitsPicked;
			var pickLinesToPack = new List<WhsPickLine>();
			var qtyPacked = 0m;
			foreach (var pickLine in pickLinesToPackFrom.OrderByDescending(pl => pl.WZ_Units))
			{
				var qtyPackedForPickLine = 0m;
				WhsPickLine splitPickLine;
				var itemQuantity = pickLine.WZ_Units;
				if (itemQuantity <= unitsToPack)
				{
					pickLinesToPack.Add(pickLine);
					qtyPackedForPickLine = itemQuantity;
					pickLinesToPackFrom.Remove(pickLine);
				}
				else
				{
					splitPickLine = pickLine.Split(unitsToPack);
					qtyPackedForPickLine = unitsToPack;
					pickLinesToPack.Add(splitPickLine);
					PickLineUpdater.AddNewSplitPickLine(newlySplitPickLines, pickLine, splitPickLine);
				}

				unitsToPack -= qtyPackedForPickLine;
				qtyPacked += qtyPackedForPickLine;

				if (unitsToPack <= 0)
				{
					break;
				}
			}
			UpdatePackageInfos(packageJob, packTypeInfo, orgSupplierPart, packagesToCreate, pickLinesToPack);

			return qtyPacked;
		}

		static void CreatePackages(
			Dictionary<PkgPackageJob, List<(PickedPackTypeInfo PackTypeInfo, OrgSupplierPart Product, List<WhsPickLine> PickLines)>> packagesToCreate,
			Dictionary<ZString, decimal> totalPalletQuantities,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines)
		{
			foreach (var packageToCreate in packagesToCreate)
			{
				var packageJob = packageToCreate.Key;
				var pkgInfos = packageToCreate.Value;
				foreach (var (packTypeInfo, product, pickLines) in pkgInfos)
				{
					var package = CreatePackage(packTypeInfo, packageJob, totalPalletQuantities, product);
					foreach (var pickLine in pickLines)
					{
						package.Pack(pickLine, allReleaseLines.Value[((IPackableItem)pickLine).Key]);
					}
					package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				}
			}
		}

		static PkgPackage CreatePackage(PickedPackTypeInfo packageInfo, PkgPackageJob packageJob, Dictionary<ZString, decimal> totalPalletQuantities, OrgSupplierPart orgSupplierPart)
		{
			var package = packageJob.Packages.AddNew(packageInfo.PackType);
			AllocatePackageLabelsHelper.FillPackagePropertiesFromProduct(package, orgSupplierPart, packageInfo.PackType);

			var pickedPalletID = packageInfo.PalletID.Trim().ToUpper();

			if (!string.IsNullOrWhiteSpace(pickedPalletID))
			{
				var totalPalletQty = totalPalletQuantities[pickedPalletID];

				if (packageInfo.UnitsPicked == totalPalletQty)
				{
					package.KP_PackageID = pickedPalletID;
				}
			}
			return package;
		}

		static void UpdatePickLinesWithLooseRCAs(
			IReadOnlyList<WhsPickLine> pickedPickLines,
			IEnumerable<WhsReleaseCapturedInfo> releaseCapturedInfosForPickLines,
			Dictionary<WhsPickLine, List<WhsPickLine>> newlySplitPickLines)
		{
			var pickLinesWithoutRCAs = pickedPickLines.Where(p => !p.HasReleaseCapturedAttribs).ToArray();
			if (pickLinesWithoutRCAs.Length > 0)
			{
				PickLineUpdater.UpdateReleaseCapturedAttribs(releaseCapturedInfosForPickLines, pickLinesWithoutRCAs, newlySplitPickLines);
			}
		}

		static Dictionary<ZString, decimal> LoadInventoryQuantitiesPerPallet(BusinessObjectFactory factory, IEnumerable<string> palletIDs, ZGuid whsPK)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, whsPK);
			query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, palletIDs);
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			var inventoryLines = factory.Load<WhsInventoryView>(query);

			return inventoryLines
				.GroupBy(i => i.WI_PalletID.ToUpper())
				.ToDictionary(i => i.Key, i => i.Sum(l => l.WI_TotalUnits));
		}
	}
}
