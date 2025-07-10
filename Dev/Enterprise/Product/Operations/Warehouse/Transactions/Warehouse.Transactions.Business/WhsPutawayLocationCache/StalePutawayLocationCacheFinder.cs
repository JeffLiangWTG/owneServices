using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transactions.Business
{
	class StalePutawayLocationCacheFinder : IStalePutawayLocationCacheFinder
	{
		static class Schema
		{
			public const string TempTableName = "#StaleLocationCachesByProduct";
		}

		public IEnumerable<WhsPutawayLocationCacheInfo> FindCacheEntriesThatNeedUpdating(BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, ProductWarehouseTypes);
			query.AddToFilter(WhsWarehouseSchema.WW_IsVirtualWarehouse, false);
			query.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);

			var connection = ((IDbConnected)factory).Connection;
			var minCacheEntry = new Lazy<DateTime?>(() => GetMinCacheEntry(connection));
			var warehouses = factory.Load<WhsWarehouse>(query);
			WhsPutawayLocationCacheInfo[] result;

			using (warehouses.Length > 0 ? PopulateProductLocationTempTable(connection, minCacheEntry.Value) : null)
			{
				result = GetLocationCacheInfos().ToArray();
			}

			const string unusedLocationsSQL = @"
SELECT
	DISTINCT WPC_WL_Location, WPC_WW_Warehouse
FROM
	dbo.WhsWarehouse
	JOIN dbo.WhsPutawayLocationCache ON WPC_WW_Warehouse = WW_PK
WHERE
	WW_WarehouseType NOT IN ('PRW', 'FTZ') OR WW_IsVirtualWarehouse = 1 OR WW_IsActive = 0";

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(unusedLocationsSQL);

			return result.Concat(GetUnusedLocationCacheInfos());

			IEnumerable<WhsPutawayLocationCacheInfo> GetLocationCacheInfos()
			{
				foreach (var warehouse in warehouses)
				{
					foreach (var locationPK in FindCacheEntriesThatNeedUpdatingCore(factory, warehouse.PK, minCacheEntry.Value))
					{
						yield return new WhsPutawayLocationCacheInfo(locationPK, warehouse.PK);
					}
				}
			}

			IEnumerable<WhsPutawayLocationCacheInfo> GetUnusedLocationCacheInfos()
			{
				foreach (var row in dynamicCollection)
				{
					yield return new WhsPutawayLocationCacheInfo((ZGuid)row[WhsPutawayLocationCacheSchema.Constants.WPC_WL_Location], (ZGuid)row[WhsPutawayLocationCacheSchema.Constants.WPC_WW_Warehouse]);
				}
			}
		}

		static ImmutableArray<string> ProductWarehouseTypes { get; } = ImmutableArray.Create(WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL Query")]
		static DateTime? GetMinCacheEntry(DbConnection connection)
		{
			return connection.ExecuteScalar(@"
SELECT
	DATEADD(minute, -1, MIN(WPC_SystemLastEditTimeUtc))
FROM
	dbo.WhsPutawayLocationCache") is DateTime dateTime ? dateTime : null;
		}

		static IDisposable PopulateProductLocationTempTable(DbConnection connection, DateTime? minCacheEntry)
		{
			return new DisposableAction(() => PopulateProductLocationTempTableCore(connection, minCacheEntry), () => DropTempTable(connection));

			static void PopulateProductLocationTempTableCore(DbConnection connection, DateTime? minCacheEntry)
			{
				// need to create the temp table separately as the second query will be invoked via a stored procedure
				// with parameters and the local temp table will no longer be in scope for the rest of the queries.
				connection.ExecuteNonQuery(Invariant($@"
CREATE TABLE {Schema.TempTableName}
(
	WE_WL UNIQUEIDENTIFIER NOT NULL,
	WLV_WW_Whs UNIQUEIDENTIFIER NOT NULL,
	INDEX [NR_UC__WLV_WW_Whs_WE_WL] UNIQUE CLUSTERED ([WLV_WW_Whs],[WE_WL])
);"));

				connection.ExecuteNonQuery(Invariant($@"
INSERT INTO {Schema.TempTableName}
SELECT DISTINCT
	WE_WL,
	WLV_WW_Whs
FROM
	dbo.OrgSupplierPart
	JOIN
	(
		SELECT
			WE_OP,
			WE_WL
		FROM
			dbo.WhsDocketLine
		WHERE
			(WE_DocketLineStatus = 'FIN' AND WE_StockOnHand > 0)

		UNION ALL

		SELECT
			WE_OP,
			WE_WL
		FROM
			dbo.WhsDocketLine
		WHERE
			(WE_DocketLineStatus NOT IN ('FIN', 'PFU', 'CAN') AND WE_TransactionQuantity > 0 AND WE_WL IS NOT NULL AND WE_DocketLineType <> 'ORD')
	) as DocketLines ON WE_OP = OP_PK
	JOIN dbo.WhsLocationView_DoNotUse WITH(NOEXPAND) ON WE_WL = WLV_PK
WHERE
	OP_SystemLastEditTimeUtc IS NOT NULL
	AND OP_SystemLastEditTimeUtc > @MinCacheEntry
	AND EXISTS
	(
		SELECT NULL
		FROM
			dbo.WhsPutawayLocationCache
		WHERE
			WPC_WL_Location = WE_WL
			AND WPC_SystemLastEditTimeUtc < DATEADD(minute, 1, OP_SystemLastEditTimeUtc)
			AND WPC_SystemLastEditTimeUtc >= @MinCacheEntry
	)
	AND WLV_IsValidLocationForProductWarehousePutaway = 1

UPDATE STATISTICS {Schema.TempTableName};"), command => command.AddParameterBasedOnDbColumn("@MinCacheEntry", (object)minCacheEntry ?? DBNull.Value, OrgSupplierPartSchema.OP_SystemLastEditTimeUtc));
			}

			static void DropTempTable(DbConnection connection)
			{
				connection.ExecuteNonQuery(Invariant($"DROP TABLE IF EXISTS {Schema.TempTableName}"));
			}
		}

		public IEnumerable<ZGuid> FindCacheEntriesThatNeedUpdating(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			var connection = ((IDbConnected)factory).Connection;
			var minCacheEntry = GetMinCacheEntry(connection);
			using (PopulateProductLocationTempTable(connection, minCacheEntry))
			{
				return FindCacheEntriesThatNeedUpdatingCore(factory, warehousePK, minCacheEntry);
			}
		}

		IEnumerable<ZGuid> FindCacheEntriesThatNeedUpdatingCore(BusinessObjectFactory factory, ZGuid warehousePK, DateTime? minCacheEntry)
		{
			var sql = "EXEC WhsGetLocationsNeedTobeUpdatedForSpecificWhsPutawayLocationCache @WarehousePK = @WhsPK, @MinCacheEntry = @MinEntry";
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WhsPK", warehousePK, WhsLocationViewSchema.WLV_WW_Whs);
			sqlParams.Add("@MinEntry", minCacheEntry, WhsLocationSchema.WL_LastAllocatedOrChangedDateUtc);

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(sql, sqlParams);

			return dynamicCollection.Select(row => (ZGuid)row[WhsPutawayLocationCacheSchema.Constants.WPC_WL_Location]);
		}
	}
}
