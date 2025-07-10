using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RemoteDbSchemaFixture
	{
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void LockEscalationDisabledForRefCustariff(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			var tariffDataSet = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x.Contains("RefCusTariff"));
			var tariffTableNames = string.Join(", ", tariffDataSet.Select(x => $"'{x}'"));
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				var sql = $"select count(1) from sys.tables where name in ({tariffTableNames}) and lock_escalation_desc = 'DISABLE'";
				Assert.AreEqual(tariffDataSet.Length, TestDBHelper.ExecuteScalar(conn, sql), "LOCK_ESCALATION is disabled for tariff DataSet tables.");
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void NKDataGroupingShouldBeClusteredIndexed(DbSchema dbSchema)
		{
			var exclusionTables = new[] { nameof(RefCusVATApplicability) };
			var tariffDataSet = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x.Contains("RefCusTariff")).Except(exclusionTables);
			var tariffTableNames = string.Join(", ", tariffDataSet.Select(x => $"'{x}'"));

			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"
select col.name
from sys.columns col
join sys.tables tab on tab.object_id=col.object_id
join sys.indexes ix on ix.object_id=col.object_id
left join sys.index_columns ic on ix.object_id=ic.object_id and ix.index_id=ic.index_id and col.column_id=ic.column_id
where col.name like '%_ZZZ_NKDataGrouping' and tab.name in ({tariffTableNames}) and ix.type_desc='CLUSTERED' and ic.object_id is null;";
					var colNames = new List<string>();
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							colNames.Add(reader.GetString(0));
						}
					}
					var missingColumns = string.Join(", ", colNames);
					Assert.AreEqual(0, colNames.Count, $"NKDataGrouping column should be Clustered Indexed. Missing NKDataGrouping column(s): {missingColumns}");
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void NKDataGroupingShouldBeFirstIndexed(DbSchema dbSchema)
		{
			var tariffDataSet = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x.Contains("RefCusTariff"));
			var excludedTables = new[] { nameof(RefCusRate), nameof(RefCusCondition), nameof(RefCusTariffUOM) };
			var tariffTableNames = string.Join(", ", tariffDataSet.Except(excludedTables).Select(x => $"'{x}'"));

			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"
select ix.name
from sys.columns col
join sys.tables tab on tab.object_id=col.object_id
join sys.indexes ix on ix.object_id=col.object_id
left join sys.index_columns ic on ix.object_id=ic.object_id and ix.index_id=ic.index_id and col.column_id=ic.column_id
where col.name like '%_ZZZ_NKDataGrouping' and tab.name in ({tariffTableNames}) and ic.object_id is not null and ic.key_ordinal>1;";
					var indexNames = new List<string>();
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							indexNames.Add(reader.GetString(0));
						}
					}
					var incorrectIndexes = string.Join(", ", indexNames);
					Assert.AreEqual(0, indexNames.Count, $"NKDataGrouping column should be the first column in Indexes. Incorrect Index(es): {incorrectIndexes}");
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void FKColumnShouldBeIndexed_RefCusTariff(DbSchema dbSchema)
		{
			var tariffDataSet = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x.Contains("RefCusTariff"));
			var tariffTableNames = string.Join(", ", tariffDataSet.Select(x => $"'{x}'"));
			var excludedColumns = new[] { "ZX1_ZY7_NKConditionCode" };

			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"
SELECT 
	tab.name AS TableName, 
	col.name AS ColumnName
FROM sys.columns col
JOIN sys.tables tab ON col.object_id = tab.object_id
JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id AND fkc.parent_column_id = col.column_id
LEFT JOIN sys.index_columns ic ON col.object_id = ic.object_id AND col.column_id = ic.column_id
WHERE 
	tab.name in ({tariffTableNames}) AND ic.key_ordinal IS NULL";
					var missingColumns = new List<string>();
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							if (!excludedColumns.Contains(reader.GetString(1)))
							{
								missingColumns.Add($"{reader.GetString(1)} in {reader.GetString(0)}");
							}
						}
					}
					Assert.AreEqual(0, missingColumns.Count, $"ForeignKey columns should be Indexed. Missing column(s): {string.Join(", ", missingColumns)}");
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void FKColumnShouldBeFirstIndexed_RefCusTariff(DbSchema dbSchema)
		{
			var tariffDataSet = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x.Contains("RefCusTariff"));
			var tariffTableNames = string.Join(", ", tariffDataSet.Select(x => $"'{x}'"));
			var excludedColumns = new[] { "ZY2_ZY2_TariffAdditionalCode", "ZZ8_ZZA_TradeGroup", "ZZ8_ZZA_SecondTradeGroup" };

			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"
