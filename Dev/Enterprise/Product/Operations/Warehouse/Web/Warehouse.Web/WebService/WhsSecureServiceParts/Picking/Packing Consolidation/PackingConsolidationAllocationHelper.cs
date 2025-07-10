using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PackingConsolidationAllocationHelper
	{
		#region Constructor

		public PackingConsolidationAllocationHelper(BusinessObjectFactory factory, WhsWarehouse warehouse)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(warehouse, nameof(warehouse));

			ConsolidationLocationLookup = new Dictionary<ZGuid, WhsLocationInfo>();
			OrderExistingConsolidationLocationAllocationLookup = new Dictionary<ZGuid, ZGuid>();
			OrdersWithFinalisedLinesOnConsolidationLocation = new HashSet<ZGuid>();
			ConsolidationLocationAllocations = PrepareAllocationCache(factory, warehouse.PK);
			UnallocatedOrderGroups = new Dictionary<(ZGuid loadPK, ZGuid clientAddressPK, ZGuid carrierOrgPK, ZString carrierServiceLevel), List<WhsOrder>>();
		}
		BusinessObjectFactory Factory { get; }
		Dictionary<ZGuid, WhsLocationInfo> ConsolidationLocationLookup { get; }
		Dictionary<ZGuid, ZGuid> OrderExistingConsolidationLocationAllocationLookup { get; }
		Dictionary<(ZGuid loadPK, ZGuid clientAddressPK, ZGuid carrierOrgPK, ZString carrierServiceLevel), List<AllocationInfo>> ConsolidationLocationAllocations { get; }
		Dictionary<(ZGuid loadPK, ZGuid clientAddressPK, ZGuid carrierOrgPK, ZString carrierServiceLevel), List<WhsOrder>> UnallocatedOrderGroups { get; }
		HashSet<ZGuid> OrdersWithFinalisedLinesOnConsolidationLocation { get; }
		bool NoEmptyConsolidationLocationsRemaining { get; set; }

		#endregion

		#region AllocationConsolidationLocationForOrder

		public ZGuid AllocationConsolidationLocationForOrder(WhsOrder order)
		{
			if (!OrderExistingConsolidationLocationAllocationLookup.TryGetValue(order.PK, out var allocatedLocationPK))
			{
				var distributionCenterAddressPK = order.DistributionCentreAddressPK;
				var consigneeAddressPK = order.ConsigneeAddressPK;
				var transportCoPK = order.TransportCoPK;

				var addressPK = distributionCenterAddressPK != ZGuid.Empty ? distributionCenterAddressPK : consigneeAddressPK;

				var consolidationLocationAllocationTupleKey = (order.WD_WLO_PlannedLoad, addressPK, transportCoPK, order.WD_PL_NKCarrierServiceLevel);

				if (ConsolidationLocationAllocations.TryGetValue(consolidationLocationAllocationTupleKey, out var allocationInfoList))
				{
					allocatedLocationPK = allocationInfoList.FirstOrDefault(al => al.RequiredDate >= order.WD_RequiredDate)?.AllocatedLocationPK ?? ZGuid.Empty;
				}

				if (allocatedLocationPK.IsEmpty && !NoEmptyConsolidationLocationsRemaining)
				{
					var locationQuery = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, order.WD_WW_Whs);
					locationQuery.AddToFilter(WhsLocationViewSchema.WLV_LocationClass, LocationClasses.Codes.CON);
					locationQuery.AddToFilter(WhsLocationViewSchema.PK, SQLComparisonOperator.NotEqual, ConsolidationLocationLookup.Keys);

					var consolidationLocation = Factory.LoadTop1<WhsLocation>(locationQuery);

					if (consolidationLocation != null)
					{
						allocatedLocationPK = consolidationLocation.PK;
						ConsolidationLocationAllocations[consolidationLocationAllocationTupleKey] = new List<AllocationInfo>();

						ConsolidationLocationLookup.Add(
							allocatedLocationPK,
							new WhsLocationInfo(consolidationLocation.PK.ToGuid(), consolidationLocation.WLV_LocationString, consolidationLocation.WLV_LocationString_UserFriendly, consolidationLocation.WLV_LocationClass));
					}
					else
					{
						NoEmptyConsolidationLocationsRemaining = true;
					}
				}

				if (!allocatedLocationPK.IsEmpty)
				{
					ConsolidationLocationAllocations[consolidationLocationAllocationTupleKey].Add(new AllocationInfo(allocatedLocationPK, order.PK, order.WD_WLO_PlannedLoad, addressPK, transportCoPK, order.WD_PL_NKCarrierServiceLevel, order.WD_RequiredDate));
				}
				else
				{
					var key = (order.WD_WLO_PlannedLoad, addressPK, transportCoPK, order.WD_PL_NKCarrierServiceLevel);
					if (!UnallocatedOrderGroups.TryGetValue(key, out var unallocatedOrders))
					{
						UnallocatedOrderGroups[key] = unallocatedOrders = new List<WhsOrder>();
					}
					unallocatedOrders.Add(order);
				}
			}

			return allocatedLocationPK;
		}

		#endregion

		#region RetrieveUnallocatedOrderGroupings

		public IEnumerable<IEnumerable<WhsOrder>> RetrieveUnallocatedOrderGroupings()
			=> UnallocatedOrderGroups.Values;

		#endregion

		#region RetrieveConsolidationLocationInfoForLocationPK

		public WhsLocationInfo RetrieveConsolidationLocationInfoForLocationPK(ZGuid consolidationLocationPK)
			=> ConsolidationLocationLookup[consolidationLocationPK];

		#endregion

		#region HasExistingConsolidationLocationAllocation

		public bool HasExistingFinalisedConsolidationLocationAllocation(ZGuid orderPK) => OrdersWithFinalisedLinesOnConsolidationLocation.Contains(orderPK);

		#endregion

		#region Implementation

		Dictionary<(ZGuid loadPK, ZGuid clientAddressPK, ZGuid carrierOrgPK, ZString carrierServiceLevel), List<AllocationInfo>> PrepareAllocationCache(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			var allocationCache = new Dictionary<(ZGuid loadPK, ZGuid clientAddressPK, ZGuid carrierOrgPK, ZString carrierServiceLevel), List<AllocationInfo>>();

			var processedOrderPKs = new HashSet<ZGuid>();
			var existingAllocationsDynamicObjects = GetConsolidationLocationAllocationInfosWithCurrentPackages(factory, warehousePK, Enumerable.Empty<ZGuid>());
			foreach (var allocation in existingAllocationsDynamicObjects)
			{
				var allocationOrderPK = (ZGuid)allocation[nameof(AllocationInfo.OrderPK)];
				if (!processedOrderPKs.Contains(allocationOrderPK))
				{
					var consolidationLocationPK = (ZGuid)allocation[nameof(WhsLocationInfo.LocationPK)];
					if (!ConsolidationLocationLookup.ContainsKey(consolidationLocationPK))
					{
						var consolidationLocationLocationString = (ZString)allocation[nameof(WhsLocationInfo.LocationString)];
						var consolidationLocationLocationString_UserFriendly = (ZString)allocation[nameof(WhsLocationInfo.LocationString_UserFriendly)];
						var consolidationLocationInfo = new WhsLocationInfo(consolidationLocationPK.ToGuid(), consolidationLocationLocationString, consolidationLocationLocationString_UserFriendly, LocationClasses.Codes.CON);

						ConsolidationLocationLookup.Add(consolidationLocationPK, consolidationLocationInfo);
					}

					var allocationPlannedLoad = (ZGuid)allocation[nameof(AllocationInfo.PlannedLoad)];
					var allocationClientAddressPK = (ZGuid)allocation[nameof(AllocationInfo.ClientAddressPK)];
					var allocationCarrierOrgPK = (ZGuid)allocation[nameof(AllocationInfo.CarrierOrgPK)];
					var allocationCarrierServiceLevel = (ZString)allocation[nameof(AllocationInfo.CarrierServiceLevel)];
					var allocationRequiredDate = (ZDateTimeOffset)allocation[nameof(AllocationInfo.RequiredDate)];
					var allocatedOrderTupleKey = (allocationPlannedLoad, allocationClientAddressPK, allocationCarrierOrgPK, allocationCarrierServiceLevel);
					if (!allocationCache.TryGetValue(allocatedOrderTupleKey, out var existingAllocation))
					{
						allocationCache[allocatedOrderTupleKey] = existingAllocation = new List<AllocationInfo>();
					}

					var allocationInfo = new AllocationInfo(consolidationLocationPK, allocationOrderPK, allocationPlannedLoad, allocationClientAddressPK, allocationCarrierOrgPK, allocationCarrierServiceLevel, allocationRequiredDate);
					existingAllocation.Add(allocationInfo);

					OrderExistingConsolidationLocationAllocationLookup[allocationOrderPK] = consolidationLocationPK;
					processedOrderPKs.Add(allocationOrderPK);

					var isFinalised = (ZBool)allocation["IsFinalised"];
					if (isFinalised)
					{
						OrdersWithFinalisedLinesOnConsolidationLocation.Add(allocationOrderPK);
					}
				}
			}

			return allocationCache;
		}

		public static IEnumerable<ZGuid> GetConsolidationLocationsWithCurrentPackages(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ZGuid> orderPKs)
		{
			return GetConsolidationLocationAllocationInfosWithCurrentPackages(factory, warehousePK, orderPKs, finalisedInventoryLinesOnly: true)
				.Select(allocation => (ZGuid)allocation[nameof(WhsLocationInfo.LocationPK)]);
		}

		static DynamicBusinessObjectCollection GetConsolidationLocationAllocationInfosWithCurrentPackages(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ZGuid> orderPKs, bool finalisedInventoryLinesOnly = false)
		{
			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@Warehouse", warehousePK, WhsLocationViewSchema.WLV_WW_Whs)
			};

			var orderPKsFilter = string.Empty;
			var orderPKsArray = orderPKs.ToArray();
			if (orderPKsArray.Any())
			{
				orderPKsFilter = @"AND WhsOrder.WD_PK IN (SELECT Value FROM @OrderPKs)";
				sqlParams.Add(ZSqlParameter.New("@OrderPKs", orderPKsArray, WhsDocketSchema.PK, isTableValued: true));
			}

			var inventoryLineStatusFilter = finalisedInventoryLinesOnly
				? $"AND InventoryLine.WE_DocketLineStatus IN ('{DocketLineStatus.Codes.Finalised}')"
				: $"AND InventoryLine.WE_DocketLineStatus IN ('{DocketLineStatus.Codes.HeldForTransfer}', '{DocketLineStatus.Codes.Finalised}')";

			var rawQuery = $@"
