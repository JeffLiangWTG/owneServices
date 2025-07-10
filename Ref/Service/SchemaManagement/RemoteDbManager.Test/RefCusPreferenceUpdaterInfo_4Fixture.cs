using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusPreferenceUpdaterInfo_4Fixture
	{
		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusPreferenceUpdaterInfo_4(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','EUN','EUN')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(NEWID(), 'DE', 'German')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','EUN')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','EUN','Test','T','')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'CLASS', 'ZADOC', 'Testing', 'EUN')

INSERT INTO RefCusConditionCode (ZY7_PK, ZY7_ConditionCode, ZY7_Description, ZY7_ZZZ_NKDataGrouping)
VALUES('04F0025B-EA0C-4367-B547-99922DFE0F3D', 'AAA', 'code description', 'EUN')

INSERT INTO RefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_Description,ZX4_IsFormula,ZX4_ZZZ_NKDataGrouping) 
VALUES ('CB81B42C-3B0E-4149-804A-DC38A81CB7BA','TEST','Test Description',1,'EUN')

INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES ('49899FBA-46ED-4CDB-AFBA-339757D3B696','200','2000','EUN'),
('144228C5-E1B7-48B0-9992-D476D6E2BC9F','BBB','BBBB','EUN')

INSERT RefCusPreferenceLanguage (ZX9_PK,ZX9_ZX6_NKLanguage,ZX9_ZZS_Preference,ZX9_Description)
VALUES (NEWID(), 'DE', '49899FBA-46ED-4CDB-AFBA-339757D3B696', 'AAA'),
(NEWID(), 'DE', '144228C5-E1B7-48B0-9992-D476D6E2BC9F', 'BBB')

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description) 
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','EUN', 'GSP (R 12/978) - Annex IV'),
('4F367102-358F-41D6-84F4-A8C33BF9EFEC','METRO','EUN', 'Continental and Corsica');

INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula) 
VALUES ('D83003BA-A3B6-437B-8312-9079C190BB3C','49899FBA-46ED-4CDB-AFBA-339757D3B696','FDCA07B0-8E98-4A44-A609-D441FCF0E585','1900-01-01','2079-06-06','EUN','T1'),
('24354B32-E70B-468C-A547-38B6A0D964D5','144228C5-E1B7-48B0-9992-D476D6E2BC9F','FDCA07B0-8E98-4A44-A609-D441FCF0E585','1900-01-01','2079-06-06','EUN','T2');

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup, ZX1_ZY7_NKConditionCode, ZX1_AdditionalComment)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', NULL, '2019-12-14 00:00:00', '2079-06-06 23:59:00', '49899FBA-46ED-4CDB-AFBA-339757D3B696', 'EUN', 'A1', '', 1, 0, 1, 1, 'AAA', 'B'),
(NEWID(), 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '49899FBA-46ED-4CDB-AFBA-339757D3B696', 'EUN', 'A2', '', 1, 0, 1, 1, 'AAA', 'C'),
('1A394C90-EED7-404C-B132-029BDFE9A19B', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '144228C5-E1B7-48B0-9992-D476D6E2BC9F', 'EUN', 'B', '', 1, 0, 1, 1, 'AAA', 'D')

INSERT INTO RefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'AAA', 1),
(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', '1A394C90-EED7-404C-B132-029BDFE9A19B', 'BBB', 1)

INSERT RefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition, ZXJ_AdditionalComment)
VALUES(NEWID(), 'DE', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'Comment A'),
(NEWID(), 'DE', 'B', 'B','1A394C90-EED7-404C-B132-029BDFE9A19B', 'Comment B')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_ZZA_SecondTradeGroup)
VALUES ('236353D1-77F3-4F1A-A262-68548903FBC4','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','A',NULL),
('AEA4F10D-5E6E-47D6-92DA-35CEEE568E37','1A394C90-EED7-404C-B132-029BDFE9A19B','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','B',NULL)

