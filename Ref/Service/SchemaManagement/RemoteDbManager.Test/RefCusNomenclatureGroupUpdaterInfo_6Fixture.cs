using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusNomenclatureGroupUpdaterInfo_6Fixture
	{
		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusNomenclatureGroupUpdaterInfo_6(DbSchema dbSchema)
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

INSERT INTO RefCusConditionCode (ZY7_PK, ZY7_ConditionCode, ZY7_Description, ZY7_ZZZ_NKDataGrouping)
VALUES('066B8530-055E-4FED-96E5-6DE5924533D0', 'AAA', 'code description', 'EUN')

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
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','EUN','GSP (R 12/978) - Annex IV'),
('4F367102-358F-41D6-84F4-A8C33BF9EFEC','METRO','EUN','Continental and Corsica');

INSERT INTO RefCusTariffBRCharacteristic (ZB1_PK,ZB1_CharacteristicType,ZB1_ZZ1_Tariff,ZB1_ZZ5_Nomenclature,ZB1_Style,ZB1_MaxLength,ZB1_DecimalPlaces,ZB1_Code,ZB1_Text,ZB1_StartDate,ZB1_EndDate,ZB1_IsImport,ZB1_IsExport,ZB1_IsMandatory,ZB1_IsConditioningAttribute)
VALUES('BF158FF4-A240-4188-8552-8A254E830A7B', 'NVE', NULL, '2718BC12-085A-42C6-AA91-B085E006E0EF', 'LIST', 10, 4, 'A1', 'A1', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 1, 0, 1, 1),
('41845144-ED33-4A3F-966D-80C99EB08187', 'NVE', NULL, '8C0880B2-F2D3-491D-9B58-688E123AEFB0', 'LIST', 10, 4, 'B', 'B', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 1, 0, 1, 1)

INSERT INTO RefCusTariffBRCharacteristicAttribute (ZB3_PK,ZB3_ZB1_Characteristic,ZB3_Name,ZB3_Code,ZB3_Value)
VALUES('817B669C-498F-40D4-9AD4-FE39D41CA238', 'BF158FF4-A240-4188-8552-8A254E830A7B', 'A', 'A', 'A'),
('CFB07E80-C1E3-4F05-AF01-533E51338453', '41845144-ED33-4A3F-966D-80C99EB08187', 'B', 'B', 'B')

INSERT INTO RefCusTariffBRCharacteristicValue (ZB2_PK,ZB2_ZB1_Characteristic,ZB2_Value,ZB2_Description)
VALUES('38364175-1781-4CBD-9636-6E366883836D', 'BF158FF4-A240-4188-8552-8A254E830A7B', 'AAA', 'AAA'),
('C6890E13-E0BA-4BBE-A503-D6AACA4B2141', '41845144-ED33-4A3F-966D-80C99EB08187', 'BBB', 'BBB')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ5_Nomenclature, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup, ZX1_ZY7_NKConditionCode, ZX1_AdditionalComment, ZX1_Severity)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', '2718BC12-085A-42C6-AA91-B085E006E0EF', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'A1', '', 1, 0, 1, 1, 'AAA', 'comment 1', 'MSG'),
('1A394C90-EED7-404C-B132-029BDFE9A19B', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', '8C0880B2-F2D3-491D-9B58-688E123AEFB0', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'B', '', 1, 0, 1, 1, 'AAA', 'comment 2', 'WAR')

INSERT INTO RefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'AAA', 1),
(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', '1A394C90-EED7-404C-B132-029BDFE9A19B', 'BBB', 1)

INSERT RefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition, ZXJ_AdditionalComment)
VALUES(NEWID(), 'DE', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'C'),
(NEWID(), 'DE', 'B', 'B','1A394C90-EED7-404C-B132-029BDFE9A19B', 'D')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_ZZA_SecondTradeGroup)
VALUES ('236353D1-77F3-4F1A-A262-68548903FBC4','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','A',NULL),
('AEA4F10D-5E6E-47D6-92DA-35CEEE568E37','1A394C90-EED7-404C-B132-029BDFE9A19B','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','B',NULL)

