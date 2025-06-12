using CargoWise.eHub.DataModel.Comparer.DBInfo;
using CargoWise.eHub.DataModel.IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CargoWise.eHub.DataModel.Comparer
{
	public class EFAndSQLDBComparer
	{
		DbContext dbContext;
		private IDbInfoFactory dbInfoFactory;
		private string connectionString;

		public EFAndSQLDBComparer(DbContext dbContext)
		{
			this.dbContext = dbContext;
			dbInfoFactory = new DbInfoFactory();
			connectionString = dbContext.Database.Connection.ConnectionString;
		}

		public List<CompareResult> CompareEFWithTable(string[] tablesToCompare, bool allowMissingColumnsInEF = false, Dictionary<string, List<string>> columnsToExcludeFromTables = null)
		{
			var dicSqlInfos = dbInfoFactory.GetSQLTablesWithColInfo(connectionString, tablesToCompare);
			var efInfos = dbInfoFactory.GetAllEfTablesWithColInfo(dbContext, tablesToCompare);

			if (efInfos.Count == 0)
				return new List<CompareResult>() { new CompareResult(string.Join(", ", tablesToCompare), new List<string>() { $"EF DB Context does not contain following tables: {string.Join(", ", tablesToCompare)}" }) };

			return CompareEfWithSql(efInfos, dicSqlInfos, allowMissingColumnsInEF, columnsToExcludeFromTables);
		}

		public CompareResult CompareEFWithTable(string tableToCompare, bool allowMissingColumnsInEF = true)
		{
			return CompareEFWithTable(new[] { tableToCompare }, allowMissingColumnsInEF).Single();
		}

		List<CompareResult> CompareEfWithSql(IList<ITableInfo> efInfos, IList<ITableInfo> sqlInfoDictionary, bool allowMissingColumnsInEF, Dictionary<string, List<string>> columnsToExcludeFromTables)
		{
			var tableResult = new List<CompareResult>();

			foreach (var efInfo in efInfos)
			{
				var errorList = new List<string>();
				tableResult.Add(new CompareResult(efInfo.TableName, errorList));

				if (sqlInfoDictionary.All(x => x.CombinedName != efInfo.CombinedName))
					errorList.Add(
						string.Format("Missing Table: The SQL database does not contain a table called {0}. Needed by EF class {1}.",
						efInfo.CombinedName, efInfo.ClrClassType.Name));
				else
				{
					var sqlTableInfo = sqlInfoDictionary.FirstOrDefault(x => x.CombinedName.Equals(efInfo.CombinedName));
					var sqlColsDict = sqlTableInfo.ColumnInfos.ToDictionary(x => x.ColumnName);

					foreach (var clrCol in efInfo.ColumnInfos)
					{
						if (!sqlColsDict.ContainsKey(clrCol.ColumnName))
							errorList.Add(
								string.Format("Missing Column: The SQL database table {0} does not contain a column called {1}. Needed by EF class {2}.",
								efInfo.CombinedName, clrCol.ColumnName, efInfo.ClrClassType.Name));
						else
						{
							var sqlCol = sqlColsDict[clrCol.ColumnName];
							sqlColsDict.Remove(clrCol.ColumnName);

							if (columnsToExcludeFromTables != null && columnsToExcludeFromTables.TryGetValue(efInfo.CombinedName, out List<string> columnsToExclude) &&
								columnsToExclude != null && columnsToExclude.Contains(clrCol.ColumnName))
							{
								continue;
							}

							CheckColumn(sqlCol, clrCol, efInfo.CombinedName, errorList);
						}
					}

					if (!allowMissingColumnsInEF && sqlColsDict.Any())
					{
						foreach (var missingCol in sqlColsDict.Values)
						{
							errorList.Add(string.Format("SQL database table {0} has a column called {1} (.NET type {2}) that EF does not access.",
								efInfo.CombinedName, missingCol.ColumnName, missingCol.TypeName));
						}
					}

					sqlInfoDictionary.Remove(sqlTableInfo);
				}
			}

			return tableResult;
		}

		private void CheckColumn(IColumnInfo sqlCol, IColumnInfo clrCol, string combinedName, List<string> errorList)
		{
			if (sqlCol.TypeName != clrCol.TypeName && !CompareExceptionalClrType(sqlCol.TypeName, clrCol))
					errorList.Add(
						string.Format("Column Type: The SQL database column {0}.{1} type does not match EF. SQL type = {2}, EF type = {3}.", combinedName, clrCol.ColumnName, sqlCol.TypeName, clrCol.TypeName));


			if (clrCol.ClrColumnType.IsValueType && sqlCol.IsNullable != clrCol.IsNullable)
				errorList.Add(string.Format("Column Nullable: SQL database column {0}.{1} nullability does not match. SQL is {2}NULL, EF is {3}NULL.",
					combinedName, clrCol.ColumnName,
					sqlCol.IsNullable ? "" : "NOT ",
					clrCol.IsNullable ? "" : "NOT "));

			if (sqlCol.ReferencedTableName != clrCol.ReferencedTableName)
			{
				errorList.Add($"Referenced Table Mismatch: column {combinedName}.{clrCol.ColumnName} refers to " +
					$"{(string.IsNullOrEmpty(sqlCol.ReferencedTableName) ? "NOTHING" : sqlCol.ReferencedTableName)} in SQL, but " +
					$"{(string.IsNullOrEmpty(clrCol.ReferencedTableName) ? "NOTHING" : clrCol.ReferencedTableName)} in EF.");
			}
		}

		bool CompareExceptionalClrType(string sqlType, IColumnInfo efColumnInfo)
		{
			if (ExceptionalClrTypes.Mappings.ContainsKey(sqlType))
			{
				var edmClrType = Nullable.GetUnderlyingType(efColumnInfo.ClrColumnType) ?? efColumnInfo.ClrColumnType;
				return edmClrType == ExceptionalClrTypes.Mappings[sqlType];
			}
			return false;
		}
	}

	public class CompareResult
	{
		public CompareResult(string tableName, List<string> differences)
		{
			TableName = tableName;
			Differences = differences;
		}

		public string TableName { get; private set; }

		public List<string> Differences { get; private set; }
	}
}
