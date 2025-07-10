using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class SchemaTest
{
	[Test]
	public void FKColumnsShouldBeIndexed()
	{
		var exceptionList = new[] { nameof(RefCusTariffType.ZZI_ZZR_RateType), nameof(RefErrorReport.RER_SDA), nameof(RefApplicationAttribute.RAA_RAT_NKType) }
			.Select(x => $"'{x}'"); // ZZI_ZZR_RateType should be removed
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = $@"select COUNT(*) from sys.foreign_key_columns fk
join sys.columns col on fk.parent_object_id = col.object_id and fk.parent_column_id = col.column_id
left join sys.index_columns ic on fk.parent_object_id = ic.object_id and fk.parent_column_id = ic.column_id
where ic.index_id is null AND col.name NOT IN ( {string.Join(",", exceptionList)})
";
		Assert.AreEqual(0, cmd.ExecuteScalar());
	}

	[Test]
	public void FKColumnShouldBeFirstColumnIndexed()
	{
		var exceptionIndexList = new[] {
			"IX_RefCusRateTypeLanguage_ZXT_ZX6_NKLanguage_ZXT_ZZR_RateType",
			"IX_ProcessorStatus_PRC_SchedName_PRC_JobGroup_PRC_JobName"
		}.Select(x => $"'{x}'");
		// ZXT_ZZR_RateType is already first-column indexed in a no-unique index
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = $@"
select count(1)
from sys.foreign_key_columns fk
join sys.tables st on fk.parent_object_id=st.object_id
left join sys.index_columns ic on fk.parent_object_id = ic.object_id and fk.parent_column_id = ic.column_id
left join sys.indexes si on ic.object_id = si.object_id and ic.index_id = si.index_id
where ic.index_id is not null and ic.Key_ordinal > 1 and st.name not like 'QRTZ%' and si.name not in ({string.Join(",", exceptionIndexList)})
";
		Assert.AreEqual(0, cmd.ExecuteScalar());
	}

	[Test]
	public void TestExplicitNamingOfDbObjects()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = CheckDefaultNamingViolation.Query;
		using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
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

	[Test]
	public void TestAllConstraintsAreTrusted()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = @"select name
from sys.foreign_keys f
where f.is_not_trusted = 1";
		using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
		var sb = new StringBuilder();
		while (reader.Read())
		{
			sb.AppendLine(CultureInfo.InvariantCulture, $"Constraint {reader.GetString(0)} is flagged as not trusted.");
		}
		if (sb.Length != 0)
		{
			Assert.That(false, sb.ToString());
		}
	}

	[Test]
	public void ProcessorStatusHasFK_ReferenceQrtzJobDetails()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = @"
select count(*) from sys.foreign_keys
	where name='FK_ProcessorStatus_QrtzJobDetails'
	and OBJECT_NAME(parent_object_id)='ProcessorStatus'
	and OBJECT_NAME(referenced_object_id)='Qrtz_JOB_DETAILS';
";
		Assert.AreEqual(1, cmd.ExecuteScalar());
	}

	[Test]
	public void TestAllCostraintsShouldNotCheckEmptyOrNull()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = $@"
SELECT
	[table_name] = t.name,
	[constraint_name] = c.name,
	[constraint_expression] = c.definition
FROM
	sys.tables AS t
	INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id
	INNER JOIN sys.objects AS o ON t.object_id = o.object_id
	INNER JOIN sys.check_constraints AS c ON o.object_id = c.parent_object_id
WHERE
	o.type = 'U'
	and c.definition like '%<>''''%'
	and t.name not in ({string.Join(",", ignoreUnBussinessTableList.Select(x => $"'{x}'"))})
";
		using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
		var sb = new StringBuilder();
		while (reader.Read())
		{
			sb.AppendLine(CultureInfo.InvariantCulture, $"Unexpected Constraint: Table[{reader.GetString(0)}],[{reader.GetString(1)}]: {reader.GetString(2)}");
		}
		if (sb.Length != 0)
		{
			Assert.That(false, sb.ToString());
		}
	}

	readonly string[] ignoreUnBussinessTableList =
	[
		"AutoSchema",
		"DataChangeCapture",
		"DataProcessingClone",
		"DataProcessingInformation",
		"DataProcessingResult",
		"DataSourceInformation",
		"NamedEntityClassification",
		"ProcessData",
		"ProcessorStatus",
		"QRTZ_BLOB_TRIGGERS",
		"QRTZ_CALENDARS",
		"QRTZ_CRON_TRIGGERS",
		"QRTZ_FIRED_TRIGGERS",
		"QRTZ_JOB_DETAILS",
		"QRTZ_LOCKS",
		"QRTZ_PAUSED_TRIGGER_GRPS",
		"QRTZ_SCHEDULER_STATE",
		"QRTZ_SIMPLE_TRIGGERS",
		"QRTZ_SIMPROP_TRIGGERS",
		"QRTZ_TRIGGERS",
		"SourceData",
		"SubscriptionEvent",
		"SubscriptionEventType",
		"SubscriptionNotificationQueue",
		"SystemData"
	];
}
