using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using LazyAvailInvCache = System.Lazy<System.Collections.Generic.Dictionary<CargoWise.Types.ZGuid, Enterprise.Warehouse.Transactions.Business.WhsPickAvailableInventory>>;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class AllocatePackageLabelsStrategy : IAllocatePackageLabelsStrategy
	{
		#region RunChecksPriorToCartonisingOrPickingByLabel

		public bool RunChecksPriorToCartonisingOrPickingByLabel(WhsPick pick, bool saveFactory = true)
		{
			Argument.NotNull(pick, nameof(pick));

			var canProceed = false;

			if (saveFactory && pick.HasChanges)
			{
				pick.NotificationSubscriber.AddError(Res.GetString("0a415e87-651b-4f09-a11c-d52af40d2035",
					"You must save the Pick before Allocating Package Labels."));
			}
			else if (pick.IsReadyForPlanningOrPlanned)
			{
				pick.NotificationSubscriber.AddError(Res.GetString("afcfbb7a-5e28-4226-a5cf-22fec656e36f",
					"You cannot Allocate Package Labels because the pick is Ready For Planning or Planned."));
			}
			else if (!(pick.Orders.Count > 0))
			{
				pick.NotificationSubscriber.AddError(Res.GetString("84ab64ce-9351-4a3f-a72c-fa2aaf80faf8",
					"No Orders to Allocate Package Labels for."));
			}
			else if (pick.WP_IsAwaitingReplenishment)
			{
				pick.NotificationSubscriber.AddError(Res.GetString("F99F4DA9-AB72-4E9E-8D6A-7D34C5D40B9A",
					"You cannot Allocate Package Labels if the pick is awaiting replenishment."));
			}
			else if (pick.WP_PickStatus == PickStatus.Codes.Building)
			{
				pick.NotificationSubscriber.AddError(Res.GetString("A78EB6C3-52EF-4FBE-8289-37767CA020A3",
					"You cannot Allocate Package Labels until Fulfillment Rules are met or overridden."));
			}
			else if (!pick.IsPickByUOMEnabled)
			{
				if (pick.Warehouse.WW_IsPickByUOMEnabled)
				{
					pick.NotificationSubscriber.AddError(Res.GetString("92fed0b3-8674-4867-9240-0c724ee0eb63",
					"Pick By UOM must be enabled to Allocate Package Labels. If the Pick was created prior to setting Pick By UOM, you must Reallocate Stock to the Pick."));
				}
				else
				{
					pick.NotificationSubscriber.AddError(Res.GetString("d1074668-a045-4b72-b17d-1cbfd4258594",
					"Pick By UOM must be enabled to Allocate Package Labels."));
				}
			}
			// IsPickByUOMEnabled check above will load all pickline fetch hints when it is true. If this changes, add fetch hints for Loading Picklines here.
			else if (!pick.Orders.Cast<WhsPickableDocket>().SelectMany(o => o.Lines).SelectMany(l => l.PickLines).Any())
			{
				pick.NotificationSubscriber.AddError(Res.GetString("46e045bf-aa93-4f13-8b8d-f6f4711795a0",
					"No Allocations to Allocate Package Labels to."));
			}
			else
			{
				var ordersToPossiblyRemove = GetOrdersToPossiblyRemove(pick).ToArray();
				canProceed = ordersToPossiblyRemove.Length < pick.Orders.Count;

				if (ordersToPossiblyRemove.Length > 0)
				{
					if (!canProceed)
					{
						pick.NotificationSubscriber.AddError(Res.GetString("25035eaf-570e-43e5-9293-37ed7cf39fef",
							"No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details."));
					}
					else
					{
						ordersToPossiblyRemove.ForEach(pick.Orders.Remove);
					}

					if (saveFactory)
					{
						FactorySaveSafe(pick.Factory);
					}
				}
			}

			return canProceed;
		}

		static IEnumerable<WhsOrder> GetOrdersToPossiblyRemove(WhsPick pick)
		{
			foreach (var order in pick.Orders.Cast<WhsOrder>())
			{
				var orderLines = order.Lines.Cast<WhsPickableDocketLine>();
				var nonComponentPickLine = orderLines.Cast<WhsPickableDocketLine>().Where(l => !l.IsComponentLineOnSalesOrder).SelectMany(l => l.PickLines);

				var packTypesMissingUOM = nonComponentPickLine
					.Select(pl => pl.WZ_F3_NKAllocatedPackType)
					.Where(packTypeCode =>
					{
						var packType = pick.Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, packTypeCode);
						return packType == null || packType.F3_UOMType.IsEmpty;
					}).ToArray();

				var productCodesUsingRCAs = nonComponentPickLine.Where(pl => pl.HasReleaseCapturedAttribs).Select(pl => pl.ProductCode).Distinct().ToArray();
				var productCodesHasBeenPicked = nonComponentPickLine.Where(pl => pl.IsPickedFromPutawayLocation).Select(pl => pl.ProductCode).Distinct().ToArray();

				if (packTypesMissingUOM.Length > 0 || productCodesUsingRCAs.Length > 0 || productCodesHasBeenPicked.Length > 0)
				{
					var now = ZDateTimeOffset.Now;
					var orderDocketID = order.WD_DocketID;

					CreateErrorLogIfNeeded(pick, packTypesMissingUOM, CodeType_PackType, orderDocketID, Constants.EventReferenceParameterReasons.NoUOMType, now);
					CreateErrorLogIfNeeded(pick, productCodesUsingRCAs, CodeType_Product, orderDocketID, Constants.EventReferenceParameterReasons.LineHasBeenReleaseCaptured, now);
					CreateErrorLogIfNeeded(pick, productCodesHasBeenPicked, CodeType_Product, orderDocketID, Constants.EventReferenceParameterReasons.LineHasBeenPicked, now);

					yield return order;
				}
			}
		}

		static string CodeType_PackType
		{
			get { return Res.GetString("bde61ef7-c9bc-484b-b8c4-abb1e3c997aa", "Pack Type"); }
		}

		static string CodeType_Product
		{
			get { return Res.GetString("1cef65a1-37a2-412c-82c9-188e2901ecfa", "Product"); }
		}

		static void CreateErrorLogIfNeeded(WhsPick pick, IEnumerable<ZString> codesToLog, string codeType, ZString orderID, string eventReferenceReason, ZDateTimeOffset logTime)
		{
			foreach (var code in codesToLog)
			{
				var reference = string.Format(Culture.Invariant, (NoResString)"Order: {0} {1}: {2}", orderID, codeType, code); // Log Reference Values should be in English Only
				CreateErrorLog(pick, reference, eventReferenceReason, logTime);
			}
		}

		#endregion

		#region CartoniseSplitCases

		public CartonisationResult CartoniseSplitCases(WhsPick pick, bool saveFactory = true)
		{
			Argument.NotNull(pick, nameof(pick));
			var result = CartonisationResult.NothingToCartonise;

			var cartonisationAlgorithm = ObjectFactory.Get<ICartonisation>();
			var pickLinesFailedToCartonise = new List<WhsPickLine>();
			var pickLinesAvailableInventoryCache = new LazyAvailInvCache(() => BuildPickLinesAvailableInventoryCache(pick));
			var releaseLinesByAttributes = new Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>>(() => BuildReleaseLinesDictionary(pick));
			AddFetchHints(pick);

			var ordersToPossiblyRemove = new List<WhsOrder>();
			foreach (var order in pick.Orders.Cast<WhsOrder>())
			{
				var resultForOrder = TryToCartoniseOrder(pick, cartonisationAlgorithm, order, pickLinesAvailableInventoryCache, releaseLinesByAttributes, pickLinesFailedToCartonise);
				if (resultForOrder == CartonisationResult.Error)
				{
					AddErrorLogsForPickLinesFailedToCartonise(pick, pickLinesFailedToCartonise);
					ordersToPossiblyRemove.Add(order);
				}
				else if (resultForOrder == CartonisationResult.Cartonised)
				{
					result = CartonisationResult.Cartonised;
				}
			}

			if (ordersToPossiblyRemove.Count > 0)
			{
				var shouldRemoveOrders = ordersToPossiblyRemove.Count < pick.Orders.Count;
				if (shouldRemoveOrders)
				{
					ordersToPossiblyRemove.ForEach(pick.Orders.Remove);
				}
				else
				{
					result = CartonisationResult.Error;
					pick.NotificationSubscriber.AddError(Res.GetString("a57aa672-ea24-46a4-99b4-db7708633d05", "No Order(s) could be Cartonized. Check the Events on the Pick for more details."));
				}

				if (saveFactory)
				{
					FactorySaveSafe(pick.Factory);
				}
			}

			return result;
		}

		CartonisationResult TryToCartoniseOrder(WhsPick pick, ICartonisation algorithm, WhsOrder order, LazyAvailInvCache pickLinesAvailableInventoryCache,
			Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>> releaseLinesByAttributes, List<WhsPickLine> pickLinesFailedToCartonise)
		{
			var result = CartonisationResult.Error;
			var pickLinesToCartonise = GetPickLinesToCartonise(order);
			var packageJob = order.PackageJob;

			if (packageJob != null && pickLinesToCartonise.Count > 0)
			{
				var orderCartonGroup = order.CartonGroup;
				var client = order.Client;
				var pickingParams = order.ClientPickingParams;
				var cartonisationGroupingKeyDict = new Dictionary<WhsPickLine, CartonisationGroupingKey>();
				var allPickLines = pickLinesToCartonise.Values;
				foreach (var pickLine in allPickLines)
				{
					var cartonisationGroupKey = GetCartonisationGroupKey(client, pickingParams, pickLine, orderCartonGroup);
					if (cartonisationGroupKey is null)
					{
						break;
					}
					else
					{
						cartonisationGroupingKeyDict.Add(pickLine, cartonisationGroupKey);
					}
				}

				if (cartonisationGroupingKeyDict.Count != allPickLines.Count)
				{
					CreateErrorLog(pick, Res.GetString("167cfc8c-ea29-44f0-aef9-f2382ef3b729", "Order: {0}", order.WD_DocketID), Constants.EventReferenceParameterReasons.NoCartonGroup, ZDateTimeOffset.Now);
				}
				else
				{
					var groupedPickLines = cartonisationGroupingKeyDict.Keys.GroupBy(pl => cartonisationGroupingKeyDict[pl]);

					var groupedCartonisationInstructions = new Dictionary<CartonisationGroupingKey, IEnumerable<ICartonWithItems>>();
					foreach (var pickLineAndCartonGroupGrouping in groupedPickLines)
					{
						var cartonisationInstructions = TryToCartonisePickLines(algorithm, pickLineAndCartonGroupGrouping, pickLinesFailedToCartonise);

						if (cartonisationInstructions.Any())
						{
							groupedCartonisationInstructions.Add(pickLineAndCartonGroupGrouping.Key, cartonisationInstructions);
							result = CartonisationResult.Cartonised;
						}
					}

					if (result == CartonisationResult.Cartonised)
					{
						CreateCartonisedPackages(packageJob, pickLinesAvailableInventoryCache, releaseLinesByAttributes, pickLinesToCartonise, groupedCartonisationInstructions);
					}
				}
			}
			else
			{
				result = CartonisationResult.NothingToCartonise;
			}

			return result;
		}

		#region GetCartonisationGroupKey

		CartonisationGroupingKey GetCartonisationGroupKey(OrgHeader client, WhsClientPickPackParamsByWhs clientPickingParams, WhsPickLine pickLine, CartonGroupResult orderCartonGroup)
		{
			CartonisationGroupingKey cartonisationGroupingKey = null;
			var pickLineProduct = pickLine.Product;
			var cartonGroup = GetCartonGroupWithHighestPriority(client, pickLineProduct, orderCartonGroup);
			if (cartonGroup != null)
			{
				var isCartoniseByProduct = clientPickingParams?.WPP_CartonizeByProduct ?? false;
				var isCartoniseByProductCategory = clientPickingParams?.WPP_CartonizeByProductCategory ?? false;

				var categoryPK = new Lazy<ZGuid>(() => GetCachedCategoryPKByClientAndProduct(client, pickLineProduct));

				if (isCartoniseByProductCategory && categoryPK.Value == ZGuid.Empty)
				{
					isCartoniseByProduct = true;
					isCartoniseByProductCategory = false;
				}

				cartonisationGroupingKey = new CartonisationGroupingKey(clientPickingParams?.WPP_CartoniseByArea ?? true ? (pickLine.InventoryLine.Location?.WLV_WA_PickingArea ?? ZGuid.Empty) : ZGuid.Empty,
					isCartoniseByProduct ? pickLineProduct.PK : ZGuid.Empty,
					isCartoniseByProductCategory ? categoryPK.Value : ZGuid.Empty,
					cartonGroup);
			}
			return cartonisationGroupingKey;
		}

		WhsCartonGroup GetCartonGroupWithHighestPriority(OrgHeader client, WhsProduct product, CartonGroupResult cartonGroupResult)
		{
			WhsCartonGroup cartonGroup = null;

			if (cartonGroupResult.ProductCartonGroupTakesPrecedence)
			{
				cartonGroup = GetCachedOverridenCartonGroup(client, product);
			}

			if (cartonGroup == null && cartonGroupResult.OrgCartonGroupPK.IsValid)
			{
				cartonGroup = client.Factory.Load<WhsCartonGroup>(cartonGroupResult.OrgCartonGroupPK);
			}

			return cartonGroup;
		}

		class CartonisationGroupingKey
		{
			public CartonisationGroupingKey(ZGuid areaPK, ZGuid productPK, ZGuid categoryPK, WhsCartonGroup cartonGroup)
			{
				AreaPK = areaPK;
				CartonGroup = cartonGroup;
				ProductPK = productPK;
				CategoryPK = categoryPK;
			}

			ZGuid AreaPK { get; }
			ZGuid ProductPK { get; }
			ZGuid CategoryPK { get; }
			public WhsCartonGroup CartonGroup { get; }

			#region Operator Overloads

			public override bool Equals(object obj) => obj != null && (obj as CartonisationGroupingKey) == this;
			public static bool operator ==(CartonisationGroupingKey x, CartonisationGroupingKey y) => x.CartonGroup == y.CartonGroup && x.AreaPK == y.AreaPK && x.ProductPK == y.ProductPK && x.CategoryPK == y.CategoryPK;
			public static bool operator !=(CartonisationGroupingKey x, CartonisationGroupingKey y) => !(x == y);
			public override int GetHashCode() => CartonGroup.GetHashCode() ^ AreaPK.GetHashCode() ^ ProductPK.GetHashCode() ^ CategoryPK.GetHashCode();

			#endregion
		}

		ZGuid GetCachedCategoryPKByClientAndProduct(OrgHeader client, WhsProduct whsProduct)
		{
			if (!CategoryPKCache.TryGetValue(client, out var cacheByClient))
			{
				cacheByClient = new Dictionary<WhsProduct, ZGuid>();
				CategoryPKCache.Add(client, cacheByClient);
			}

			if (!cacheByClient.TryGetValue(whsProduct, out var cachedGuid))
			{
				cachedGuid = whsProduct.GetOwnerRelationship(client).OU_OPC_Category;
				cacheByClient.Add(whsProduct, cachedGuid);
			}

			return cachedGuid;
		}

		Dictionary<OrgHeader, Dictionary<WhsProduct, ZGuid>> CategoryPKCache
		{
			get { return categoryPKCache ?? (categoryPKCache = new Dictionary<OrgHeader, Dictionary<WhsProduct, ZGuid>>()); }
		}
		Dictionary<OrgHeader, Dictionary<WhsProduct, ZGuid>> categoryPKCache;

		#endregion

		static IEnumerable<ICartonWithItems> TryToCartonisePickLines(ICartonisation algorithm, IGrouping<CartonisationGroupingKey, WhsPickLine> pickLineAndCartonGroupGrouping, List<WhsPickLine> pickLinesFailedToCartonise)
		{
			var cartonisationResults = Enumerable.Empty<ICartonWithItems>();

			var cartonDefinitions = pickLineAndCartonGroupGrouping.Key.CartonGroup.CartonGroupSizeLinks;
			if (cartonDefinitions.Count > 0)
			{
				cartonisationResults = algorithm.CartoniseItems(pickLineAndCartonGroupGrouping, cartonDefinitions);
			}

			var cartonisedPickLinesPKs = new HashSet<Guid>(cartonisationResults.SelectMany(ci => ci.Items).Select(pl => pl.CartonisableItemPK).Distinct().ToArray());
			var linesNotCartonised = pickLineAndCartonGroupGrouping.Where(pl => !cartonisedPickLinesPKs.Contains(pl.PK.ToGuid())).ToArray();

			if (linesNotCartonised.Length > 0)
			{
				pickLinesFailedToCartonise.AddRange(linesNotCartonised);
				cartonisationResults = Enumerable.Empty<ICartonWithItems>();
			}

			return cartonisationResults;
		}

		static void CreateCartonisedPackages(PkgPackageJob packageJob,
			LazyAvailInvCache pickLinesAvailableInventoryCache, Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>> releaseLinesByAttributes, Dictionary<ZGuid, WhsPickLine> pickLinesToCartonise, Dictionary<CartonisationGroupingKey, IEnumerable<ICartonWithItems>> cartonisationInstructionsByCartonGroup)
		{
			RemoveAllocatedPackTypesAndPickedBy(pickLinesToCartonise.Values, pickLinesAvailableInventoryCache);
			var packagesToGenerateIDsFor = CreateAndReturnCartonisedPackages(packageJob, pickLinesAvailableInventoryCache, releaseLinesByAttributes, pickLinesToCartonise, cartonisationInstructionsByCartonGroup).ToArray();
			packagesToGenerateIDsFor.ForEach(p => ((ISupportPackageIDGeneration)p).ShouldGenerateIDOnSaving = true);
		}

		static IEnumerable<PkgPackage> CreateAndReturnCartonisedPackages(PkgPackageJob packageJob,
			LazyAvailInvCache pickLinesAvailableInventoryCache, Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>> releaseLinesByAttributes, Dictionary<ZGuid, WhsPickLine> pickLinesToCartonise, Dictionary<CartonisationGroupingKey, IEnumerable<ICartonWithItems>> cartonisationInstructionsByCartonGroup)
		{
			foreach (var cartonGroupAndInstructions in cartonisationInstructionsByCartonGroup)
			{
				var cartonGroup = cartonGroupAndInstructions.Key.CartonGroup;
				var cartonisationInstructions = cartonGroupAndInstructions.Value;

				foreach (var cartonAndItemGrouping in cartonisationInstructions)
				{
					var carton = packageJob.Factory.Load<WhsCartonSize>(cartonAndItemGrouping.CartonPK);
					var package = packageJob.Packages.AddNew(carton == null ? Constants.PkgUnit.Carton : carton.WCS_F3_NKPackType);
					if (carton != null)
					{
						package.SetValuesFromTemplate(carton);
						package.CartonGroupAndSize = string.Format("{0} - {1}", cartonGroup.WCG_Code, carton.WCS_Code);
					}

					var pickLinesToAttach = SplitPickLinesIfNeccessary(pickLinesAvailableInventoryCache, pickLinesToCartonise, cartonAndItemGrouping);
					foreach (var pickLine in pickLinesToAttach)
					{
						var attributesKey = WhsReleaseLineCollection.GetKey(pickLine.InventoryLine);
						var releaseLinesKey = new ReleaseLineKey(pickLine.WZ_WE_TransactionLine, attributesKey);
						LinkPickLineToPackage(pickLine, package, releaseLinesByAttributes.Value[releaseLinesKey]);
					}

					yield return package;
				}
			}
		}

		static IEnumerable<WhsPickLine> SplitPickLinesIfNeccessary(LazyAvailInvCache pickLinesAvailableInventoryCache, Dictionary<ZGuid, WhsPickLine> pickLinesToCartonise, ICartonWithItems cartonAndItemGrouping)
		{
			foreach (var pickLineAndQty in cartonAndItemGrouping.Items)
			{
				var pickLine = pickLinesToCartonise[pickLineAndQty.CartonisableItemPK];
				if (pickLine.WZ_Units > pickLineAndQty.Quantity)
				{
					var availableInventory = pickLinesAvailableInventoryCache.Value[pickLine.PK];
					var splitPickLine = pickLine.Split(pickLineAndQty.Quantity);
					pickLinesAvailableInventoryCache.Value[splitPickLine.PK] = availableInventory; // update AvailableInventoryCache so that we can get it for the Split PickLine
					yield return splitPickLine;
				}
				else
				{
					yield return pickLine;
				}
			}
		}

		static void RemoveAllocatedPackTypesAndPickedBy(IEnumerable<WhsPickLine> pickLines, LazyAvailInvCache pickLinesAvailableInventoryCache)
		{
			var availableInventoriesToRefresh = new HashSet<WhsPickAvailableInventory>();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_GS_NKAssignedTo = string.Empty;
				pickLine.WZ_F3_NKAllocatedPackType = pickLine.WZ_UnitsUQ;
				availableInventoriesToRefresh.Add(pickLinesAvailableInventoryCache.Value[pickLine.PK]);
			}

			foreach (var availInv in availableInventoriesToRefresh)
			{
				availInv.AvailableInventoriesSplitByUOM.NeedsRefresh = true;
				availInv.AvailableInventoriesSplitByUOM.RefreshBinding();
			}
		}

		Dictionary<ZGuid, WhsPickLine> GetPickLinesToCartonise(WhsOrder order)
		{
			// Dictionary because the cartonisation result references pick line PKs, not pick lines. Saves us searching for pick lines.
			const string splitCaseUOMType = UOMPackTypesList.Codes.SplitCase;

			return order.Lines.Cast<WhsOrderLine>()
				.Where(l => !l.IsComponentLineOnSalesOrder)
				.SelectMany(l => l.PickLines)
				.Where(pl => GetCachedPackTypeFromQuantityUQ(pl.WZ_F3_NKAllocatedPackType, order.Factory) == splitCaseUOMType)
				.ToDictionary(pl => pl.PK);
		}

		static Dictionary<ZGuid, WhsPickAvailableInventory> BuildPickLinesAvailableInventoryCache(WhsPick pick)
		{
			// We need access to available inventory to split pick lines (or we would have to invalidate the caches)
			// We cant iterate through AvailableInventory in this case as we run the algorithm per order
			// Instead, we create this cache to minimise computation for searching for available inventory. Lazy as we may not need to split lines.
			var cache = new Dictionary<ZGuid, WhsPickAvailableInventory>();
			foreach (WhsPickAvailableInventory availableInventory in pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(o => o.AvailableInventories))
			{
				foreach (var pickLine in availableInventory.PickLines)
				{
					cache.Add(pickLine.PK, availableInventory);
				}
			}

			return cache;
		}

		static Dictionary<ReleaseLineKey, WhsReleaseLine> BuildReleaseLinesDictionary(WhsPick pick)
		{
			var result = new Dictionary<ReleaseLineKey, WhsReleaseLine>();

			foreach (WhsPickableDocket order in pick.Orders)
			{
				foreach (WhsPickableDocketLine orderLine in order.Lines)
				{
					foreach (WhsReleaseLine releaseLine in orderLine.ReleaseLines)
					{
						var key = new ReleaseLineKey(orderLine.PK, WhsReleaseLineCollection.GetKey(releaseLine));
						result[key] = releaseLine;
					}
				}
			}

			return result;
		}

		class ReleaseLineKey
		{
			public ReleaseLineKey(ZGuid orderLinePK, string key)
			{
				Key = key;
				OrderLinePK = orderLinePK;
			}

			public string Key { get; }
			public ZGuid OrderLinePK { get; }

			public override bool Equals(object obj)
			{
				var objAsKey = obj as ReleaseLineKey;
				return !object.ReferenceEquals(objAsKey, null) && objAsKey == this;
			}

			public static bool operator ==(ReleaseLineKey x, ReleaseLineKey y) => x.OrderLinePK == y.OrderLinePK && x.Key == y.Key;
			public static bool operator !=(ReleaseLineKey x, ReleaseLineKey y) => !(x == y);

			public override int GetHashCode() => OrderLinePK.GetHashCode() ^ Key.GetHashCode();
		}

		static void AddFetchHints(WhsPick pick)
		{
			// Tested in PickEntryForm.cs
			var factory = pick.Factory;
			var inventoryLinePKs = new List<ZGuid>();
			foreach (var pickLine in pick.GetAllPickLines())
			{
				inventoryLinePKs.Add(pickLine.WZ_WE_InventoryLine);
				factory.AddFetchHint(PkgPackageItemDivotSchema.KI_ParentID, pickLine.PK);
			}

			var inventoryLinePKsQuery = new ZQuery(WhsDocketLineSchema.PK, inventoryLinePKs);

			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLineSubQuery.AddToFilter(inventoryLinePKsQuery);

			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), StmALogSchema.SL_Parent);
			docketSubQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			inventoryLinePKs.ForEach(pk => factory.AddFetchHint(WhsDocketLineSchema.PK, pk)); // Need to load inventory for Product on pick line
			factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);

			foreach (var orderAndPackageJob in pick.Orders.Cast<WhsOrder>().Select(o => new { Order = o, PackageJob = o.PackageJob }).Where(o => o.PackageJob != null))
			{
				var order = orderAndPackageJob.Order;
				factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
				factory.AddFetchHint(PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, orderAndPackageJob.PackageJob.PK);
				factory.AddFetchHint(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, order.WD_OH_Client);

				var orgPKs = new[] { order.TransportCoPK, order.ConsigneePK, order.WD_OH_Client, order.Warehouse?.WarehouseAddress?.OA_OH };
				foreach (var orgPK in orgPKs.Where(op => op.HasValue))
				{
					factory.AddFetchHint(OrgMiscServSchema.OM_OH, orgPK);
				}
			}
		}

		static void AddErrorLogsForPickLinesFailedToCartonise(WhsPick pick, IEnumerable<WhsPickLine> pickLinesNotCartonised)
		{
			var now = ZDateTimeOffset.Now;

			var pickLinesNotCartonisedInfos = pickLinesNotCartonised.Select(pl =>
				Res.GetString("78689a3a-082e-4480-ad08-a57c1a072851", "Order: {0} Product: {1}", pl.DocketLine.Docket.WD_DocketID, pl.SupplierPart.OP_PartNum)).Distinct();

			foreach (var info in pickLinesNotCartonisedInfos)
			{
				CreateErrorLog(pick, info, Constants.EventReferenceParameterReasons.FailedToCartonize, now);
			}
		}

		static void CreateErrorLog(WhsPick pick, string reference, string reason, ZDateTimeOffset time)
		{
			var typeParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Cartonization);
			var reasonParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason);
			pick.Logs.CreateOrRecreateEventLog(Events.ErrorReport, EstimateActual.Actual, time, reference, typeParameter, reasonParameter);
		}

		#endregion

		#region PickCasesByLabel

		public bool PickCasesByLabel(WhsPick pick)
		{
			Argument.NotNull(pick, nameof(pick));
			return PickByLabelCore(pick, UOMPackTypesList.Codes.Case);
		}

		#endregion

		#region PickPalletsByLabel

		public bool PickPalletsByLabel(WhsPick pick)
		{
			Argument.NotNull(pick, nameof(pick));
			return PickByLabelCore(pick, UOMPackTypesList.Codes.Pallet);
		}

		#endregion

		#region PickByLabelCore

		bool PickByLabelCore(WhsPick pick, string uomType)
		{
			var factory = pick.Factory;
			var packagesToGenerateIDsFor = new List<PkgPackage>();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(factory)) // for performance
			{
				foreach (var orderAndPackageJob in pick.Orders.Cast<WhsOrder>().Select(o => new { Order = o, o.PackageJob }).Where(o => o.PackageJob != null))
				{
					factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, orderAndPackageJob.Order.PK);
					factory.AddFetchHint(PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, orderAndPackageJob.PackageJob.PK);
				}

				foreach (var pickLine in pick.GetAllPickLines())
				{
					factory.AddFetchHint(PkgPackageItemDivotSchema.KI_ParentID, pickLine.PK);
				}

				var releaseLinesByAttributes = new Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>>(() => BuildReleaseLinesDictionary(pick));

				foreach (var availInventory in pick
						.OrderedInventories.Cast<WhsPickOrderedInventory>()
						.SelectMany(orderedInventory => orderedInventory.AvailableInventories).Cast<WhsPickAvailableInventory>())
				{
					packagesToGenerateIDsFor.AddRange(PickAvailableInventoryByLabel(releaseLinesByAttributes, availInventory, uomType));
				}

				packagesToGenerateIDsFor.ForEach(p => ((ISupportPackageIDGeneration)p).ShouldGenerateIDOnSaving = true);
			}

			return packagesToGenerateIDsFor.Count > 0;
		}

		IEnumerable<PkgPackage> PickAvailableInventoryByLabel(Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>> releaseLinesByAttributes, WhsPickAvailableInventory availableInventory, string uomType)
		{
			var pickLineToPackGroupedByOrders = availableInventory.PickLines
				.Where(pl => GetCachedPackTypeFromQuantityUQ(pl.WZ_F3_NKAllocatedPackType, pl.Factory) == uomType && ShouldPackPickLine(pl))
				.GroupBy(pl => pl?.DocketLine.WE_WD);

			foreach (var pickLineGroupings in pickLineToPackGroupedByOrders)
			{
				var order = (WhsOrder)(pickLineGroupings.First()?.DocketLine)?.Docket;
				var packageJob = order?.PackageJob;

				if (packageJob != null)
				{
					foreach (var pickLines in pickLineGroupings.GroupBy(pl => pl.WZ_F3_NKAllocatedPackType))
					{
						var unitsPerPackType = GetUnitsPerPackType(availableInventory?.WhsProduct?.Parent, pickLines.Key);
						pickLines.ForEach(pl => pl.WZ_GS_NKAssignedTo = ZString.Empty);

						foreach (var pkg in
							CreatePackagesForNonAggregatedPickLines(releaseLinesByAttributes, pickLines.Where(pl => pl.WZ_Units >= unitsPerPackType).ToArray(), packageJob, unitsPerPackType).ToArray()
							.Concat(CreatePackagesForAggregatedPickLines(releaseLinesByAttributes, pickLines.Where(pl => pl.WZ_Units < unitsPerPackType).OrderByDescending(pl => pl.WZ_Units).ToArray(), packageJob, unitsPerPackType).ToArray()))
						{
							yield return pkg;
						}
					}
				}
			}
		}

		bool ShouldPackPickLine(WhsPickLine pickLine)
		{
			var orderLine = (WhsPickableDocketLine)pickLine?.DocketLine;
			var result = !orderLine.IsComponentLineOnSalesOrder;
			if (result)
			{
				var orderPackageJob = ((WhsOrder)orderLine?.Docket)?.PackageJob; // None should be null in practice
				result = orderPackageJob != null && !orderPackageJob.IsPacked(pickLine); // i.e. already packed by Cartonisation
			}
			return result;
		}

		static IEnumerable<PkgPackage> CreatePackagesForNonAggregatedPickLines(Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>> releaseLinesByAttributes,
			WhsPickLine[] pickLines, PkgPackageJob packageJob, decimal unitsPerPack)
		{
			foreach (var pickLine in pickLines)
			{
				while (pickLine.WZ_Units > unitsPerPack)
				{
					yield return CreatePackageForPickLines(releaseLinesByAttributes, packageJob, new[] { pickLine.Split(unitsPerPack) });
				}

				if (pickLine.WZ_Units == unitsPerPack)
				{
					yield return CreatePackageForPickLines(releaseLinesByAttributes, packageJob, new[] { pickLine });
				}
			}
		}

		static IEnumerable<PkgPackage> CreatePackagesForAggregatedPickLines(Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>> releaseLinesByAttributes,
			WhsPickLine[] pickLinesToAggregate, PkgPackageJob packageJob, decimal unitsPerPack)
		{
			var currentUnits = 0m;
			var currentGrouping = new List<WhsPickLine>();

			var pickLineQueue = new Queue<WhsPickLine>(pickLinesToAggregate);
			while (pickLineQueue.Count > 0)
			{
				var currentPickLine = pickLineQueue.Dequeue();

				if ((currentPickLine.WZ_Units + currentUnits) <= unitsPerPack)
				{
					currentUnits += currentPickLine.WZ_Units;
					currentGrouping.Add(currentPickLine);
				}
				else
				{
					// Have to split
					var splitLine = currentPickLine.Split(unitsPerPack - currentUnits);
					currentUnits += splitLine.WZ_Units;
					currentGrouping.Add(splitLine);
					pickLineQueue.Enqueue(currentPickLine);
				}

				if (currentUnits == unitsPerPack)
				{
					yield return CreatePackageForPickLines(releaseLinesByAttributes, packageJob, currentGrouping.ToArray());
					currentGrouping = new List<WhsPickLine>();
					currentUnits = 0m;
				}
			}
		}

		static PkgPackage CreatePackageForPickLines(Lazy<Dictionary<ReleaseLineKey, WhsReleaseLine>> releaseLinesByAttributes, PkgPackageJob packageJob, WhsPickLine[] pickLineToUse)
		{
			var packtype = pickLineToUse[0].WZ_F3_NKAllocatedPackType;
			var package = packageJob.Packages.AddNew(packtype);

			foreach (var pickLine in pickLineToUse)
			{
				var attributesKey = WhsReleaseLineCollection.GetKey(pickLine.InventoryLine);
				var releaseLineKey = new ReleaseLineKey(pickLine.WZ_WE_TransactionLine, attributesKey);
				LinkPickLineToPackage(pickLine, package, releaseLinesByAttributes.Value[releaseLineKey]);
			}

			AllocatePackageLabelsHelper.FillPackagePropertiesFromProduct(package, pickLineToUse[0].Product.Parent, packtype);

			return package;
		}

		static decimal GetUnitsPerPackType(OrgSupplierPart product, ZString pkgType)
		{
			var unitsToSplit = 1m; // in real world should not happen

			if (product != null)
			{
				var conversionFactor = product.UnitConverter.ConversionFactor(pkgType, product.OP_StockKeepingUnit);
				unitsToSplit = conversionFactor != 0 ? conversionFactor : 1;
			}
			return unitsToSplit;
		}

		#endregion

		#region Implementation

		#region LinkPickLineToPackage

		static void LinkPickLineToPackage(WhsPickLine pickLine, PkgPackage package, WhsReleaseLine releaseLine)
		{
			package.Pack(pickLine, releaseLine);
		}

		#endregion

		#region GetCachedPackTypeFromQuantityUQ

		ZString GetCachedPackTypeFromQuantityUQ(ZString quantityUQ, BusinessObjectFactory factory)
		{
			ZString packType;
			if (!PackTypeToUOMType.TryGetValue(quantityUQ, out packType))
			{
				var refPackType = factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, quantityUQ);
				packType = refPackType?.F3_UOMType ?? ZString.Empty;
				PackTypeToUOMType.Add(quantityUQ, packType);
			}

			return packType;
		}

		Dictionary<ZString, ZString> PackTypeToUOMType
		{
			get { return packTypeToUOMType ?? (packTypeToUOMType = new Dictionary<ZString, ZString>()); }
		}
		Dictionary<ZString, ZString> packTypeToUOMType;

		#endregion

		#region GetCachedOverridenCartonGroup

		WhsCartonGroup GetCachedOverridenCartonGroup(OrgHeader client, WhsProduct product)
		{
			Dictionary<WhsProduct, WhsCartonGroup> cacheByClient;
			if (!OverridenCartonGroupCache.TryGetValue(client, out cacheByClient))
			{
				cacheByClient = new Dictionary<WhsProduct, WhsCartonGroup>();
				OverridenCartonGroupCache.Add(client, cacheByClient);
			}

			WhsCartonGroup cachedCartonGroup;
			if (!cacheByClient.TryGetValue(product, out cachedCartonGroup))
			{
				cachedCartonGroup = product.GetOverridenCartonGroup(client);
				cacheByClient.Add(product, cachedCartonGroup);
			}

			return cachedCartonGroup;
		}

		Dictionary<OrgHeader, Dictionary<WhsProduct, WhsCartonGroup>> OverridenCartonGroupCache
		{
			get { return overridenCartonGroupCache ?? (overridenCartonGroupCache = new Dictionary<OrgHeader, Dictionary<WhsProduct, WhsCartonGroup>>()); }
		}
		Dictionary<OrgHeader, Dictionary<WhsProduct, WhsCartonGroup>> overridenCartonGroupCache;

		#endregion

		#region FactorySaveSafe

		static void FactorySaveSafe(BusinessObjectFactory factory)
		{
			try
			{
				factory.Save();
			}
			catch (ZSaveException exception)
			{
				ZExceptionReporting.HandleSaveException(exception);
			}
			catch (ZCannotSaveException exception)
			{
				ZExceptionReporting.HandleSaveException(exception);
			}
		}

		#endregion

		#endregion
	}
}
