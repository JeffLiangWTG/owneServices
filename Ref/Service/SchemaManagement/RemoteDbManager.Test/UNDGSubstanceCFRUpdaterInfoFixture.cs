using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class UNDGSubstanceCFRUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void UNDGSubstanceCFRUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT UNDGSubstanceCFR (CFR_PK, CFR_UNNO, CFR_Variant, CFR_Prefix, CFR_CVL, CFR_PSN, CFR_Variation, CFR_PrimaryClass, CFR_SecondaryClass, CFR_TertiaryClass, CFR_MarinePollutant, CFR_ExceptedQuantity, CFR_LimitedQuantityPermitted, CFR_ReportableQuantity, CFR_ReportableQuantityUnit, CFR_GeneralStowage, CFR_PassengerStowage, CFR_StowageCategory, CFR_StowageCodes, CFR_StowageIMDGCodes, CFR_BulkPackingInstructions, CFR_BulkPackingProvisions, CFR_IBCInstructions, CFR_IBCProvisions, CFR_PackingExceptions, CFR_PackingInstructions, CFR_PackingProvisions, CFR_PackingGroup, CFR_SpecialProvisions, CFR_TankInstructions, CFR_TankProvisions, CFR_PoisonInhalationHazard, CFR_State, CFR_IsFixedPSN, CFR_AppliesForAirTransport, CFR_AppliesForDomesticTransport, CFR_AppliesForInternationalTransport, CFR_AppliesForVesselTransport, CFR_RequiresTechnicalNameInParenthesis, CFR_EmergencyResponseGuide, CFR_TechnicalName, CFR_TreatAs, CFR_PAXAirRailLimit, CFR_PAXAirRailLimitUnit, CFR_CargoAirRailLimit, CFR_CargoAirRailLimitUnit, CFR_IsActive, CFR_LQMaxAmt, CFR_LQMaxAmtUQ, CFR_PAXAirRailLimitType, CFR_CargoAirRailLimitType)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 'AAA', 'AA', '', '','Test1', '', '', '', '', '', '', 0, 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1, 1, 1, 1, 1, 1, '', '', '', 0, '', 0, '', 1, 0, '', 'FOB', 'FOB' ),
('A1C7AD0E-79C0-4431-A680-EEEED653F320', 'BBB', 'BB', '', '','Test2', '', '', '', '', '', '', 0, 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1, 1, 1, 1, 1, 1, '', '', '', 0, '', 0, '', 1, 0, '', 'FOB', 'FOB' ),
('23D8BD6F-2421-47A9-BF7C-BB455012EF7A', 'DDD', 'DD', '', '','Test4', '', '', '', '', '', '', 0, 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1, 1, 1, 1, 1, 1, '', '', '', 0, '', 0, '', 1, 0, '', 'FOB', 'FOB' )

INSERT UNDGAttributeZZ (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','AAA','CFR','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'B','B','B','BBB','CFR','A1C7AD0E-79C0-4431-A680-EEEED653F320'),
(NEWID(),'D','D','D','DDD','CFR','23D8BD6F-2421-47A9-BF7C-BB455012EF7A')", trans);
					var info = new UNDGSubstanceCFRUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (CFR_PK, CFR_UNNO, CFR_Variant, CFR_Prefix, CFR_CVL, CFR_PSN, CFR_Variation, CFR_PrimaryClass, CFR_SecondaryClass, CFR_TertiaryClass, CFR_MarinePollutant, CFR_ExceptedQuantity, CFR_LimitedQuantityPermitted, CFR_ReportableQuantity, CFR_ReportableQuantityUnit, CFR_GeneralStowage, CFR_PassengerStowage, CFR_StowageCategory, CFR_StowageCodes, CFR_StowageIMDGCodes, CFR_BulkPackingInstructions, CFR_BulkPackingProvisions, CFR_IBCInstructions, CFR_IBCProvisions, CFR_PackingExceptions, CFR_PackingInstructions, CFR_PackingProvisions, CFR_PackingGroup, CFR_SpecialProvisions, CFR_TankInstructions, CFR_TankProvisions, CFR_PoisonInhalationHazard, CFR_State, CFR_IsFixedPSN, CFR_AppliesForAirTransport, CFR_AppliesForDomesticTransport, CFR_AppliesForInternationalTransport, CFR_AppliesForVesselTransport, CFR_RequiresTechnicalNameInParenthesis, CFR_EmergencyResponseGuide, CFR_TechnicalName, CFR_TreatAs, CFR_PAXAirRailLimit, CFR_PAXAirRailLimitUnit, CFR_CargoAirRailLimit, CFR_CargoAirRailLimitUnit, CFR_IsActive, CFR_LQMaxAmt, CFR_LQMaxAmtUQ, CFR_PAXAirRailLimitType, CFR_CargoAirRailLimitType, Deleted)
VALUES ('1E37BAE2-7389-4515-8EAC-18B4FE903356', 'AAA', 'AA', '', '', 'Test1111', '', '', '', '', '', '', 0, 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1, 1, 1, 1, 1, 1, '', '', '', 0, '', 0, '', 1, 0, '', 'FOB', 'FOB', 0),
(NEWID(), 'BBB', 'BB', '', '','Test2', '', '', '', '', '', '', 0, 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1, 1, 1, 1, 1, 1, '', '', '', 0, '', 0, '', 1, 0, '', 'FOB', 'FOB', 1),
('0B358C81-9724-443C-B4DF-2C04EF7F9988', 'CCC', 'CC', '', '','Test3', '', '', '', '', '', '', 0, 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1, 1, 1, 1, 1, 1, '', '', '', 0, '', 0, '', 1, 0, '', 'FOB', 'FOB', 0),
(NEWID(), 'DDD', 'DD', '', '','Test4', '', '', '', '', '', '', 0, 0, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 1, 1, 1, 1, 1, 1, '', '', '', 0, '', 0, '', 1, 0, '', 'FOB', 'FOB', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (DAZ_PK,DAZ_Language,DAZ_Type,DAZ_Index,DAZ_Descriptor,DAZ_ParentCode,DAZ_ParentPK)
VALUES(NEWID(),'A','A','A','QQQ','CFR','1E37BAE2-7389-4515-8EAC-18B4FE903356'),
(NEWID(),'C','C','C','CCC','CFR','0B358C81-9724-443C-B4DF-2C04EF7F9988')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceCFR WHERE CFR_UNNO = 'AAA' AND CFR_PSN = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceCFR WHERE CFR_UNNO = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceCFR WHERE CFR_UNNO = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGSubstanceCFR WHERE CFR_UNNO = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'QQQ'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM UNDGAttributeZZ WHERE DAZ_Descriptor = 'DDD'", trans));
				}
			}
		}
	}
}
