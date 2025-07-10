using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class AddZZZ_NKDataGroupColumnTaskFixture
	{
		[Test]
		[TransactionedTestCase]
		public void Run()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				dbCreator.ExcuteDbScript(dbName, @"
CREATE TABLE RefVesselZZ (
	ZZO_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefVesselZZ DEFAULT (NEWID()),
	ZZO_Code VARCHAR(35) NOT NULL
)
GO
CREATE TRIGGER RefVesselZZ_Version_Update
	ON RefVesselZZ
	FOR Update
	AS
	UPDATE RefVesselZZ SET ZZO_Code = ZZO_Code + 'BOOM'
");
				dbCreator.ExcuteDbScript(dbName, @"
INSERT INTO RefVesselZZ (ZZO_Code) VALUES ('ABC');
UPDATE RefVesselZZ SET ZZO_Code = 'CBA';
");
				using (var trans = conn.BeginTransaction())
				{
					var task = new AddZZZ_NKDataGroupColumnTask(8);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"SELECT 1 FROM sys.columns WHERE name = 'ZZO_ZZZ_NKDataGrouping' AND object_id = OBJECT_ID('dbo.RefVesselZZ')";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = @"SELECT ZZO_Code FROM RefVesselZZ";
					Assert.AreEqual("CBABOOM", cmd.ExecuteScalar());
				}
				using (var trans = conn.BeginTransaction())
				{
					var task = new AddZZZ_NKDataGroupColumnTask(8);
					task.Run(trans);
					trans.Commit();
				}
			}
		}
	}
}
