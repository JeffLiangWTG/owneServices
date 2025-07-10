using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Tools.Common
{
	public class DataSetHelper : IDataSetHelper
	{
		public DataSetHelper(IDbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));
			this.connection = connection;
		}

		readonly IDbConnection connection;
		public Dictionary<string, List<string>> GetAllTableAndColumns()
		{
			var tableAndColumns = new Dictionary<string, List<string>>();
			using (var command = connection.CreateCommand())
			{
				command.CommandText = TableAndColumnsSql;
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var tableName = (string)reader[0];
						var columnName = (string)reader[1];
						if (!tableAndColumns.ContainsKey(tableName))
						{
							tableAndColumns[tableName] = new List<string>();
						}
						tableAndColumns[tableName].Add(columnName);
					}
				}
			}
			return tableAndColumns;
		}

		public Dictionary<string, List<FKRelationship>> GetAllTableAndFKs()
		{
			var tableAndFKs = new Dictionary<string, List<FKRelationship>>();
			using (var command = connection.CreateCommand())
			{
				command.CommandText = TableAndFKsSql;
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var table = (string)reader[0];
						var referencedTable = (string)reader[1];
						if (table == referencedTable)
						{
							continue;
						}
						var fkRelation = new FKRelationship(table, referencedTable, (string)reader[2], (string)reader[3], (string)reader[4]);
						if (!tableAndFKs.ContainsKey(table))
						{
							tableAndFKs[table] = new List<FKRelationship>();
						}
						tableAndFKs[table].Add(fkRelation);
					}
				}
			}
			return tableAndFKs;
		}

		public static Tuple<string, string> GetPKAndTableCode(List<string> columnList)
		{
			Argument.NotNull(columnList, nameof(columnList));
			var pkColumn = columnList.FirstOrDefault(x => x.EndsWith("_PK", StringComparison.OrdinalIgnoreCase));
			Argument.NotNullOrEmpty(pkColumn, nameof(pkColumn));
			var code = pkColumn.Split('_')[0];
			return Tuple.Create(pkColumn, code);
		}

		public static bool ContainDataSetPKAndCodeColumns(List<string> columnList, out Tuple<string, string> dataSetColumns)
		{
			Argument.NotNull(columnList, nameof(columnList));
			dataSetColumns = null;
			var containDataSetColumns = columnList.Any(x => x.EndsWith("_DataSetPK", StringComparison.OrdinalIgnoreCase)) && columnList.Any(y => y.EndsWith("_DataSetCode", StringComparison.OrdinalIgnoreCase));
			if (containDataSetColumns)
			{
				var dataSetPKColumn = columnList.First(x => x.EndsWith("_DataSetPK", StringComparison.OrdinalIgnoreCase));
				var dataSetCodeColumn = columnList.First(x => x.EndsWith("_DataSetCode", StringComparison.OrdinalIgnoreCase));
				dataSetColumns = Tuple.Create(dataSetPKColumn, dataSetCodeColumn);
			}

			return containDataSetColumns;
		}

		public static bool ContainParentPKAndCodeColumns(List<string> columnList, out Tuple<string, string> parentPKAndCodeColumns)
		{
			Argument.NotNull(columnList, nameof(columnList));
			parentPKAndCodeColumns = null;
			var containPKAndCodeColumns = columnList.Any(x => x.EndsWith("_ParentPK", StringComparison.OrdinalIgnoreCase)) && columnList.Any(y => y.EndsWith("_ParentCode", StringComparison.OrdinalIgnoreCase));
			if (containPKAndCodeColumns)
			{
				var parentPKColumn = columnList.First(x => x.EndsWith("_ParentPK", StringComparison.OrdinalIgnoreCase));
				var parentCodeColumn = columnList.First(x => x.EndsWith("_ParentCode", StringComparison.OrdinalIgnoreCase));
				parentPKAndCodeColumns = Tuple.Create(parentPKColumn, parentCodeColumn);
			}
			else if (columnList.Any(x => x.EndsWith("_ParentId", StringComparison.OrdinalIgnoreCase)) && columnList.Any(y => y.EndsWith("_ParentTableCode", StringComparison.OrdinalIgnoreCase)))
			{
				containPKAndCodeColumns = true;
				var parentPKColumn = columnList.First(x => x.EndsWith("_ParentId", StringComparison.OrdinalIgnoreCase));
				var parentCodeColumn = columnList.First(x => x.EndsWith("_ParentTableCode", StringComparison.OrdinalIgnoreCase));
				parentPKAndCodeColumns = Tuple.Create(parentPKColumn, parentCodeColumn);
			}

			return containPKAndCodeColumns;
		}

		const string TableAndColumnsSql = @"SELECT tab.name, col.name FROM sys.tables tab 
INNER JOIN sys.columns col ON tab.object_id = col.object_id
WHERE tab.name NOT LIKE '%History' ORDER BY tab.name;";

		const string TableAndFKsSql = @"SELECT OBJECT_NAME(fk.parent_object_id) TableName,OBJECT_NAME(fk.referenced_object_id), c.name, c2.name, t.name FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id ORDER BY TableName;";
	}
}
