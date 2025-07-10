using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class DataSetPKTransformationOnCodeOrAttributeTransportModeTaskFixture
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
CREATE TABLE RefCusCodeOrAttributeTransportMode (
	ZZU_PK uniqueidentifier,
	ZZU_ZZD_CodeList uniqueidentifier,
	ZZU_ZZE_Attribute uniqueidentifier
)
GO
CREATE TRIGGER RefCusCodeOrAttributeTransportMode_Version_Update
	ON RefCusCodeOrAttributeTransportMode
	FOR Update
	AS
	THROW 51000, 'Error', 1;  
GO
 CREATE TABLE RefCusCodeListAttribute(
	ZZE_PK uniqueidentifier NOT NULL,
	ZZE_ZZD_CodeList uniqueidentifier NOT NULL,
	ZZE_Name varchar(32) NOT NULL,
	ZZE_Value varchar(100) NOT NULL
)
GO
");
				var transportMode1 = Guid.NewGuid();
				var codeList = Guid.NewGuid();
				var transportMode2 = Guid.NewGuid();
				var attribute = Guid.NewGuid();
				var codeList2 = Guid.NewGuid();

				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_ZZD_CodeList) VALUES ('{transportMode1}', '{codeList}');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_ZZE_Attribute) VALUES ('{transportMode2}', '{attribute}');
");

				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusCodeListAttribute(ZZE_PK,ZZE_ZZD_CodeList,ZZE_Name,ZZE_Value) 
VALUES ('{attribute}','{codeList2}','XXX','XXX')
");
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationOnRefCusCodeOrAttributeTransportModeTask(21);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_DataSetPK = '{codeList}' AND ZZU_DataSetCode = 'ZZD'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_DataSetPK = '{codeList2}' AND ZZU_DataSetCode = 'ZZD'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
				using (var trans = conn.BeginTransaction())
				{
					var task = new DataSetPKTransformationOnRefCusCodeOrAttributeTransportModeTask(21);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_DataSetPK = '{codeList}' AND ZZU_DataSetCode = 'ZZD'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_DataSetPK = '{codeList2}' AND ZZU_DataSetCode = 'ZZD'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
			}
		}
	}
}
