using System.Text;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

[TestFixture]
class DataSetTimstampTransformationFixture
{
	[Test]
	[TransactionedTestCase]
	public void TestDepdentTableHasNewerTimestamp()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, CreateDb());
		var sql = @"
INSERT MainTbl (M_PK) VALUES ('2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT DepTbl (D_PK, D_M_PK) VALUES ('CAA5A690-3884-4A89-BB19-C298CE5BBFCF', '2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT RefDbVersionControl (ParentPK, LastUpdatedUTC, ParentCode)
VALUES 
	('2F2955B7-8E5C-4638-A45E-8F4B402D4656', '2017-06-02', 'M'),
	('CAA5A690-3884-4A89-BB19-C298CE5BBFCF', '2017-06-03', 'D')
";
		dbCreator.ExcuteDbScript(dbName, sql);
		dbCreator.ExcuteDbScript(dbName, DataSetTimstampTransformation.UpdateLastUpdatedUTC("MainTbl", "DepTbl", "D_PK", "D_M_PK", "D"));
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT COUNT(*) FROM RefDbVersionControl WHERE LastUpdatedUTC = '2017-06-03'";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
		cmd.CommandText = "SELECT COUNT(*) FROM MainTbl";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
		cmd.CommandText = "SELECT COUNT(*) FROM DepTbl";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
	}

	[Test]
	[TransactionedTestCase]
	public void TestSecondDepdentTableHasNewerTimestamp()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, CreateDb());
		var sql = @"
INSERT MainTbl (M_PK) VALUES ('2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT DepTbl (D_PK, D_M_PK) VALUES ('CAA5A690-3884-4A89-BB19-C298CE5BBFCF', '2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT DepTbl (D_PK, D_M_PK) VALUES ('48760DD6-3AC1-44D4-9716-B6ACE0D5E5AE', '2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT Dep2Tbl (D2_PK, D2_D_PK) VALUES ('6F9C30D6-38AF-44A8-918A-C2F4479A18FA', 'CAA5A690-3884-4A89-BB19-C298CE5BBFCF');
INSERT Dep2Tbl (D2_PK, D2_D_PK) VALUES ('E6105731-9F7C-4EA6-97E8-DC24C9AC7451', '48760DD6-3AC1-44D4-9716-B6ACE0D5E5AE');
INSERT RefDbVersionControl (ParentPK, LastUpdatedUTC, Deleted, ParentCode)
VALUES 
	('2F2955B7-8E5C-4638-A45E-8F4B402D4656', '2017-06-02', 0, 'M'),
	('CAA5A690-3884-4A89-BB19-C298CE5BBFCF', '2017-06-03', 0, 'D'),
	('48760DD6-3AC1-44D4-9716-B6ACE0D5E5AE', '2017-06-03', 1, 'D'),
	('6F9C30D6-38AF-44A8-918A-C2F4479A18FA', '2017-06-04', 0, 'D2'),
	('E6105731-9F7C-4EA6-97E8-DC24C9AC7451', '2017-06-04', 1, 'D2')
";
		dbCreator.ExcuteDbScript(dbName, sql);
		var updateSql = new StringBuilder(DataSetTimstampTransformation.UpdateLastUpdatedUTC("DepTbl", "Dep2Tbl", "D2_PK", "D2_D_PK", "D2"));
		updateSql.AppendLine(DataSetTimstampTransformation.UpdateLastUpdatedUTC("MainTbl", "DepTbl", "D_PK", "D_M_PK", "D"));
		dbCreator.ExcuteDbScript(dbName, updateSql.ToString());
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT COUNT(*) FROM RefDbVersionControl WHERE LastUpdatedUTC = '2017-06-04'";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
		cmd.CommandText = "SELECT COUNT(*) FROM Dep2Tbl";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
		cmd.CommandText = "SELECT COUNT(*) FROM DepTbl";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
		cmd.CommandText = "SELECT COUNT(*) FROM MainTbl";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
	}

	[Test]
	[TransactionedTestCase]
	public void TestMainTableHasNewerTimestamp()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, CreateDb());
		var sql = @"
INSERT MainTbl (M_PK) VALUES ('2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT DepTbl (D_PK, D_M_PK) VALUES ('CAA5A690-3884-4A89-BB19-C298CE5BBFCF', '2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT RefDbVersionControl (ParentPK, LastUpdatedUTC, Deleted, ParentCode)
VALUES 
	('2F2955B7-8E5C-4638-A45E-8F4B402D4656', '2017-06-03', 0, 'M'),
	('CAA5A690-3884-4A89-BB19-C298CE5BBFCF', '2017-06-02', 1, 'D')
";
		dbCreator.ExcuteDbScript(dbName, sql);
		dbCreator.ExcuteDbScript(dbName, DataSetTimstampTransformation.UpdateLastUpdatedUTC("MainTbl", "DepTbl", "D_PK", "D_M_PK", "D"));
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT COUNT(*) FROM RefDbVersionControl WHERE LastUpdatedUTC = '2017-06-03'";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
		cmd.CommandText = "SELECT COUNT(*) FROM DepTbl";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
		cmd.CommandText = "SELECT COUNT(*) FROM MainTbl";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
	}

	[Test]
	[TransactionedTestCase]
	public void TestDepdentRecordDoesNotExists()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, CreateDb());
		var sql = @"
INSERT MainTbl (M_PK) VALUES ('2F2955B7-8E5C-4638-A45E-8F4B402D4656');
INSERT RefDbVersionControl (ParentPK, LastUpdatedUTC, ParentCode)
VALUES 
	('2F2955B7-8E5C-4638-A45E-8F4B402D4656', '2017-06-03', 'M')
";
		dbCreator.ExcuteDbScript(dbName, sql);
		dbCreator.ExcuteDbScript(dbName, DataSetTimstampTransformation.UpdateLastUpdatedUTC("MainTbl", "DepTbl", "D_PK", "D_M_PK", "D"));
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT COUNT(*) FROM RefDbVersionControl WHERE LastUpdatedUTC = '2017-06-03'";
		Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
	}

	static string CreateDb()
	{
		return @"
CREATE TABLE MainTbl(
	M_PK UNIQUEIDENTIFIER
)
GO
CREATE TABLE DepTbl (
	D_PK UNIQUEIDENTIFIER,
	D_M_PK UNIQUEIDENTIFIER,
)
GO
CREATE TABLE Dep2Tbl (
	D2_PK UNIQUEIDENTIFIER,
	D2_D_PK UNIQUEIDENTIFIER
)
GO
CREATE TABLE RefDbVersionControl
(
	ParentPK uniqueidentifier NOT NULL,
	LastUpdatedUTC datetime2 NOT NULL,
	Deleted BIT,
	ParentCode NVARCHAR(3)
)
GO
CREATE TRIGGER DepTbl_Delete
ON dbo.DepTbl
FOR DELETE
AS
THROW 5000, 'Cannot delete this record', 1
GO
CREATE TRIGGER DepTbl2_Delete
ON dbo.Dep2Tbl
FOR DELETE
AS
THROW 5000, 'Cannot delete this record', 1
";
	}
}
