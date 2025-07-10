using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTariffUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusTariffUpdaterInfo(DbSchema dbSchema)
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

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','EUN')

INSERT RefCusTariffAdditionalCodeCategory (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'AAA', 'EUN')

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description) 
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','EUN', 'GSP (R 12/978) - Annex IV')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','EUN','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01')

INSERT INTO RefCusTariffNationalCode (ZZW_PK,ZZW_ZZ1_Tariff,ZZW_NationalCode,ZZW_Description,ZZW_ZZF_NKTaxOrFeeCode,ZZW_StartDate,ZZW_EndDate,ZZW_ZZZ_NKDataGrouping,ZZW_PublishedDate)
VALUES ('8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'AAAAA', 'AAAA','', '1900-01-01','2079-06-06', 'EUN', '1900-01-01')

INSERT INTO RefCusTariffAttribute (ZZ3_PK,ZZ3_ZZ1_Tariff,ZZ3_Name,ZZ3_Value,ZZ3_ZZW_TariffNationalCode)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'AAA', 'AAA1', NULL),
(NEWID(), NULL, 'AAA', 'AAA2', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8')

INSERT INTO RefCusTariffUOM (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_ZZW_TariffNationalCode,ZZ8_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'CU2', 'A1', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', NULL, 'EUN'),
(NEWID(), NULL, 'CU2', 'A2', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'EUN')

INSERT INTO RefCusTariffAdditionalCode (ZY2_PK,ZY2_ZZ1_Tariff,ZY2_ZZW_NationalCode,ZY2_AdditionalCode,ZY2_Description,ZY2_ZY3_NKCategory,ZY2_IsMandatory,ZY2_ZZZ_NKDataGrouping)
VALUES ('7558A41B-F5AE-46A0-AB6E-510117207DFF', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, 'AAA', 'A1', 'A', 1, 'EUN'),
('7558A41B-F5AE-46A0-AB6E-510117207DF1', NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'AAA', 'A2', 'A', 1, 'EUN')

INSERT INTO RefCusTariffAdditionalCodeLanguage (ZY4_PK,ZY4_ZX6_NKLanguage,ZY4_ZY2_TariffAdditionalCode,ZY4_Description)
VALUES (NEWID(), 'DE', '7558A41B-F5AE-46A0-AB6E-510117207DFF', 'A1'),
(NEWID(), 'DE', '7558A41B-F5AE-46A0-AB6E-510117207DF1', 'A2')

INSERT INTO RefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_ZZA_TradeGroup,ZX5_VATCategory)
VALUES ('7558A41B-F5AE-46A0-AB6E-510117207DFF', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, '', '1900-01-01','2079-06-06', 'A', 'A1', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', ''),
('7558A41B-F5AE-46A0-AB6E-510117207DF1', NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '', '1900-01-01','2079-06-06', 'A', 'A2', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '')

INSERT INTO RefCusTariffLanguage (ZX7_PK,ZX7_ZX6_NKLanguage,ZX7_ZZ1_Tariff,ZX7_Description)
VALUES (NEWID(), 'DE', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'AAA')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'CLASS', 'ZADOC', 'Testing', 'EUN')

INSERT INTO RefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_Description,ZX4_IsFormula,ZX4_ZZZ_NKDataGrouping) 
VALUES ('CB81B42C-3B0E-4149-804A-DC38A81CB7BA','TEST','Test Description',1,'EUN')

INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_ZZW_TariffNationalCode) 
VALUES ('D83003BA-A3B6-437B-8312-9079C190BB3C','FDCA07B0-8E98-4A44-A609-D441FCF0E585','1900-01-01','2079-06-06','EUN','A1', NULL),
('D83003BA-A3B6-437B-8312-9079C190BB3D',NULL,'1900-01-01','2079-06-06','EUN','A2', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'A', '', 1, 0, 1, 1)

