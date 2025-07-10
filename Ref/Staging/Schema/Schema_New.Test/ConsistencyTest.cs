using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class ConsistencyTest
	{
		static readonly List<string> safeColumnExclusionList = new List<string>() { "ZZ1_PublishedDate", "ZZW_PublishedDate", "ZZI_ZZR_RateType" };
		static readonly List<string> columnInfoList = new List<string>() { "TABLE_NAME", "COLUMN_NAME", "IS_NULLABLE", "DATA_TYPE", "CHARACTER_MAXIMUM_LENGTH", "CHARACTER_OCTET_LENGTH", "NUMERIC_PRECISION", "NUMERIC_PRECISION_RADIX", "NUMERIC_SCALE", "DATETIME_PRECISION" };
		static readonly (string, string, string, string)[] columnMappingExceptions = new[] {
			("ZZT_ZZA_SecondTradeGroup", "RefCusTradeGroup", "ZZA_TradeGroup", "ZZT_ZZA_NKSecondTradeGroup"),
			("ZZT_ZZA_SecondTradeGroup", "RefCusTradeGroup", "ZZA_ZZZ_NKDataGrouping", "ZZT_ZZA_ZZZ_NKSecondDataGrouping") };
		static readonly List<string> safeColumnExclusionPatternList = new List<string>() { "%_SysStartTime", "%_SysEndTime", "%_DataSetPK", "%_DataSetCode" };
		readonly List<dynamic> safeDbColumnList = new List<dynamic>();
		readonly List<dynamic> stagingDbColumnList = new List<dynamic>();
		readonly List<dynamic> safeDbUniqueIndexList = new List<dynamic>();
		readonly List<dynamic> safeDbFKList = new List<dynamic>();

		[Test]
		public void TestConsistency()
		{
			var tableListInClause = "(" + string.Join(",", DataSetStructureProvider.StructuredDataSets.SelectMany(x => x).Select(x => "'" + x + "'").ToArray()) + ")";
			var columnInfoClause = string.Join(",", columnInfoList.Select(x => x).ToArray()) + ",ISNULL(COLUMN_DEFAULT, '('''')')";
			var exclusiveColumnPattern = string.Join(" ", safeColumnExclusionPatternList.Select(x => $"AND COLUMN_NAME NOT LIKE '{x}'"));

			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connString = TestConnectionString.GetAdmin(dbName);

			using (var connection = new SqlConnection(connString))
			{
				connection.Open();
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $@"SELECT {columnInfoClause} FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN {tableListInClause} {exclusiveColumnPattern}";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							dynamic columnInfo = new ExpandoObject();
							columnInfo.TableName = reader.GetString(0);
							columnInfo.ColumnName = reader.GetString(1);
							columnInfo.IsNullable = reader.GetString(2);
							columnInfo.DataType = reader.GetString(3);
							columnInfo.CharacterMaximumLength = reader.IsDBNull(4) ? int.MinValue : reader.GetInt32(4);
							columnInfo.CharacterOctetLength = reader.IsDBNull(5) ? int.MinValue : reader.GetInt32(5);
							columnInfo.NumericPrecision = reader.IsDBNull(6) ? byte.MinValue : reader.GetByte(6);
							columnInfo.NumericPrecisionRadix = reader.IsDBNull(7) ? short.MinValue : reader.GetInt16(7);
							columnInfo.NumericScale = reader.IsDBNull(8) ? int.MinValue : reader.GetInt32(8);
							columnInfo.DatetimePrecision = reader.IsDBNull(9) ? short.MinValue : reader.GetInt16(9);
							columnInfo.ColumnDefault = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
							safeDbColumnList.Add(columnInfo);
						}
					}

					cmd.CommandText = $@"select t.[name] as [table_name],
substring(column_names, 1, len(column_names)-1) as [columns],
i.[name] as index_name
from sys.objects t
    left outer join sys.indexes i
        on t.object_id = i.object_id
    cross apply (select col.[name] + ', '
        from sys.index_columns ic
            inner join sys.columns col
                on ic.object_id = col.object_id
                and ic.column_id = col.column_id
        where ic.object_id = t.object_id
            and ic.index_id = i.index_id
                order by col.column_id
                for xml path ('') ) D (column_names)
where is_unique = 1
and t.is_ms_shipped <> 1
and t.[type] = 'U' and i.[name] not like 'PK_%'
and t.[name] in  {tableListInClause}";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							dynamic indexInfo = new ExpandoObject();
							indexInfo.TableName = reader.GetString(0);
							indexInfo.IndexColumnNames = reader.GetString(1);
							indexInfo.IndexName = reader.GetString(2);
							safeDbUniqueIndexList.Add(indexInfo);
						}
					}

					cmd.CommandText = $@"SELECT OBJECT_NAME(fk.parent_object_id) TableName,OBJECT_NAME(fk.referenced_object_id), c.name, c2.name, t.name FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE OBJECT_NAME(fk.parent_object_id) in  {tableListInClause}";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							dynamic fkInfo = new ExpandoObject();
							fkInfo.TableName = reader.GetString(0);
							fkInfo.ReferredTableName = reader.GetString(1);
							fkInfo.ColumnName = reader.GetString(2);
							fkInfo.ReferredColumnName = reader.GetString(3);
							fkInfo.ReferredColumnType = reader.GetString(4);
							safeDbFKList.Add(fkInfo);
						}
					}
				}
			}

			dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			connString = TestConnectionString.GetAdmin(dbName);

			using (var connection = new SqlConnection(connString))
			{
				connection.Open();
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = $@"SELECT {columnInfoClause} FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN {tableListInClause}";

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							dynamic columnInfo = new ExpandoObject();
							columnInfo.TableName = reader.GetString(0);
							columnInfo.ColumnName = reader.GetString(1);
							columnInfo.IsNullable = reader.GetString(2);
							columnInfo.DataType = reader.GetString(3);
							columnInfo.CharacterMaximumLength = reader.IsDBNull(4) ? int.MinValue : reader.GetInt32(4);
							columnInfo.CharacterOctetLength = reader.IsDBNull(5) ? int.MinValue : reader.GetInt32(5);
							columnInfo.NumericPrecision = reader.IsDBNull(6) ? byte.MinValue : reader.GetByte(6);
							columnInfo.NumericPrecisionRadix = reader.IsDBNull(7) ? short.MinValue : reader.GetInt16(7);
							columnInfo.NumericScale = reader.IsDBNull(8) ? int.MinValue : reader.GetInt32(8);
							columnInfo.DatetimePrecision = reader.IsDBNull(9) ? short.MinValue : reader.GetInt16(9);
							columnInfo.ColumnDefault = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
							stagingDbColumnList.Add(columnInfo);
						}
					}
				}
			}

			foreach (var group in safeDbColumnList.GroupBy(x => x.TableName))
			{
				foreach (var column in group)
				{
					var safeColumnName = column.ColumnName;
					if (safeColumnExclusionList.Contains(safeColumnName) || column.TableName == "RefLanguageType" || column.TableName == "RefDataGrouping")
					{
						break;
					}
					if (stagingDbColumnList.Any(x => x.ColumnName == column.ColumnName))
					{
						if (!safeColumnName.Contains("_NK") && (safeColumnName.IndexOf("_") != safeColumnName.LastIndexOf("_")))
						{
							var referredTablePrefix = safeColumnName.Substring(safeColumnName.IndexOf("_") + 1, safeColumnName.LastIndexOf("_") - safeColumnName.IndexOf("_") - 1);
							string referredTable = safeDbColumnList.First(x => x.ColumnName.StartsWith(referredTablePrefix)).TableName;
							Assert.True(DataSetStructureProvider.StructuredDataSets.Any(x => x.Contains(referredTable) && x.Contains((string)column.TableName)), $@"{column.TableName}  is allowed only when {referredTable} and {column.TableName} are in the same data set (defined in data set.tt).
If they are not in the same data set, {column.ColumnName} should be converted to natural key");
						}
						var stagingColumn = stagingDbColumnList.First(x => x.ColumnName == safeColumnName);
						CompareColumn(stagingColumn, column, true);
					}
					else
					{
						CheckNaturalKeyReferenceColumns(safeColumnName, column.TableName, safeColumnName.Substring(0, safeColumnName.IndexOf("_")));
					}
				}
			}
		}

		void CheckNaturalKeyReferenceColumns(string fkColumnName, string originalTable, string originalTablePrefix)
		{
			var columnNameWithoutPrefix = fkColumnName.Substring(fkColumnName.IndexOf("_") + 1);
			var referredTable = "";
			var referredTablePrefix = "";
			if (columnMappingExceptions.Any(x => x.Item1 == fkColumnName))
			{
				referredTable = columnMappingExceptions.First(x => x.Item1 == fkColumnName).Item2;
			}
			else if (safeDbFKList.Any(x => x.ColumnName == fkColumnName && x.ReferredColumnType == "uniqueidentifier"))
			{
				columnNameWithoutPrefix = safeDbFKList.First(x => x.ColumnName == fkColumnName).ReferredColumnName;
				referredTablePrefix = columnNameWithoutPrefix.Substring(0, columnNameWithoutPrefix.IndexOf("_"));
				referredTable = safeDbFKList.First(x => x.ColumnName == fkColumnName).ReferredTableName;
			}
			else
			{
				referredTablePrefix = columnNameWithoutPrefix.Substring(0, columnNameWithoutPrefix.IndexOf("_"));
				referredTable = safeDbColumnList.First(x => x.ColumnName.StartsWith(referredTablePrefix)).TableName;
			}
			if (DataSetStructureProvider.StructuredDataSets.Where(x => x.First() != originalTable).Any(x => x.Contains(referredTable)))
			{
				var indexColumns = safeDbUniqueIndexList.First(x => x.TableName == referredTable).IndexColumnNames.Split(',');
				foreach (var indexColumn in indexColumns)
				{
					var trimmedIndexColumn = indexColumn.Trim();
					var correctColumnName = originalTablePrefix + "_";
					if (columnMappingExceptions.Any(x => (x.Item1 == fkColumnName) && (x.Item3 == trimmedIndexColumn)))
					{
						correctColumnName = columnMappingExceptions.First(x => (x.Item1 == fkColumnName) && (x.Item3 == trimmedIndexColumn)).Item4;
					}
					else if (trimmedIndexColumn.Contains("_NK"))
					{
						correctColumnName += trimmedIndexColumn;
					}
					else if (trimmedIndexColumn.IndexOf("_") == trimmedIndexColumn.LastIndexOf("_"))
					{
						correctColumnName += trimmedIndexColumn.Insert(trimmedIndexColumn.LastIndexOf("_") + 1, "NK");
					}
					else
					{
						CheckNaturalKeyReferenceColumns(trimmedIndexColumn, originalTable, originalTablePrefix + "_" + referredTablePrefix);
						break;
					}
					if (stagingDbColumnList.Any(x => x.ColumnName == correctColumnName))
					{
						var stagingColumn = stagingDbColumnList.First(x => x.ColumnName == correctColumnName);
						var safeColumn = safeDbColumnList.First(x => x.ColumnName == trimmedIndexColumn);
						CompareColumn(stagingColumn, safeColumn, false);
					}
					else
					{
						Assert.Fail($"{correctColumnName} missing from Staging Database");
					}
				}
			}
			else
			{
				Assert.Fail($"{fkColumnName} missing from Staging Database");
			}
		}

		void CompareColumn(dynamic stagingColumn, dynamic safeColumn, bool checkNullable)
		{
			if (checkNullable)
			{
				Assert.AreEqual(stagingColumn.IsNullable, safeColumn.IsNullable, $"{safeColumn.ColumnName} in {safeColumn.TableName} has IS_NULLABLE = {safeColumn.IsNullable}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.IsNullable}");
				Assert.AreEqual(stagingColumn.ColumnDefault, safeColumn.ColumnDefault, $"{safeColumn.ColumnName} in {safeColumn.TableName} has COLUMN_DEFAULT = {safeColumn.ColumnDefault}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.ColumnDefault}");
			}
			Assert.AreEqual(stagingColumn.DataType, safeColumn.DataType, $"{safeColumn.ColumnName} in {safeColumn.TableName} has DATA_TYPE = {safeColumn.DataType}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.DataType}");
			Assert.AreEqual(stagingColumn.CharacterMaximumLength, safeColumn.CharacterMaximumLength, $"{safeColumn.ColumnName} in {safeColumn.TableName} has CHARACTER_MAXIMUM_LENGTH = {safeColumn.CharacterMaximumLength}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.CharacterMaximumLength}");
			Assert.AreEqual(stagingColumn.NumericPrecision, safeColumn.NumericPrecision, $"{safeColumn.ColumnName} in {safeColumn.TableName} has CHARACTER_OCTET_LENGTH = {safeColumn.NumericPrecision}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.NumericPrecision}");
			Assert.AreEqual(stagingColumn.NumericPrecisionRadix, safeColumn.NumericPrecisionRadix, $"{safeColumn.ColumnName} in {safeColumn.TableName} has NUMERIC_PRECISION = {safeColumn.NumericPrecisionRadix}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.NumericPrecisionRadix}");
			Assert.AreEqual(stagingColumn.NumericScale, safeColumn.NumericScale, $"{safeColumn.ColumnName} in {safeColumn.TableName} has NUMERIC_PRECISION_RADIX = {safeColumn.NumericScale}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.NumericScale}");
			Assert.AreEqual(stagingColumn.DatetimePrecision, safeColumn.DatetimePrecision, $"{safeColumn.ColumnName} in {safeColumn.TableName} has NUMERIC_SCALE = {safeColumn.DatetimePrecision}, but staging column {stagingColumn.ColumnName} in {stagingColumn.TableName} is {stagingColumn.DatetimePrecision}");
		}
	}
}
