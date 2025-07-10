using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTariffTypeUpdaterInfo_7Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusTariffTypeUpdaterInfo_7(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.RestoreColumnNameQuestionCodeForRefCusProfileQuestion(conn, trans);

					var refCusProfile = new RefCusProfile();
					TestDBHelper.ExecuteNonQuery(conn, SharedDbSchemaChange.GetRenameColumnIfExistsScript(refCusProfile.TableName, "XX0_AppliesToCode", "XX0_TariffCode"), trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','EUN','EUN')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(NEWID(), 'DE', 'German')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','EUN'),
('C908F23F-79B9-4094-84A7-17BBF876C25A', 'BBB', 'BBB','EUN'),
('64490155-01F3-4F4B-AED9-7E51DD536806', 'DDD', 'DDD','EUN')

INSERT INTO RefCusTariffTypeLanguage (ZXK_PK, ZXK_ZZI_TariffType, ZXK_ZX6_NKLanguage, ZXK_Description)
VALUES (NEWID(), '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'DE', 'AAA'),
(NEWID(), 'C908F23F-79B9-4094-84A7-17BBF876C25A', 'DE', 'BBB'),
(NEWID(), '64490155-01F3-4F4B-AED9-7E51DD536806', 'DE', 'DDD')

INSERT RefCusTariffAdditionalCodeCategory (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'AAA', 'EUN')

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description) 
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','EUN', 'GSP (R 12/978) - Annex IV')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','EUN','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01')

