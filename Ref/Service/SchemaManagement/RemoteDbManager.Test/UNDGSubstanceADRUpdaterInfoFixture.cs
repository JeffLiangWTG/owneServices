using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class UNDGSubstanceADRUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void UNDGSubstanceADRUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT UNDGSubstanceADR (ADR_PK, ADR_IsActive, ADR_UNNO, ADR_Variant, ADR_PSN, ADR_Class, ADR_ClassificationCode, ADR_PG, ADR_Labels, ADR_SpecialProvisions, ADR_LQMaxAmt, ADR_LQMaxAmtUQ, ADR_LQ2MaxAmt, ADR_LQ2MaxAmtUQ, ADR_ExceptedQuantityCode, ADR_PackIns, ADR_PackProv, ADR_MixedPackingProv, ADR_BulkTankIns, ADR_BulkTankSpecProv, ADR_ADRTankCode, ADR_ADRTankSpecProv, ADR_TankVehicle, ADR_TransportCategory, ADR_PackingSpecialProv, ADR_BulkSpecialProv, ADR_LoadingSpecialProv, ADR_OperationSpecialProv, ADR_HazardIDNumber)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 1, 'AAA', 'AA', 'Test1', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', ''),
('A1C7AD0E-79C0-4431-A680-EEEED653F320', 1, 'BBB', 'BB', 'Test2', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', ''),
('23D8BD6F-2421-47A9-BF7C-BB455012EF7A', 1, 'DDD', 'DD', 'Test4', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '')

INSERT UNDGAttributeZZ (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','AAA','ADR','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'B','B','B','BBB','ADR','A1C7AD0E-79C0-4431-A680-EEEED653F320'),
(NEWID(),'D','D','D','DDD','ADR','23D8BD6F-2421-47A9-BF7C-BB455012EF7A')", trans);
					var info = new UNDGSubstanceADRUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ADR_PK, ADR_IsActive, ADR_UNNO, ADR_Variant, ADR_PSN, ADR_Class, ADR_ClassificationCode, ADR_PG, ADR_Labels, ADR_SpecialProvisions, ADR_LQMaxAmt, ADR_LQMaxAmtUQ, ADR_LQ2MaxAmt, ADR_LQ2MaxAmtUQ, ADR_ExceptedQuantityCode, ADR_PackIns, ADR_PackProv, ADR_MixedPackingProv, ADR_BulkTankIns, ADR_BulkTankSpecProv, ADR_ADRTankCode, ADR_ADRTankSpecProv, ADR_TankVehicle, ADR_TransportCategory, ADR_PackingSpecialProv, ADR_BulkSpecialProv, ADR_LoadingSpecialProv, ADR_OperationSpecialProv, ADR_HazardIDNumber, Deleted)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 1, 'AAA', 'AA', 'Test1111', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 0),
(NEWID(), 1, 'BBB', 'BB', 'Test2', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1),
('0B358C81-9724-443C-B4DF-2C04EF7F9988', 1, 'CCC', 'CC', 'Test3', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 0),
(NEWID(), 1, 'DDD', 'DD', 'Test4', '', '', '', '', '', 0, '', 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','QQQ','ADR','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'C','C','C','CCC','ADR','0B358C81-9724-443C-B4DF-2C04EF7F9988')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADR WHERE ADR_UNNO = 'AAA' AND ADR_PSN = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADR WHERE ADR_UNNO = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADR WHERE ADR_UNNO = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADR WHERE ADR_UNNO = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'QQQ'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'DDD'", trans));
				}
			}
		}
	}
}
