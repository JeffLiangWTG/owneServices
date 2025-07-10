using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class PickFaceReplenishmentProcessingManager
	{
		public PickFaceReplenishmentProcessingManager(ILogger logger)
		{
			Logger = logger;
		}

		readonly ILogger Logger;

		#region CreateTransfersForPickfaceReplenishment

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public void CreateTransfersForPickfaceReplenishment()
		{
			try
			{
				var currentUtcTimeToUpdateRegistry = ZDateTime.UtcNow;
				var pickFaceInfos = LoadPickFaceInfos().ToArray();
				CreateTransfers(pickFaceInfos);
				UpdateLastPickFaceReplenishmentRun(currentUtcTimeToUpdateRegistry);
			}
			catch (SqlException exception)
			{
				Logger.Error(exception.Message);
				Logger.Information(AutoCreatingFailedMessage);
			}
		}

		static void UpdateLastPickFaceReplenishmentRun(ZDateTime updatedDateTime)
		{
			WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, updatedDateTime.ToDateTime());
		}

		static string AutoCreatingFailedMessage => Res.GetString("b43fb648-ed48-4b24-8c57-b40fda29bfb1", "Auto-creation of Replenishment Transfers failed.");

		#region LoadPickFaceInfos

		const int Query30MinuteTimeOut = 1800;

		// This is used to test new function(TestGetPickFaceInfos).
#if DEBUG
		public
