using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

[TestFixture]
class DropSanctionTablesTaskFixture
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
CREATE TABLE RefSanctionType (
	ZZV_PK UNIQUEIDENTIFIER DEFAULT (newid())
)
CREATE TABLE RefSanctionCountry (
	ZZU_PK UNIQUEIDENTIFIER DEFAULT (newid())
)
");
		using (var trans = conn.BeginTransaction())
		{
			var task = new DropSanctionTablesTask(14);
			task.Run(trans);
			trans.Commit();
		}
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = @"SELECT COUNT(*)
FROM sys.tables t
WHERE 
t.name = 'RefSanctionType' OR
t.name = 'RefSanctionCountry'
";
			Assert.AreEqual(0, cmd.ExecuteScalar());
		}
		using (var trans = conn.BeginTransaction())
		{
			var task = new DropUnnamedConstraintTask(8);
			task.Run(trans);
			trans.Commit();
		}
	}
}