SELECT
	WhsLocationView.WLV_PK AS LocationPK,
	WhsLocationView.WLV_LocationString AS LocationString,
	WhsLocationView.WLV_LocationString_UserFriendly AS LocationString_UserFriendly,
	OrderPK,
	PlannedLoad,
	RequiredDate,
	CarrierServiceLevel,
	ClientAddressPK,
	CarrierOrgPK,
	CAST(IIF(InventoryLineStatus = '{DocketLineStatus.Codes.Finalised}', 1, 0) AS BIT) AS IsFinalised
FROM
	dbo.WhsLocationView
	CROSS APPLY
	(
		SELECT Distinct
			WhsOrder.WD_PK as OrderPK,
			WhsOrder.WD_WLO_PlannedLoad as PlannedLoad,
			WhsOrder.WD_PL_NKCarrierServiceLevel as CarrierServiceLevel,
			WhsOrder.WD_RequiredDate as RequiredDate,
			IIF (DistributionCentre.E2_OA_Address IS NOT NULL, DistributionCentre.E2_OA_Address, Consignee.E2_OA_Address) as ClientAddressPK,
			OA_OH as CarrierOrgPK,
			InventoryLine.WE_DocketLineStatus as InventoryLineStatus
		FROM
			dbo.PkgPackage package
			JOIN dbo.PkgPackageItemDivot on KI_KP_Package = package.KP_PK
			JOIN dbo.WhsPickLine on WZ_PK = KI_ParentID
			JOIN dbo.WhsDocketLine InventoryLine on InventoryLine.WE_PK = WZ_WE_InventoryLine
			JOIN dbo.WhsDocketLine OrderLine on OrderLine.WE_PK = WZ_WE_TransactionLine
			JOIN dbo.WhsDocket WhsOrder on OrderLine.WE_WD = WD_PK
			JOIN dbo.JobDocAddress Consignee on Consignee.E2_ParentID = WhsOrder.WD_PK AND Consignee.E2_AddressType = 'CEA' AND Consignee.E2_AddressOverride = 0
			LEFT JOIN dbo.JobDocAddress DistributionCentre on DistributionCentre.E2_ParentID = WhsOrder.WD_PK AND DistributionCentre.E2_AddressType = 'DCA' AND DistributionCentre.E2_AddressOverride = 0
			LEFT JOIN dbo.JobDocAddress TransportCo on TransportCo.E2_ParentID = WhsOrder.WD_PK AND TransportCo.E2_AddressType = 'TRA' AND TransportCo.E2_AddressOverride = 0
			LEFT JOIN dbo.OrgAddress ON OA_PK = TransportCo.E2_OA_Address
		WHERE
			InventoryLine.WE_WL = WLV_PK
			AND WZ_WE_OriginalPickedInventoryLine IS NOT NULL
			AND InventoryLine.WE_StockOnHand > 0
			{inventoryLineStatusFilter}
			{orderPKsFilter}
	) package
