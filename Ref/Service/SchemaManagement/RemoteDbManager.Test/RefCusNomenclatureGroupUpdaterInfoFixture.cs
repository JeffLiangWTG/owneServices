using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusNomenclatureGroupUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusNomenclatureGroupUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','EUN','EUN')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(NEWID(), 'DE', 'German')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'CLASS', 'ZADOC', 'Testing', 'EUN')

INSERT INTO RefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_Description,ZX4_IsFormula,ZX4_ZZZ_NKDataGrouping) 
VALUES ('CB81B42C-3B0E-4149-804A-DC38A81CB7BA','TEST','Test Description',1,'EUN')

INSERT INTO RefCusNomenclatureGroup (ZZ5_PK,ZZ5_Value,ZZ5_Description,ZZ5_StartDate,ZZ5_EndDate,ZZ5_CompositeKey,ZZ5_ZZZ_NKDataGrouping,ZZ5_ZZ9_NKNomenclatureGroupType)
VALUES ('2718BC12-085A-42C6-AA91-B085E006E0EF', 'AAA', 'AAAAA', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'A','EUN', ''),
('8C0880B2-F2D3-491D-9B58-688E123AEFB0', 'BBB', 'BBBBB', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'B','EUN', '')

INSERT INTO RefCusNomenclatureGroupNote (ZZL_PK,ZZL_ZZ5_NomenclatureGroup,ZZL_NoteType,ZZL_Note,ZZL_ZZZ_NKDataGrouping,ZZL_ZX6_NKLanguage)
VALUES (NEWID(), '2718BC12-085A-42C6-AA91-B085E006E0EF', 'AAA', 'A', 'EUN', 'DE'),
(NEWID(), '8C0880B2-F2D3-491D-9B58-688E123AEFB0', 'BBB', 'B', 'EUN', 'DE')

INSERT INTO RefCusNomenclatureLanguage (ZX8_PK,ZX8_ZX6_NKLanguage,ZX8_ZZ5_NomenclatureGroup,ZX8_Description)
VALUES (NEWID(), 'DE', '2718BC12-085A-42C6-AA91-B085E006E0EF', 'BBB'),
(NEWID(), 'DE', '8C0880B2-F2D3-491D-9B58-688E123AEFB0', 'BBB')

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description) 
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','EUN', 'GSP (R 12/978) - Annex IV')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ5_Nomenclature, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', '2718BC12-085A-42C6-AA91-B085E006E0EF', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'A1', '', 1, 0, 1, 1),
('1A394C90-EED7-404C-B132-029BDFE9A19B', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', '8C0880B2-F2D3-491D-9B58-688E123AEFB0', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'B', '', 1, 0, 1, 1)

INSERT INTO RefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'AAA', 1),
(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', '1A394C90-EED7-404C-B132-029BDFE9A19B', 'BBB', 1)

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('236353D1-77F3-4F1A-A262-68548903FBC4','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','A'),
('AEA4F10D-5E6E-47D6-92DA-35CEEE568E37','1A394C90-EED7-404C-B132-029BDFE9A19B','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','B')

INSERT RefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES ('FDB07712-41B8-445A-ACDB-EB92F0FE0FDE', '236353D1-77F3-4F1A-A262-68548903FBC4','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
('876B756E-7931-48FB-9A56-2FAE983120F1', 'AEA4F10D-5E6E-47D6-92DA-35CEEE568E37','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')
", trans);
					var info = new RefCusNomenclatureGroupUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusNomenclatureGroup (ZZ5_PK,ZZ5_Value,ZZ5_Description,ZZ5_StartDate,ZZ5_EndDate,ZZ5_CompositeKey,ZZ5_ZZZ_NKDataGrouping,ZZ5_ZZ9_NKNomenclatureGroupType, Deleted)
VALUES (NEWID(), 'AAA', 'AAAAA', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'A','EUN', '', 1),
('35511E42-31C4-469B-B232-F85A5240B333', 'BBB', 'XBBBB', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'B','EUN', '', 0),
('C1A1E679-2FED-424B-B5EB-30C8A48F1489', 'CCC', 'CCCCC', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'C','EUN', '', 0)

INSERT INTO #TempRefCusNomenclatureGroupNote (ZZL_PK,ZZL_ZZ5_NomenclatureGroup,ZZL_NoteType,ZZL_Note,ZZL_ZZZ_NKDataGrouping,ZZL_ZX6_NKLanguage)
VALUES (NEWID(), 'C1A1E679-2FED-424B-B5EB-30C8A48F1489', 'CCC', 'C', 'EUN', 'DE'),
(NEWID(), '35511E42-31C4-469B-B232-F85A5240B333', 'BBB', 'X', 'EUN', 'DE')

INSERT INTO #TempRefCusNomenclatureLanguage (ZX8_PK,ZX8_ZX6_NKLanguage,ZX8_ZZ5_NomenclatureGroup,ZX8_Description)
VALUES (NEWID(), 'DE', 'C1A1E679-2FED-424B-B5EB-30C8A48F1489', 'CCC'),
(NEWID(), 'DE', '35511E42-31C4-469B-B232-F85A5240B333', 'XBB')

INSERT INTO #TempRefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('34F3646E-AE80-4528-BEAE-1710562D6F1B','2005','EUN')

INSERT INTO #TempRefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ5_Nomenclature, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup)
VALUES ('6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'C1A1E679-2FED-424B-B5EB-30C8A48F1489', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'C', '', 1, 0, 1, 1),
('E02CC3E0-B738-47E4-924E-0982F9300D23', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', '35511E42-31C4-469B-B232-F85A5240B333', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'X', '', 1, 0, 1, 1)

INSERT INTO #TempRefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', '6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'CCC', 1),
(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'E02CC3E0-B738-47E4-924E-0982F9300D23', 'XBB', 1)

INSERT INTO #TempRefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('426CC028-D989-4091-B9B1-D6BF3AB79E76','6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','C'),
('8A2CD7D8-0372-4439-A7F2-B9F2EC2A8ADF','E02CC3E0-B738-47E4-924E-0982F9300D23','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','X')

INSERT #TempRefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES (NEWID(), '426CC028-D989-4091-B9B1-D6BF3AB79E76','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '8A2CD7D8-0372-4439-A7F2-B9F2EC2A8ADF','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroup WHERE ZZ5_Description = 'AAAAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupNote WHERE ZZL_Note = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureLanguage WHERE ZX8_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup WHERE ZZC_PK = 'FDB07712-41B8-445A-ACDB-EB92F0FE0FDE'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroup WHERE ZZ5_Description = 'XBBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupNote WHERE ZZL_Note = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureLanguage WHERE ZX8_Description = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_OrderNumber = 'X'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroup WHERE ZZ5_Description = 'CCCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupNote WHERE ZZL_Note = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureLanguage WHERE ZX8_Description = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_OrderNumber = 'C'", trans));
				}
			}
		}
	}
}
