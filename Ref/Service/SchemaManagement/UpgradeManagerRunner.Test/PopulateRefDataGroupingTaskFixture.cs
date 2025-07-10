using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class PopulateRefDataGroupingTaskFixture
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
CREATE TABLE RefDbVersionControl
(
	ParentPK uniqueidentifier NOT NULL,
	ParentCode char(3) NOT NULL,
	LastUpdatedUTC datetime2 NOT NULL,
	Deleted BIT NOT NULL DEFAULT 0,
	CreatedTimeUTC datetime2 NOT NULL CONSTRAINT DF_RefDbVersionControl_CreatedTimeUTC DEFAULT SYSUTCDATETIME(),
)");
				using (var trans = conn.BeginTransaction())
				{
					var task = new PopulateRefDataGroupingTask(8);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"SELECT COUNT(*) FROM RefDataGrouping";
					Assert.That(cmd.ExecuteScalar(), Is.GreaterThan(0));
				}
				using (var trans = conn.BeginTransaction())
				{
					var task = new PopulateRefDataGroupingTask(8);
					task.Run(trans);
					trans.Commit();
				}
			}
		}
	}
}