INSERT INTO RefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'AAA', 1)

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_ZZ2_Rate,ZZT_ZY2_AdditionalCode,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber )
VALUES ('236353D1-77F3-4F1A-A262-68548903FBC1','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', NULL, NULL,'1901-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-68548903FBC2',NULL, 'D83003BA-A3B6-437B-8312-9079C190BB3C',NULL,'1902-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-68548903FBC3',NULL, NULL,'7558A41B-F5AE-46A0-AB6E-510117207DFF','1903-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-68548903FBC4',NULL, 'D83003BA-A3B6-437B-8312-9079C190BB3D', NULL,'1904-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-68548903FBC5',NULL, NULL,'7558A41B-F5AE-46A0-AB6E-510117207DF1','1905-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','')

INSERT RefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES (NEWID(), '236353D1-77F3-4F1A-A262-68548903FBC1','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-68548903FBC2','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-68548903FBC3','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-68548903FBC4','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-68548903FBC5','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')
", trans);
					var info = new RefCusTariffUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5071', '1P1','EUN')

INSERT INTO #TempRefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate, Deleted)
VALUES (NEWID(), '7EABDFE4-F6F0-4F33-BC3A-D20863ED5071','EUN','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01', 1),
('FDCA07B0-8E98-4A44-A609-0441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5071','EUN','CCCC','C','', 1,'1900-01-01','2079-06-06','A','1900-01-01', 0)

INSERT INTO #TempRefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','EUN')

INSERT INTO #TempRefCusTariffNationalCode (ZZW_PK,ZZW_ZZ1_Tariff,ZZW_NationalCode,ZZW_Description,ZZW_ZZF_NKTaxOrFeeCode,ZZW_StartDate,ZZW_EndDate,ZZW_ZZZ_NKDataGrouping,ZZW_PublishedDate)
VALUES 
('8F6E0739-434F-4D9A-95B6-093EA040ABC8', 'FDCA07B0-8E98-4A44-A609-0441FCF0E585', 'CCCCC', 'CCCC','', '1900-01-01','2079-06-06', 'EUN', '1900-01-01')

INSERT INTO #TempRefCusTariffAttribute (ZZ3_PK,ZZ3_ZZ1_Tariff,ZZ3_Name,ZZ3_Value,ZZ3_ZZW_TariffNationalCode)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-0441FCF0E585', 'CCC', 'CCC1', NULL),
(NEWID(), NULL, 'CCC', 'CCC2', '8F6E0739-434F-4D9A-95B6-093EA040ABC8')

INSERT INTO #TempRefCusTariffUOM (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_ZZW_TariffNationalCode,ZZ8_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-0441FCF0E585', 'CU3', 'C1', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', NULL, 'EUN'),
(NEWID(), NULL, 'CU3', 'C2', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '8F6E0739-434F-4D9A-95B6-093EA040ABC8', 'EUN')

INSERT INTO #TempRefCusTariffAdditionalCode (ZY2_PK,ZY2_ZZ1_Tariff,ZY2_ZZW_NationalCode,ZY2_AdditionalCode,ZY2_Description,ZY2_ZY3_NKCategory,ZY2_IsMandatory,ZY2_ZZZ_NKDataGrouping)
VALUES ('7558A41B-F5AE-46A0-AB6E-010117207DFF', 'FDCA07B0-8E98-4A44-A609-0441FCF0E585', NULL, 'CCC', 'C1', 'A', 1, 'EUN'),
('7558A41B-F5AE-46A0-AB6E-010117207DF1', NULL, '8F6E0739-434F-4D9A-95B6-093EA040ABC8', 'CCC', 'C2', 'A', 1, 'EUN')

INSERT INTO #TempRefCusTariffAdditionalCodeLanguage (ZY4_PK,ZY4_ZX6_NKLanguage,ZY4_ZY2_TariffAdditionalCode,ZY4_Description)
VALUES (NEWID(), 'DE', '7558A41B-F5AE-46A0-AB6E-010117207DFF', 'C1'),
(NEWID(), 'DE', '7558A41B-F5AE-46A0-AB6E-010117207DF1', 'C2')

