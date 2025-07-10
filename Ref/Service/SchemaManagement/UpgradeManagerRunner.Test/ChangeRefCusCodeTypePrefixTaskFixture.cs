using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

[TestFixture]
class ChangeRefCusCodeTypePrefixTaskFixture
{
	[Test]
	[TransactionedTestCase]
	public void Run()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, @"
CREATE TABLE RefCusCodeType(
	ZZN_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusCodeType_ZZN_PK DEFAULT (NEWID()),
	ZZN_CodeType VARCHAR(5) NOT NULL,
	ZZN_Description VARCHAR(500) NOT NULL
)
GO
CREATE TRIGGER RefCusCodeType_Version_Update
	ON RefCusCodeType
	FOR Update
	AS
	UPDATE RefCusCodeType SET ZZN_Description = ZZN_Description + 'BOOM'
GO
CREATE TABLE RefCusCodeList (
	ZZD_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusCodeList_ZZD_PK DEFAULT (NEWID()),
	ZZD_ZZN_NKCodeType VARCHAR(5) NOT NULL,
	ZZD_Description NVARCHAR(MAX) NOT NULL,
)
GO
CREATE TRIGGER RefCusCodeList_Version_Update
	ON RefCusCodeList
	FOR Update
	AS
	UPDATE RefCusCodeList SET ZZD_Description = ZZD_Description + 'BOOM'
");
		dbCreator.ExcuteDbScript(dbName, @"
INSERT INTO RefCusCodeType (ZZN_CodeType, ZZN_Description) VALUES ('ABC', 'HELLO');
UPDATE RefCusCodeType SET ZZN_CodeType = 'CBA';
INSERT INTO RefCusCodeList (ZZD_ZZN_NKCodeType, ZZD_Description) VALUES ('ABC', 'CIAO');
UPDATE RefCusCodeList SET ZZD_Description = 'NICE';
");
		using (var trans = conn.BeginTransaction())
		{
			var task = new ChangeRefCusCodeTypePrefixTask(8);
			task.Run(trans);
			trans.Commit();
		}
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = @"SELECT COUNT(*) FROM sys.columns WHERE name IN ('ZZK_PK', 'ZZK_CodeType', 'ZZK_Description') AND object_id = OBJECT_ID('dbo.RefCusCodeType')";
			Assert.AreEqual(3, cmd.ExecuteScalar());
			cmd.CommandText = @"SELECT COUNT(*) FROM sys.columns WHERE name IN ('ZZD_ZZK_NKCodeType') AND object_id = OBJECT_ID('dbo.RefCusCodeList')";
			Assert.AreEqual(1, cmd.ExecuteScalar());
			cmd.CommandText = @"SELECT ZZN_Description FROM RefCusCodeType";
			Assert.AreEqual("HELLOBOOM", cmd.ExecuteScalar());
			cmd.CommandText = @"SELECT ZZD_Description FROM RefCusCodeList";
			Assert.AreEqual("NICEBOOM", cmd.ExecuteScalar());
		}
		using (var trans = conn.BeginTransaction())
		{
			var task = new ChangeRefCusCodeTypePrefixTask(8);
			task.Run(trans);
			trans.Commit();
		}
	}
}