WHERE
	WLV_LocationClass = '{LocationClasses.Codes.CON}'
	AND WLV_WW_Whs = @Warehouse
ORDER BY
	RequiredDate DESC, IsFinalised DESC";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(rawQuery, sqlParams);

			return collection;
		}

		class AllocationInfo
		{
			public AllocationInfo(ZGuid allocatedLocationPK, ZGuid orderPK, ZGuid plannedLoadPK, ZGuid clientAddressPK, ZGuid carrierOrgPK, ZString carrierServiceLevel, ZDateTimeOffset requiredDate)
			{
				AllocatedLocationPK = allocatedLocationPK;
				OrderPK = orderPK;
				PlannedLoad = plannedLoadPK;
				ClientAddressPK = clientAddressPK;
				CarrierOrgPK = carrierOrgPK;
				CarrierServiceLevel = carrierServiceLevel;
				RequiredDate = requiredDate;
			}

			public ZGuid AllocatedLocationPK { get; }
			public ZGuid OrderPK { get; }
			public ZGuid PlannedLoad { get; }
			public ZGuid ClientAddressPK { get; }
			public ZGuid CarrierOrgPK { get; }
			public ZString CarrierServiceLevel { get; }
			public ZDateTimeOffset RequiredDate { get; }
		}

		#endregion
	}
}
