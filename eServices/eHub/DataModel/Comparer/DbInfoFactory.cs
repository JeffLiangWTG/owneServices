using CargoWise.eHub.DataModel.Comparer.DBInfo;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.DataModel.Comparer
{
	internal class DbInfoFactory : IDbInfoFactory
	{
		public IList<ITableInfo> GetAllEfTablesWithColInfo(DbContext context, string[] filterTables)
		{
			var efDecoder = new EntityMetadataDecoder(Assembly.GetAssembly(context.GetType()));
			return efDecoder.DecodeTable(context, filterTables);
		}

		public IList<ITableInfo> GetSQLTablesWithColInfo(string connectionString, string[] filterTables)
		{
			var allTablesAndCol = SqlTableColumnDefinition.GetSqlTablesAndColumns(connectionString, filterTables);

			var tableInfos = from tableGroup in allTablesAndCol.GroupBy(x => x.TableName)
				let schemaName = tableGroup.First().SchemaName
				select (new TableInfo(tableGroup.Key, schemaName, null, tableGroup.Select(y => new ColumnInfo(y.ColumnName, y.SqlTypeName, y.IsNullable, referencedTableName: y.ReferencedTableName)).ToList<IColumnInfo>()));
			return tableInfos.ToList<ITableInfo>();
		}
	}
}
