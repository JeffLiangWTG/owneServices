using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

[TestFixture]
class SafeDbSchemaFixture
{
	[Test]
	[TransactionedTestCase]
	public void NoDiffWhenSchemaHasNoChanges()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		var schemaUpgrade = new SchemaUpgrade("SafeDb.dacpac", TestConnectionString.DataSource, null, null);
		var diff = schemaUpgrade.GetDiffSql(dbName);
		Assert.That(diff, Is.Empty);
	}

	[Test]
	[TransactionedTestCase]
	public void AllProceduresHavePermissionsToExecute()
	{
		var users = new[] { "'refdbrepowriter'" };
		var storeProcedureObjectIds = new List<int>();
		var grantedExecuteObjectIds = new List<int>();
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = "select object_id from sys.procedures prc";
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					storeProcedureObjectIds.Add(reader.GetInt32(0));
				}
			}
		}
		if (!storeProcedureObjectIds.Any())
		{
			Assert.IsTrue(true);
			return;
		}
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = $@"select dp.major_id from sys.procedures prc
	join sys.syspermissions p on p.id = prc.object_id
	join sys.sysusers u on p.grantee = u.uid
	join sys.database_permissions dp on dp.major_id = p.id and dp.grantee_principal_id = p.grantee
	where
	dp.permission_name = 'EXECUTE'
	and state_desc = 'GRANT'
	and u.name in ({string.Join(",", users)})";
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					grantedExecuteObjectIds.Add(reader.GetInt32(0));
				}
			}
			Assert.That(grantedExecuteObjectIds, Has.Count.GreaterThan(0), "There are store procedures, however there is no permission to execute in the object.");
		}

		CollectionAssert.AreEquivalent(storeProcedureObjectIds, grantedExecuteObjectIds);
	}

	[Test]
	[TransactionedTestCase]
	public void AllFunctionsHavePermissionsToExecute()
	{
		var users = new string[] { "'refdbrepowriter'" };
		var functionObjectIds = new List<int>();
		var grantedFunctionObjectIds = new List<int>();
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
		{
			conn.Open();
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "select object_id from sys.objects o where type_desc = 'SQL_SCALAR_FUNCTION'";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						functionObjectIds.Add(reader.GetInt32(0));
					}
				}
			}
			if (!functionObjectIds.Any())
			{
				Assert.IsTrue(true);
				return;
			}
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"select dp.major_id from sys.objects o
	join sys.syspermissions p on p.id = o.object_id
	join sys.sysusers u on p.grantee = u.uid
	join sys.database_permissions dp on dp.major_id = p.id and dp.grantee_principal_id = p.grantee
	where
	o.type_desc = 'SQL_SCALAR_FUNCTION'
	and dp.permission_name = 'EXECUTE'
	and state_desc = 'GRANT'
	and u.name in ({string.Join(",", users)})";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						grantedFunctionObjectIds.Add(reader.GetInt32(0));
					}
				}
				Assert.That(grantedFunctionObjectIds, Has.Count.GreaterThan(0), "There are scalar functions, however there is no permission to execute in the object(s).");
			}

			CollectionAssert.AreEquivalent(functionObjectIds, grantedFunctionObjectIds);
		}
	}

	[Test]
	[TransactionedTestCase]
	public void TriggerOnlyFireOnce()
	{
		var tableAndColumnDetails = new List<(string tableName, string ColumnName, string dataType, bool isPrimaryKey, string foreignTable, string ForeignColumn)>();
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = @"SELECT DISTINCT tbl.name 'TableName',c.name 'ColumnName',t.Name 'Data type',ISNULL(i.is_primary_key, 0) 'Primary Key',a.[ParentTable],a.ParentColumn
FROM sys.tables tbl
inner join sys.columns c on tbl.object_id = c.object_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
LEFT OUTER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
Left join (
SELECT * FROM (
Select t.[name] as [ForeignTable],COL_NAME(fkc.parent_object_id,fkc.parent_column_id) as [ForeignKeyColumn],COL_NAME(fkc.referenced_object_id,fkc.referenced_column_id) as [ParentColumn] ,t2.name [ParentTable]
,ROW_NUMBER() OVER(PARTITION BY t.name,fkc.parent_column_id ORDER BY t.name,fkc.parent_column_id) RN
from sys.foreign_keys fk
join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
join sys.tables t on t.object_id = fkc.parent_object_id
join sys.tables t2 on t2.object_id = fkc.referenced_object_id
) AS FKS WHERE FKS.RN = 1
) a on a.ForeignTable = tbl.[name]
and a.ForeignKeyColumn = c.name
WHERE tbl.type_desc = 'USER_TABLE'
and generated_always_type = 0
and temporal_type <> 1";

		using (var reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				tableAndColumnDetails.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), !reader.IsDBNull(4) ? reader.GetString(4) : null, !reader.IsDBNull(5) ? reader.GetString(5) : null));
			}
		}

		var tableCollection = new TableInfoCollection(tableAndColumnDetails);
		var insertStatements = tableCollection.GetInsertStatements();
		var updateStatements = tableCollection.GetUpdateStatements();
		var deleteStatements = tableCollection.GetDeleteStatements();

		cmd.CommandText = @"EXEC sp_msforeachtable ""ALTER TABLE ? NOCHECK CONSTRAINT all""";
		cmd.ExecuteNonQuery();

		foreach (var query in insertStatements.Concat(updateStatements).Concat(deleteStatements))
		{
			cmd.CommandText = @"DBCC FREEPROCCACHE
DBCC DROPCLEANBUFFERS";
			cmd.ExecuteNonQuery();

			cmd.CommandText = query;
			cmd.ExecuteNonQuery();

			cmd.CommandText = @"select obj.[name] as [TriggerName], stat.execution_count as [TriggerFireCount]
from sys.dm_exec_trigger_stats stat
join sys.objects obj on obj.object_id = stat.object_id";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var triggerName = reader.GetString(0);
				var triggerFireCount = reader.GetInt64(1);
				var expectedTriggerFireCount = 1;
				if ("RefCusTariffAdditionalCode_INS_UPD_Parents" == triggerName && query.StartsWith("Insert", StringComparison.OrdinalIgnoreCase))
				{
					expectedTriggerFireCount = 2;
				}
				Assert.IsTrue(expectedTriggerFireCount == triggerFireCount, $"Trigger fired more than {expectedTriggerFireCount} times for {triggerName}. Query: {query}");
			}
		}
	}

	[Test]
	[TransactionedTestCase]
	public void NoTableContainIsSystemField()
	{
		var result = new List<string>();
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
		{
			conn.Open();
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = @"select t.name [TableName]
from sys.tables t
join sys.columns c on t.object_id = c.object_id
Where c.[name] like '%IsSystem'";

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetString(0));
					}
				}
			}
		}
		Assert.AreEqual(0, result.Count, $"The following table(s) contains IsSystem columns: {string.Join(",", result)}");
	}
}