INSERT INTO RefCusTariffRelationship (ZZH_PK,ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1111111')

INSERT INTO RefCusTariffNationalCode (ZZW_PK,ZZW_ZZ1_Tariff,ZZW_NationalCode,ZZW_Description,ZZW_ZZF_NKTaxOrFeeCode,ZZW_StartDate,ZZW_EndDate,ZZW_ZZZ_NKDataGrouping,ZZW_PublishedDate)
VALUES ('8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'AAA', 'AAAA','', '1900-01-01','2079-06-06', 'EUN', '1900-01-01')

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

INSERT RefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition)
VALUES(NEWID(), 'DE', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45')

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

INSERT INTO RefCusTariffBRCharacteristic (ZB1_PK,ZB1_CharacteristicType,ZB1_ZZ1_Tariff,ZB1_Style,ZB1_Code)
VALUES ('12093CF0-8490-41E9-A4D8-61ECB8CCE127','NVE','FDCA07B0-8E98-4A44-A609-D441FCF0E585','STRING','AA');
INSERT INTO RefCusTariffBRCharacteristicValue (ZB2_PK,ZB2_ZB1_Characteristic,ZB2_Value,ZB2_Description)
VALUES ('C6ADCE04-3416-43C2-8B02-6DB39DCA3539','12093CF0-8490-41E9-A4D8-61ECB8CCE127','AAA','AA');
INSERT INTO RefCusTariffBRCharacteristicAttribute (ZB3_PK,ZB3_ZB1_Characteristic,ZB3_Name,ZB3_Code,ZB3_Value)
VALUES ('07C374F3-E396-478C-99C6-413E0E7E5C13','12093CF0-8490-41E9-A4D8-61ECB8CCE127','AAA','AA','A');

INSERT INTO RefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','A Description','EUN'),
('D272AC2C-C3CB-4981-851E-764899C55D73','B','C908F23F-79B9-4094-84A7-17BBF876C25A','B Description','EUN');

INSERT INTO RefCusProfile (XX0_PK,XX0_XXX_ProfileType,XX0_TariffCode,XX0_QuestionCode,XX0_StartDate,XX0_EndDate,XX0_ZZZ_NKDataGrouping)
VALUES ('9EC9F58A-0DE4-48AA-8081-18A45E5E216F','1799FE44-08BE-42CF-B130-E89921BC3E85','1001','QC1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','EUN'),
('6EDA47B3-F6E3-4FAE-819E-4CD5BC107E98','D272AC2C-C3CB-4981-851E-764899C55D73','1002','QC2','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','EUN');
INSERT INTO RefCusProfileAttribute(XXY_PK,XXY_XX0_Profile,XXY_Name,XXY_Value)
VALUES (NEWID(),'9EC9F58A-0DE4-48AA-8081-18A45E5E216F','Name1','Value1'),
(NEWID(),'6EDA47B3-F6E3-4FAE-819E-4CD5BC107E98','Name2','Value2');

INSERT INTO RefCusProfileQuestion (XQ2_PK,XQ2_XXX_ProfileType,XQ2_Code,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping)
VALUES ('C673520E-9D5E-4388-AF33-03166005DC19','1799FE44-08BE-42CF-B130-E89921BC3E85','Code1','NUMBER','Q1','Text1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','EUN');
INSERT INTO RefCusProfileQuestionAnswerList (XQ4_PK,XQ4_XQ2_Question,XQ4_Value,XQ4_Description)
VALUES ('5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','C673520E-9D5E-4388-AF33-03166005DC19','Value1','desc 1');
INSERT INTO RefCusProfileQuestionAnswerListLanguage(XAL_PK,XAL_XQ4_QuestionAnswer,XAL_Description,XAL_ZX6_NKLanguage)
VALUES (NEWID(),'5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','desc 1','DE');
INSERT INTO RefCusProfileQuestionAttribute(XQ3_PK,XQ3_XQ2_Question,XQ3_Name,XQ3_Value)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','Name1','Value1');
INSERT INTO RefCusProfileQuestionLanguage(XQL_PK,XQL_XQ2_Question,XQL_Name,XQL_Text,XQL_Note,XQL_ZX6_NKLanguage)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','Name1','Text1','','DE');
INSERT INTO RefCusProfileQuestionPathway(XQP_PK,XQP_XQ2_QuestionParent,XQP_XQ2_QuestionChild,XQP_Description,XQP_StartDate,XQP_EndDate)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','C673520E-9D5E-4388-AF33-03166005DC19','desc 1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000');
", trans);
					var info = new RefCusTariffTypeUpdaterInfo_7();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_ZZZ_NKDataGrouping,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,Deleted)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5071', '1P1','EUN', '1P1','', 1),
('5FC8D11D-34A3-4DC7-8A8D-1741F8E6D11A', 'BBB','EUN','XBB','', 0),
('C98F13BC-F968-41A6-B7ED-98046B17ED4E', 'CCC','EUN','CCC','', 0),
(NEWID(), 'DDD','EUN','DDD','', 0)

INSERT INTO #TempRefCusTariffTypeLanguage (ZXK_PK, ZXK_ZZI_TariffType, ZXK_ZX6_NKLanguage, ZXK_Description)
VALUES (NEWID(), '5FC8D11D-34A3-4DC7-8A8D-1741F8E6D11A', 'DE', 'XBB'),
(NEWID(), 'C98F13BC-F968-41A6-B7ED-98046B17ED4E', 'DE', 'CCC')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariff WHERE ZZ1_Description = 'T'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffNationalCode WHERE ZZW_Description = 'AAAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffAttribute WHERE ZZ3_Value = 'AAA1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffAttribute WHERE ZZ3_Value = 'AAA2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffUOM WHERE ZZ8_UOM = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffUOM WHERE ZZ8_UOM = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffAdditionalCode WHERE ZY2_Description = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffAdditionalCode WHERE ZY2_Description = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffAdditionalCodeLanguage WHERE ZY4_Description = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffAdditionalCodeLanguage WHERE ZY4_Description = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusVATApplicability WHERE ZX5_Description = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusVATApplicability WHERE ZX5_Description = 'A2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffLanguage WHERE ZX7_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusCondition WHERE ZX1_Source = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusConditionValue WHERE ZX3_Value = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusConditionLanguage WHERE ZXJ_Source = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusApplicability WHERE ZZT_StartDate = '1901-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusApplicability WHERE ZZT_StartDate = '1902-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusApplicability WHERE ZZT_StartDate = '1903-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusApplicability WHERE ZZT_StartDate = '1904-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusApplicability WHERE ZZT_StartDate = '1905-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1901-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1902-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1903-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1904-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusExcludedTradeGroup JOIN RefCusApplicability ON ZZC_ZZT_Applicability = ZZT_PK WHERE ZZT_StartDate = '1905-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffTypeLanguage WHERE ZXK_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffRelationship WHERE ZZH_TariffCode = '1111111'", trans));

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffBRCharacteristic WHERE ZB1_CharacteristicType = 'NVE' AND ZB1_Code = 'AA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffBRCharacteristicValue WHERE ZB2_Value = 'AAA' AND ZB2_Description = 'AA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffBRCharacteristicAttribute WHERE ZB3_Name = 'AAA' AND ZB3_Code = 'AA'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffType WHERE ZZI_TariffType = 'BBB' AND ZZI_Description = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffTypeLanguage WHERE ZXK_Description = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffType WHERE ZZI_TariffType = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffTypeLanguage WHERE ZXK_Description = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffType WHERE ZZI_TariffType = 'DDD'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusTariffTypeLanguage WHERE ZXK_Description = 'DDD'", trans));

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'A'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'B'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfile WHERE XX0_TariffCode = '1001' AND XX0_QuestionCode = 'QC1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfile WHERE XX0_TariffCode = '1002' AND XX0_QuestionCode = 'QC2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileAttribute WHERE XXY_Name = 'Name1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileAttribute WHERE XXY_Name = 'Name2'", trans));

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_Code = 'Code1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAnswerList WHERE XQ4_Value = 'Value1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAnswerListLanguage WHERE XAL_Description = 'desc 1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAttribute WHERE XQ3_Name = 'Name1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionLanguage WHERE XQL_Name = 'Name1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionPathway WHERE XQP_Description = 'desc 1'", trans));
				}
			}
		}
	}
}
