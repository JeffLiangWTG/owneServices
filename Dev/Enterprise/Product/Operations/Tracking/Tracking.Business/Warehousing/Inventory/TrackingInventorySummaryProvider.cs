using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public static class TrackingInventorySummaryProvider
	{
		public static IEnumerable<TrackingInventorySummary> GetSummaries(BusinessObjectFactory factory, ZQuery inventoryFilter)
		{
			var data = new DynamicBusinessObjectCollection(factory);

			var maxRows = inventoryFilter.MaximumRows;
			var topClause = inventoryFilter.MaximumRows > 0 ? $"TOP {inventoryFilter.MaximumRows}" : string.Empty; // SQL string

			var orderBySql = inventoryFilter.OrderBy;
			var orderByClause = !string.IsNullOrEmpty(orderBySql) ? $@"ORDER BY {orderBySql}" : string.Empty; // SQL string

			var parameterisedWhereSql = inventoryFilter.FilterString;
			var whereClause = !string.IsNullOrEmpty(parameterisedWhereSql) ? $"WHERE {parameterisedWhereSql}" : string.Empty;

			var sql = $@"
SELECT
		{topClause}
		WI_PK,
		WI_ArrivalDate,
		WI_Currency,
		WI_OH_Client,
		WI_WW_Whs,
		WI_OP,
		WI_WD,
		WI_UnitsUQ,
		WI_ClientUQ,
		WI_WE_InDocketLine,
		WI_InventoryStatus,
		WI_TotalUnits,
		WI_AvailableUnits,
		WI_CommittedUnits,
		WI_CrossDockQuantity
FROM
	(SELECT 
		WI_PK,
		WI_ArrivalDate,
		WI_Currency,
		WI_OH_Client,
		WI_WW_Whs,
		WI_OP,
		WI_WD,
		WI_UnitsUQ,
		WI_ClientUQ,
		WI_WE_InDocketLine,
		WI_InventoryStatus,
		ROW_NUMBER() OVER (PARTITION BY WI_OH_Client, WI_WW_Whs, WI_OP, WI_UnitsUQ, WI_ClientUQ ORDER BY WI_ArrivalDateOrETA, WI_PK) AS RowNumber,
		SUM(WI_TotalUnits) OVER (PARTITION BY WI_OH_Client, WI_WW_Whs, WI_OP, WI_UnitsUQ, WI_ClientUQ) AS WI_TotalUnits,
		SUM(WI_AvailableUnits) OVER (PARTITION BY WI_OH_Client, WI_WW_Whs, WI_OP, WI_UnitsUQ, WI_ClientUQ) AS WI_AvailableUnits,
		SUM (WI_CommittedUnits) OVER (PARTITION BY WI_OH_Client, WI_WW_Whs, WI_OP, WI_UnitsUQ, WI_ClientUQ) AS WI_CommittedUnits,
		SUM (WI_CrossDockQuantity) OVER (PARTITION BY WI_OH_Client, WI_WW_Whs, WI_OP, WI_UnitsUQ, WI_ClientUQ) AS WI_CrossDockQuantity
	FROM
		{WhsTrackingInventorySummaryItemViewSchema.Constants.SqlSchemaName}.{WhsTrackingInventorySummaryItemViewSchema.Constants.TableName}
	{whereClause}) inventories
WHERE
	RowNumber=1
{orderByClause}";

			data.Load(sql, new ZSqlParameterCollection(inventoryFilter.Params));
			inventoryFilter.OrderBy = string.Empty;

			return data.Select(d => new TrackingInventorySummary(d, inventoryFilter)).ToArray();
		}

		public static TrackingInventorySummary GetEmptySummary(BusinessObjectFactory factory, ZQuery filter = null)
		{
			var table = new DataTable(WhsTrackingInventorySummaryItemViewSchema.Constants.TableName);
			foreach (var column in WhsTrackingInventorySummaryItemViewSchema.All)
			{
				table.Columns.Add(column.Name, column.DotNetType);
			}
			var emptyData = new DynamicBusinessObject(factory, table.NewRow());
			return new TrackingInventorySummary(emptyData, filter ?? ZQuery.NoResultQuery);
		}
	}
}