#endif
		IEnumerable<PickFaceInfo> LoadPickFaceInfos()
		{
			var pickFacesToReplenish = GetFixedPickFacesWithChanges().ToArray();
			var picksWaitingForReplenishmentOnDynamicLocations = GetWaitingOnReplenishmentPicksOnDynamicLocations().ToArray();

			return pickFacesToReplenish.Length > 0 || picksWaitingForReplenishmentOnDynamicLocations.Length > 0 ? GetPickFaceInfos(pickFacesToReplenish, picksWaitingForReplenishmentOnDynamicLocations)
				: Enumerable.Empty<PickFaceInfo>();
		}

		IEnumerable<Guid> GetFixedPickFacesWithChanges()
		{
			var sql = $@"
SELECT
	WF_PK
FROM
	dbo.WhsPickFace
WHERE
	WF_SystemLastEditTimeUtc >= @LastPickFaceReplenishmentRun
	
UNION ALL

SELECT
	WF_PK
FROM
	dbo.WhsPickFace
	JOIN dbo.OrgSupplierPart ON WF_OP = OP_PK AND OP_IsActive = 1
WHERE
	OP_SystemLastEditTimeUtc >= @LastPickFaceReplenishmentRun
	
UNION ALL

SELECT
	WF_PK
FROM
	dbo.WhsPickFace
	JOIN dbo.WhsLocation ON WL_PK = WF_WL
WHERE
	WL_LocationStatus = '{LocationStatus.Codes.Normal}' AND
	WL_LastAllocatedOrChangedDateUtc >= @LastPickFaceReplenishmentRun
	
UNION ALL

SELECT
	WF_PK
FROM
	dbo.WhsPickFace
	JOIN dbo.WhsLocation ON WL_PK = WF_WL
	JOIN dbo.WhsRow ON WL_WR = WR_PK
WHERE
	EXISTS 
	(
		SELECT
			null
		FROM
			dbo.WhsDocket
			JOIN dbo.WhsDocketLine ON WE_WD = WD_PK AND	WE_OP = WF_OP
		WHERE
			WD_WW_Whs = WR_WW_Whs AND
			WF_OH_Client = WD_OH_Client AND
			WE_StockOnHand > 0 AND
			WE_CurrentInventoryStatus = '{InventoryStatus.Codes.Available}' AND
			WE_SystemLastEditTimeUtc >= @LastPickFaceReplenishmentRun
	)";

			var lastReplenishmentRunUtc = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			var sqlParams = new ZSqlParameterCollection
			{
				{ "@LastPickFaceReplenishmentRun", lastReplenishmentRunUtc, WhsDocketSchema.WD_SystemLastEditTimeUtc }
			};

			var dynamicBizOCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			dynamicBizOCollection.Load(sql, sqlParams);

			return dynamicBizOCollection.Select(d => ((ZGuid)d[WhsPickFaceSchema.Constants.PK]).ToGuid());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<Guid> GetWaitingOnReplenishmentPicksOnDynamicLocations()
		{
			var sql = $@"
SELECT
	W3_OP,
	W3_OH,
	W3_WW,
	W3_WA_DynamicPickFaceArea 
INTO
	#AllDynamicProducts
FROM
	dbo.WhsProductParamsByWhsAndClient
WHERE
	W3_WA_DynamicPickFaceArea IS NOT NULL

SELECT
	WP_PK,
	WP_WA_DynamicPickAreaOverride,
	WP_SystemLastEditTimeUtc
INTO
	#PicksAwaitingReplenishment
FROM
	dbo.WhsPick
WHERE
	WP_IsAwaitingReplenishment = 1

SELECT
	WP_PK,
	WP_WA_DynamicPickAreaOverride,
	WP_SystemLastEditTimeUtc,
	WD_PK,
	WD_SystemLastEditTimeUtc,
	WD_OH_Client,
	WD_WW_Whs
INTO #Orders
FROM
	#PicksAwaitingReplenishment
	JOIN dbo.WhsDocket ON WD_WP = WP_PK
WHERE
	WP_WA_DynamicPickAreaOverride IS NOT NULL

INSERT INTO #Orders
SELECT DISTINCT
	WP_PK,
	WP_WA_DynamicPickAreaOverride,
	WP_SystemLastEditTimeUtc,
	WD_PK,
	WD_SystemLastEditTimeUtc,
	WD_OH_Client,
	WD_WW_Whs
FROM
	#PicksAwaitingReplenishment
	JOIN dbo.WhsDocket ON WD_WP = WP_PK
	JOIN #AllDynamicProducts ON W3_OH = WD_OH_Client AND W3_WW = WD_WW_Whs
	JOIN dbo.WhsDocketLine ON WE_WD = WD_PK AND WE_OP = W3_OP
WHERE
	WP_WA_DynamicPickAreaOverride IS NULL AND WD_DocketStatus = 'ATP' AND WD_DocketType = 'ORD' AND WE_DocketLineStatus = ''

SELECT
	DISTINCT WP_PK
FROM
	#Orders
	JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
	LEFT JOIN #AllDynamicProducts ON W3_OH = WD_OH_Client AND W3_WW = WD_WW_Whs AND WE_OP = W3_OP
WHERE
    WE_DocketLineStatus = ''
	AND (
	WP_SystemLastEditTimeUtc >= @LastPickFaceReplenishmentRun
	OR
	WD_SystemLastEditTimeUtc >= @LastPickFaceReplenishmentRun
	OR
	EXISTS
	(
		SELECT NULL
		FROM
			dbo.OrgSupplierPart
		WHERE
			OP_PK = WE_OP AND
			OP_IsActive = 1 AND
			OP_SystemLastEditTimeUtc IS NOT NULL AND
			OP_SystemLastEditTimeUtc >= @LastPickFaceReplenishmentRun
	)
	OR
	EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsDocketLine InventoryLine
			JOIN dbo.WhsDocket InventoryDocket ON WE_WD = InventoryDocket.WD_PK
			JOIN dbo.WhsLocation ON WE_WL = WL_PK
		WHERE
			WE_OP = W3_OP AND
			InventoryDocket.WD_OH_Client = W3_OH AND
			InventoryDocket.WD_WW_Whs = W3_WW AND
			WE_StockOnHand > 0 AND
			WE_CurrentInventoryStatus = 'AVL' AND
			WL_LocationStatus = 'NOR' AND
			WL_LastAllocatedOrChangedDateUtc IS NOT NULL AND
			WL_LastAllocatedOrChangedDateUtc >= @LastPickFaceReplenishmentRun

		UNION ALL

		SELECT
			NULL
		FROM
			dbo.WhsDocketLine
			JOIN dbo.WhsDocket InventoryDocket ON WE_WD = InventoryDocket.WD_PK
			JOIN dbo.WhsLocation ON WE_WL = WL_PK
		WHERE
			WE_OP = W3_OP AND
			InventoryDocket.WD_OH_Client = W3_OH AND
			InventoryDocket.WD_WW_Whs = W3_WW AND
			WE_StockOnHand > 0 AND
			WE_CurrentInventoryStatus = 'AVL' AND
			WL_LocationStatus = 'NOR' AND
			WE_SystemLastEditTimeUtc IS NOT NULL AND
			WE_SystemLastEditTimeUtc >= @LastPickFaceReplenishmentRun
	))
";

			var lastReplenishmentRunUtc = WarehouseDataRegistry.Instance.PickFaceReplenishmentLastRunUTC.Value;
			var sqlParams = new ZSqlParameterCollection
			{
				{ "@LastPickFaceReplenishmentRun", lastReplenishmentRunUtc, WhsDocketSchema.WD_SystemLastEditTimeUtc }
			};

			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(sql, sqlParams, Query30MinuteTimeOut);

			return collection.Select(pk => ((ZGuid)pk[WhsPickSchema.Constants.PK]).ToGuid());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<PickFaceInfo> GetPickFaceInfos(IEnumerable<Guid> pickFaces, IEnumerable<Guid> dynamicProductPicks)
		{
			var hasFixedPickFaces = pickFaces.Any();
			var hasDynamicProductPicks = dynamicProductPicks.Any();
			var sql = GetPickFacesNeedingReplenishmentSQL(hasFixedPickFaces, hasDynamicProductPicks);

			using (var command = Db.Connection.Command(sql, Query30MinuteTimeOut))     // Accessing data using a SQL View
			{
				if (hasFixedPickFaces)
				{
					command.AddTableValuedParameter("@PickFacePKs", "dbo.TVP_uniqueidentifier", pickFaces.Distinct());
				}

				if (hasDynamicProductPicks)
				{
					command.AddTableValuedParameter("@PickPKs", "dbo.TVP_uniqueidentifier", dynamicProductPicks.Distinct());
				}

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return PopulatePickFaceInfo(reader);
					}
				}
			}
		}

		ZString GetPickFacesNeedingReplenishmentSQL(bool hasFixedPickFaces, bool hasDynamicProductPicks)
		{
			#region pickFacesNeedingReplenishmentSql

			#region SuppressResourceStringsCheckRegion

			var dynamicPickFacesNeedingReplenishmentSql = $@"
	-- Find all orders for dynamic products that are waiting replenishment
	WaitingOnReplenishmentPicksWithShortfallByClientAndProductAndPickGroupedByAttributed as (
		SELECT
			WD_WW_Whs as WarehousePK,
			WD_OH_Client as ClientPK,
			WE_OP as ProductPK,
			SUM(WE_TransactionQuantity - ISNULL(PickLineQty, 0)) as Shortfall,
			WE_PartAttrib1 as OrderedAttribute1,
			WE_PartAttrib2 as OrderedAttribute2,
			WE_PartAttrib3 as OrderedAttribute3,
			WE_SerialNumber as OrderedSerialNumber,
			WE_ExpiryDate as OrderedExpiryDate,
			WE_PackingDate as OrderedPackingDate,
			WP_PK as PickToReplenishPK,
			WP_WA_DynamicPickAreaOverride as OrderedDynamicAreaOverride
		FROM
			dbo.WhsPick
			JOIN dbo.WhsDocket ON WD_WP = WP_PK
			JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
			CROSS APPLY
			(
				SELECT
					SUM(WZ_Units) as PickLineQty
				FROM
					dbo.WhsPickLine
				WHERE
					WZ_WE_TransactionLine = WE_PK
			) as OrderLinePickLines
		WHERE
			WP_PK IN (SELECT * FROM @PickPKs) AND
			WP_IsAwaitingReplenishment = 1 AND
			ISNULL(PickLineQty, 0) < WE_TransactionQuantity
		GROUP BY
			WD_WW_Whs,
			WD_OH_Client,
			WE_OP,
			WP_PK,
			WP_WA_DynamicPickAreaOverride,
			-- Group by Ordered Attributes
			WE_PartAttrib1,
			WE_PartAttrib2,
			WE_PartAttrib3,
			WE_SerialNumber,
			WE_ExpiryDate,
			WE_PackingDate
	),
	DynamicProductAreasNeedingReplenishment as (
		SELECT
			COALESCE(OrderedDynamicAreaOverride, W3_WA_DynamicPickFaceArea) as AreaForReplenishmentPK,
			PickToReplenishPK,
			ProductPK,
			ClientPK,
			WarehousePK,
			Shortfall,
			OrderedAttribute1,
			OrderedAttribute2,
			OrderedAttribute3,
			OrderedSerialNumber,
			OrderedExpiryDate,
			OrderedPackingDate
		FROM
			WaitingOnReplenishmentPicksWithShortfallByClientAndProductAndPickGroupedByAttributed
			LEFT JOIN dbo.WhsProductParamsByWhsAndClient ON
				W3_OP = ProductPK AND
				W3_OH = ClientPK AND
				W3_WW = WarehousePK
		WHERE
			(
				W3_WA_DynamicPickFaceArea IS NOT NULL OR
				OrderedDynamicAreaOverride IS NOT NULL
			) AND
			Shortfall > 0
	),
	
	-- Determine what inventory is already available in the dynamic area
	DistinctProductsNeedingReplenishment as (
		SELECT DISTINCT
			AreaForReplenishmentPK,
			ProductPK,
			ClientPK
		FROM
			DynamicProductAreasNeedingReplenishment
	),

	InventoryInDynamicAreas as (
		SELECT
			AreaForReplenishmentPK,
			ProductPK,
			ClientPK,
			WE_PartAttrib1 as PartAttribute1,
			WE_PartAttrib2 as PartAttribute2,
			WE_PartAttrib3 as PartAttribute3,
			WE_SerialNumber as SerialNumber,
			WE_ExpiryDate as PartExpiryDate,
			WE_PackingDate as PartPackingDate,
			WE_StockOnHand - ISNULL(CommittedUnits, 0) as AvailableUnits
		FROM
			DistinctProductsNeedingReplenishment
			JOIN dbo.WhsLocation ON AreaForReplenishmentPK = WL_WA_PickingArea
			JOIN dbo.WhsLocationType ON WL_WLT_LocationType = WLT_PK
			JOIN dbo.WhsDocketLine ON WE_WL = WL_PK AND WE_OP = ProductPK
			JOIN dbo.WhsDocket ON WE_WD = WD_PK AND WD_OH_Client = ClientPK
			LEFT JOIN PickLines ON WZ_WE_InventoryLine = WE_PK
		WHERE
			WE_StockOnHand > 0 AND
			WE_StockOnHand - ISNULL(CommittedUnits, 0) > 0 AND
			WLT_LocationClass = '{LocationClasses.Codes.DPF}' AND
			WL_LocationStatus = '{LocationStatus.Codes.Normal}' AND
			WE_CurrentInventoryStatus = '{InventoryStatus.Codes.Available}'
	),
	-- Determine what inventory is already incoming through other transfers
	TransferringInventoryToDynamicAreas as (
		SELECT
			AreaForReplenishmentPK,
			ProductPK,
			ClientPK,
			WE_PartAttrib1 as PartAttribute1,
			WE_PartAttrib2 as PartAttribute2,
			WE_PartAttrib3 as PartAttribute3,
			WE_SerialNumber as SerialNumber,
			WE_ExpiryDate as PartExpiryDate,
			WE_PackingDate as PartPackingDate,
			WE_TransactionQuantity as AvailableUnits
		FROM
			DistinctProductsNeedingReplenishment
			JOIN dbo.WhsLocation ON AreaForReplenishmentPK = WL_WA_PickingArea
			JOIN dbo.WhsLocationType ON WL_WLT_LocationType = WLT_PK AND WLT_LocationClass = '{LocationClasses.Codes.DPF}'
			JOIN dbo.WhsDocketLine ON
				WE_WL = WL_PK AND
				WE_OP = ProductPK AND
				WE_DocketLineStatus <> '{DocketLineStatus.Codes.Finalised}' AND
				WE_DocketLineType = '{DocketType.Codes.Transfer}'
			JOIN dbo.WhsDocket ON WE_WD = WD_PK AND WD_OH_Client = ClientPK
	),
	-- Determine shortfall considering incoming transfers and stock already available
	IncomingAndAvailableDynamicInventory as (
		SELECT
			AreaForReplenishmentPK,
			ProductPK,
			ClientPK,
			PartAttribute1,
			PartAttribute2,
			PartAttribute3,
			SerialNumber,
			PartExpiryDate,
			PartPackingDate,
			SUM(AvailableUnits) as ShortfallReductionAmount
		FROM
		(
			SELECT * FROM InventoryInDynamicAreas
			UNION ALL
			SELECT * FROM TransferringInventoryToDynamicAreas
		) as IncomingAndAvailableInventory
		GROUP BY
			AreaForReplenishmentPK,
			ProductPK,
			ClientPK,
			PartAttribute1,
			PartAttribute2,
			PartAttribute3,
			SerialNumber,
			PartExpiryDate,
			PartPackingDate
	),
	DynamicReplenishmentsWithMatchingInventory as (
		SELECT
			DynamicProductAreasNeedingReplenishment.AreaForReplenishmentPK,
			DynamicProductAreasNeedingReplenishment.ClientPK,
			DynamicProductAreasNeedingReplenishment.ProductPK,
			DynamicProductAreasNeedingReplenishment.PickToReplenishPK,
			WarehousePK,
			OrderedAttribute1,
			OrderedAttribute2,
			OrderedAttribute3,
			OrderedSerialNumber,
			OrderedExpiryDate,
			OrderedPackingDate,
			AmountToTake,
			Shortfall,
			ShortfallReduction,
			ShortfallReductionRunningTotal = SUM(AmountToTake) OVER
			(
				PARTITION BY
					-- Inventory in IncomingAndAvailableDynamicInventory is already grouped by the
					-- following fields, so this is to make sure we don't grab the same inventory twice.
					DynamicProductAreasNeedingReplenishment.ClientPK,
					DynamicProductAreasNeedingReplenishment.ProductPK,
					WarehousePK,
					PartAttribute1,
					PartAttribute2,
					PartAttribute3,
					SerialNumber,
					PartExpiryDate,
					PartPackingDate
				ORDER BY
					-- Order by Inventory that matches the most Ordered Attributes First then by the Largest Amount
					CASE WHEN PartAttribute1 = OrderedAttribute1 THEN 1 ELSE 0 END +
					CASE WHEN PartAttribute2 = OrderedAttribute2 THEN 1 ELSE 0 END +
					CASE WHEN PartAttribute3 = OrderedAttribute3 THEN 1 ELSE 0 END +
					CASE WHEN SerialNumber = OrderedSerialNumber THEN 1 ELSE 0 END +
					CASE WHEN PartExpiryDate = OrderedExpiryDate THEN 1 ELSE 0 END +
					CASE WHEN PartPackingDate = OrderedPackingDate THEN 1 ELSE 0 END DESC,
					DynamicProductAreasNeedingReplenishment.PickToReplenishPK,
					ShortfallReduction DESC
			)
		FROM
			DynamicProductAreasNeedingReplenishment
			LEFT JOIN IncomingAndAvailableDynamicInventory ON
				DynamicProductAreasNeedingReplenishment.ProductPK = IncomingAndAvailableDynamicInventory.ProductPK AND
				DynamicProductAreasNeedingReplenishment.ClientPK = IncomingAndAvailableDynamicInventory.ClientPK AND
				DynamicProductAreasNeedingReplenishment.AreaForReplenishmentPK = IncomingAndAvailableDynamicInventory.AreaForReplenishmentPK AND
				(OrderedAttribute1 = '' OR PartAttribute1 = OrderedAttribute1) AND
				(OrderedAttribute2 = '' OR PartAttribute2 = OrderedAttribute2) AND
				(OrderedAttribute3 = '' OR PartAttribute3 = OrderedAttribute3) AND
				(OrderedSerialNumber = '' OR SerialNumber = OrderedSerialNumber) AND
				(OrderedExpiryDate IS NULL OR PartExpiryDate = OrderedExpiryDate) AND
				(OrderedPackingDate IS NULL OR PartPackingDate = OrderedPackingDate)
			CROSS APPLY
			(
				SELECT ISNULL(ShortfallReductionAmount, 0) as ShortfallReduction
			) as ShortfallReduction
			CROSS APPLY
			(
				SELECT CASE WHEN ShortfallReduction < Shortfall THEN ShortfallReduction ELSE Shortfall END as AmountToTake
			) as AmountToTake
	),
	GroupedIncomingAndAvailableInventoryForDynamicReplenishments as (
		SELECT
			AreaForReplenishmentPK,
			ClientPK,
			ProductPK,
			WarehousePK,
			PickToReplenishPK,
			OrderedAttribute1,
			OrderedAttribute2,
			OrderedAttribute3,
			OrderedSerialNumber,
			OrderedExpiryDate,
			OrderedPackingDate,
			Shortfall -
				SUM
				(
					-- If we require less than is already being replenished, we reduce the shortfall by the full amount
					CASE WHEN ShortfallReductionRunningTotal <= ShortfallReduction
						THEN AmountToTake
						ELSE
							-- If we can't reduce the shortfall by the full amount but there we can
							-- reduce it by less than the full amount, we reduce the shortfall by the difference
							CASE WHEN ShortfallReductionRunningTotal - AmountToTake < ShortfallReduction
								THEN ShortfallReduction - (ShortfallReductionRunningTotal - AmountToTake)
								ELSE 0
							END
					END
				) as Shortfall
		FROM
			DynamicReplenishmentsWithMatchingInventory
		GROUP BY
			AreaForReplenishmentPK,
			ClientPK,
			ProductPK,
			WarehousePK,
			PickToReplenishPK,
			OrderedAttribute1,
			OrderedAttribute2,
			OrderedAttribute3,
			OrderedSerialNumber,
			OrderedExpiryDate,
			OrderedPackingDate,
			Shortfall
	),
	
	-- Determine where to transfer dynamic products to
	DynamicLocationsWithAmounts as (
		SELECT
			WL_PK AS DynamicLocationPK,
			WL_WA_PickingArea AS AreaPK,
			ISNULL(Amount, 0) AS Amount
		FROM
			dbo.WhsLocation
			JOIN dbo.WhsLocationType ON WLT_PK = WL_WLT_LocationType
			CROSS APPLY
			(
				SELECT SUM(CurrentPickFaceQty) as Amount
				FROM InventoryForPickFace
				WHERE WL_PK = LocationPK
			) as Inventory
		WHERE
			WLT_LocationClass = '{LocationClasses.Codes.DPF}'
	),

	MostEmptyDynamicLocationsPerArea as (
		SELECT
			AreaForReplenishmentPK as AreaPK,
			DynamicLocationPK
		FROM
		(
			SELECT DISTINCT AreaForReplenishmentPK
			FROM DynamicProductAreasNeedingReplenishment
		) as Areas
		CROSS APPLY
		(
			SELECT TOP 1
				DynamicLocationPK
			FROM
				DynamicLocationsWithAmounts
			WHERE
				AreaForReplenishmentPK = AreaPK
			ORDER BY
				Amount ASC,
				DynamicLocationPK
		) AS OrderedLocationsPerArea
	),
	
	-- Final results for Dynamic Replenishments
	DynamicLocationPickFaces as (
		SELECT
			WarehousePK,
			PickToReplenishPK,
			ClientPK,
			ProductPK,
			DynamicLocationPK as TransferToLocationPK,
			Shortfall as ReplenishQuantity,
			1.0 as ReplenishMultiple,
			0 as IsDeadLocked,
			1 as IsDynamicTransfer,
			OrderedExpiryDate,
			OrderedPackingDate,
			OrderedAttribute1,
			OrderedAttribute2,
			OrderedAttribute3,
			OrderedSerialNumber
		FROM
			GroupedIncomingAndAvailableInventoryForDynamicReplenishments
			LEFT JOIN MostEmptyDynamicLocationsPerArea ON AreaForReplenishmentPK = AreaPK
		WHERE
			Shortfall > 0
	)";

			var fixedPickFacesNeedingReplenishmentSql = $@"
	-- Fixed Pick Face Replenishments
	WaitingOnReplenishmentPicksWithShortfallByClientAndProduct as (
		SELECT
			WP_PK,
			WD_OH_Client as ClientPK,
			WE_OP as ProductPK
		FROM
			dbo.WhsPick
			JOIN dbo.WhsDocket ON WD_WP = WP_PK
			JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
			CROSS APPLY
			(
				SELECT
					SUM(WZ_Units) as PickLineQty
				FROM
					dbo.WhsPickLine as OrderLinePickLines
				WHERE
					OrderLinePickLines.WZ_WE_TransactionLine = WE_PK
			) as OrderLinePickLines
		WHERE
			WP_IsAwaitingReplenishment = 1
		GROUP BY
			WP_PK, WD_OH_Client, WE_OP
		HAVING
			SUM(PickLineQty) < SUM(WE_TransactionQuantity)
	),
	DeadLockedPickFaces as (
		-- Find pickfaces which are deadlocked and must be replenished to relieve the deadlock
		SELECT
			WF_PK AS DeadLockedPickFacePK,
			PickToReplenishPK
		FROM
			dbo.WhsPickFace
			JOIN
			(
				SELECT
					InventoryLine.WE_WL,
					InventoryLine.WE_OP,
					WD_OH_Client,
					WP_PK AS PickToReplenishPK,
					SUM(WZ_Units) as CommittedUnits
				FROM
					dbo.WhsPickLine
					JOIN dbo.WhsDocketLine as TransactionLine ON TransactionLine.WE_PK = WZ_WE_TransactionLine
					JOIN dbo.WhsDocketLine as InventoryLine ON InventoryLine.WE_PK = WZ_WE_InventoryLine
					JOIN dbo.WhsDocket ON WD_PK = TransactionLine.WE_WD AND WD_DocketStatus <> 'CAN'
					JOIN WaitingOnReplenishmentPicksWithShortfallByClientAndProduct ON WP_PK = WD_WP AND WD_OH_Client = ClientPK AND InventoryLine.WE_OP = ProductPK
				WHERE
					WZ_PickedDateTime IS NULL -- If a PickLine is not Picked it is Committed
				GROUP BY
					InventoryLine.WE_WL, InventoryLine.WE_OP, WD_OH_Client, WP_PK
			) as LocationsWithStockCommitted ON WF_WL = WE_WL AND WF_OH_Client = WD_OH_Client AND WF_OP = WE_OP

			OUTER APPLY
			(
				-- Check if there are any ready picks with stock committed on the pick face
				SELECT TOP 1 
					WP_PK as PickablePickPK
				FROM
					dbo.WhsPick
					JOIN dbo.WhsDocket ON WD_WP = WP_PK
					JOIN dbo.WhsDocketLine as TransactionLine ON WE_WD = WD_PK
					JOIN dbo.WhsPickLine ON WZ_WE_TransactionLine = TransactionLine.WE_PK
					JOIN dbo.WhsDocketLine as InventoryLine ON InventoryLine.WE_PK = WZ_WE_InventoryLine
				WHERE
					WP_IsAwaitingReplenishment = 0 AND -- Not waiting on replenishment
					WZ_PickedDateTime IS NULL AND -- Can be picked
					InventoryLine.WE_WL = WF_WL AND
					WD_OH_Client = WF_OH_Client AND
					InventoryLine.WE_OP = WF_OP
			) as PickablePicks

			OUTER APPLY
			(
				-- Check if there is any inventory available to commit on the pick face
				SELECT TOP 1
					WE_PK as PickableInventoryPK
				FROM
					dbo.WhsDocketLine
					JOIN dbo.WhsDocket ON WE_WD = WD_PK
					LEFT JOIN PickLines ON WZ_WE_InventoryLine = WE_PK
				WHERE
					WE_CurrentInventoryStatus = '{InventoryStatus.Codes.Available}' AND
					(WE_StockOnHand - ISNULL(CommittedUnits, 0)) > 0 AND
					WE_WL = WF_WL AND
					WD_OH_Client = WF_OH_Client AND
					WE_OP = WF_OP
			) as PickableInventory
		WHERE
			WF_PK IN (SELECT * FROM @PickFacePKs) AND
			WF_ReplenishMinimum < CommittedUnits AND
			PickablePickPK IS NULL AND
			PickableInventoryPK IS NULL
	),
	PickFacesNotDead as (
		SELECT
			WR_WW_WHS AS WarehousePK,
			NULL AS PickToReplenishPK,
			WF_OH_Client AS ClientPK,
			WF_OP AS ProductPK,
			WL_PK AS TransferToLocationPK,
			WF_ReplenishMaximum - ISNULL(CurrentPickFaceQty, 0) as ReplenishQuantity,
			WF_ReplenishmentMultiple AS ReplenishMultiple
		FROM
			dbo.WhsPickFace
			JOIN dbo.WhsLocation ON WL_PK = WF_WL
			JOIN dbo.WhsRow ON WR_PK = WL_WR
			JOIN dbo.OrgSupplierPart ON WF_OP = OP_PK AND OP_IsActive = 1
			LEFT JOIN InventoryForPickFace ON
				LocationPK = WF_WL AND
				InventoryClient = WF_OH_Client AND
				InventoryProduct = WF_OP
		WHERE
			WF_PK IN (SELECT * FROM @PickFacePKs) AND
			WF_ReplenishMinimum >= ISNULL(CurrentPickFaceQty, 0) AND
			WF_ReplenishMaximum >= WF_ReplenishmentMultiple + ISNULL(CurrentPickFaceQty, 0)  -- do not take less than the multiple
	),
	PickFacesDead as (
		SELECT
			WR_WW_WHS AS WarehousePK,
			PickToReplenishPK,
			WF_OH_Client AS ClientPK,
			WF_OP AS ProductPK,
			WL_PK AS TransferToLocationPK,
			WF_ReplenishMaximum - ISNULL(CurrentPickFaceQty, 0) as ReplenishQuantity,
			WF_ReplenishmentMultiple AS ReplenishMultiple
		FROM
			dbo.WhsPickFace
			JOIN DeadLockedPickFaces on WF_PK = DeadLockedPickFacePK
			JOIN dbo.WhsLocation ON WL_PK = WF_WL
			JOIN dbo.WhsRow ON WR_PK = WL_WR
			JOIN dbo.OrgSupplierPart ON WF_OP = OP_PK AND OP_IsActive = 1
			LEFT JOIN InventoryForPickFace ON
				LocationPK = WF_WL AND
				InventoryClient = WF_OH_Client AND
				InventoryProduct = WF_OP
	),
	-- Final results for fixed pick face replenishments
	FixedPickFaces as (
		SELECT
			WarehousePK,
			PickToReplenishPK,
			ClientPK,
			ProductPK,
			TransferToLocationPK,
			ReplenishQuantity,
			ReplenishMultiple,
			MAX(IsDeadLocked) AS IsDeadLocked,
			0 as IsDynamicTransfer,
			NULL as OrderedExpiryDate,
			NULL as OrderedPackingDate,
			'' as OrderedAttribute1,
			'' as OrderedAttribute2,
			'' as OrderedAttribute3,
			'' as OrderedSerialNumber
		FROM
		(
			SELECT *, 0 AS IsDeadLocked FROM PickFacesNotDead
			UNION ALL
			SELECT *, 1 AS IsDeadLocked FROM PickFacesDead -- Deadlocked pickfaces must be replenished irrespective of min/max
		) DeadAndNotDeadPickFaces
		GROUP BY WarehousePK, PickToReplenishPK, ClientPK, ProductPK, TransferToLocationPK, ReplenishQuantity, ReplenishMultiple
	)";

			var pickFacesNeedingReplenishmentSql = $@"
;with
	PickLines as (
		SELECT
			WZ_WE_InventoryLine,
			SUM(WZ_Units) AS CommittedUnits
		FROM
			dbo.WhsCommittedStock
		GROUP BY
			WZ_WE_InventoryLine
	),
	InventoryForPickFace as (
		SELECT
			WE_WL AS LocationPK,
			WD_OH_Client AS InventoryClient,
			WE_OP AS InventoryProduct,
			SUM(WE_StockOnHand) AS CurrentPickFaceQty
		FROM
			dbo.WhsDocketLine
			JOIN dbo.WhsDocket ON WE_WD = WD_PK
			JOIN dbo.WhsLocation ON WE_WL = WL_PK AND WL_LocationStatus = '{LocationStatus.Codes.Normal}'
		WHERE
			WE_CurrentInventoryStatus = '{InventoryStatus.Codes.Available}' AND
			WE_StockOnHand > 0
		GROUP BY
			WE_WL,
			WD_OH_Client,
			WE_OP
	),
	{(hasDynamicProductPicks ? dynamicPickFacesNeedingReplenishmentSql : "")}
	{(hasDynamicProductPicks && hasFixedPickFaces ? "," : "")}
	{(hasFixedPickFaces ? fixedPickFacesNeedingReplenishmentSql : "")}
	SELECT 
		WarehousePK,
		PickToReplenishPK,
		ClientPK,
		ProductPK,
		TransferToLocationPK,
		ReplenishQuantity,
		ReplenishMultiple,
		IsDeadLocked,
		IsDynamicTransfer,
		OrderedExpiryDate,
		OrderedPackingDate,
		OrderedAttribute1,
		OrderedAttribute2,
		OrderedAttribute3,
		OrderedSerialNumber
	FROM
	(
		{(hasFixedPickFaces ? "SELECT * FROM FixedPickFaces" : "")}
		{(hasFixedPickFaces && hasDynamicProductPicks ? "UNION ALL" : "")}
		{(hasDynamicProductPicks ? "SELECT * FROM DynamicLocationPickFaces" : "")}
	) as AllPickFaces";

			#endregion

			#endregion

			return pickFacesNeedingReplenishmentSql;
		}

		PickFaceInfo PopulatePickFaceInfo(IDataReader reader)
		{
			var warehousePK = reader.GetGuid(reader.GetOrdinal(nameof(IPickFaceInfo.WarehousePK)));
			var pickToReplenishOrdinal = reader.GetOrdinal(nameof(IPickFaceInfo.PickToReplenishPK));
			var pickToReplenishPK = reader.IsDBNull(pickToReplenishOrdinal)
				? ZGuid.Empty
				: reader.GetGuid(pickToReplenishOrdinal);

			var clientPK = reader.GetGuid(reader.GetOrdinal(nameof(IPickFaceInfo.ClientPK)));
			var productPK = reader.GetGuid(reader.GetOrdinal(nameof(IPickFaceInfo.ProductPK)));
			var transferToLocationOrdinal = reader.GetOrdinal(nameof(IPickFaceInfo.TransferToLocationPK));
			var transferToLocationPK = reader.IsDBNull(transferToLocationOrdinal)
				? ZGuid.Empty
				: reader.GetGuid(transferToLocationOrdinal);

			var replenishQuantity = reader.GetDecimal(reader.GetOrdinal(nameof(IPickFaceInfo.ReplenishQuantity)));
			var replenishMultiple = reader.GetDecimal(reader.GetOrdinal(nameof(IPickFaceInfo.ReplenishMultiple)));
			var isDeadLocked = reader.GetInt32(reader.GetOrdinal(nameof(IPickFaceInfo.IsDeadLocked))) == 1;
			var isDynamicTransfer = reader.GetInt32(reader.GetOrdinal(nameof(IPickFaceInfo.IsDynamicTransfer))) == 1;
			var expiryDateOrdinal = reader.GetOrdinal(nameof(IPickFaceInfo.OrderedExpiryDate));
			var orderedExpiryDate = reader.IsDBNull(expiryDateOrdinal)
				? ZDate.Empty
				: ((ZDateTime)reader.GetDateTime(expiryDateOrdinal)).Date;

			var packingDateOrdinal = reader.GetOrdinal(nameof(IPickFaceInfo.OrderedPackingDate));
			var orderedPackingDate = reader.IsDBNull(packingDateOrdinal)
				? ZDate.Empty
				: ((ZDateTime)reader.GetDateTime(packingDateOrdinal)).Date;

			var orderedAttribute1 = reader.GetString(reader.GetOrdinal(nameof(IPickFaceInfo.OrderedAttribute1)));
			var orderedAttribute2 = reader.GetString(reader.GetOrdinal(nameof(IPickFaceInfo.OrderedAttribute2)));
			var orderedAttribute3 = reader.GetString(reader.GetOrdinal(nameof(IPickFaceInfo.OrderedAttribute3)));
			var orderedSerialNumber = reader.GetString(reader.GetOrdinal(nameof(IPickFaceInfo.OrderedSerialNumber)));

			return new PickFaceInfo(warehousePK, pickToReplenishPK, clientPK, productPK, transferToLocationPK, replenishQuantity, replenishMultiple, isDeadLocked,
				isDynamicTransfer, orderedExpiryDate, orderedPackingDate, orderedAttribute1, orderedAttribute2, orderedAttribute3, orderedSerialNumber);
		}

		#endregion

		#region CreateTransfers

		void CreateTransfers(IPickFaceInfo[] pickFaceInfos)
		{
			const bool hasValidTransferToLocation = true;

			if (pickFaceInfos.Length > 0)
			{
				var groupedInfos = pickFaceInfos.ToLookup(info => !info.TransferToLocationPK.IsEmpty);
				LogErrorForInvalidPickFaces(Logger, groupedInfos[!hasValidTransferToLocation].ToArray());

				var pickFacesWithValidTransferToLocation = groupedInfos[hasValidTransferToLocation].ToArray();
				if (pickFacesWithValidTransferToLocation.Length > 0)
				{
					ObjectFactory.Get<IPickFaceCreateTransfers>().CreateAndSaveTransfers(pickFacesWithValidTransferToLocation, new WhsDocketILoggerWrapper(Logger));
				}
			}
			else
			{
				Logger.Information(Res.GetString("c87ca80b-71e4-4695-a56b-7bf7676c0ba9", "Did not find any Locations that needed replenishing."));
			}
		}

		static void LogErrorForInvalidPickFaces(ILogger logger, IPickFaceInfo[] invalidPickFaces)
		{
			if (invalidPickFaces.Length > 0)
			{
				var factory = new BusinessObjectFactory();

				foreach (var pickFaceCombination in invalidPickFaces.Select(info => new { info.ClientPK, info.ProductPK, info.WarehousePK }).Distinct())
				{
					var client = factory.Load<OrgHeader>(pickFaceCombination.ClientPK);
					var part = factory.Load<OrgSupplierPart>(pickFaceCombination.ProductPK);
					var warehouse = factory.Load<WhsWarehouse>(pickFaceCombination.WarehousePK);
					logger.Error(Res.GetString("d6049c23-5aba-435a-8b72-90216aba3162", "Product: {0} for Client: {1} from Warehouse {2} needs replenishment, but there are no locations available to transfer to.",
						part.OP_PartNum, client.OH_Code, warehouse.WW_WarehouseNameMultilingual));
				}
			}
		}

		#endregion

		#endregion
	}
}
