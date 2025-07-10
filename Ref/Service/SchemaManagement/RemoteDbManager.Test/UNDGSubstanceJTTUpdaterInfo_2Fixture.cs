using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class UNDGSubstanceJTTUpdaterInfo_2Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void UNDGSubstanceJTTUpdaterInfo_2(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT UNDGSubstanceJTT (JTT_PK, JTT_IsActive, JTT_UNNO, JTT_Variant, JTT_PSN, JTT_Class, JTT_ClassificationCode, JTT_PG, JTT_Labels, JTT_SpecialProvisions, JTT_LQMaxAmt, JTT_LQMaxAmtUQ, JTT_LQ2MaxAmt, JTT_LQ2MaxAmtUQ, JTT_ExceptedQuantityCode, JTT_PackIns, JTT_PackProv, JTT_MixedPackingProv, JTT_BulkTankIns, JTT_BulkTankSpecProv, JTT_TankCode, JTT_TankSpecProv, JTT_TankVehicle, JTT_TransportCategory, JTT_PackingSpecialProv, JTT_BulkSpecialProv, JTT_LoadingSpecialProv, JTT_OperationSpecialProv, JTT_HazardIDNumber)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 1, 'AAA', 'AA', '{new string('a', 280)}', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', ''),
('A1C7AD0E-79C0-4431-A680-EEEED653F320', 1, 'BBB', 'BB', '{new string('b', 290)}', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', ''),
('23D8BD6F-2421-47A9-BF7C-BB455012EF7A', 1, 'DDD', 'DD', '{new string('e', 300)}', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '')

INSERT UNDGAttributeZZ (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','AAA','JTT','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'B','B','B','BBB','JTT','A1C7AD0E-79C0-4431-A680-EEEED653F320'),
(NEWID(),'D','D','D','DDD','JTT','23D8BD6F-2421-47A9-BF7C-BB455012EF7A')", trans);
					var info = new UNDGSubstanceJTTUpdaterInfo_2();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (JTT_PK, JTT_IsActive, JTT_UNNO, JTT_Variant, JTT_PSN, JTT_Class, JTT_ClassificationCode, JTT_PG, JTT_Labels, JTT_SpecialProvisions, JTT_LQMaxAmt, JTT_LQMaxAmtUQ, JTT_LQ2MaxAmt, JTT_LQ2MaxAmtUQ, JTT_ExceptedQuantityCode, JTT_PackIns, JTT_PackProv, JTT_MixedPackingProv, JTT_BulkTankIns, JTT_BulkTankSpecProv, JTT_TankCode, JTT_TankSpecProv, JTT_TankVehicle, JTT_TransportCategory, JTT_PackingSpecialProv, JTT_BulkSpecialProv, JTT_LoadingSpecialProv, JTT_OperationSpecialProv, JTT_HazardIDNumber, Deleted)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 1, 'AAA', 'AA', '{new string('c', 270)}', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 0),
(NEWID(), 1, 'BBB', 'BB', '{new string('b', 290)}', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1),
('0B358C81-9724-443C-B4DF-2C04EF7F9988', 1, 'CCC', 'CC', '{new string('d', 260)}', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 0),
(NEWID(), 1, 'DDD', 'DD', '{new string('e', 300)}', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','QQQ','JTT','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'C','C','C','CCC','JTT','0B358C81-9724-443C-B4DF-2C04EF7F9988')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, $"SELECT COUNT(*) FROM UNDGSubstanceJTT WHERE JTT_UNNO = 'AAA' AND JTT_PSN = '{new string('c', 270)}'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGSubstanceJTT WHERE JTT_UNNO = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGSubstanceJTT WHERE JTT_UNNO = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGSubstanceJTT WHERE JTT_UNNO = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'QQQ'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'DDD'", trans));
				}
			}
		}
	}
}
