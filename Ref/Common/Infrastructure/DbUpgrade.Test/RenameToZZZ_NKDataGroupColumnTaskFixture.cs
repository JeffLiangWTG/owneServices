using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test;

//TODO : Extract to a base class
[TestFixture]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class RenameToZZZ_NKDataGroupColumnTaskFixture
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
CREATE TABLE RefCusRateType 
(
	ZZR_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusRateType_ZZR_PK DEFAULT (NEWID()),
	ZZR_Description VARCHAR(50) NOT NULL,
	ZZR_RN_CountryOrGrouping VARCHAR(3) NOT NULL
)
GO
CREATE TRIGGER RefCusRateType_Version_Update
	ON RefCusRateType
	FOR Update
	AS
	UPDATE RefCusRateType SET ZZR_Description = ZZR_Description + 'BOOM'
GO
CREATE TABLE RefCusTariffRule (
	ZZ1_PK UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
	ZZ1_RN_CountryOrGrouping VARCHAR(3) NOT NULL
)
");
		dbCreator.ExcuteDbScript(dbName, @"
INSERT INTO RefCusRateType (ZZR_Description, ZZR_RN_CountryOrGrouping) VALUES ('HELLO', 'ZA');
UPDATE RefCusRateType SET ZZR_Description = 'HELLOTHERE';
");
		using (var trans = conn.BeginTransaction())
		{
			var task = new RenameToZZZNKDataGroupColumnTask(0);
			task.Run(trans);
			trans.Commit();
		}
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = @"SELECT 1 FROM sys.columns WHERE name = 'ZZR_ZZZ_NKDataGrouping' AND object_id = OBJECT_ID('dbo.RefCusRateType')";
			Assert.AreEqual(1, cmd.ExecuteScalar());
			cmd.CommandText = @"SELECT 1 FROM sys.columns WHERE name = 'ZZ1_ZZZ_NKDataGrouping' AND object_id = OBJECT_ID('dbo.RefCusTariffRule')";
			Assert.AreEqual(1, cmd.ExecuteScalar());
			cmd.CommandText = @"SELECT ZZR_Description FROM RefCusRateType";
			Assert.AreEqual("HELLOTHEREBOOM", cmd.ExecuteScalar());
		}
		using (var trans = conn.BeginTransaction())
		{
			var task = new RenameToZZZNKDataGroupColumnTask(0);
			task.Run(trans);
			trans.Commit();
		}
	}
}
