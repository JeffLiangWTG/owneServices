using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class UNDGSubstanceADNUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void UNDGSubstanceADNUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT UNDGSubstanceADN (ADN_PK, ADN_IsActive, ADN_UNNO, ADN_Variant, ADN_PSN, ADN_Class, ADN_ClassificationCode, ADN_PG, ADN_Labels, ADN_SpecialProvisions, ADN_ExceptedQuantityCode, ADN_LQMaxAmt, ADN_LQMaxAmtUQ, ADN_LQ2MaxAmt, ADN_LQ2MaxAmtUQ, ADN_CarriagePermittedPacks, ADN_CarriagePermittedBulk, ADN_CarriagePermittedTanks, ADN_CarriagePermittedDetails, ADN_EquipPPE, ADN_EquipEscapeDevice, ADN_EquipGasDetector, ADN_EquipToximeter, ADN_EquipBreathingApparatus, ADN_EquipmentDetails, ADN_Ventilation, ADN_LoadingSpecialProv, ADN_LoadingSpecialProvNote, ADN_UnloadingSpecialProv, ADN_UnloadingSpecialProvNote, ADN_OperationSpecialProv, ADN_OperationSpecialProvNote, ADN_BlueCones)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 1, 'AAA', 'AA', 'Test1', '', '', '', '', '', '', 0, '', 0, '', 1, 1, 1, '', 1, 1, 1, 1, 1, '', '', '', '', '', '', '', '', 0),
('A1C7AD0E-79C0-4431-A680-EEEED653F320', 1, 'BBB', 'BB', 'Test2', '', '', '', '', '', '', 0, '', 0, '', 1, 1, 1, '', 1, 1, 1, 1, 1, '', '', '', '', '', '', '', '', 0),
('23D8BD6F-2421-47A9-BF7C-BB455012EF7A', 1, 'DDD', 'DD', 'Test4', '', '', '', '', '', '', 0, '', 0, '', 1, 1, 1, '', 1, 1, 1, 1, 1, '', '', '', '', '', '', '', '', 0)

INSERT UNDGAttributeZZ (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','AAA','ADN','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'B','B','B','BBB','ADN','A1C7AD0E-79C0-4431-A680-EEEED653F320'),
(NEWID(),'D','D','D','DDD','ADN','23D8BD6F-2421-47A9-BF7C-BB455012EF7A')", trans);
					var info = new UNDGSubstanceADNUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ADN_PK, ADN_IsActive, ADN_UNNO, ADN_Variant, ADN_PSN, ADN_Class, ADN_ClassificationCode, ADN_PG, ADN_Labels, ADN_SpecialProvisions, ADN_ExceptedQuantityCode, ADN_LQMaxAmt, ADN_LQMaxAmtUQ, ADN_LQ2MaxAmt, ADN_LQ2MaxAmtUQ, ADN_CarriagePermittedPacks, ADN_CarriagePermittedBulk, ADN_CarriagePermittedTanks, ADN_CarriagePermittedDetails, ADN_EquipPPE, ADN_EquipEscapeDevice, ADN_EquipGasDetector, ADN_EquipToximeter, ADN_EquipBreathingApparatus, ADN_EquipmentDetails, ADN_Ventilation, ADN_LoadingSpecialProv, ADN_LoadingSpecialProvNote, ADN_UnloadingSpecialProv, ADN_UnloadingSpecialProvNote, ADN_OperationSpecialProv, ADN_OperationSpecialProvNote, ADN_BlueCones, Deleted)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 1, 'AAA', 'AA', 'Test1111', '', '', '', '', '', '', 0, '', 0, '', 1, 1, 1, '', 1, 1, 1, 1, 1, '', '', '', '', '', '', '', '', 0, 0),
(NEWID(), 1, 'BBB', 'BB', 'Test2', '', '', '', '', '', '', 0, '', 0, '', 1, 1, 1, '', 1, 1, 1, 1, 1, '', '', '', '', '', '', '', '', 0, 1),
('0B358C81-9724-443C-B4DF-2C04EF7F9988', 1, 'CCC', 'CC', 'Test3', '', '', '', '', '', '', 0, '', 0, '', 1, 1, 1, '', 1, 1, 1, 1, 1, '', '', '', '', '', '', '', '', 0, 0),
(NEWID(), 1, 'DDD', 'DD', 'Test4', '', '', '', '', '', '', 0, '', 0, '', 1, 1, 1, '', 1, 1, 1, 1, 1, '', '', '', '', '', '', '', '', 0, 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','QQQ','ADN','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'C','C','C','CCC','ADN','0B358C81-9724-443C-B4DF-2C04EF7F9988')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADN WHERE ADN_UNNO = 'AAA' AND ADN_PSN = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADN WHERE ADN_UNNO = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADN WHERE ADN_UNNO = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceADN WHERE ADN_UNNO = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'QQQ'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'DDD'", trans));
				}
			}
		}
	}
}
