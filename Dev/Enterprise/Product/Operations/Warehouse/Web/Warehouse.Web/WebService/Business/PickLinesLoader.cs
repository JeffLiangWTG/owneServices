using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public sealed class PickLinesLoader
	{
		#region Constructors

		public PickLinesLoader(BusinessObjectFactory factory, WhsWarehouse warehouse, GlbStaff picker, SearchFilterCriteriaInfo criteria)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			Warehouse = Argument.NotNull(warehouse, nameof(warehouse));
			Picker = Argument.NotNull(picker, nameof(picker));
			Criteria = Argument.NotNull(criteria, nameof(criteria));
			Criteria.AreaCode = Criteria.AreaCode.Trim().ToUpper(CultureInfo.InvariantCulture);
			Criteria.PickMethod = Criteria.PickMethod.Trim().ToUpper(CultureInfo.InvariantCulture);
		}

		readonly BusinessObjectFactory Factory;
		public readonly WhsWarehouse Warehouse;
		public readonly GlbStaff Picker;
		public readonly SearchFilterCriteriaInfo Criteria;
		static readonly object FindAndAssignNextPickLock = new object();

		#endregion

		#region FindAndAssignNextPick

		public FindPickResult FindAndAssignNextPick(ZString reference)
		{
			lock (FindAndAssignNextPickLock)
			{
				var parameters = new ZSqlParameterCollection();
				var pickQuery = GetPickQueryWithSorting(reference, parameters);
				var results = new DynamicBusinessObjectCollection(Factory);
				results.Load(pickQuery, parameters);

				var findPickResult = FindPickResult.NoPick;
				if (results.Any())
				{
					var isPutawayOnly = results.Any(x => (ZBool)x["IsPutawayOnly"]);
					if (isPutawayOnly)
					{
						var pick = Factory.Load<WhsPick>((ZGuid)results.First()[WhsPickSchema.PK]);
						findPickResult = new FindPickResult(pick);
					}
					else
					{
						var pickLineQuery = new ZQuery(WhsPickLineSchema.PK, results.Select(x => x[WhsPickLineSchema.PK]));
						var pickLinesByPK = Factory.Load<WhsPickLine>(pickLineQuery).ToDictionary(o => o.PK);
						var pickLines = results.Select(o => pickLinesByPK[(ZGuid)o[WhsPickLineSchema.PK]]).ToList(); // We do this to retain the ordering from SQL without resorting

						SplitPickLine(results, pickLines);

						var pick = pickLines[0].Pick;
						pick.UpdateIsLocationEmptyAfterFinalisingPickOnPickLines(pickLines);
						findPickResult = AssignPickLines(pickLines);
					}
				}

				return findPickResult;
			}
		}

		#region GetPickQueryWithSorting

		ZString GetPickQueryWithSorting(string reference, ZSqlParameterCollection parameters)
		{
			var pickQuery = GetRawPickQueryWithSorting(CartonisedAndPickByLabelWhereClause, reference, parameters);
			GetParametersForQuery(reference, Picker.GS_Code, parameters);

			return pickQuery;
		}

		static string CartonisedAndPickByLabelWhereClause
		{
			get
			{
				return @"AND (WP_CartoniseSplitCases = 0 OR COALESCE(F3_UOMType, '') <> 'SPC') 
					AND (WP_PickCasesByLabel = 0 OR COALESCE(F3_UOMType, '') <> 'CAS') 
					AND (WP_PickPalletsByLabel = 0 OR COALESCE(F3_UOMType, '') <> 'PLT')";
			}
		}

		#region GetRawPickQueryWithSorting

		ZString GetRawPickQueryWithSorting(string cartonisePickbyLabelContraints, string reference, ZSqlParameterCollection parameters)
		{
			var buildingStatusRegValue = WarehouseDataRegistry.Instance.EnablePickLineLoaderBuildingStatusFilter.Value;

			#region SuppressResourceStringsCheckRegion

			var pickMethodWhereClause = "";
			if (!string.IsNullOrEmpty(Criteria.PickMethod) && !WhsAreaAndPickMethodHelper.IsAnyCode(Criteria.PickMethod))
			{
				pickMethodWhereClause = $"AND (WL_PickMethod = @PickMethod OR WL_PickMethod = '{WhsAreaAndPickMethodHelper.AnyCode}') ";
			}

			var areaWhereClause = "";
			if (!string.IsNullOrEmpty(Criteria.AreaCode) && !WhsAreaAndPickMethodHelper.IsAnyCode(Criteria.AreaCode))
			{
				areaWhereClause = "AND WA_Name = @AreaCode ";
			}

			var clientCodeWhereClause = "";
			if (!string.IsNullOrEmpty(Criteria.ClientCode))
			{
				clientCodeWhereClause = "AND OH_Code = @ClientCode ";
			}

			var pickGroupWhereClause = "";
			if (Criteria.PickGroup != 0)
			{
				pickGroupWhereClause = "AND OrderLine.WE_PickGroup = @PickGroup ";
			}

			var uomWhereClause = "";
			if (!string.IsNullOrEmpty(Criteria.UOMType))
			{
				uomWhereClause = " AND COALESCE(F3_UOMType, '') in ('', @UOMType)";
			}

			var filterPick = "";
			var withReferenceMatch = "";
			var orderbyReferenceMatch = "";
			var isAssignedToUserAndReadyForPickOrPutawayQuery = " AND WZ_GS_NKAssignedTo IN('', @User) AND WZ_WE_OriginalPickedInventoryLine IS NULL";
			if (!string.IsNullOrEmpty(reference))
			{
				isAssignedToUserAndReadyForPickOrPutawayQuery = @" 
					AND (WZ_WE_OriginalPickedInventoryLine IS NOT NULL OR WZ_GS_NKAssignedTo IN ('', @User))	
					AND ((WZ_WE_OriginalPickedInventoryLine IS NULL OR PutawayLine.WE_GS_NKPutawayBy = @User))";

				filterPick = "AND WP_PK IN (SELECT ReferenceMatchPickPK FROM ReferenceMatch)";

				#region ReferenceMatch

				withReferenceMatch = @"
ReferenceMatch AS
(
	SELECT DISTINCT WD_WP AS ReferenceMatchPickPK, 4 AS ReferenceMatchOrder
	FROM dbo.WhsDocket
	WHERE WD_ExternalReference = @Reference
	AND WD_WW_Whs = @WhsPK

	UNION ALL

	SELECT DISTINCT WD_WP AS ReferenceMatchPickPK, 3 AS ReferenceMatchOrder
	FROM dbo.WhsDocket
	LEFT JOIN dbo.WhsDocketReference ON WX_WD = WD_PK
	WHERE WX_Reference = @Reference
	AND WD_WW_Whs = @WhsPK

	UNION ALL

	SELECT WD_WP AS ReferenceMatchPickPK, 2 AS ReferenceMatchOrder
	FROM dbo.WhsDocket
	WHERE WD_DocketID = @Reference

	UNION ALL

	SELECT WP_PK AS ReferenceMatchPickPK, 1 AS ReferenceMatchOrder
	FROM dbo.WhsPick
	WHERE WP_PickNo = @Reference
),";

				orderbyReferenceMatch = "ISNULL((SELECT MAX(ReferenceMatchOrder) AS ReferenceMatchOrder FROM ReferenceMatch WHERE ReferenceMatchPickPK = WP_PK),0) DESC,";

				#endregion
			}

			var customsAcceptJobOuterApply = "";
			var customsAcceptJobCondition = "";
			if (!Warehouse.WW_IsVirtualWarehouse)
			{
				customsAcceptJobOuterApply = FormattableString.Invariant($@"
		OUTER APPLY
		(
			SELECT TOP 1 SL_SE_NKEvent as CustomsEventType
			FROM dbo.StmALog
			WHERE
				WD_DocketSubType IN ('{OrderType.Codes.Customs}', '{OrderType.Codes.CustomsReleaseWithPermit}')
				AND SL_Parent = WD_PK 
				AND SL_SE_NKEvent IN ('{Events.WarehouseJobCanNowBeFinalisedCode}', '{Events.HoldTheWarehouseOrderCode}') 
				AND SL_IsCancelled = 'N'
			ORDER BY SL_PostedTimeUtc DESC
			) AS CustomsEventType");
				customsAcceptJobCondition = FormattableString.Invariant($"AND ISNULL(CustomsEventType, '{Events.WarehouseJobCanNowBeFinalisedCode}') = '{Events.WarehouseJobCanNowBeFinalisedCode}'");
			}

			#endregion

			return FormattableString.Invariant($@"

DECLARE @PickMethods TABLE
(
	Code NVARCHAR(5) UNIQUE,
	PickMethodDesc NVARCHAR(100)
)

{GetPickMethodSqlAndCreateParameters(parameters)}

DECLARE @WeightCapacity AS DECIMAL(28, 8) = CASE WHEN @EquipmentRego <> '' THEN (SELECT RQ_WeightCapacity FROM dbo.RefEquipment WHERE RQ_Registration = @EquipmentRego) ELSE 0 END
DECLARE @VolumeCapacity AS DECIMAL(28, 8) = CASE WHEN @EquipmentRego <> '' THEN (SELECT RQ_CubicCapacity FROM dbo.RefEquipment WHERE RQ_Registration = @EquipmentRego) ELSE 0 END
DECLARE @PackCapacity AS INT = CASE WHEN @EquipmentRego <> '' THEN (SELECT RQ_PackCapacity FROM dbo.RefEquipment WHERE RQ_Registration = @EquipmentRego) ELSE 0 END

SET @WeightCapacity = CASE WHEN @WeightCapacity = 0 THEN 99999999999999999999 ELSE @WeightCapacity END
SET @VolumeCapacity = CASE WHEN @VolumeCapacity = 0 THEN 99999999999999999999 ELSE @VolumeCapacity END
SET @PackCapacity = CASE WHEN @PackCapacity = 0 THEN 2147483647 ELSE @PackCapacity END

; WITH {withReferenceMatch}
PickLines AS 
(
	SELECT
		WP_PK,
		WZ_PK,
		WD_PK,
		ISNULL(PutawayLine.WE_GS_NKPutawayBy, WZ_GS_NKAssignedTo) as AssignedTo,
		WD_PickPriority,
		WP_PickNo,
		WZ_Units,
		CASE WHEN WZ_WE_OriginalPickedInventoryLine IS NULL THEN 0 ELSE 1 END AS IsPutaway,
		WL_PickMethod AS WLV_PickMethod,
		WR_Name AS WLV_RowName,
		WL_PickPathSequence AS WLV_PickPathSequence,
		WL_Column AS WLV_Column,
		WL_Level AS WLV_Level,
		WL_Tray AS WLV_Tray,
		InventoryLine.WE_PalletID AS PalletID,
		OrderLine.WE_PartAttrib1 AS OrderedPartAttrib1,
		OrderLine.WE_PartAttrib2 AS OrderedPartAttrib2,
		OrderLine.WE_PartAttrib3 AS OrderedPartAttrib3,
		OrderLine.WE_SerialNumber AS OrderedSerialNumber,
		OrderLine.WE_PackingDate AS OrderedPackingDate,
		OrderLine.WE_ExpiryDate AS OrderedExpiryDate,
		WLT_IsPalletIDNeutral AS IsPalletIDNeutral, 
		OH_Code,
		OrderLine.WE_PickGroup,
		WR_PickPathSequence,
		OP_PartNum,
		OP_PK as ProductPK,
		CASE WHEN WZ_F3_NKAllocatedPackType <> '' THEN 0 ELSE OP_CountDecimalPlaces END AS OP_CountDecimalPlaces,
		WZ_F3_NKAllocatedPackType,
		PickMethodDesc,
		RQ_PK as EquipmentPK,
		CASE WHEN InventoryLine.WE_PalletID <> '' THEN SUM(WZ_Units) OVER(PARTITION BY InventoryLine.WE_PalletID, WP_PK) END AS RequestedPalletStockUnits,
		CASE WHEN InventoryLine.WE_PalletID <> ''
			THEN
				-- To emulate a COUNT(DISTINCT) with a Window Function, we add the ASC and DESC Rank of each Product which will equal the number of unique products + 1
				DENSE_RANK() OVER(PARTITION BY InventoryLine.WE_PalletID, WP_PK ORDER BY OP_PK ASC)
				+ DENSE_RANK() OVER(PARTITION BY InventoryLine.WE_PalletID, WP_PK ORDER BY OP_PK DESC) - 1
			ELSE 0
		END AS ProductsPerPallet

	FROM
		dbo.WhsPick
		JOIN dbo.WhsDocket ON WD_WP = WP_PK
		JOIN dbo.WhsDocketLine AS OrderLine ON WE_WD = WD_PK
		JOIN dbo.WhsPickLine ON WZ_WE_TransactionLine = OrderLine.WE_PK
		JOIN dbo.WhsDocketLine AS InventoryLine ON InventoryLine.WE_PK = ISNULL(WZ_WE_OriginalPickedInventoryLine, WZ_WE_InventoryLine)
		LEFT JOIN dbo.WhsDocketLine AS PutawayLine ON PutawayLine.WE_PK = WZ_WE_InventoryLine AND WZ_WE_OriginalPickedInventoryLine IS NOT NULL AND OrderLine.WE_DocketLineType = 'ORD' -- Dont support DDL putaway for work order
		LEFT JOIN dbo.RefEquipment ON RQ_Registration = @EquipmentRego
		JOIN dbo.OrgSupplierPart ON OrderLine.WE_OP = OP_PK
		JOIN dbo.OrgHeader ON OH_PK = WD_OH_Client
		JOIN dbo.WhsLocation ON InventoryLine.WE_WL = WL_PK
		JOIN dbo.WhsLocationType ON WLT_PK = WL_WLT_LocationType
		JOIN dbo.WhsRow ON WL_WR = WR_PK
		JOIN dbo.WhsArea ON WL_WA_PickingArea = WA_PK
		LEFT JOIN dbo.WhsDockDoorAssignment ON WDA_PK = WP_WDA_DockDoorAssignment
		LEFT JOIN @PickMethods ON Code = WL_PickMethod
		LEFT JOIN dbo.RefPackType ON F3_Code = WZ_F3_NKAllocatedPackType 
		LEFT JOIN dbo.PkgPackageItemDivot ON WZ_PK = KI_ParentID
		LEFT JOIN dbo.GenAddOnColumn AS IsPackedIntoToteColumn ON XA_ParentID = KI_KP_Package AND XA_ParentTableCode = 'KP' AND XA_Name = 'TotePackage'
		{customsAcceptJobOuterApply}
	WHERE
		WP_PickStatus NOT IN ('{PickStatus.Codes.Finalised}', '{PickStatus.Codes.Cancelled}'{(buildingStatusRegValue ? $", '{PickStatus.Codes.Building}'" : "")})
		AND WP_IsAwaitingReplenishment = 0
		AND WP_WW_Whs = @WhsPK
		AND WD_WW_Whs = @WhsPK
		AND WR_WW_Whs = @WhsPK
		AND WZ_PickedDateTime IS NULL
		AND WZ_Units > 0
		AND IsPackedIntoToteColumn.XA_PK is NULL
		AND (RQ_Registration IS NULL OR RQ_F3_NKPackType = '' OR RQ_F3_NKPackType = WZ_F3_NKAllocatedPackType)
		AND PutawayLine.WE_FinalisedDate IS NULL
		{filterPick}
		{isAssignedToUserAndReadyForPickOrPutawayQuery}
		{cartonisePickbyLabelContraints}
		{pickMethodWhereClause}
		{areaWhereClause}
		{pickGroupWhereClause}
		{clientCodeWhereClause}
		{uomWhereClause}
		{customsAcceptJobCondition}
)

SELECT
	ROW_NUMBER() OVER
	(
		ORDER BY
		WP_PK ASC, 
		{SortPickLinesForPickingSlip.EquivalentOrderBySql},
		WZ_PK ASC 
	) AS RunningPK,
	DENSE_RANK() OVER
	(
		ORDER BY
		{orderbyReferenceMatch}
		AssignedTo DESC,
		CASE WHEN WD_PickPriority = 0 THEN 256 ELSE WD_PickPriority END ASC,
		CASE WHEN WLV_PickMethod = @PickMethod THEN 1 ELSE 0 END DESC,
		CASE WHEN PickMethodDesc IS NOT NULL THEN 1 ELSE 0 END DESC,
		ISNULL(PickMethodDesc, '') ASC,
		WP_PickNo ASC
	) as PickPriority,
	PalletID,
	IsPutaway,
	WP_PK,
	WZ_PK,
	WD_PK,
	ProductPK,
	EquipmentPK,
	WZ_Units,
	WZ_F3_NKAllocatedPackType,
	RequestedPalletStockUnits,
	ProductsPerPallet
INTO
	#PickLinesWithRanking
FROM
	PickLines

SELECT
	RunningPK,
	PickPriority,
	WP_PK AS PickPK,
	WZ_PK,
	WD_PK,
	ConvertedWeight,
	ConvertedVolume,
	WZ_Units,
	AvailablePalletStockUnits,
	ConvertedWeight * WZ_Units AS TotalWeight,
	ConvertedVolume * WZ_Units AS TotalVolume,
	PalletID AS WE_PalletID,
	UnitsPerUOM,
	(
		SELECT MIN(MaxUomUnits)
		FROM
		(
			-- Get the Max amount of Uoms possible using the Packs Capacity, ignore 0 as that means no limit
			SELECT CAST(RQ_PackCapacity AS BIGINT) * UnitsPerUOM AS MaxUomUnits WHERE ISNULL(RQ_PackCapacity, 0) > 0
			UNION ALL
			-- To Prevent an arithmetic overflow, we will make the maximum units to pick equal to the Max Int Value
			SELECT 2147483647 AS MaxUomUnits
		) AS MaxUomUnits
	) AS MaxUomUnits,
	IsSplittable,
	CAST(0 AS DECIMAL(38, 13)) AS RunningPickWeight,
	CAST(0 AS DECIMAL(38, 13)) AS RunningPickVolume,
	CAST(0 AS INT) AS MaxUnitsWithinPackConstraint,
	CAST(0 AS BIT) AS CanPick
INTO
	#QuirkyRunningTotal
FROM 
	#PickLinesWithRanking
	JOIN dbo.OrgSupplierPart ON OP_PK = ProductPK
	LEFT JOIN dbo.RefEquipment ON RQ_PK = EquipmentPK

	CROSS APPLY ConvertWeight(OP_Weight, OP_WeightUQ, ISNULL(RQ_WeightUnit, 'KG')) AS CW
	CROSS APPLY ConvertVolume(OP_Cubic, OP_CubicUQ, ISNULL(RQ_CubicUnit, 'M3')) AS CV
	OUTER APPLY
	(
		SELECT CAST(ConversionFactor AS INT) AS UnitsPerUOM
		FROM OrgSupplierPartUnitsConverter(OP_PK, WZ_F3_NKAllocatedPackType, OP_StockKeepingUnit)
		WHERE
			WZ_F3_NKAllocatedPackType <> ''
	) AS UnitsPerUOM
	OUTER APPLY
	(
		SELECT TOP 1 OF_Weight AS UomWeight, OF_Cubic AS UomVolume
		FROM dbo.OrgPartUnit
		WHERE
			OF_OP = OP_PK
			AND OF_ParentPackType = WZ_F3_NKAllocatedPackType
			AND WZ_F3_NKAllocatedPackType <> ''
			AND WZ_F3_NKAllocatedPackType <> OP_StockKeepingUnit -- We want to use the weight and volume on the Product Master if the Pack Type is the Stock Keeping Unit
		ORDER BY
			OF_Weight DESC,
			OF_Cubic DESC
	) AS UomUnits
	CROSS APPLY
	(
		SELECT
			SUM(WE_StockOnHand) AS AvailablePalletStockUnits
		FROM
			dbo.WhsDocketLine
			JOIN dbo.WhsDocket ON WD_PK = WE_WD
		WHERE
			WD_WW_WHS = @WhsPk
			AND WE_PalletID <> ''
			AND WE_PalletID = PalletID
			AND WE_StockOnHand > 0
	) AS AvailablePalletStock
	CROSS APPLY
	(
		SELECT
			COALESCE(UomWeight / NULLIF(UnitsPerUOM, 0), CW.Value) AS ConvertedWeight,
			COALESCE(UomVolume / NULLIF(UnitsPerUOM, 0), CV.Value) AS ConvertedVolume
	) AS ConvertedWeightAndVolume
	CROSS APPLY
	(
		SELECT
			RequestedPalletStockUnits * ConvertedWeight AS RequestedPalletWeight,
			RequestedPalletStockUnits * ConvertedVolume AS RequestedPalletVolume
	) AS RequestedPalletStock
	CROSS APPLY
	(
		SELECT CASE WHEN
			(PalletID = '' OR AvailablePalletStockUnits <> RequestedPalletStockUnits OR ProductsPerPallet > 1)
			THEN 1
			ELSE 0
		END AS IsSplittable
	) AS IsSplittable
WHERE
	IsPutaway = 0 -- We only check equipment capacities when Picking, putaway is 'blind' and we will trust what the system says the user is putting away
	AND
	(
		(
			IsSplittable = 0
			AND ConvertedWeight <= @WeightCapacity
			AND ConvertedVolume <= @VolumeCapacity
			AND ISNULL(RequestedPalletWeight, 0) <= @WeightCapacity
			AND ISNULL(RequestedPalletVolume, 0) <= @VolumeCapacity
		)
		OR
		(
			IsSplittable = 1
			AND	ConvertedWeight / POWER(10, OP_CountDecimalPlaces) <= @WeightCapacity
			AND ConvertedVolume / POWER(10, OP_CountDecimalPlaces) <= @VolumeCapacity
			AND ISNULL(UomWeight, 0) <= @WeightCapacity
			AND ISNULL(UomVolume, 0) <= @VolumeCapacity
		)
	)

-- this stops SQL server creating statistics on a table that's just going to get dropped anyway
-- make sure to add null statistics for any column used in a WHERE clause
CREATE STATISTICS _s ON #QuirkyRunningTotal (CanPick) WITH NORECOMPUTE 

DECLARE @Anchor AS INT
DECLARE @CanPick AS TINYINT
DECLARE @RunningWeight AS DECIMAL(38, 13) = 0.0
DECLARE @RunningVolume AS DECIMAL(38, 13) = 0.0
DECLARE @RunningPalletUnits AS DECIMAL(18, 3) = 0.0
DECLARE @RunningUnitsRemainder AS INT = 0
DECLARE @RunningPacks AS INT = 0
DECLARE @MaxUomUnitsBasedOnRunningPacks AS INT = 0
DECLARE @MaxUomUnitsBasedOnWeight AS INT = 0
DECLARE @MaxUomUnitsBasedOnVolume AS INT = 0
DECLARE @MaxUomCapacity AS INT = 0
DECLARE @Partition AS UNIQUEIDENTIFIER = NULL
DECLARE @PalletPartition AS VARCHAR(30) = ''

UPDATE #QuirkyRunningTotal
SET
	@Partition = CASE WHEN @Partition IS NULL THEN PickPK ELSE @Partition END,
	@RunningWeight = CASE WHEN @Partition <> PickPK THEN 0 ELSE @RunningWeight END,
	@RunningVolume = CASE WHEN @Partition <> PickPK THEN 0 ELSE @RunningVolume END,
	@RunningPacks = CASE WHEN @Partition <> PickPK THEN 0 ELSE @RunningPacks END,
	@RunningUnitsRemainder = CASE WHEN @Partition <> PickPK THEN 0 ELSE @RunningUnitsRemainder END,
	@RunningPalletUnits = CASE WHEN WE_PalletID = '' OR @PalletPartition <> WE_PalletID THEN 0 ELSE @RunningPalletUnits END,
	@Partition = PickPK,
	@PalletPartition = WE_PalletID,
	-- Based on the Remaining Packs Capacity how many Units can we Pick
	@MaxUomUnitsBasedOnRunningPacks = IIF((CAST(@RunningPacks AS BIGINT) * ISNULL(UnitsPerUOM, 0)) > MaxUomUnits, 0, MaxUomUnits - (@RunningPacks * ISNULL(UnitsPerUOM, 0))),
	-- Based on the Remaining Weight Capacity how many Units can we Pick
	@MaxUomUnitsBasedOnWeight = IIF(ISNULL(FLOOR((@WeightCapacity - @RunningWeight) / NULLIF(ConvertedWeight * UnitsPerUOM, 0)) * UnitsPerUOM, 2147483647) > 2147483647,
		2147483647, -- If the Weight Capacity is huge or unlimited, we set the Max Units to the Max Int value to prevent overflow
		ISNULL(FLOOR((@WeightCapacity - @RunningWeight) / NULLIF(ConvertedWeight * UnitsPerUOM, 0)) * UnitsPerUOM, 2147483647)),
	-- Based on the Remaining Volume Capacity how many Units can we Pick
	@MaxUomUnitsBasedOnVolume = IIF(ISNULL(FLOOR((@VolumeCapacity - @RunningVolume) / NULLIF(ConvertedVolume * UnitsPerUOM, 0)) * UnitsPerUOM, 2147483647) > 2147483647,
		2147483647, -- If the Volume Capacity is huge or unlimited, we set the Max Units to the Max Int value to prevent overflow
		ISNULL(FLOOR((@VolumeCapacity - @RunningVolume) / NULLIF(ConvertedVolume * UnitsPerUOM, 0)) * UnitsPerUOM, 2147483647)),
	-- Find the MaxUomUnits by finding the smallest Capacity Limit based on UnitsPerUOM
	@MaxUomCapacity = IIF
	(
		@MaxUomUnitsBasedOnRunningPacks < @MaxUomUnitsBasedOnWeight AND @MaxUomUnitsBasedOnRunningPacks < @MaxUomUnitsBasedOnVolume,
		@MaxUomUnitsBasedOnRunningPacks,
		IIF(@MaxUomUnitsBasedOnWeight < @MaxUomUnitsBasedOnVolume, @MaxUomUnitsBasedOnWeight, @MaxUomUnitsBasedOnVolume)
	),
	@CanPick = CASE
		WHEN @MaxUomUnitsBasedOnRunningPacks = 0 THEN 0
		WHEN
		(
			IsSplittable = 0 AND
			@RunningWeight + TotalWeight + ISNULL((AvailablePalletStockUnits - @RunningPalletUnits - WZ_Units) * ConvertedWeight, 0) <= @WeightCapacity AND -- Make sure the whole Pallet can be Picked by Weight
			@RunningVolume + TotalVolume + ISNULL((AvailablePalletStockUnits - @RunningPalletUnits - WZ_Units) * ConvertedVolume, 0) <= @VolumeCapacity AND -- Make sure the whole Pallet can be Picked by Volume
			@RunningPacks < @PackCapacity
		)
		OR
		(IsSplittable = 1 AND @RunningWeight < @WeightCapacity AND @RunningVolume < @VolumeCapacity AND @RunningPacks < @PackCapacity)
		OR
		(TotalWeight = 0 AND TotalVolume = 0 AND @RunningPacks < @PackCapacity)
		THEN 1
		ELSE 0
	END,
	-- We track how many Packs we've processed so far as we could be assigning multiple products which have different UnitsPerUOM
	@RunningPacks = IIF(@CanPick = 1 AND UnitsPerUOM IS NOT NULL,
		@RunningPacks + ISNULL(FLOOR(IIF(@RunningUnitsRemainder + WZ_Units < @MaxUomCapacity, @RunningUnitsRemainder + WZ_Units, @MaxUomCapacity) / NULLIF(UnitsPerUOM, 0)), 0),
		@RunningPacks),
	@RunningWeight = CASE WHEN @CanPick = 1 THEN @RunningWeight + TotalWeight ELSE @RunningWeight END,
	@RunningVolume = CASE WHEN @CanPick = 1 THEN @RunningVolume + TotalVolume ELSE @RunningVolume END,
	@RunningPalletUnits = CASE WHEN @CanPick = 1 THEN @RunningPalletUnits + WZ_Units ELSE @RunningPalletUnits END,
	@RunningUnitsRemainder = ISNULL((@RunningUnitsRemainder + WZ_Units) % NULLIF(UnitsPerUOM, 0), 0), -- need to track how many units less than a UOM we have processed
	@Anchor = RunningPK,
	-- FYI columns are updated last, no matter what order they're written in within the SET, they're at the end to enhance readability
	RunningPickWeight = @RunningWeight,
	RunningPickVolume = @RunningVolume,
	MaxUnitsWithinPackConstraint =
		IIF(WZ_Units < @MaxUomUnitsBasedOnRunningPacks, WZ_Units, @MaxUomUnitsBasedOnRunningPacks)
		- IIF(@RunningPacks < @PackCapacity, 0, (@RunningPacks - @PackCapacity) * ISNULL(UnitsPerUOM, 0)),
	CanPick = @CanPick
OPTION (MAXDOP 1) -- parallelism breaks the ordering of the update

;WITH
RankedPicks AS
(
	SELECT
		PickLinesIncludingPutawayOnly.RunningPK,
		PickLinesIncludingPutawayOnly.WP_PK,
		PickLinesIncludingPutawayOnly.WZ_PK,
		PickLinesIncludingPutawayOnly.WD_PK,
		PickLinesIncludingPutawayOnly.WZ_Units,
		CAST(IsPutaway AS BIT) AS IsPutaway,
		MIN(IsPutaway) OVER (PARTITION BY PickLinesIncludingPutawayOnly.WP_PK) AS IsWholePickPutawayOnly,
		ISNULL(LinesToPick.ConvertedWeight, 0) AS ConvertedWeight,
		ISNULL(LinesToPick.ConvertedVolume, 0) AS ConvertedVolume,
		ISNULL(LinesToPick.RunningPickWeight, 0) AS RunningPickWeight,
		ISNULL(LinesToPick.RunningPickVolume, 0) AS RunningPickVolume,
		ISNULL(LinesToPick.UnitsPerUOM, 0) AS UnitsPerUOM,
		CAST(ISNULL(LinesToPick.IsSplittable, 0) AS BIT) AS IsSplittable,
		ISNULL(LinesToPick.MaxUnitsWithinPackConstraint, 0) AS UnitCapacity,
		FIRST_VALUE(PickLinesIncludingPutawayOnly.WP_PK) OVER (ORDER BY PickLinesIncludingPutawayOnly.PickPriority ASC) AS FirstPickPK
	FROM 
		#PickLinesWithRanking as PickLinesIncludingPutawayOnly
		LEFT JOIN #QuirkyRunningTotal as LinesToPick ON LinesToPick.WZ_PK = PickLinesIncludingPutawayOnly.WZ_PK
	WHERE
		CanPick = 1 OR
		IsPutaway = 1
)

SELECT
	WP_PK,
	WZ_PK,
	WD_PK,
	ConvertedWeight,
	ConvertedVolume,
	RunningPickWeight,
	RunningPickVolume,
	WZ_Units,
	UnitsPerUOM,
	IsSplittable,
	IsPutaway as IsPutawayOnly,
	UnitCapacity,
	@WeightCapacity AS WeightCapacity,
	@VolumeCapacity AS VolumeCapacity
FROM
	RankedPicks
WHERE
	FirstPickPK = WP_PK AND
	IsPutaway = IsWholePickPutawayOnly -- Only take putaway lines if there is nothing to pick
ORDER BY RunningPK ASC

DROP TABLE #PickLinesWithRanking
DROP TABLE #QuirkyRunningTotal");
		}

		static string GetPickMethodSqlAndCreateParameters(ZSqlParameterCollection parameters)
		{
			var pickMethods = WarehouseDataRegistry.Instance.PickMethod.Value;
			var pickMethodsSql = new ZStringBuilder();
			var codes = pickMethods.GetCodeDescriptionPairList().GetAllCodesZString();
			if (codes.Length > 0)
			{
				pickMethodsSql.Append("INSERT INTO @PickMethods VALUES ");

				for (int index = 0; index < codes.Length; index++)
				{
					var key = codes[index];
					var codeParameterName = string.Format(CultureInfo.InvariantCulture, "@PickMethod{0}", index); // internal, for parameter forming
					var descParameterName = string.Format(CultureInfo.InvariantCulture, "@PickMethodDesc{0}", index); // internal, for parameter forming
					pickMethodsSql.Append(string.Format(CultureInfo.InvariantCulture, "({0}, {1})", codeParameterName, descParameterName));

					if (index < codes.Length - 1)
					{
						pickMethodsSql.Append(", ");
					}

					parameters.Add(ZSqlParameter.New(codeParameterName, key, WhsLocationSchema.WL_PickMethod));
					parameters.Add(ZSqlParameter.New(descParameterName, pickMethods.GetDescriptionFromCode(key), WhsLocationTypeSchema.WLT_Description));
				}
			}

			return pickMethodsSql.ToString();
		}

		#endregion
			
		void GetParametersForQuery(string reference, string picker, ZSqlParameterCollection parameters)
		{
			var newParameters = new[]
			{
				ZSqlParameter.New("@WhsPK", Warehouse.PK, WhsDocketSchema.WD_WW_Whs),
				ZSqlParameter.New("@User", picker, WhsPickLineSchema.WZ_GS_NKAssignedTo),
				ZSqlParameter.New("@PickMethod", Criteria.PickMethod, WhsLocationViewSchema.WLV_PickMethod),
				ZSqlParameter.New("@AreaCode", Criteria.AreaCode, WhsAreaSchema.WA_Name),
				ZSqlParameter.New("@PickGroup", Criteria.PickGroup, WhsDocketLineSchema.WE_PickGroup),
				ZSqlParameter.New("@ClientCode", Criteria.ClientCode, OrgHeaderSchema.OH_Code),
				ZSqlParameter.New("@UOMType", Criteria.UOMType, RefPackTypeSchema.F3_UOMType),
				ZSqlParameter.New("@EquipmentRego", Criteria.EquipmentRegistrationNumber, RefEquipmentSchema.RQ_Registration),
				ZSqlParameter.New("@Reference", reference, WhsDocketSchema.WD_ExternalReference),
			};

			parameters.AddRange(newParameters);
		}

		#endregion

		#region SplitPickLine

		static void SplitPickLine(DynamicBusinessObjectCollection results, List<WhsPickLine> pickLines)
		{
			var sqlLinesToSplit = results
				.Select(s => SqlLineWrapper.New(s))
				.Where(s => s.IsSplittable && (s.IsWeightOverCapacity || s.IsVolumeOverCapacity || s.Units > s.UnitCapacity))
				.ToArray();

			foreach (var lineToSplit in sqlLinesToSplit)
			{
				var exceedingWeightUnits = FindExceedingUnits(lineToSplit.ConvertedWeight, lineToSplit.RunningPickWeight, lineToSplit.WeightCapacity);
				var exceedingVolumeUnits = FindExceedingUnits(lineToSplit.ConvertedVolume, lineToSplit.RunningPickVolume, lineToSplit.VolumeCapacity);

				var uomCapacityBasedOnWeight = lineToSplit.UnitsPerUOM == 0 || exceedingWeightUnits == 0
					? 0
					: ((int)(lineToSplit.Units - exceedingWeightUnits) / lineToSplit.UnitsPerUOM) * lineToSplit.UnitsPerUOM;

				var uomCapacityBasedOnVolume = lineToSplit.UnitsPerUOM == 0 || exceedingVolumeUnits == 0
					? 0
					: ((int)(lineToSplit.Units - exceedingVolumeUnits) / lineToSplit.UnitsPerUOM) * lineToSplit.UnitsPerUOM;

				var unitCapacity = new int?[] { lineToSplit.UnitCapacity, uomCapacityBasedOnWeight, uomCapacityBasedOnVolume }.Where(c => c > 0).Min() ?? 0;
				var exceedingPickUnits = unitCapacity == 0 || (lineToSplit.Units < unitCapacity) ? 0 : lineToSplit.Units - unitCapacity;

				if (exceedingVolumeUnits != 0 || exceedingWeightUnits != 0 || exceedingPickUnits != 0)
				{
					var exceedingUnits = Math.Max(Math.Max(exceedingWeightUnits, exceedingVolumeUnits), exceedingPickUnits);
					SplitPickLine(pickLines, lineToSplit.PickLinePK, exceedingUnits);
				}
			}
		}

		static void SplitPickLine(List<WhsPickLine> pickLines, ZGuid pickLinePK, decimal exceedingUnits)
		{
			var pickLine = pickLines.Single(x => x.PK == pickLinePK);
			var decimalCountExpanded = (decimal)Math.Pow(10, pickLine.Product.Parent.OP_CountDecimalPlaces);
			var exceedingAtomicUnits = Math.Ceiling(exceedingUnits * decimalCountExpanded);

			var exceedingUnitsCeiled = exceedingAtomicUnits / decimalCountExpanded;
			if (exceedingUnitsCeiled >= pickLine.WZ_Units)
			{
				pickLines.Remove(pickLine);
			}
			else
			{
				pickLine.Split(exceedingUnitsCeiled);
			}
		}

		static decimal FindExceedingUnits(decimal unit, decimal runningUnit, decimal unitCapacity)
		{
			return unit == 0 || unitCapacity == 0 || runningUnit < unitCapacity
				? 0
				: (runningUnit - unitCapacity) / unit;
		}

		class SqlLineWrapper
		{
			SqlLineWrapper(ZGuid pickLinePK, bool isSplittable, ZDecimal runningPickWeight, ZDecimal runningPickVolume, ZDecimal units, ZInt unitsPerUOM, ZDecimal convertedWeight, ZDecimal convertedVolume, ZDecimal weightCapacity, ZDecimal volumeCapacity, ZInt unitCapacity)
			{
				PickLinePK = pickLinePK;

				IsSplittable = isSplittable;

				RunningPickWeight = runningPickWeight;
				RunningPickVolume = runningPickVolume;
				Units = units;

				UnitsPerUOM = unitsPerUOM;
				ConvertedWeight = convertedWeight;
				ConvertedVolume = convertedVolume;

				WeightCapacity = weightCapacity;
				VolumeCapacity = volumeCapacity;
				UnitCapacity = unitCapacity;
			}

			public ZGuid PickLinePK { get; }

			public bool IsSplittable { get; }

			public bool IsWeightOverCapacity => ConvertedWeight != 0 && RunningPickWeight > WeightCapacity;
			public bool IsVolumeOverCapacity => ConvertedVolume != 0 && RunningPickVolume > VolumeCapacity;

			public ZDecimal RunningPickWeight { get; }
			public ZDecimal RunningPickVolume { get; }
			public ZDecimal Units { get; }

			public ZInt UnitsPerUOM { get; }
			public ZDecimal ConvertedWeight { get; }
			public ZDecimal ConvertedVolume { get; }

			public ZDecimal WeightCapacity { get; }
			public ZDecimal VolumeCapacity { get; }
			public ZInt UnitCapacity { get; }

			public static SqlLineWrapper New(DynamicBusinessObject bizO)
			{
				return new SqlLineWrapper(
					(ZGuid)bizO[WhsPickLineSchema.Constants.PK],
					(ZBool)bizO[nameof(IsSplittable)],
					(ZDecimal)bizO[nameof(RunningPickWeight)],
					(ZDecimal)bizO[nameof(RunningPickVolume)],
					(ZDecimal)bizO[WhsPickLineSchema.Constants.WZ_Units],
					(ZInt)bizO[nameof(UnitsPerUOM)],
					(ZDecimal)bizO[nameof(ConvertedWeight)],
					(ZDecimal)bizO[nameof(ConvertedVolume)],
					(ZDecimal)bizO[nameof(WeightCapacity)],
					(ZDecimal)bizO[nameof(VolumeCapacity)],
					(ZInt)bizO[nameof(UnitCapacity)]);
			}
		}

		#endregion

		#region AssignPickLines

		FindPickResult AssignPickLines(IEnumerable<WhsPickLine> pickLinesToAssign)
		{
			var pickLinesAssigned = new List<WhsPickLine>();

			foreach (var line in pickLinesToAssign)
			{
				AllocatePickLine(line, pickLinesAssigned);
			}

			return SavePickLines(pickLinesAssigned);
		}

		#region AllocatePickLine

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void AllocatePickLine(WhsPickLine pickLine, List<WhsPickLine> pickLinesToKeep)
		{
			var inventory = pickLine.Inventory;
			if (inventory != null)
			{
				var location = inventory.Location;
				var locationPickMethod = location.WLV_PickMethod.ToUpper();
				var areaCode = Criteria.AreaCode;
				var pickMethod = Criteria.PickMethod;
				var pickGroup = Criteria.PickGroup;

				if ((string.IsNullOrEmpty(areaCode) || WhsAreaAndPickMethodHelper.IsAnyCode(areaCode) || areaCode == location.PickingArea.WA_Name.ToUpper()) && // inventory is located in correct area.
					(string.IsNullOrEmpty(pickMethod) || WhsAreaAndPickMethodHelper.IsAnyCode(pickMethod) || WhsAreaAndPickMethodHelper.IsAnyCode(locationPickMethod) || pickMethod == locationPickMethod) && // inventory is located for correct pick method.
					(pickGroup == 0 || pickGroup == pickLine.DocketLine.WE_PickGroup) && // pick line has correct Pick Group
					(location.Row.WR_WW_Whs == Warehouse.PK)) // inventory is located in correct warehouse.
				{
					pickLine.WZ_GS_NKAssignedTo = Picker.GS_Code;
					pickLine.WZ_IsPicking = true;
					pickLinesToKeep.Add(pickLine);
				}
			}
		}

		#endregion

		#region AttemptSave

		FindPickResult SavePickLines(List<WhsPickLine> assignedPickLines)
		{
			FindPickResult result;

			if (assignedPickLines.Count > 0)
			{
				try
				{
					// We save because each pickline has been assigned to the user.
					Factory.Save();
					result = new FindPickResult(assignedPickLines[0].Pick, assignedPickLines.ToArray());
				}
				catch (ZSaveConcurrencyException)
				{
					result = FindPickResult.ConcurrencyError;

					// rollback picklines changes
					var query = new ZDBOnlyQuery(typeof(WhsPickLine));
					query.AddToFilter(WhsPickLineSchema.PK, assignedPickLines.Select(p => p.PK));
					query.ReLoadExistingRows = true;
					Factory.Load<WhsPickLine>(query);
				}
				catch (ZSaveException exception)
				{
					var errorMessage = string.IsNullOrEmpty(exception.FriendlyMessage) ? exception.Message : exception.FriendlyMessage;
					result = new FindPickResult(errorMessage);
				}
			}
			else
			{
				result = FindPickResult.NoPick;
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#region GetOrder

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL Query")]
		public WhsOrder GetOrder(ZGuid? dockDoorLocationPK, bool disallowWithInventoriesAtDockdoor, string orderID = "")
		{
			WhsOrder result = null;

			var assignableOrders = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();
			GetParametersForQuery(string.Empty, string.Empty, parameters); // only unassigned orders

			var whereClause = NotCartonisedAndAtLeast1SplitCaseLineAndValidOrdersOnlyWhereClause;
			if (dockDoorLocationPK.HasValue)
			{
				whereClause += " AND ISNULL(WDA_WL_AssignedDockDoor, WP_WL_DockDoor) = @DockDoorLocation"; // This is direct sql
				parameters.Add("@DockDoorLocation", dockDoorLocationPK.Value, WhsPickSchema.WP_WL_DockDoor);
			}

			if (!orderID.IsNullOrEmpty())
			{
				whereClause += " AND WD_ExternalReference = @DocketReference "; // This is direct sql
				parameters.Add("@DocketReference", orderID, WhsDocketSchema.WD_ExternalReference);
			}

			if (disallowWithInventoriesAtDockdoor)
			{
				whereClause += @" AND NOT EXISTS
		(
			SELECT TOP 1 WE_PK
			FROM
				dbo.WhsDocket PutawayTransfer
				JOIN dbo.WhsDocketLine PutawayTransferLine ON PutawayTransferLine.WE_WD = PutawayTransfer.WD_PK
			WHERE
				PutawayTransfer.WD_WP_ParentPickForTransfer = WP_PK
				AND PutawayTransferLine.WE_PutawayTime IS NOT NULL
				AND PutawayTransferLine.WE_WL = ISNULL(WDA_WL_AssignedDockDoor, WP_WL_DockDoor)
		)
 "; // This is direct sql
			}

			var pickQuery = GetRawPickQueryWithSorting(whereClause, string.Empty, parameters); // This is direct sql

			assignableOrders.Load(pickQuery, parameters);  // we only want to return uncartonised Split Case orders
			if (assignableOrders.Count > 0)
			{
				var pk = (ZGuid)assignableOrders[0][WhsDocketSchema.PK];
				result = Factory.Load<WhsOrder>(pk);
			}

			return result;
		}

		string NotCartonisedAndAtLeast1SplitCaseLineAndValidOrdersOnlyWhereClause
			=> " AND WP_CartoniseSplitCases = 0 AND F3_UOMType = 'SPC' AND WD_DocketType = 'ORD' AND WD_ExcludeFromTotePicking = 0 AND KI_PK IS NULL";

		#endregion
	}
}
