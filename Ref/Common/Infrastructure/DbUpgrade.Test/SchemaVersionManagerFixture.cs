using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class SchemaVersionManagerFixture
{
	[Test]
	public void Get()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		var mgr = new SchemaVersionManager(conn, VersionType.DbSchemaVersion, dbName);
		Assert.That(mgr.GetVersion(null), Is.EqualTo(0));
		dbCreator.ExcuteDbScript(dbName, $@"CREATE TABLE SystemData
(
	SD_PK uniqueidentifier NOT NULL CONSTRAINT DF_SystemData_SD_PK DEFAULT NEWID(),
	SD_Name varchar(50) NOT NULL CONSTRAINT DF_SystemData_SD_Name DEFAULT '',
	SD_Value varchar(max) NOT NULL CONSTRAINT DF_SystemData_SD_Value DEFAULT '',
	CONSTRAINT PK_SystemData PRIMARY KEY CLUSTERED (SD_PK)
)

GO

INSERT SystemData (SD_Name, SD_Value) VALUES ('{VersionType.DbSchemaVersion}', '5')");
		Assert.That(mgr.GetVersion(null), Is.EqualTo(5));
	}

	[Test]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public void Update()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, @"CREATE TABLE SystemData
(
	SD_PK uniqueidentifier NOT NULL CONSTRAINT DF_SystemData_SD_PK DEFAULT NEWID(),
	SD_Name varchar(50) NOT NULL CONSTRAINT DF_SystemData_SD_Name DEFAULT '',
	SD_Value varchar(max) NOT NULL CONSTRAINT DF_SystemData_SD_Value DEFAULT '',
	CONSTRAINT PK_SystemData PRIMARY KEY CLUSTERED (SD_PK)
)");

		var mgr = new SchemaVersionManager(conn, VersionType.DbSchemaVersion, dbName);
		Assert.That(mgr.GetVersion(null), Is.EqualTo(0));
		mgr.UpdateVersion(5, null);
		Assert.That(mgr.GetVersion(null), Is.EqualTo(5));
	}

	[Test]
	public void GetWithOldColumnsTriggersRenameAndDoesNotThrown()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
		conn.Open();
		using var trans = conn.BeginTransaction();
		var dbCreator = new DbCreator(conn);
		var mgr = new SchemaVersionManager(conn, VersionType.DbSchemaVersion, dbName);
		Assert.That(mgr.GetVersion(trans), Is.EqualTo(0));
		dbCreator.ExcuteDbScript(dbName, $@"CREATE TABLE SystemData
(
	Id uniqueidentifier NOT NULL CONSTRAINT DF_SystemData_SD_PK DEFAULT NEWID(),
	Name varchar(50) NOT NULL CONSTRAINT DF_SystemData_SD_Name DEFAULT '',
	Value varchar(max) NOT NULL CONSTRAINT DF_SystemData_SD_Value DEFAULT '',
	CONSTRAINT PK_SystemData PRIMARY KEY CLUSTERED (Id)
)

GO

INSERT SystemData (Name, Value) VALUES ('{VersionType.DbSchemaVersion}', '5')", trans);
		Assert.That(mgr.GetVersion(trans), Is.EqualTo(5));
	}
}