INSERT INTO #TempRefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_ZZA_TradeGroup,ZX5_VATCategory)
VALUES ('7558A41B-F5AE-46A0-AB6E-010117207DFF', 'FDCA07B0-8E98-4A44-A609-0441FCF0E585', NULL, '', '1900-01-01','2079-06-06', 'C', 'C1', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', ''),
('7558A41B-F5AE-46A0-AB6E-010117207DF1', NULL, '8F6E0739-434F-4D9A-95B6-093EA040ABC8', '', '1900-01-01','2079-06-06', 'C', 'C2', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '')

INSERT INTO #TempRefCusTariffLanguage (ZX7_PK,ZX7_ZX6_NKLanguage,ZX7_ZZ1_Tariff,ZX7_Description)
VALUES (NEWID(), 'DE', 'FDCA07B0-8E98-4A44-A609-0441FCF0E585', 'CCC')

INSERT INTO #TempRefCusConditionType (ZX2_PK, ZX2_ConditionType, ZX2_ZZZ_NKDataGrouping)
VALUES('A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'ZADOC', 'EUN')

INSERT INTO #TempRefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_ZZZ_NKDataGrouping) 
VALUES ('CB81B42C-3B0E-4149-804A-DC38A81CB7BA','TEST','EUN')

INSERT INTO #TempRefCusRate(ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_ZZW_TariffNationalCode, ZZ2_RX_NKCurrencyOverride) 
VALUES ('D83003BA-A3B6-437B-8312-0079C190BB3C','FDCA07B0-8E98-4A44-A609-0441FCF0E585','1900-01-01','2079-06-06','EUN','C1', NULL,'C'),
('D83003BA-A3B6-437B-8312-0079C190BB3D',NULL,'1900-01-01','2079-06-06','EUN','C2', '8F6E0739-434F-4D9A-95B6-093EA040ABC8','C')

INSERT INTO #TempRefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-0769FB817A45', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-0441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'EUN', 'C', '', 1, 0, 1, 1)

INSERT INTO #TempRefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES (NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'C2CD44CA-A4D1-45B2-A3F6-0769FB817A45', 'CCC', 1)

INSERT INTO #TempRefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_ZZ2_Rate,ZZT_ZY2_AdditionalCode,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber )
VALUES ('236353D1-77F3-4F1A-A262-08548903FBC1','C2CD44CA-A4D1-45B2-A3F6-0769FB817A45', NULL, NULL,'1911-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-08548903FBC2',NULL, 'D83003BA-A3B6-437B-8312-0079C190BB3C',NULL,'1912-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-08548903FBC3',NULL, NULL,'7558A41B-F5AE-46A0-AB6E-010117207DFF','1913-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-08548903FBC4',NULL, 'D83003BA-A3B6-437B-8312-0079C190BB3D', NULL,'1914-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','',''),
('236353D1-77F3-4F1A-A262-08548903FBC5',NULL, NULL,'7558A41B-F5AE-46A0-AB6E-010117207DF1','1915-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','')

INSERT #TempRefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES (NEWID(), '236353D1-77F3-4F1A-A262-08548903FBC1','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-08548903FBC2','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-08548903FBC3','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-08548903FBC4','879DD842-4D80-46AB-9EFA-73DC72C5D5A8'),
(NEWID(), '236353D1-77F3-4F1A-A262-08548903FBC5','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariff WHERE ZZ1_Description = 'T'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffNationalCode WHERE ZZW_Description = 'AAAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAttribute WHERE ZZ3_Value = 'AAA1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAttribute WHERE ZZ3_Value = 'AAA2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_UOM = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_UOM = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCode WHERE ZY2_Description = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCode WHERE ZY2_Description = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeLanguage WHERE ZY4_Description = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeLanguage WHERE ZY4_Description = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_Description = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_Description = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffLanguage WHERE ZX7_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1901-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1902-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1903-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1904-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1905-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1901-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1902-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1903-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1904-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1905-01-01'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariff WHERE ZZ1_Description = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffNationalCode WHERE ZZW_Description = 'CCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAttribute WHERE ZZ3_Value = 'CCC1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAttribute WHERE ZZ3_Value = 'CCC2'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_UOM = 'C1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_UOM = 'C2'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCode WHERE ZY2_Description = 'C1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCode WHERE ZY2_Description = 'C2'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeLanguage WHERE ZY4_Description = 'C1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeLanguage WHERE ZY4_Description = 'C2'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_Description = 'C1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_Description = 'C2'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffLanguage WHERE ZX7_Description = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1911-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1912-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1913-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1914-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1915-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1911-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1912-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1913-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1914-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1915-01-01'", trans));
				}
			}
		}
	}
}
