using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PickFaceReplenishmentManager
	{
		#region CreateTransfers

		public static IEnumerable<WhsTransfer> CreateTransfersForPickFaceReplenishment(IEnumerable<IPickFaceInfo> pickFaceInfos, bool allowMultipleTransfersToReplenish = false)
		{
			Argument.NotNull(pickFaceInfos, nameof(pickFaceInfos));

			IEnumerable<WhsTransfer> result;

			var pickFaceInfosEvaluated = pickFaceInfos.ToArray();
			var fixedReplenishmentInfos = LoadFixedLocationInfos(pickFaceInfosEvaluated, allowMultipleTransfersToReplenish).ToArray();
			var dynamicReplenishmentInfos = LoadDynamicLocationInfos(pickFaceInfosEvaluated).ToArray();

			// there may not be any stock to even partially replenish a single pick face
			if (fixedReplenishmentInfos.Length > 0 || dynamicReplenishmentInfos.Length > 0)
			{
				var groupedLines = GetGroupedFixedPickFaceLinesForCreatingTransfers(fixedReplenishmentInfos).Values
					.Concat(GetGroupedDynamicPickFaceLinesForCreatingTransfers(dynamicReplenishmentInfos).Values)
					.ToArray();

				result = CreateAndPreSaveValidatePickLinesAndTransfers(groupedLines, GetInventoryPalletInfos(fixedReplenishmentInfos));
			}
			else
			{
				result = Enumerable.Empty<WhsTransfer>();
			}

			return result;
		}

		#region GetGroupedLinesForCreatingTransfers

		static Dictionary<string, List<ReplenishmentLocationInfo>> GetGroupedFixedPickFaceLinesForCreatingTransfers(IEnumerable<ReplenishmentLocationInfo> fixedReplenishmentInfos)
		{
			var isAlsoGroupedByFromLocationAndPalletID = WarehouseDataRegistry.Instance.SingleLineAutoPickFaceReplenishmentTransfers.Value;
			return GroupLinesForCreatingTransfers(fixedReplenishmentInfos, info => GetFixedPickFaceHashKey(info, isAlsoGroupedByFromLocationAndPalletID));
		}

		static Dictionary<string, List<ReplenishmentLocationInfo>> GetGroupedDynamicPickFaceLinesForCreatingTransfers(IEnumerable<ReplenishmentLocationInfo> dynamicReplenishmentInfos)
		{
			return GroupLinesForCreatingTransfers(dynamicReplenishmentInfos, info => GetDynamicPickFaceHashKey(info));
		}

		static Dictionary<string, List<ReplenishmentLocationInfo>> GroupLinesForCreatingTransfers(IEnumerable<ReplenishmentLocationInfo> replenishmentInfos, Func<ReplenishmentLocationInfo, string> getKey)
		{
			var groupedLines = new Dictionary<string, List<ReplenishmentLocationInfo>>();

			foreach (var info in replenishmentInfos)
			{
				var key = getKey(info);
				if (!groupedLines.TryGetValue(key, out var lines))
				{
					groupedLines[key] = lines = new List<ReplenishmentLocationInfo>();
				}

				lines.Add(info);
			}

			return groupedLines;
		}

		/// <summary>
		/// Group transfer by the following:
		///		From Location  //
		///		Pallet ID      // only if registry-enabled
		///		
		///		Client
		///		Warehouse
		///		Destination (PickFace) Location
		///		Product
		/// </summary>
		static string GetFixedPickFaceHashKey(ReplenishmentLocationInfo info, bool isAlsoGroupedByFromLocationAndPalletID)
		{
			var fromLocationAndPalletIDKey = isAlsoGroupedByFromLocationAndPalletID ? info.TransferFromLocationPk.ToString() + info.SourcePalletID.ToUpper() : "";
			var clientWarehousePickFaceLocationAndProductKey = info.ClientPK.ToString() + info.WarehousePK.ToString() + info.TransferToLocationPk.ToString() + info.ProductPK.ToString();

			return Invariant($"F|{clientWarehousePickFaceLocationAndProductKey}{fromLocationAndPalletIDKey}|F"); // Dictionary key
		}

		static string GetDynamicPickFaceHashKey(ReplenishmentLocationInfo info)
		{
			var clientWarehouseAndAreaKey = info.ClientPK.ToString() + info.WarehousePK.ToString() + info.TransferToAreaPK.ToString();

			return Invariant($"D|{clientWarehouseAndAreaKey}|D"); // Dictionary key
		}

		static (ZGuid LocationPK, string PalletID) GetPalletHashKey(ZGuid locationPK, string palletID) => (locationPK, palletID.ToUpper());

		#endregion

		#region GetInventoryPalletInfos

		static Dictionary<(ZGuid LocationPK, string PalletID), decimal> GetInventoryPalletInfos(ReplenishmentLocationInfo[] replenishmentInfos)
		{
			var palletDictonary = new Dictionary<(ZGuid LocationPK, string PalletID), decimal>();
			var palletsToCheck = replenishmentInfos.Where(r => r.RetainPalletIDsInFixedPickFaces && !string.IsNullOrEmpty(r.SourcePalletID)).ToArray();
			if (palletsToCheck.Length > 0)
			{
				var result = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
				var sql = @"
SELECT
	WI_WL,
	WI_PalletID,
	SUM(WI_TotalUnits) AS WI_TotalUnits
FROM
	dbo.WhsInventoryView
WHERE
	WI_WL IN (SELECT Value FROM @Locations)
	AND WI_TotalUnits > 0
	AND WI_PalletID IN (SELECT Value FROM @PalletIDs)
	AND WI_PalletID <> ''
GROUP BY
	WI_WL, WI_PalletID
";

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add(ZSqlParameter.New("@Locations", palletsToCheck.Select(p => p.TransferFromLocationPk.ToGuid()).Distinct().ToArray(), WhsInventoryViewSchema.WI_WL, isTableValued: true));
				sqlParams.Add(ZSqlParameter.New("@PalletIDs", palletsToCheck.Select(p => p.SourcePalletID.ToString()).Distinct().ToArray(), WhsInventoryViewSchema.WI_PalletID, isTableValued: true));

				result.Load(sql, sqlParams);

				if (result.Count > 0)
				{
					foreach (var l in result)
					{
						var locationPK = (ZGuid)l[WhsInventoryViewSchema.Constants.WI_WL];
						var palletID = (ZString)l[WhsInventoryViewSchema.Constants.WI_PalletID];
						var totalUnitsInInventory = (ZDecimal)l[WhsInventoryViewSchema.Constants.WI_TotalUnits];
						palletDictonary[GetPalletHashKey(locationPK, palletID)] = totalUnitsInInventory;
					}
				}
			}

			return palletDictonary;
		}

		#endregion

		#region CreateAndSavePickLinesAndTransfers

		static IEnumerable<WhsTransfer> CreateAndPreSaveValidatePickLinesAndTransfers(IEnumerable<List<ReplenishmentLocationInfo>> groupedLines, IReadOnlyDictionary<(ZGuid LocationPK, string PalletID), decimal> inventoryPalletInfos)
		{
			var transferList = new List<WhsTransfer>();

			foreach (var group in groupedLines)
			{
				var newFactory = new BusinessObjectFactory();
				foreach (var pickSplitLines in group.GroupBy(info => info.PickToReplenishPK))
				{
					var transfer = CreateTransferWithLines(newFactory, pickSplitLines, inventoryPalletInfos);
					transfer.RunPreSaveValidation();
					transferList.Add(transfer);
				}
			}

			return transferList;
		}

		#region CreateTransferWithLines

		static WhsTransfer CreateTransferWithLines(BusinessObjectFactory factory, IEnumerable<ReplenishmentLocationInfo> lines, IReadOnlyDictionary<(ZGuid LocationPK, string PalletID), decimal> inventoryPalletInfos)
		{
			var transfer = factory.New<WhsTransfer>();

			var firstLine = lines.First();
			transfer.WD_OH_Client = firstLine.ClientPK;
			transfer.WD_WW_Whs = firstLine.WarehousePK;
			transfer.WD_IsPickFaceReplenishment = true;
			transfer.WD_WP_PickBeingReplenished = firstLine.PickToReplenishPK;

			var transferPalletInfos = lines.GroupBy(l => GetPalletHashKey(l.TransferFromLocationPk, l.SourcePalletID), (key, groupedLines) => (K: key, V: groupedLines.Sum(l => l.QuantityToTransfer)))
				.ToDictionary(g => g.K, g => g.V);

			var warehouse = transfer.Warehouse;
			foreach (var line in lines)
			{
				CreateTransferLine(warehouse, transfer, line, inventoryPalletInfos, transferPalletInfos);
			}

			return transfer;
		}

		static WhsTransferLine CreateTransferLine(
			WhsWarehouse warehouse,
			WhsTransfer transfer,
			ReplenishmentLocationInfo replenishmentInfoForTransferLine,
			IReadOnlyDictionary<(ZGuid LocationPK, string PalletID), decimal> inventoryPalletInfos,
			Dictionary<(ZGuid LocationPK, string PalletID), decimal> transferPalletInfos)
		{
			var line = transfer.Lines.AddNew();
			line.WE_OP = replenishmentInfoForTransferLine.ProductPK;
			line.WE_WL_TransferFrom = replenishmentInfoForTransferLine.TransferFromLocationPk;
			line.WE_WL = replenishmentInfoForTransferLine.TransferToLocationPk;
			line.WE_TransferFromPalletId = replenishmentInfoForTransferLine.SourcePalletID;
			line.WE_AdjustmentArrivalDate = warehouse.GetWarehouseBranchLocalDateTimeOffset(replenishmentInfoForTransferLine.ArrivalDate);
			line.WE_ExpiryDate = replenishmentInfoForTransferLine.ExpiryDate;
			line.WE_PackingDate = replenishmentInfoForTransferLine.PackingDate;
			line.WE_PartAttrib1 = replenishmentInfoForTransferLine.PartAttrib1;
			line.WE_PartAttrib2 = replenishmentInfoForTransferLine.PartAttrib2;
			line.WE_PartAttrib3 = replenishmentInfoForTransferLine.PartAttrib3;
			line.WE_SerialNumber = replenishmentInfoForTransferLine.SerialNumber;
			line.WE_BondedEntryKey = replenishmentInfoForTransferLine.BondedEntryKey;
			line.WE_TransactionQuantity = replenishmentInfoForTransferLine.QuantityToTransfer;

			var palletKey = GetPalletHashKey(replenishmentInfoForTransferLine.TransferFromLocationPk, replenishmentInfoForTransferLine.SourcePalletID);
			if (replenishmentInfoForTransferLine.RetainPalletIDsInFixedPickFaces
				&& transferPalletInfos.TryGetValue(palletKey, out var totalQuantityToTransfer)
				&& inventoryPalletInfos.TryGetValue(palletKey, out var totalUnitsInInventory)
				&& totalUnitsInInventory == totalQuantityToTransfer)
			{
				line.WE_PalletID = replenishmentInfoForTransferLine.SourcePalletID;
			}

			return line;
		}

		#endregion

		#endregion

		#endregion

		#region LoadLocationInfos

		const int Query30MinuteTimeOut = 1800;

		static IEnumerable<ReplenishmentLocationInfo> LoadFixedLocationInfos(IPickFaceInfo[] pickFaceInfos, bool allowMultipleTransfersToReplenish = false)
		{
			return LoadLocationInfosCore(pickFaceInfos.Where(info => !info.IsDynamicTransfer).ToArray(), () => GetFixedPickFaceReplenishmentLocationQuery(allowMultipleTransfersToReplenish));
		}

		static IEnumerable<ReplenishmentLocationInfo> LoadDynamicLocationInfos(IPickFaceInfo[] pickFaceInfos)
		{
			return LoadLocationInfosCore(pickFaceInfos.Where(info => info.IsDynamicTransfer).ToArray(), () => GetDynamicPickFaceReplenishmentLocationQuery());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static IEnumerable<ReplenishmentLocationInfo> LoadLocationInfosCore(IPickFaceInfo[] pickFaceInfos, Func<ZQuery> getReplenishmentQuery)
		{
			if (pickFaceInfos.Length == 0)
			{
				yield break;
			}

			var tempPickFacesDataTable = GetTempPickFacesDataTable(pickFaceInfos);
			var query = getReplenishmentQuery();

			using (var command = Db.Connection.Command(query.LiteralTextADO, Query30MinuteTimeOut)) // Accessing data using a SQL View
			{
				command.AddTableValuedParameter("@PickFaces", "dbo.TVP_TempPickFacesMapping", tempPickFacesDataTable);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return PopulateLocationInfo(reader);
					}
				}
			}
		}

		static DataTable GetTempPickFacesDataTable(IEnumerable<IPickFaceInfo> pickFaceInfos)
		{
			var dataTable = new DataTable("@PickFaces");
			dataTable.Columns.Add("WarehousePK", typeof(Guid));
			dataTable.Columns.Add("PickToReplenishPK", typeof(Guid));
			dataTable.Columns.Add("ClientPK", typeof(Guid));
			dataTable.Columns.Add("ProductPK", typeof(Guid));
			dataTable.Columns.Add("TransferToLocationPK", typeof(Guid));
			dataTable.Columns.Add("ReplenishQuantity", typeof(decimal));
			dataTable.Columns.Add("ReplenishMultiple", typeof(decimal));
			dataTable.Columns.Add("IsDeadLocked", typeof(bool));
			dataTable.Locale = CultureInfo.InvariantCulture;

			var orderedExpiryDateColumn = dataTable.Columns.Add("OrderedExpiryDate", typeof(DateTime));
			orderedExpiryDateColumn.AllowDBNull = true;

			var orderedPackingDateColumn = dataTable.Columns.Add("OrderedPackingDate", typeof(DateTime));
			orderedPackingDateColumn.AllowDBNull = true;

			dataTable.Columns.Add("OrderedAttribute1", typeof(string));
			dataTable.Columns.Add("OrderedAttribute2", typeof(string));
			dataTable.Columns.Add("OrderedAttribute3", typeof(string));
			dataTable.Columns.Add("OrderedSerialNumber", typeof(string));

			foreach (var info in pickFaceInfos)
			{
				// If OrderedExpiryDate or OrderedPackingDate are Date.Empty (no specific date ordered), we need to insert NULL
				var orderedExpiryDate = info.OrderedExpiryDate.IsEmpty ? (DateTime?)null : info.OrderedExpiryDate.ToDateTime();
				var orderedPackingDate = info.OrderedPackingDate.IsEmpty ? (DateTime?)null : info.OrderedPackingDate.ToDateTime();

				dataTable.Rows.Add(
					new object[]
					{
						info.WarehousePK.ToGuid(),
						info.PickToReplenishPK.IsEmpty ? Guid.Empty : info.PickToReplenishPK.ToGuid(),
						info.ClientPK.ToGuid(),
						info.ProductPK.ToGuid(),
						info.TransferToLocationPK.ToGuid(),
						(decimal)info.ReplenishQuantity,
						(decimal)info.ReplenishMultiple,
						(bool)info.IsDeadLocked,
						orderedExpiryDate,
						orderedPackingDate,
						info.OrderedAttribute1.ToString(),
						info.OrderedAttribute2.ToString(),
						info.OrderedAttribute3.ToString(),
						info.OrderedSerialNumber.ToString()
					});
			}

			return dataTable;
		}

		#region GetDynamicPickFaceReplenishmentLocationQuery

		static ZQuery GetDynamicPickFaceReplenishmentLocationQuery()
		{
			var replenishmentLocationSql = new ZStringBuilder((NoResString)";with"); // Sql Command key word
			replenishmentLocationSql.Append(GetPickFromInventorySQL());

			#region replenishmentLocationSql

			replenishmentLocationSql.AppendLine(@"
PickFacesWithAllPossibleInventories as (
	SELECT
		WR_Name,
		WL_Column,
		WL_Level,
		WL_Tray,
		WarehousePK,
		PickToReplenishPK,
		ClientPK,
		ProductPK,
		TransferToLocationPK,
		PickFromLocationPK,
		PickFromQuantityAvailable,
		ReplenishQuantity,
		PickFromExpiryDate,
		PickFromPackingDate,
		OrderedAttribute1,
		OrderedAttribute2,
		OrderedAttribute3,
		OrderedSerialNumber,
		OrderedExpiryDate,
		OrderedPackingDate,
		PickFromPartAttrib1,
		PickFromPartAttrib2,
		PickFromPartAttrib3,
		PickFromSerialNumber,
		PickFromPalletID,
		PickFromArrivalDate,
		PickFromBondedEntryKey,
		-- Find the number of combinations of Ordered Attributes that can be satisfied per unique inventory
		-- For example:
		--  Order - Blue  ''
		--  Order - ''    Large
		--  Order - Blue  Large
		--
		-- For inventory 'Blue Large', this number will be 3, because the inventory can be allocated to any of these (groups of) order lines.
		-- This is to understand how contested the inventory is, and such stock will be allocated LAST. Thus, more specific allocation
		-- occurs first, reducing the amount of over-allocation.
		CountOfOrdersRequiringThisInventory = COUNT(*) OVER (
			PARTITION BY
				ClientPK,
				WarehousePK,
				ProductPK,
				PickFromExpiryDate,
				PickFromPackingDate,
				PickFromArrivalDate,
				PickFromLocationPK,
				PickFromPalletID,
				PickFromBondedEntryKey,
				PickFromPartAttrib1,
				PickFromPartAttrib2,
				PickFromPartAttrib3,
				PickFromSerialNumber)
	FROM
		@PickFaces
		JOIN PickFromInventory ON
			PickFromClient = ClientPK AND
			PickFromPart = ProductPK AND
			PickFromWarehousePK = WarehousePK AND
			(OrderedAttribute1 = '' OR PickFromPartAttrib1 = OrderedAttribute1) AND
			(OrderedAttribute2 = '' OR PickFromPartAttrib2 = OrderedAttribute2) AND
			(OrderedAttribute3 = '' OR PickFromPartAttrib3 = OrderedAttribute3) AND
			(OrderedSerialNumber = '' OR PickFromSerialNumber = OrderedSerialNumber) AND
			(OrderedExpiryDate IS NULL OR PickFromExpiryDate = OrderedExpiryDate) AND
			(OrderedPackingDate IS NULL OR PickFromPackingDate = OrderedPackingDate)
),
PickFacesWithAllPossibleInventoriesWithTotals as (
	SELECT
		WarehousePK,
		PickToReplenishPK,
		ClientPK,
		ProductPK,
		TransferToLocationPK,
		PickFromLocationPK,
		PickFromQuantityAvailable,
		ReplenishQuantity,
		PickFromExpiryDate,
		PickFromPackingDate,
		PickFromPartAttrib1,
		PickFromPartAttrib2,
		PickFromPartAttrib3,
		PickFromSerialNumber,
		PickFromPalletID,
		PickFromArrivalDate,
		PickFromBondedEntryKey,
		-- Create running total (using the FIFO ordering) of Inventory that can replenish the pick face
		Total_Available = SUM(PickFromQuantityAvailable) OVER (
			PARTITION BY
				ClientPK,
				WarehousePK,
				PickToReplenishPK,
				ProductPK,
				TransferToLocationPK,
				OrderedPackingDate,
				OrderedExpiryDate,
				OrderedAttribute1,
				OrderedAttribute2,
				OrderedAttribute3,
				OrderedSerialNumber
			ORDER BY
				-- As we are grouping by Product and Client, and both Expiry & Packing Date are mandatory
				-- attribs, then Expiry & Packing Date will either all be null or all have a value. Arrival
				-- date will always have a value for available inventory, therefore even though nulls sort
				-- first when sorting ascending we do not need to handle it.
				PickFromExpiryDate,
				PickFromPackingDate,
				PickFromArrivalDate,
				-- We want to consider Inventory that is required by multiple orders last
				-- in case orders are satisfied by stock that only they require first. This leaves
				-- the Inventory that satisfies multiple orders available to the other orders.
				CountOfOrdersRequiringThisInventory,
				PickFromQuantityAvailable DESC,
				WR_Name,
				WL_Column,
				WL_Level,
				WL_Tray,
				PickFromPalletID,
				PickFromBondedEntryKey,
				PickFromPartAttrib1,
				PickFromPartAttrib2,
				PickFromPartAttrib3,
				PickFromSerialNumber),
		Total_Pick = SUM(ReplenishQuantity) OVER (
			PARTITION BY
				ClientPK,
				WarehousePK,
				ProductPK,
				PickFromExpiryDate,
				PickFromPackingDate,
				PickFromArrivalDate,
				PickFromLocationPK,
				PickFromPalletID,
				PickFromBondedEntryKey,
				PickFromPartAttrib1,
				PickFromPartAttrib2,
				PickFromPartAttrib3,
				PickFromSerialNumber
			ORDER BY
				ReplenishQuantity DESC,
				TransferToLocationPK)
	FROM
		PickFacesWithAllPossibleInventories
),
CalculateInventories as (
	SELECT
		*,
		CASE
			WHEN Total_Pick - ReplenishQuantity <= Total_Available - PickFromQuantityAvailable THEN
				CASE
					WHEN Total_Pick > Total_Available THEN PickFromQuantityAvailable
					ELSE Total_Pick - (Total_Available - PickFromQuantityAvailable)
				END
			ELSE
				CASE
					WHEN Total_Pick >= Total_Available THEN Total_Available - (Total_Pick - ReplenishQuantity)
					ELSE ReplenishQuantity
				END
		END AS QuantityToTransfer
	FROM
		PickFacesWithAllPossibleInventoriesWithTotals
)
SELECT
	WarehousePK,
	PickToReplenishPK,
	ClientPK,
	ProductPK,
	TransferToLocationPK,
	WL_WA_PickingArea as TransferToAreaPK,
	PickFromLocationPK,
	PickFromExpiryDate,
	PickFromPackingDate,
	PickFromPartAttrib1,
	PickFromPartAttrib2,
	PickFromPartAttrib3,
	PickFromSerialNumber,
	PickFromPalletID,
	PickFromArrivalDate,
	PickFromBondedEntryKey,
	-- Create running total of Proposed Transfer Quantities, Grouped by each PickFace
	SUM (QuantityToTransfer) as QuantityToTransfer,
	CAST(0 AS BIT) AS RetainPalletIDsInFixedPickFaces
FROM
	CalculateInventories
	JOIN dbo.WhsLocation ON WL_PK = TransferToLocationPK
WHERE
	QuantityToTransfer > 0
GROUP BY
	WarehousePK,
	PickToReplenishPK,
	ClientPK,
	ProductPK,
	TransferToLocationPK,
	WL_WA_PickingArea,
	PickFromLocationPK,
	PickFromExpiryDate,
	PickFromPackingDate,
	PickFromPartAttrib1,
	PickFromPartAttrib2,
	PickFromPartAttrib3,
	PickFromSerialNumber,
	PickFromPalletID,
	PickFromArrivalDate,
	PickFromBondedEntryKey");

			#endregion

			var sqlParams = new ZSqlParameterCollection();

			var query = new ZQuery();
			query.AddFilterAndZSQLParameterCollection(replenishmentLocationSql.ToString(), sqlParams);

			return query;
		}

		#endregion

		#region GetFixedPickFaceReplenishmentLocationQuery

		static ZQuery GetFixedPickFaceReplenishmentLocationQuery(bool allowMultipleTransfersToReplenish)
		{
			var replenishmentLocationSql = new ZStringBuilder((NoResString)";with"); // Sql Command key word
			if (!allowMultipleTransfersToReplenish)
			{
				#region replenishmentLocationSql with transfer checks

				replenishmentLocationSql.AppendLine(@"
UnfinalisedTransferLines as (
	SELECT
		WD_PK, 
		WD_OH_Client, 
		WE_OP, 
		WE_WL
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WD_PK = WE_WD
	WHERE
		WD_DocketType = @TransferDocketType and
		WE_DocketLineStatus != @FinalisedDocketLineStatus
),
PickFacesWithoutUnfinalisedTransferLines as (
	SELECT 
		WarehousePK,
		PickToReplenishPK,
		ClientPK,
		ProductPK,
		TransferToLocationPK,
		ReplenishQuantity,
		ReplenishMultiple,
		IsDeadLocked
	FROM
		@PickFaces
		LEFT JOIN UnfinalisedTransferLines ON
			WD_OH_Client = ClientPK AND 
			WE_OP = ProductPK AND 
			WE_WL = TransferToLocationPK
	WHERE
		WD_PK IS NULL -- exclude PickFaces that already have an open transfer
),
");

				#endregion
			}

			#region replenishmentLocationSql

			replenishmentLocationSql.Append(GetPickFromInventorySQL());
			replenishmentLocationSql.AppendLine(string.Format(CultureInfo.InvariantCulture, @"
PickFacesWithAllPossibleInventories as (	
	SELECT 	
		WR_Name,
		WL_Column,
		WL_Level,
		WL_Tray,
		WarehousePK,
		PickToReplenishPK,
		ClientPK,
		ProductPK,
		TransferToLocationPK,
		PickFromLocationPK,
		PickFromQuantityAvailable,
		ReplenishQuantity,
		case
			when ReplenishMultiple <= max(PickFromQuantityAvailable) over (partition by ClientPK, WarehousePK, ProductPK) then ReplenishMultiple
			else 0 -- do not use replenish multiple if no locations can replenish with that multiple
		end as ReplenishMultiple,
		IsDeadLocked,
		PickFromExpiryDate,
		PickFromPackingDate,
		PickFromPartAttrib1,
		PickFromPartAttrib2,
		PickFromPartAttrib3,
		PickFromSerialNumber,
		PickFromPalletID,
		PickFromArrivalDate,
		PickFromBondedEntryKey
	FROM
		{0}
		JOIN PickFromInventory ON
			PickFromClient = ClientPK AND
			PickFromPart = ProductPK AND
			PickFromWarehousePK = WarehousePK
),
PickFacesWithAllPossibleInventoriesWithTotals as (	
	SELECT
		WarehousePK,
		PickToReplenishPK,
		ClientPK,
		ProductPK,
		TransferToLocationPK,
		PickFromLocationPK,
		PickFromQuantityAvailable,
		ReplenishQuantities.ReplenishQuantity,
		ReplenishMultiple,
		PickFromExpiryDate,
		PickFromPackingDate,
		PickFromPartAttrib1,
		PickFromPartAttrib2,
		PickFromPartAttrib3,
		PickFromSerialNumber,
		PickFromPalletID,
		PickFromArrivalDate,
		PickFromBondedEntryKey,
		-- Create running total (using the FIFO ordering) of Inventory that can replenish the pick face
		Total_Available = SUM(PickFromQuantityAvailable) OVER (
			PARTITION BY
				ClientPK, 
				WarehousePK,
				PickToReplenishPK,
				ProductPK,
				TransferToLocationPK
			ORDER BY
				-- As we are grouping by Product and Client, and both Expiry & Packing Date are mandatory
				-- attribs, then Expiry & Packing Date will either all be null or all have a value. Arrival
				-- date will always have a value for available inventory, therefore even though nulls sort
				-- first when sorting ascending we do not need to handle it.
				PickFromExpiryDate,
				PickFromPackingDate,
				PickFromArrivalDate,
				PickFromQuantityAvailable DESC,
				WR_Name,
				WL_Column,
				WL_Level,
				WL_Tray,
				PickFromPalletID,
				PickFromBondedEntryKey,
				PickFromPartAttrib1,
				PickFromPartAttrib2,
				PickFromPartAttrib3,
				PickFromSerialNumber),
		-- Create running total of the PickFace Replenish Quantities (for pickfaces that need
		-- replenishing). The sum is grouped by each unique inventory and ordered by
		-- TransferToLocation to keep unique ordering in case there is a duplicate Replenish Quantity.
		Total_Pick = SUM(ReplenishQuantities.ReplenishQuantity) OVER (
			PARTITION BY
				ClientPK,
				WarehousePK,
				ProductPK,
				PickFromExpiryDate,
				PickFromPackingDate,
				PickFromArrivalDate,
				PickFromLocationPK,
				PickFromPalletID,
				PickFromBondedEntryKey,
				PickFromPartAttrib1,
				PickFromPartAttrib2,
				PickFromPartAttrib3,
				PickFromSerialNumber
			ORDER BY
				PickFacesWithAllPossibleInventories.ReplenishQuantity DESC,
				TransferToLocationPK)
	FROM
		PickFacesWithAllPossibleInventories
		-- If Replenish Multiple is 0 or 1, we can simply replenish using the full amount
		-- otherwise we should replenish by an amount that is divisible by the Replenish Multiple.
		-- For deadlocked picks, we must take at least one multiple - this may overfill the pickface but will relieve the deadlock
		CROSS APPLY
		(
			SELECT
				CASE WHEN IsDeadLocked = 1 AND ReplenishQuantity < ReplenishMultiple
					THEN ReplenishMultiple
					ELSE
						CASE WHEN ReplenishMultiple = 0 OR ReplenishMultiple = 1
								THEN ReplenishQuantity
								ELSE ReplenishQuantity - (ReplenishQuantity % ReplenishMultiple)
						END
				END as ReplenishQuantity
		) as ReplenishQuantities
	WHERE
		PickFromQuantityAvailable > 0
),
CalculateInventories as (
	SELECT
		*,
		CASE
			WHEN Total_Pick - ReplenishQuantity <= Total_Available - PickFromQuantityAvailable THEN
				CASE
					WHEN Total_Pick > Total_Available THEN PickFromQuantityAvailable
					ELSE Total_Pick - (Total_Available - PickFromQuantityAvailable)
				END
			ELSE
				CASE
					WHEN Total_Pick >= Total_Available THEN Total_Available - (Total_Pick - ReplenishQuantity)
					ELSE ReplenishQuantity
				END
		END AS QuantityToTransfer
	FROM
		PickFacesWithAllPossibleInventoriesWithTotals
),
CalculateInventoriesWithTotals as (
	SELECT
		WarehousePK,
		PickToReplenishPK,
		ClientPK,
		ProductPK,
		TransferToLocationPK,
		PickFromLocationPK,
		PickFromExpiryDate,
		PickFromPackingDate,
		PickFromPartAttrib1,
		PickFromPartAttrib2,
		PickFromPartAttrib3,
		PickFromSerialNumber,
		PickFromPalletID,
		PickFromArrivalDate,
		PickFromBondedEntryKey,
		-- Create running total of Proposed Transfer Quantities, Grouped by each PickFace
		SUM (FixedQuantities.QuantityToTransfer) OVER (
			PARTITION BY
				ClientPK,
				ProductPK,
				TransferToLocationPK
			ORDER BY
				Total_Available) as QuantityToTransfer_Total,
		FixedQuantities.QuantityToTransfer,
		ReplenishQuantity
	FROM
		CalculateInventories
		CROSS APPLY
		(
			-- If we have the case where the current inventory to consider has a surplus (i.e. pick face is pretty much replenished)
			-- then our QuantityToTransfer will become negative for any inventory newer than it. However there is a possibility that
			-- newer stock (as everything is ordered by FIFO) has a quantity less than the replenish multiple and may be able to
			-- still fit in the pickface and thus we will consider whether the FULL available amount can be transferred.
			SELECT CASE WHEN QuantityToTransfer < 0
				THEN
					PickFromQuantityAvailable
				-- In the case where the amount we can take from the location is not the full amount
				-- and this amount is not divisible by the Replenish Multiple we minus the remainder
				-- otherwise we take everything from the location.
				ELSE CASE WHEN ReplenishMultiple > 0
				AND ReplenishMultiple <> 1
				AND QuantityToTransfer < PickFromQuantityAvailable
				AND QuantityToTransfer % ReplenishMultiple >= 0
				THEN
					QuantityToTransfer - (QuantityToTransfer % ReplenishMultiple)
				ELSE
					QuantityToTransfer
				END
			END as QuantityToTransfer
		) as FixedQuantities
)

SELECT
	WarehousePK,
	PickToReplenishPK,
	ClientPK,
	ProductPK,
	TransferToLocationPK,
	CAST(0x0 as uniqueidentifier) as TransferToAreaPK, -- Not needed for Fixed Pickface, hence empty guid.
	PickFromLocationPK,
	QuantityToTransfer,
	PickFromExpiryDate,
	PickFromPackingDate,
	PickFromPartAttrib1,
	PickFromPartAttrib2,
	PickFromPartAttrib3,
	PickFromSerialNumber,
	PickFromPalletID,
	PickFromArrivalDate,
	PickFromBondedEntryKey,
	WLT_RetainPalletIDsInFixedPickFaces AS RetainPalletIDsInFixedPickFaces
FROM
	CalculateInventoriesWithTotals
	JOIN dbo.WhsLocation ON WL_PK = TransferToLocationPK
	JOIN dbo.WhsLocationType ON WLT_PK = WL_WLT_LocationType
WHERE
	QuantityToTransfer > 0 AND
	-- If the addition of this Inventory exceeds how much we can replenish then ignore it
	QuantityToTransfer_Total <= ReplenishQuantity", allowMultipleTransfersToReplenish ? "@PickFaces" : "PickFacesWithoutUnfinalisedTransferLines"));

			#endregion

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@TransferDocketType", DocketType.Codes.Transfer, WhsDocketSchema.WD_DocketType },
				{ "@FinalisedDocketLineStatus", DocketLineStatus.Codes.Finalised, WhsDocketLineSchema.WE_DocketLineStatus }
			};

			var query = new ZQuery();
			query.AddFilterAndZSQLParameterCollection(replenishmentLocationSql.ToString(), sqlParams);

			return query;
		}

		#endregion

		#region GetPickFromInventorySQL

		static string GetPickFromInventorySQL()
		{
			return $@"
PickFromInventory as (
	SELECT
		WD_OH_Client AS PickFromClient,
		WE_OP AS PickFromPart,
		WLV_WW_Whs AS PickFromWarehousePK,
		WLV_PK AS PickFromLocationPK,
		-- Calculating what is available to replenish the pickface with
		SUM(WE_StockOnHand - ISNULL(CommittedUnits, 0)) AS PickFromQuantityAvailable,
		WE_ExpiryDate AS PickFromExpiryDate,
		WE_PackingDate AS PickFromPackingDate,
		WE_BondedEntryKey AS PickFromBondedEntryKey,
		WE_PartAttrib1 AS PickFromPartAttrib1,
		WE_PartAttrib2 AS PickFromPartAttrib2,
		WE_PartAttrib3 AS PickFromPartAttrib3,
		WE_SerialNumber AS PickFromSerialNumber,
		WE_PalletID AS PickFromPalletID,
		CAST(WE_AdjustmentArrivalDate AS date) AS PickFromArrivalDate,
		WLV_RowName as WR_Name,
		WLV_Column as WL_Column,
		WLV_Level as WL_Level,
		WLV_Tray as WL_Tray
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.WhsLocationView ON WLV_PK = WE_WL
		CROSS APPLY
		(
			SELECT
				SUM(WZ_Units) AS CommittedUnits
			FROM
				dbo.WhsCommittedStock
			WHERE
				WZ_WE_InventoryLine = WE_PK
		) as CommittedUnits
	WHERE
		WLV_LocationStatus = '{LocationStatus.Codes.Normal}' AND
		WE_CurrentInventoryStatus = '{InventoryStatus.Codes.Available}' AND
		-- Also not in another dynamic location
		WLV_LocationClass <> '{LocationClasses.Codes.DPF}' AND
		WE_StockOnHand > 0 AND
		NOT EXISTS
		(
			SELECT 1
			FROM dbo.WhsPickFace
			WHERE WE_WL = WF_WL and WF_OH_Client = WD_OH_Client AND WF_OP = WE_OP
		)
		AND WLV_WW_Whs IN ( SELECT WarehousePK FROM @PickFaces )
		AND WD_OH_Client IN ( SELECT ClientPK FROM @PickFaces )
		AND WE_OP IN ( SELECT ProductPK FROM @PickFaces )
	GROUP BY
		WE_ExpiryDate,
		WE_PackingDate,
		CAST(WE_AdjustmentArrivalDate as Date),
		WE_PartAttrib1,
		WE_PartAttrib2,
		WE_PartAttrib3,
		WE_SerialNumber,
		WE_BondedEntryKey,
		WE_PalletID,
		WD_OH_Client,
		WE_OP,
		WLV_RowName,
		WLV_PK,
		WLV_Column,
		WLV_Level,
		WLV_Tray,
		WLV_WW_Whs
	HAVING
		SUM(WE_StockOnHand - ISNULL(CommittedUnits, 0)) > 0
),";
		}

		#endregion

		#region PopulateLocationInfo

		static ReplenishmentLocationInfo PopulateLocationInfo(IDataReader reader)
		{
			var result = new ReplenishmentLocationInfo
			{
				WarehousePK = reader.GetGuid(reader.GetOrdinal("WarehousePK")),
				PickToReplenishPK = reader.GetGuid(reader.GetOrdinal("PickToReplenishPK")),
				ClientPK = reader.GetGuid(reader.GetOrdinal("ClientPK")),
				ProductPK = reader.GetGuid(reader.GetOrdinal("ProductPK")),
				TransferToLocationPk = reader.GetGuid(reader.GetOrdinal("TransferToLocationPK")),
				TransferFromLocationPk = reader.GetGuid(reader.GetOrdinal("PickFromLocationPk")),
				QuantityToTransfer = reader.GetDecimal(reader.GetOrdinal("QuantityToTransfer")),
				ExpiryDate = GetZDateTime(reader, "PickFromExpiryDate").Date,
				PackingDate = GetZDateTime(reader, "PickFromPackingDate").Date,
				PartAttrib1 = reader.GetString(reader.GetOrdinal("PickFromPartAttrib1")),
				PartAttrib2 = reader.GetString(reader.GetOrdinal("PickFromPartAttrib2")),
				PartAttrib3 = reader.GetString(reader.GetOrdinal("PickFromPartAttrib3")),
				SerialNumber = reader.GetString(reader.GetOrdinal("PickFromSerialNumber")),
				SourcePalletID = reader.GetString(reader.GetOrdinal("PickFromPalletID")),
				ArrivalDate = GetZDateTime(reader, "PickFromArrivalDate"),
				BondedEntryKey = reader.GetString(reader.GetOrdinal("PickFromBondedEntryKey")),
				TransferToAreaPK = reader.GetGuid(reader.GetOrdinal("TransferToAreaPK")),
				RetainPalletIDsInFixedPickFaces = reader.GetBoolean(reader.GetOrdinal("RetainPalletIDsInFixedPickFaces"))
			};

			return result;
		}

		static ZDateTime GetZDateTime(IDataReader reader, string columnName)
		{
			var dbValue = reader[columnName];
			var result = ZDateTime.Empty;
			if (dbValue != DBNull.Value)
			{
				result = (DateTime)dbValue;
			}

			return result;
		}

		#endregion

		#endregion

		#region ReplenishmentLocationInfo

		class ReplenishmentLocationInfo
		{
			public ZGuid WarehousePK { get; set; }
			public ZGuid PickToReplenishPK { get; set; }
			public ZGuid ClientPK { get; set; }
			public ZGuid ProductPK { get; set; }
			public ZGuid TransferToLocationPk { get; set; }
			public ZGuid TransferFromLocationPk { get; set; }
			public ZDecimal QuantityToTransfer { get; set; }
			public ZString SourcePalletID { get; set; }
			public ZDateTime ArrivalDate { get; set; }
			public ZDate ExpiryDate { get; set; }
			public ZDate PackingDate { get; set; }
			public ZString PartAttrib1 { get; set; }
			public ZString PartAttrib2 { get; set; }
			public ZString PartAttrib3 { get; set; }
			public ZString SerialNumber { get; set; }
			public ZString BondedEntryKey { get; set; }
			public ZGuid TransferToAreaPK { get; set; }
			public ZBool RetainPalletIDsInFixedPickFaces { get; set; }
		}

		#endregion
	}
}
