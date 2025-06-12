using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CargoWise.eHub.DataModel.Comparer.DBInfo;

namespace CargoWise.eHub.DataModel.Comparer
{
	public class SqlDbComparer
	{
		DbContext sourceDbContext;
		DbContext targetDbContext;
		private IDbInfoFactory dbInfoFactory;
		private string TargetConnectionString => targetDbContext?.Database.Connection.ConnectionString;
		private string SourceConnectionString => sourceDbContext.Database.Connection.ConnectionString;
		private string TargetDbName => targetDbContext?.Database.Connection.Database;
		private string SourceDbName => sourceDbContext.Database.Connection.Database;

		public SqlDbComparer(DbContext sourceDbContext, DbContext targetDbContext)
		{
			this.sourceDbContext = sourceDbContext;
			this.targetDbContext = targetDbContext;
			dbInfoFactory = new DbInfoFactory();
		}

		public List<CompareResult> CompareTablesBothExisting(Dictionary<string, List<string>> columnsToExcludeFromTables = null)
		{
			var sourceDicSqlInfos = dbInfoFactory.GetSQLTablesWithColInfo(SourceConnectionString, null);
			var targetDicSqlInfos = dbInfoFactory.GetSQLTablesWithColInfo(TargetConnectionString, null);

			return CompareSqlDatabases(sourceDicSqlInfos, targetDicSqlInfos, columnsToExcludeFromTables);
		}

		public List<CompareResult> CompareTables(Dictionary<string, string> sourceTargetTables, Dictionary<string, List<string>> columnsToExcludeFromSourceTables = null)
		{
			var sourceDicSqlInfos = dbInfoFactory.GetSQLTablesWithColInfo(SourceConnectionString, null);

			return CompareSqlTables(sourceDicSqlInfos, sourceTargetTables, columnsToExcludeFromSourceTables);
		}

		List<CompareResult> CompareSqlDatabases(IList<ITableInfo> sourceSqlInfoDictionary, IList<ITableInfo> targetSqlInfoDictionary, Dictionary<string, List<string>> columnsToExcludeFromTables)
		{
			var tableResult = new List<CompareResult>();

			foreach (var sourceSqlInfo in sourceSqlInfoDictionary)
			{
				var errorList = new List<string>();
				tableResult.Add(new CompareResult(sourceSqlInfo.TableName, errorList));

				var sqlTableInfo = targetSqlInfoDictionary.FirstOrDefault(x => x.CombinedName.Equals(sourceSqlInfo.CombinedName));
				if (sqlTableInfo ==  null) continue;

				CompareTables(sourceSqlInfo, sqlTableInfo, columnsToExcludeFromTables, errorList);
			}

			return tableResult;
		}

		List<CompareResult> CompareSqlTables(IList<ITableInfo> sqlInfoDictionary, Dictionary<string, string> sourceTargetTables, Dictionary<string, List<string>> columnsToExcludeFromTables)
		{
			var tableResult = new List<CompareResult>();

			foreach (var sourceTargetTable in sourceTargetTables)
			{
				var sourceTableName = sourceTargetTable.Key;
				var targetTableName = sourceTargetTable.Value;
				var errorList = new List<string>();
				tableResult.Add(new CompareResult(sourceTableName, errorList));

				var sqlSourceTableInfo = sqlInfoDictionary.FirstOrDefault(x => x.TableName.Equals(sourceTableName));
				var sqlTargetTableInfo = sqlInfoDictionary.FirstOrDefault(x => x.TableName.Equals(targetTableName));
				if (sqlTargetTableInfo == null) continue;

				CompareTables(sqlSourceTableInfo, sqlTargetTableInfo, columnsToExcludeFromTables, errorList);
			}

			return tableResult;
		}

		void CompareTables(ITableInfo sourceTableInfo, ITableInfo targetTableInfo, Dictionary<string, List<string>> columnsToExcludeFromTables, List<string> errorList)
		{
			var sqlTargetColsDict = targetTableInfo.ColumnInfos.ToDictionary(x => x.ColumnName);
			foreach (var sourceSqlCol in sourceTableInfo.ColumnInfos)
			{
				if (!sqlTargetColsDict.ContainsKey(sourceSqlCol.ColumnName))
					errorList.Add(
						$"Missing Column: The SQL database [{TargetDbName ?? SourceDbName}] table {targetTableInfo.CombinedName} does not contain a column called {sourceSqlCol.ColumnName}.");
				else
				{
					var sqlCol = sqlTargetColsDict[sourceSqlCol.ColumnName];
					sqlTargetColsDict.Remove(sourceSqlCol.ColumnName);

					if (columnsToExcludeFromTables != null && columnsToExcludeFromTables.TryGetValue(sourceTableInfo.CombinedName, out List<string> columnsToExclude) &&
					    columnsToExclude != null && columnsToExclude.Contains(sourceSqlCol.ColumnName))
					{
						continue;
					}

					CheckColumn(sqlCol, sourceSqlCol, sourceTableInfo.CombinedName, errorList);
				}
			}
		}

		private void CheckColumn(IColumnInfo targetCol, IColumnInfo sourceCol, string combinedName, List<string> errorList)
		{
			if (targetCol.TypeName != sourceCol.TypeName)
				errorList.Add($"Column Type: The SQL database column {combinedName}.{sourceCol.ColumnName} type in [{SourceDbName}] and [{TargetDbName}] are not matched. Type in [{TargetDbName}] = {targetCol.TypeName}, Type in [{SourceDbName}] = {sourceCol.TypeName}.");

			if (targetCol.IsNullable != sourceCol.IsNullable)
				errorList.Add(string.Format("Column Nullable: SQL database column {0}.{1} nullability in [{2}] and [{3}] are not matched. [{2}] is {4}NULL, [{3}] is {5}NULL.",
					combinedName, sourceCol.ColumnName,
					SourceDbName,
					TargetDbName,
					sourceCol.IsNullable ? "" : "NOT ",
					targetCol.IsNullable ? "" : "NOT "));
		}
	}
}
