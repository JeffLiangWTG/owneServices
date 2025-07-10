using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class DataSetPKTransformationTaskFixture
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
CREATE TABLE RefCusRate (
	ZZ2_PK uniqueidentifier,
	ZZ2_ZZ1_Tariff uniqueidentifier
)
GO
CREATE TRIGGER RefCusRate_Version_Update
	ON RefCusRate
	FOR Update
	AS
	THROW 51000, 'Error', 1;  
GO
CREATE TABLE RefCusRateAttribute (
	ZZJ_ZZ2_Rate uniqueidentifier
)
GO
CREATE TRIGGER RefCusRateAttribute_Version_Update
	ON RefCusRateAttribute
	FOR Update
	AS
	THROW 51000, 'Error', 1;  
");
				var dataSetPK = Guid.NewGuid();
				var ratePK = Guid.NewGuid();
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff) VALUES ('{ratePK}', '{dataSetPK}');
INSERT INTO RefCusRateAttribute (ZZJ_ZZ2_Rate) VALUES ('{ratePK}');
");
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationTask(12);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_DataSetPK = '{dataSetPK}' AND ZZ2_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRateAttribute WHERE ZZJ_DataSetPK = '{dataSetPK}' AND ZZJ_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationTask(12);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_DataSetPK = '{dataSetPK}' AND ZZ2_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRateAttribute WHERE ZZJ_DataSetPK = '{dataSetPK}' AND ZZJ_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
			}
		}
	}
}
