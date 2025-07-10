using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class SchemaTest
	{
		[Test]
		public void TablesShouldHavePrefix()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();

				var exceptedTables = new string[] { "SystemData", "sysdiagrams" };
				var tableColumnList = new List<Tuple<string, string>>();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "select t.name, (select top 1 c.name from sys.all_columns c where c.object_id = t.object_id) from sys.tables t";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var tableName = reader.GetString(0);
							if (exceptedTables.Contains(tableName))
							{
								continue;
							}
							tableColumnList.Add(Tuple.Create(tableName, reader.GetString(1)));
						}
					}
					var regexPrefix = @"[a-zA-Z]{1,3}[0-9]{0,1}_";
					foreach (var tableColumn in tableColumnList)
					{
						var column = tableColumn.Item2;
						var match = Regex.Match(column, regexPrefix);
						if (!match.Success)
						{
							Assert.Fail($"{tableColumn.Item1} does not have a prefix.");
						}
					}
				}
			}
		}

		[Test]
		public void TablesShouldHaveNoUniqueConstraint()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = "SELECT COUNT(*) FROM sys.key_constraints WHERE type='UQ'";
				Assert.AreEqual(0, cmd.ExecuteScalar());
			}
		}

		[Test]
		public void TablesShouldHavePKAsClustered()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = @"SELECT COUNT(1) FROM sys.indexes i WHERE OBJECT_NAME(i.object_id) IN (
	SELECT OBJECT_NAME(object_id) FROM sys.tables)
AND i.is_primary_key = 1 AND i.type <> 1";
				Assert.AreEqual(22, cmd.ExecuteScalar());	//Please use the query in this test to check the 22 violations, should Assert 0 after all violations removed
			}
		}

		[Test]
		public void TestInsteadOfTriggerIncludeAllColumns()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var ignoreViews = new string[]
			{
				nameof(RefCusCodeListUserView),
				nameof(RefAccTaxRateUserView),
				nameof(RefCusCodeListAttributeUserView),
				nameof(RefCusProcedureAttributeUserView),
				nameof(RefCusProcedureUserView),
				nameof(RefPortPolygonUserView),
				nameof(RefShippingLineUserView),
				nameof(RefStlScriptUserView),
				nameof(RefUNLOCOUserView),
				nameof(RefVesselUserView)
			};
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = $@"SELECT Count(*) from sys.triggers tr
join INFORMATION_SCHEMA.COLUMNS on OBJECT_NAME(parent_id) = TABLE_NAME
where OBJECTPROPERTY(tr.object_id, 'ExecIsInsteadOfTrigger') = 1 AND CHARINDEX(COLUMN_NAME, OBJECT_DEFINITION(tr.object_id)) <= 0
AND TABLE_NAME NOT IN ({string.Join(",", ignoreViews.Select(x => $"'{x}'"))})";
				Assert.AreEqual(0, cmd.ExecuteScalar());
			}
		}

		[Test]
		public void TestDisableLockEscalationOnAllTables()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name not like '%Test' and lock_escalation_desc <> 'DISABLE' AND temporal_type <> 1";
				Assert.AreEqual(0, cmd.ExecuteScalar());
			}
		}

		[Test]
		public void TestNoTwoTablesHavingTheSamePrefix()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			using (var cmd = conn.CreateCommand())
			{
				conn.Open();
				cmd.CommandText = "SELECT COUNT(distinct name) FROM sys.columns WHERE name like '%[_]PK'";
				var noOfDistinctPrefix = (int)cmd.ExecuteScalar();
				var exceptionTbls = new[] { "RefCusTariffRule", "RefCusTariffUOMRule", "RefCusRateRule", "RefCusTariffAttributeRule", "RefCusTariffRelationshipRule" }
				.Select(x => "'" + x + "'");
				cmd.CommandText = $@"SELECT COUNT(*) FROM Sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE c.name like '%[_]PK' AND t.name NOT IN ({string.Join(",", exceptionTbls)}) AND t.temporal_type <> 1";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(noOfDistinctPrefix));
			}
		}

		[Test]
		public void TestExplicitNamingOfDbObjects()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = CheckDefaultNamingViolation.Query;
					using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						var sb = new StringBuilder();
						while (reader.Read())
						{
							sb.AppendLine(CultureInfo.InvariantCulture, $"Incorrect naming on Table:{(string)reader["TableName"]} Object name:{(string)reader["ObjectName"]} Object type:{(string)reader["ObjectDescription"]}");
						}
						if (sb.Length != 0)
						{
							Assert.That(false, sb.ToString());
						}
					}
				}
			}
		}

		[Test]
		public void SchemaBindingOptionAvailable()
		{
			var objToTest = new List<string>();
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $"select name from sys.objects where type_desc = 'VIEW'";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							objToTest.Add(reader.GetString(0));
						}
					}

					foreach (var obj in objToTest)
					{
						cmd.CommandText = $@"select definition
from sys.objects o
join sys.sql_modules m on m.object_id = o.object_id
where o.object_id = object_id('dbo.{obj}')";

						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								Assert.That(reader.GetString(0), Does.Contain("SCHEMABINDING"), $"Object {obj} does not have SCHEMABINDING configuration.");
							}
						}
					}
				}
			}
		}

		[Test]
		public void TestChildTableVersionModifyTrigger()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"SELECT so.Name AS TriggerName, sc.Text AS TriggerText
FROM sys.sysobjects so
INNER JOIN sys.syscomments sc ON so.ID = sc.ID
WHERE so.xType = 'TR' AND so.Name like '%[_]Version[_]Modify'";
					using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						while (reader.Read())
						{
							Assert.That(reader["TriggerText"].ToString().Contains(@"IF UPDATE("), $"Trigger:{reader["TriggerName"]} should include deleted referred fk column records when adding to RefDbVersionControl table");
						}
					}
				}
			}
		}

		[Test]
		public void TestDataSetPKShouldBeIndexed()
		{
			var tariffDataSet = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x.Contains("RefCusTariff"));
			var tariffRelatedTableNames = tariffDataSet.Select(x => $"'{x}'");

			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"
select count(1)
from sys.columns col
join sys.tables tab on col.object_id=tab.object_id
left join sys.index_columns ic on col.object_id=ic.object_id and col.column_id=ic.column_id
where tab.name not like '%History' and col.name like '%_DataSetPK' and ic.object_id is null
	and tab.name in ({string.Join(",", tariffRelatedTableNames)});";
					Assert.AreEqual(0, cmd.ExecuteScalar());
				}
			}
		}
	}
}