WITH FKInfo AS (
	SELECT
		tab.name AS TableName,
		col.name AS ColumnName,
		ic.key_ordinal AS KeyOrdinal,
		ROW_NUMBER() OVER (PARTITION BY tab.name, col.name ORDER BY ic.key_ordinal ASC) AS RowNum
	FROM 
		sys.columns col
	JOIN sys.tables tab ON col.object_id = tab.object_id
	JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
	JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id AND fkc.parent_column_id = col.column_id
	LEFT JOIN sys.index_columns ic ON col.object_id = ic.object_id AND ic.column_id = col.column_id
	WHERE
		tab.name in ({tariffTableNames}) AND ic.key_ordinal IS NOT NULL
)
SELECT
	TableName,
	ColumnName,
	KeyOrdinal
FROM
	FKInfo
WHERE
	RowNum = 1
ORDER BY 
	ColumnName,
	KeyOrdinal;";
					var missingColumns = new List<string>();
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							if (!excludedColumns.Contains(reader.GetString(1)) && reader.GetByte(2) > 1)
							{
								missingColumns.Add($"{reader.GetString(1)} in {reader.GetString(0)}");
							}
						}
					}
					Assert.AreEqual(0, missingColumns.Count, $"ForeignKey columns should be first Indexed. Missing column(s): {string.Join(", ", missingColumns)}");
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void NoDiffWhenSchemaHasNoChanges(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			var schemaUpgrade = new SchemaUpgrade("RemoteDb.dacpac", Common.Infrastructure.Test.TestConnectionString.DataSource, null, null);
			var diff = schemaUpgrade.GetDiffSql(dbName);
			Assert.That(diff, Is.EqualTo(string.Empty), "Check your constraints and make sure you have not included a check constraint with IN");
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void CheckRemoteDbConstraintNames(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				var sql = @"select count(1) from sys.default_constraints where name like 'DF[_][_]%' and is_ms_shipped=0";
				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, sql), "All default value constraints should have explicit names.");

				sql = @"select count(1) from sys.check_constraints where name like 'CK[_][_]%' and is_ms_shipped=0";
				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, sql), "All check constraints should have explicit names.");

				var invalidConstraintNameList = new List<string>();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"select dc.name from sys.default_constraints dc
join sys.tables t on dc.parent_object_id=t.object_id
join sys.columns c on dc.parent_column_id=c.column_id and c.object_id=t.object_id
where t.name not like '%History' and t.is_ms_shipped=0 and dc.name not like concat('DF[_]',t.name,'[_]',c.name)";

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							invalidConstraintNameList.Add(reader.GetString(0));
						}
					}
				}
				var invalidNames = string.Join(", ", invalidConstraintNameList);
				Assert.AreEqual(0, invalidConstraintNameList.Count, $"All default constraint names should be 'DF_TableName_ColumnName'.\r\nInvalid name(s): {invalidNames}");
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void StringColumnsShouldNotBeNull(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				var invalidColumnNameList = new List<string>();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"
SELECT DISTINCT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE (DATA_TYPE = 'char' OR DATA_TYPE = 'varchar' OR DATA_TYPE = 'nvarchar') AND IS_NULLABLE = 'YES';";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							invalidColumnNameList.Add(reader.GetString(0));
						}
					}
				}
				var invalidNames = string.Join(", ", invalidColumnNameList);
				Assert.That(invalidColumnNameList.Except(notNullColumnExclusiveList), Is.Empty, $"All string Columns should be NOT NULL.\r\nInvalid Column(s): {invalidNames}");
			}
		}

		static readonly string[] notNullColumnExclusiveList = {
			"SFM_BillableCount",
			"SFM_Category",
			"SFM_ClientStaffCode",
			"SFM_PriceItemCode",
			"SFM_Reference1",
			"SFM_Reference2",
			"SFM_Reference3",
			"SFM_Reference4",
			"SFM_Reference5",
			"SFM_ServiceOccuredUTC",
			"ZMC_CountryCode",
			"ZX1_Severity",
			"ZX1_ZY7_NKConditionCode",
			"ZX5_ZZF_NKTaxOrFeeCode",
			"ZXE_ZZK_NKCodeTypeForValueList", "ZXE_ZZK_NKCodeType", "ZXE_ZZK_NKCodeTypeComputed",
			"ZXH_ColumnCaption", "ZXH_Name",
			"ZY1_ZZZ_NKDataGrouping",
			"ZY2_Description",
			"ZZ1_ZZF_NKTaxOrFeeCode",
			"ZZ2_RateFormulaDerivedFrom",
			"ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping",
			"ZZW_ZZF_NKTaxOrFeeCode",
			"ZZD_ZZK_NKCodeType", "ZZD_ZZK_NKCodeTypeComputed",
			"ZZK_CodeType", "ZZK_CodeTypeComputed"
		};
	}
}
