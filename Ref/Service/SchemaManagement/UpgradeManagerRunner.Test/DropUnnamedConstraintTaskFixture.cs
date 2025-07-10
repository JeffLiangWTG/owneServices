using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class DropUnnamedConstraintTaskFixture
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
CREATE TABLE RefCusProcedure (
	ZZ6_Group VARCHAR(50) DEFAULT ('')
)
");
				using (var trans = conn.BeginTransaction())
				{
					var task = new DropUnnamedConstraintTask(8);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"SELECT COUNT(*) FROM 
	sys.tables t
	JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
	JOIN sys.columns c ON c.object_id = t.object_id and c.column_id = d.parent_column_id
WHERE t.name = 'ReCusProcedure' AND c.name = 'ZZ6_Group'";
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
	}
}
