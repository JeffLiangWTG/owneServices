using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace CargoWise.eHub.DataModel.Comparer.DBInfo
{
	public class SqlTableColumnDefinition
	{
		public string TableName { get; private set; }

		public string SchemaName { get; private set; }

		public string ColumnName { get; private set; }

		public string SqlTypeName { get; private set; }

		public bool IsNullable { get; private set; }

		public Int16 MaxLength { get; private set; }

		public bool IsComputed { get; private set; }

		public string ReferencedTableName { get; private set; }

		public override string ToString()
		{
			return $"[{TableName}].[{SchemaName}].{ColumnName}, Type: {SqlTypeName}, IsNullable: {IsNullable}, MaxLength: {MaxLength}, IsComputed: {IsComputed}, ReferencedTable: {ReferencedTableName}";
		}

		public static IList<SqlTableColumnDefinition> GetSqlTablesAndColumns(string connectionString, string[] filterTableList)
		{
			var filterTables = filterTableList == null || filterTableList.Length == 0 ? string.Empty : "'" + string.Join("','", filterTableList) + "'";
			var result = new Collection<SqlTableColumnDefinition>();
			using (var sqlcon = new SqlConnection(connectionString))
			{
				var command = sqlcon.CreateCommand();
				command.CommandText = @"SELECT t.name AS TableName,
SCHEMA_NAME(t.schema_id) AS SchemaName,
c.name AS ColumnName,
types.name AS SqlTypeName,
c.is_nullable AS IsNullable,
c.max_length AS MaxLength,
c.is_computed AS IsComputed,
rt.name AS ReferencedTableName
FROM sys.tables AS t
INNER JOIN sys.columns c ON t.OBJECT_ID = c.OBJECT_ID
INNER JOIN sys.types types ON c.system_type_id = types.system_type_id 
                           AND types.name <> 'sysname'
LEFT JOIN sys.foreign_key_columns fk ON c.object_id = fk.parent_object_id AND fk.parent_column_id = c.column_id
LEFT JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
" + (string.IsNullOrEmpty(filterTables) ? string.Empty : string.Format(" WHERE t.name IN ({0}) ", filterTables)) + "ORDER BY SchemaName, TableName";

				sqlcon.Open();
				using (var reader = command.ExecuteReader())
				{

					while (reader.Read())
					{
						var row = new SqlTableColumnDefinition();
						var i = 0;
						row.TableName = reader.GetString(i++);
						row.SchemaName = reader.GetString(i++);
						row.ColumnName = reader.GetString(i++);
						row.SqlTypeName = reader.GetString(i++);
						row.IsNullable = reader.GetBoolean(i++);
						row.MaxLength = reader.GetInt16(i++);
						row.IsComputed = reader.GetBoolean(i++);
						row.ReferencedTableName = reader.IsDBNull(i) ? null : reader.GetString(i);
						result.Add(row);
					}
				}
			}

			return result;
		}
	}
}