INSERT RefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES ('FDB07712-41B8-445A-ACDB-EB92F0FE0FDE', '236353D1-77F3-4F1A-A262-68548903FBC4','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
('876B756E-7931-48FB-9A56-2FAE983120F1', 'AEA4F10D-5E6E-47D6-92DA-35CEEE568E37','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')
", trans);
					var info = new RefCusNomenclatureGroupUpdaterInfo_6();
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
VALUES ('34F3646E-AE80-4528-BEAE-1710562D6F1B','2005','EUN'),
('4F367102-358F-41D6-84F4-A8C33BF9EFEC','METRO','EUN');

INSERT #TempRefCusTariffBRCharacteristic (ZB1_PK,ZB1_CharacteristicType,ZB1_ZZ1_Tariff,ZB1_ZZ5_Nomenclature,ZB1_Style,ZB1_MaxLength,ZB1_DecimalPlaces,ZB1_Code,ZB1_Text,ZB1_StartDate,ZB1_EndDate,ZB1_IsImport,ZB1_IsExport,ZB1_IsMandatory,ZB1_IsConditioningAttribute)
VALUES('E0E20278-9095-42FF-A454-79DB6E29ACE5', 'NVE', NULL, 'C1A1E679-2FED-424B-B5EB-30C8A48F1489', 'LIST', 10, 4, 'C', 'C', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 1, 0, 1, 1),
('84ECF728-8DFF-4CAE-8AA2-586D2CADB2FE', 'NVE', NULL, '35511E42-31C4-469B-B232-F85A5240B333', 'LIST', 10, 4, 'X', 'X', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 1, 0, 1, 1)

INSERT #TempRefCusTariffBRCharacteristicAttribute (ZB3_PK,ZB3_ZB1_Characteristic,ZB3_Name,ZB3_Code,ZB3_Value)
VALUES('817B669C-498F-40D4-9AD4-FE39D41CA238', 'E0E20278-9095-42FF-A454-79DB6E29ACE5', 'C', 'C', 'C'),
('CFB07E80-C1E3-4F05-AF01-533E51338453', '84ECF728-8DFF-4CAE-8AA2-586D2CADB2FE', 'B', 'B', 'B')

INSERT #TempRefCusTariffBRCharacteristicValue (ZB2_PK,ZB2_ZB1_Characteristic,ZB2_Value,ZB2_Description)
VALUES('38364175-1781-4CBD-9636-6E366883836D', 'E0E20278-9095-42FF-A454-79DB6E29ACE5', 'CCC', 'CCC'),
('C6890E13-E0BA-4BBE-A503-D6AACA4B2141', '84ECF728-8DFF-4CAE-8AA2-586D2CADB2FE', 'XBB', 'XBB')

INSERT INTO #TempRefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ5_Nomenclature, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup, ZX1_ZY7_NKConditionCode, ZX1_AdditionalComment, ZX1_Severity)
VALUES ('6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'C1A1E679-2FED-424B-B5EB-30C8A48F1489', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'C', '', 1, 0, 1, 1, 'AAA', 'C', 'MSG'),
('E02CC3E0-B738-47E4-924E-0982F9300D23', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', '35511E42-31C4-469B-B232-F85A5240B333', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'X', '', 1, 0, 1, 1, 'AAA', 'D', 'WAR')

INSERT INTO #TempRefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', '6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'CCC', 1),
(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'E02CC3E0-B738-47E4-924E-0982F9300D23', 'XBB', 1)

INSERT #TempRefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition, ZXJ_AdditionalComment)
VALUES(NEWID(), 'DE', 'C', 'C','6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2', 'A'),
(NEWID(), 'DE', 'XB', 'B','E02CC3E0-B738-47E4-924E-0982F9300D23', 'B')

INSERT INTO #TempRefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_ZZA_SecondTradeGroup)
VALUES ('426CC028-D989-4091-B9B1-D6BF3AB79E76','6FC4943F-52B3-4CFA-BAA2-850CE71DCEE2','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','C','4F367102-358F-41D6-84F4-A8C33BF9EFEC'),
('8A2CD7D8-0372-4439-A7F2-B9F2EC2A8ADF','E02CC3E0-B738-47E4-924E-0982F9300D23','1900-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','X','4F367102-358F-41D6-84F4-A8C33BF9EFEC')

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
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Source = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup WHERE ZZC_PK = 'FDB07712-41B8-445A-ACDB-EB92F0FE0FDE'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristic WHERE ZB1_Code = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristicValue WHERE ZB2_Value = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristicAttribute WHERE ZB3_Code = 'A'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroup WHERE ZZ5_Description = 'XBBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupNote WHERE ZZL_Note = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureLanguage WHERE ZX8_Description = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Comment = 'XB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'X' AND ZZT_ZZA_SecondTradeGroup = '4F367102-358F-41D6-84F4-A8C33BF9EFEC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_OrderNumber = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristic WHERE ZB1_Code = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristicValue WHERE ZB2_Value = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristicAttribute WHERE ZB3_Code = 'B'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroup WHERE ZZ5_Description = 'CCCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupNote WHERE ZZL_Note = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureLanguage WHERE ZX8_Description = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Comment = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_OrderNumber = 'C' AND ZZT_ZZA_SecondTradeGroup = '4F367102-358F-41D6-84F4-A8C33BF9EFEC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_OrderNumber = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristic WHERE ZB1_Code = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristicValue WHERE ZB2_Value = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristicAttribute WHERE ZB3_Code = 'C'", trans));
				}
			}
		}
	}
}
