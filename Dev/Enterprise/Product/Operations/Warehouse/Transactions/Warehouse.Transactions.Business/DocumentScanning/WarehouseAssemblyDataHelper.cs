using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WarehouseAssemblyDataHelper
	{
		#region AddDateTimeOffsetFilters

		public static void AddDateTimeOffsetFilters(ZQuery query, SchemaDateTimeOffsetColumn dateTimeOffsetColumn, ZDate fromDate, ZDate toDate)
		{
			if (!fromDate.IsEmpty)
			{
				var sql = $"CAST({dateTimeOffsetColumn.Name} AS Date) >= @fromDate";
				var parameters = new ZSqlParameterCollection();
				parameters.Add("@fromDate", fromDate, WhsDocketSchema.WD_SystemCreateTimeUtc); // we use WD_SystemCreateTimeUtc here because we want the param to be a DateTime only, without time span
				query.AddFilterAndZSQLParameterCollection(sql, parameters);
			}
			if (!toDate.IsEmpty)
			{
				var sql = $"CAST({dateTimeOffsetColumn.Name} AS Date) < @toDate";
				var parameters = new ZSqlParameterCollection();
				parameters.Add("@toDate", toDate.AddDays(1), WhsDocketSchema.WD_SystemCreateTimeUtc);
				query.AddFilterAndZSQLParameterCollection(sql, parameters);
			}
		}

		#endregion
	}
}