INSERT RefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES ('FDB07712-41B8-445A-ACDB-EB92F0FE0FDE', '236353D1-77F3-4F1A-A262-68548903FBC4','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
('876B756E-7931-48FB-9A56-2FAE983120F1', 'AEA4F10D-5E6E-47D6-92DA-35CEEE568E37','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')
", trans);
					var info = new RefCusPreferenceUpdaterInfo_4();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping, Deleted)
VALUES (NEWID(),'200','2000','EUN', 1),
('71DBEC0F-CD1C-4CC8-A4A2-E43A86D30828','CCC','CCCC','EUN', 0),
('0204D553-1F5A-493A-B20C-1B784E5599A7','BBB','XBBB','EUN', 0)

INSERT #TempRefCusPreferenceLanguage (ZX9_PK,ZX9_ZX6_NKLanguage,ZX9_ZZS_Preference,ZX9_Description)
VALUES (NEWID(), 'DE', '71DBEC0F-CD1C-4CC8-A4A2-E43A86D30828', 'CCC'),
(NEWID(), 'DE', '0204D553-1F5A-493A-B20C-1B784E5599A7', 'XBB')

INSERT INTO #TempRefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('34F3646E-AE80-4528-BEAE-1710562D6F1B','2005','EUN'),
('4F367102-358F-41D6-84F4-A8C33BF9EFEC','METRO','EUN');

INSERT INTO #TempRefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup, ZX1_ZY7_NKConditionCode, ZX1_AdditionalComment)
VALUES ('6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '71DBEC0F-CD1C-4CC8-A4A2-E43A86D30828', 'EUN', 'C', '', 1, 0, 1, 1, 'AAA', 'B'),
('E02CC3E0-B738-47E4-924E-0982F9300D23', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '0204D553-1F5A-493A-B20C-1B784E5599A7', 'EUN', 'X', '', 1, 0, 1, 1, 'AAA', 'D')

INSERT INTO #TempRefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', '6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'CCC', 1),
(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'E02CC3E0-B738-47E4-924E-0982F9300D23', 'XBB', 1)

INSERT #TempRefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition, ZXJ_AdditionalComment)
VALUES(NEWID(), 'DE', 'C', 'C','6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'comment 1'),
(NEWID(), 'DE', 'XB', 'B','E02CC3E0-B738-47E4-924E-0982F9300D23', 'comment 2')

INSERT INTO #TempRefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_ZZA_SecondTradeGroup)
VALUES ('426CC028-D989-4091-B9B1-D6BF3AB79E76','6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','C','4F367102-358F-41D6-84F4-A8C33BF9EFEC'),
('8A2CD7D8-0372-4439-A7F2-B9F2EC2A8ADF','E02CC3E0-B738-47E4-924E-0982F9300D23','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','X','4F367102-358F-41D6-84F4-A8C33BF9EFEC')

INSERT #TempRefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES (NEWID(), '426CC028-D989-4091-B9B1-D6BF3AB79E76','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '8A2CD7D8-0372-4439-A7F2-B9F2EC2A8ADF','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreference WHERE ZZS_Description = '2000'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreferenceLanguage WHERE ZX9_Description = 'AAA'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRate JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK WHERE ZZ1_TariffCode = 'Test' AND ZZ2_ZZS_Preference IS NULL", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'A1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'A2' AND ZX1_ZZS_Preference IS NULL", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'B' AND ZX1_ZZS_Preference IS NOT NULL", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Source = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup WHERE ZZC_PK = 'FDB07712-41B8-445A-ACDB-EB92F0FE0FDE'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreference WHERE ZZS_Description = 'XBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreferenceLanguage WHERE ZX9_Description = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Comment = 'XB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'X' AND ZZT_ZZA_SecondTradeGroup = '4F367102-358F-41D6-84F4-A8C33BF9EFEC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup WHERE ZZC_PK = '876B756E-7931-48FB-9A56-2FAE983120F1'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreference WHERE ZZS_Description = 'CCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreferenceLanguage WHERE ZX9_Description = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Comment = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'C' AND ZZT_ZZA_SecondTradeGroup = '4F367102-358F-41D6-84F4-A8C33BF9EFEC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_OrderNumber = 'C'", trans));
				}
			}
		}
	}
}
