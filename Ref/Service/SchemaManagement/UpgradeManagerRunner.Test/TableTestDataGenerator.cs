using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class TableInfoCollection
	{
		List<TableInfo> tableInfos = new List<TableInfo>();
		public TableInfoCollection(List<(string tableName, string ColumnName, string dataType, bool isPrimaryKey, string foreignTable, string ForeignColumn)> tableAndColumnDetails)
		{
			Action<(string tableName, string ColumnName, string dataType, bool isPrimaryKey, string foreignTable, string ForeignColumn)> addOrUpdateDictionary = tableAndColumnDetail =>
			{
				if (tableInfos.Exists(x => x.TableName == tableAndColumnDetail.tableName))
				{
					tableInfos.First(x => x.TableName == tableAndColumnDetail.tableName).columns.Add(
							new ColumnInfo()
							{
								ColumnName = tableAndColumnDetail.ColumnName,
								DataType = tableAndColumnDetail.dataType,
								ForeignColumnName = tableAndColumnDetail.ForeignColumn,
								ForeignKeyTable = tableAndColumnDetail.foreignTable,
								isPrimaryKey = tableAndColumnDetail.isPrimaryKey,
								Pk = tableAndColumnDetail.isPrimaryKey ? Guid.NewGuid() : Guid.Empty
							}
						);
				}
				else
				{
					tableInfos.Add(
						new TableInfo()
						{
							TableName = tableAndColumnDetail.tableName,
							columns = new List<ColumnInfo>()
						{
							new ColumnInfo()
							{
							ColumnName = tableAndColumnDetail.ColumnName,
							DataType = tableAndColumnDetail.dataType,
							ForeignColumnName = tableAndColumnDetail.ForeignColumn,
							ForeignKeyTable = tableAndColumnDetail.foreignTable,
							isPrimaryKey = tableAndColumnDetail.isPrimaryKey,
							Pk = tableAndColumnDetail.isPrimaryKey ? Guid.NewGuid() : Guid.Empty
							}
						}
						});
				}
			};
			tableAndColumnDetails.ForEach(x => addOrUpdateDictionary(x));
			AddSortIndex();
		}

		string GetColumnValue(ColumnInfo column, string operation)
		{
			switch (column.DataType)
			{
				case "char":
				case "varchar":
				case "nvarchar":
					return operation == "Insert" ? "'X'" : "'Y'";
				case "bit":
				case "bigint":
				case "decimal":
				case "int":
				case "tinyint":
				case "smallint":
				case "binary":
				case "varbinary":
					return operation == "Insert" ? "1" : "0";
				case "date":
				case "datetime":
				case "datetime2":
				case "smalldatetime":
					return operation == "Insert" ? "'1900-01-01'" : "'2000-01-01'";
				case "geography":
					return operation == "Insert" ? "'POINT(1 1)'" : "'POINT(2 2)'";
				case "uniqueidentifier":
					if (string.IsNullOrEmpty(column.ForeignColumnName))
					{
						return $"'{column.Pk.ToString()}'";
					}
					else
					{
						return $"'{tableInfos.First(x => x.TableName == column.ForeignKeyTable).columns.First(x => x.isPrimaryKey).Pk.ToString()}'";
					}
				default:
					return string.Empty;
			}
		}

		void AddSortIndex()
		{
			var hasSortIndex = new List<string>();
			while (tableInfos.Any(x => x.SortIndex == -1))
			{
				var indexValue = tableInfos.Max(x => x.SortIndex) + 1;
				Action<TableInfo> setSortIndex = table =>
				{
					if (table.columns.All(x => x.ForeignKeyTable == null || (hasSortIndex.Contains(x.ForeignKeyTable) && tableInfos.First(t => t.TableName == x.ForeignKeyTable).SortIndex < indexValue) || table.TableName == x.ForeignKeyTable))
					{
						hasSortIndex.Add(table.TableName);
						table.SortIndex = indexValue;
					}
				};
				tableInfos.Where(x => x.SortIndex == -1).ToList().ForEach(x => setSortIndex(x));
			}
		}

		public List<string> GetInsertStatements()
		{
			var result = new List<string>();
			Action<TableInfo> buildInsert = table =>
			{
				result.Add($"Insert into {table.TableName} ({GetCommaDelimetedColumnList(table.columns)}) Values ({GetCommaDelimetedValueList(table.columns)})");
			};
			tableInfos.OrderBy(x => x.SortIndex).ToList().ForEach(x => buildInsert(x));
			return result;
		}

		public List<string> GetDeleteStatements()
		{
			var result = new List<string>();
			Action<TableInfo> buildInsert = table =>
			{
				result.Add($"Delete from {table.TableName} Where {GetPrimaryKeyFieldName(table.columns)} = '{GetPrimaryKeyFieldValue(table.columns)}'");
			};
			tableInfos.Where(x => x.columns.Any(y => y.isPrimaryKey)).OrderByDescending(x => x.SortIndex).ToList().ForEach(x => buildInsert(x));
			return result;
		}

		public List<string> GetUpdateStatements()
		{
			var result = new List<string>();
			Action<TableInfo> buildInsert = table =>
			{
				var columnToUpdate = table.columns.First(x => x.isPrimaryKey == false);
				result.Add($"Update {table.TableName} Set {columnToUpdate.ColumnName} = {GetColumnValue(columnToUpdate, "Update")} Where {GetPrimaryKeyFieldName(table.columns)} = '{GetPrimaryKeyFieldValue(table.columns)}'");
			};
			tableInfos.Where(x => x.columns.Any(y => y.isPrimaryKey)).OrderBy(x => x.SortIndex).ToList().ForEach(x => buildInsert(x));
			return result;
		}

		string GetPrimaryKeyFieldName(List<ColumnInfo> columns)
		{
			return columns.First(x => x.isPrimaryKey).ColumnName;
		}

		string GetPrimaryKeyFieldValue(List<ColumnInfo> columns)
		{
			return columns.First(x => x.isPrimaryKey).Pk.ToString();
		}

		string GetCommaDelimetedColumnList(List<ColumnInfo> columns)
		{
			var result = string.Empty;
			Action<ColumnInfo> addColumn = column =>
			{
				if (!string.IsNullOrEmpty(result))
				{
					result += ",";
				}
				result += column.ColumnName;
			};
			columns.ForEach(x => addColumn(x));
			return result;
		}

		string GetCommaDelimetedValueList(List<ColumnInfo> columns)
		{
			var result = string.Empty;
			Action<ColumnInfo> addColumn = column =>
			{
				if (!string.IsNullOrEmpty(result))
				{
					result += ",";
				}
				result += GetColumnValue(column, "Insert");
			};
			columns.ForEach(x => addColumn(x));
			return result;
		}
	}

	class TableInfo
	{
		public string TableName;
		public List<ColumnInfo> columns = new List<ColumnInfo>();
		public int SortIndex = -1;
	}
	class ColumnInfo
	{
		public string ColumnName;
		public string DataType;
		public bool isPrimaryKey;
		public Guid Pk;
		public string ForeignKeyTable;
		public string ForeignColumnName;
	}
}
