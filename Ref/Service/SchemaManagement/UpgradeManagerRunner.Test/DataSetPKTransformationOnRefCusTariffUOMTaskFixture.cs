using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	public class DataSetPKTransformationOnRefCusTariffUOMTaskFixture
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
CREATE TABLE RefCusTariffUOM (
	ZZ8_PK uniqueidentifier,
	ZZ8_ZZ1_Tariff uniqueidentifier
)
GO
CREATE TRIGGER RefCusTariffUOM_Version_Update
	ON RefCusTariffUOM
	FOR Update
	AS
	THROW 51000, 'Error', 1;  
");
				var dataSetPK = Guid.NewGuid();
				var uomPK = Guid.NewGuid();
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTariffUOM (ZZ8_PK, ZZ8_ZZ1_Tariff) VALUES ('{uomPK}', '{dataSetPK}');
");
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationOnRefCusTariffUOMTask(15);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_DataSetPK = '{dataSetPK}' AND ZZ8_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationOnRefCusTariffUOMTask(15);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_DataSetPK = '{dataSetPK}' AND ZZ8_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
			}
		}
	}
}
