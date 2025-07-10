using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class DataSetPKTransformationOnTariffAttributeTaskFixture
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
CREATE TABLE RefCusTariffAttribute (
	ZZ3_PK uniqueidentifier,
	ZZ3_ZZ1_Tariff uniqueidentifier
)
GO
CREATE TRIGGER RefCusTariffAttribute_Version_Update
	ON RefCusTariffAttribute
	FOR Update
	AS
	THROW 51000, 'Error', 1;  
");
				var dataSetPK = Guid.NewGuid();
				var tariffAttributePK = Guid.NewGuid();
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTariffAttribute (ZZ3_PK, ZZ3_ZZ1_Tariff) VALUES ('{tariffAttributePK}', '{dataSetPK}');
");
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationOnTariffAttributeTask(15);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusTariffAttribute WHERE ZZ3_DataSetPK = '{dataSetPK}' AND ZZ3_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationOnTariffAttributeTask(15);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusTariffAttribute WHERE ZZ3_DataSetPK = '{dataSetPK}' AND ZZ3_DataSetCode = 'ZZ1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
			}
		}
	}
}
