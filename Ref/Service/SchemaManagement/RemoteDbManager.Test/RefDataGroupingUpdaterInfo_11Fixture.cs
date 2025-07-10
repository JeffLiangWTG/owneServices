using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefDataGroupingUpdaterInfo_11Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefDataGroupingUpdaterInfo_11(DbSchema dbSchema)
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
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','ZA','South Africa'),
(NEWID(),'BB','BBBBB')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(NEWID(), 'EN', 'German')

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'CUSOF', 'AAA', 'ZA')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','ZA')

INSERT RefCusTariffAdditionalCodeCategory (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'AAA', 'ZA')

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description) 
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','ZA', 'GSP (R 12/978) - Annex IV'),
('4F367102-358F-41D6-84F4-A8C33BF9EFEC','METRO','BB', 'Continental and Corsica');

INSERT INTO RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
Values ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','ZA','2018-07-19','2079-06-06 23:59:00')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','ZA','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01')

INSERT INTO RefCusTariffNationalCode (ZZW_PK,ZZW_ZZ1_Tariff,ZZW_NationalCode,ZZW_Description,ZZW_ZZF_NKTaxOrFeeCode,ZZW_StartDate,ZZW_EndDate,ZZW_ZZZ_NKDataGrouping,ZZW_PublishedDate)
VALUES ('8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'AAA', 'AAAA','', '1900-01-01','2079-06-06', 'ZA', '1900-01-01')

INSERT INTO RefCusTariffAttribute (ZZ3_PK,ZZ3_ZZ1_Tariff,ZZ3_Name,ZZ3_Value,ZZ3_ZZW_TariffNationalCode)
VALUES (NEWID(), NULL, 'AAA', 'AAA', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8')

INSERT INTO RefCusTariffUOM (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_ZZW_TariffNationalCode,ZZ8_ZZZ_NKDataGrouping,ZZ8_ZZA_SecondTradeGroup,ZZ8_StartDate,ZZ8_EndDate)
VALUES (NEWID(), NULL, 'CU2', 'A2', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'ZA', '4F367102-358F-41D6-84F4-A8C33BF9EFEC', '2019-12-14 00:00:00', '2079-06-06 23:59:00')

INSERT INTO RefCusTariffAdditionalCode (ZY2_PK,ZY2_ZZ1_Tariff,ZY2_ZZW_NationalCode,ZY2_AdditionalCode,ZY2_Description,ZY2_ZY3_NKCategory,ZY2_IsMandatory,ZY2_ZZZ_NKDataGrouping)
VALUES ('7558A41B-F5AE-46A0-AB6E-510117207DFF', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, 'AAA', 'A1', 'A', 1, 'ZA')

INSERT INTO RefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_ZZA_TradeGroup,ZX5_VATCategory)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, '', '1900-01-01','2079-06-06', 'A', 'A1', 'ZA', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'CLASS', 'ZADOC', 'Testing', 'ZA')

INSERT RefCusConditionTypeLanguage (ZXW_PK, ZXW_ZX2_ConditionType, ZXW_ZX6_NKLanguage, ZXW_Description)
VALUES(NEWID(),'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1','EN', 'AAA')

INSERT INTO RefCusConditionCode (ZY7_PK, ZY7_ConditionCode, ZY7_Description, ZY7_ZZZ_NKDataGrouping)
VALUES('BBA87CBB-0C40-44D8-B446-D3A3F271946C', 'CCC', 'Testing', 'ZA')

INSERT RefCusConditionCodeLanguage (ZY8_PK, ZY8_ZY7_ConditionCode, ZY8_ZX6_NKLanguage, ZY8_Description)
VALUES(NEWID(),'BBA87CBB-0C40-44D8-B446-D3A3F271946C','EN', 'AAA')

INSERT INTO RefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_Description,ZX4_IsFormula,ZX4_ZZZ_NKDataGrouping) 
VALUES ('CB81B42C-3B0E-4149-804A-DC38A81CB7BA','TEST','Test Description',1,'ZA')

INSERT RefCusConditionValueTypeLanguage (ZXX_PK, ZXX_ZX4_ValueType, ZXX_ZX6_NKLanguage, ZXX_Description)
VALUES(NEWID(),'CB81B42C-3B0E-4149-804A-DC38A81CB7BA','EN', 'AAA')

INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_ZZW_TariffNationalCode) 
VALUES ('D83003BA-A3B6-437B-8312-9079C190BB3D',NULL,'1900-01-01','2079-06-06','ZA','A2', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup, ZX1_ZY7_NKConditionCode, ZX1_AdditionalComment)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'ZA', 'A', '', 1, 0, 1, 1, 'CCC', 'comment')

INSERT INTO RefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'AAA', 1)

INSERT RefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition, ZXJ_AdditionalComment)
VALUES(NEWID(), 'EN', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'A')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_ZZ2_Rate,ZZT_ZY2_AdditionalCode,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber )
VALUES ('236353D1-77F3-4F1A-A262-68548903FBC1','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', NULL, NULL,'1901-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8','','')

INSERT RefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup)
VALUES ('FDB07712-41B8-445A-ACDB-EB92F0FE0FD1', '236353D1-77F3-4F1A-A262-68548903FBC1','879DD842-4D80-46AB-9EFA-73DC72C5D5A8')

INSERT RefHarbourRate (ZXF_PK, ZXF_Type, ZXF_Port, ZXF_Mode, ZXF_Commodity, ZXF_StartDate, ZXF_EndDate, ZXF_RateFormula, ZXF_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'A', 'CON', 'A', '2017/1/1', '2017/12/12', 'A', 'ZA')

INSERT RefVesselZZ (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_RN_NKCountryOfReg, ZZO_LloydsNumber, ZZO_ZZZ_NKDataGrouping)
VALUES ('C0A94205-BCA9-4E0F-A216-E6A3F0380D21', 'A','A','A','A','A','ZA')

INSERT RefCarrierCode (ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_ZZZ_NKDataGrouping, ZZ4_IsSea, ZZ4_IsRoad, ZZ4_IsAir, ZZ4_IsRail)
VALUES ('EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', 'DEND', 'Deugro A/S Denmark', 'ZA', 1, 0, 0, 0)

INSERT RefCarrierCodeAttribute(ZZG_PK, ZZG_ZZ4_CarrierCode, ZZG_Name, ZZG_Value)
VALUES(NEWID(), 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', 'SEA', 'SEA')

INSERT RefCarrierVesselPivot(ZZQ_PK, ZZQ_ZZ4, ZZQ_ZZO)
VALUES(NEWID(), 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', 'C0A94205-BCA9-4E0F-A216-E6A3F0380D21')

INSERT INTO RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory)
VALUES ('481E2D99-CA7F-4F65-8E08-32806A57B41A', 'ROLE', 'Role', 'CUSOF', 'ZA', 0, 0, 0)

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES ('3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', 'CUSOF', '4701', 'JOHN F KENNEDY AIRPORT, N', '2018-01-19', '2079-06-06 23:59:00', 'ZA')

INSERT INTO RefCusCodeListLanguage(ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description)
VALUES (NEWID(), 'EN', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', 'Desc1GRM')

INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
VALUES ('3D983217-172A-46E0-8683-E441A6D4AFFD', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', 'ROLE', 'EXP')

INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute)
VALUES 
(NEWID(), 'ROA', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', NULL)

INSERT RefCusMapType (ZZP_PK, ZZP_MapType, ZZP_Direction, ZZP_Description, ZZP_IsReadonly)
VALUES (NEWID(), 'A', 'BTH', 'AAA', 1)

INSERT RefCusMap (ZZM_PK, ZZM_ZZP_NKMapType, ZZM_CW1orCommercialValue, ZZM_CustomsValue, ZZM_StartDate, ZZM_EndDate, ZZM_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'A', 'AAA', '2017/1/1', '2017/12/12', 'ZA')

INSERT INTO RefCusNomenclatureGroup (ZZ5_PK,ZZ5_Value,ZZ5_Description,ZZ5_StartDate,ZZ5_EndDate,ZZ5_CompositeKey,ZZ5_ZZZ_NKDataGrouping,ZZ5_ZZ9_NKNomenclatureGroupType)
VALUES ('2718BC12-085A-42C6-AA91-B085E006E0EF', 'AAA', 'AAAAA', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'A','ZA', '')

INSERT INTO RefCusNomenclatureGroupNote (ZZL_PK,ZZL_ZZ5_NomenclatureGroup,ZZL_NoteType,ZZL_Note,ZZL_ZZZ_NKDataGrouping,ZZL_ZX6_NKLanguage)
VALUES (NEWID(), '2718BC12-085A-42C6-AA91-B085E006E0EF', 'AAA', 'A', 'ZA', 'EN')

INSERT INTO RefCusNomenclatureLanguage (ZX8_PK,ZX8_ZX6_NKLanguage,ZX8_ZZ5_NomenclatureGroup,ZX8_Description)
VALUES (NEWID(), 'EN', '2718BC12-085A-42C6-AA91-B085E006E0EF', 'BBB')

INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES ('49899FBA-46ED-4CDB-AFBA-339757D3B696','200','2000','ZA')

INSERT RefCusPreferenceLanguage (ZX9_PK,ZX9_ZX6_NKLanguage,ZX9_ZZS_Preference,ZX9_Description)
VALUES (NEWID(), 'EN', '49899FBA-46ED-4CDB-AFBA-339757D3B696', 'AAA')

INSERT INTO RefCusProcedure (ZZ6_PK,ZZ6_Category,ZZ6_ProcedureCode,ZZ6_PreviousProcedureCode,ZZ6_Concession,ZZ6_Description,ZZ6_ZZZ_NKDataGrouping
,ZZ6_ShipmentType,ZZ6_CalculateDuty,ZZ6_Group,ZZ6_LandedCost,ZZ6_IntoWarehouse,ZZ6_OutOfWarehouse,ZZ6_StartDate,ZZ6_EndDate, ZZ6_CalculateVAT, ZZ6_IsGuaranteeConsumed, ZZ6_IsGuaranteeReleased
,ZZ6_IntoTemporaryImport, ZZ6_OutOfTemporaryImport, ZZ6_IntoTemporaryExport, ZZ6_OutOfTemporaryExport, ZZ6_IsTransit, ZZ6_IntoInwardProcessing, ZZ6_OutOfInwardProcessing, ZZ6_IntoOutwardProcessing, ZZ6_OutofOutwardProcessing)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','AAA','15','4','ZZZ','ZA','EXW',1,'',0,'N','Y','1900-01-01','2079-06-06 23:59:00', 1, 'N', 'Y', 'N', 'N', 'N', 'N', 'Y', 'N', 'N', 'N', 'N')

INSERT INTO RefCusProcedureAttribute (ZXB_PK,ZXB_ZZ6_ProcedureCode,ZXB_Name,ZXB_Value)
VALUES (NEWID(),'1799FE44-08BE-42CF-B130-E89921BC3E85','AA','AA')

INSERT INTO RefCusProcedureLanguage (ZXV_PK,ZXV_ZZ6_Procedure,ZXV_ZX6_NKLanguage,ZXV_Description)
VALUES (NEWID(), '1799FE44-08BE-42CF-B130-E89921BC3E85', 'EN', 'AAA')

INSERT INTO RefCusRateType (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_CustomsValueFormula)
VALUES ('58F19F31-B7BB-48E3-9ED9-76733B831C1B', 'DTY', 'Duty', 1, 'ZA', '')

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description, ZY1_InternalUse) VALUES
('9FBDD063-4839-4C9F-A602-8A28A5897724','ADFM','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Flour Duty',1)

INSERT INTO RefCusRateCodeLanguage (ZXC_PK, ZXC_ZX6_NKLanguage, ZXC_ZY1_RateCode, ZXC_Description) VALUES
('78BA88D9-0E4B-4A63-8F4C-D643934568C2', 'EN', '9FBDD063-4839-4C9F-A602-8A28A5897724', 'AAA')

INSERT INTO RefCusRateTypeLanguage (ZXT_PK, ZXT_ZX6_NKLanguage, ZXT_ZZR_RateType, ZXT_Description) VALUES
(NEWID(), 'EN', '58F19F31-B7BB-48E3-9ED9-76733B831C1B', 'AAA')

INSERT INTO RefCusTariffAdditionalCodeLanguage (ZY4_PK,ZY4_ZX6_NKLanguage,ZY4_ZY2_TariffAdditionalCode,ZY4_Description)
VALUES (NEWID(), 'EN', '7558A41B-F5AE-46A0-AB6E-510117207DFF', 'A1')

INSERT INTO RefCusQuota (ZXQ_PK,ZXQ_OrderNumber,ZXQ_InitialAmount,ZXQ_UnitOfMeasure,ZXQ_Balance,ZXQ_StartDate,ZXQ_EndDate,ZXQ_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'AA', 0, 'KG', 0, '1900-01-01', '2020-12-31', 'ZA')

INSERT INTO RefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','A Description','ZA');
INSERT INTO RefCusProfile (XX0_PK,XX0_XXX_ProfileType,XX0_TariffCode,XX0_QuestionCode,XX0_StartDate,XX0_EndDate,XX0_ZZZ_NKDataGrouping)
VALUES ('9EC9F58A-0DE4-48AA-8081-18A45E5E216F','1799FE44-08BE-42CF-B130-E89921BC3E85','1001','QC1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA');
INSERT INTO RefCusProfileAttribute(XXY_PK,XXY_XX0_Profile,XXY_Name,XXY_Value)
VALUES (NEWID(),'9EC9F58A-0DE4-48AA-8081-18A45E5E216F','Name1','Value1');

INSERT INTO RefCusProfileQuestion (XQ2_PK,XQ2_XXX_ProfileType,XQ2_Code,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping)
VALUES ('C673520E-9D5E-4388-AF33-03166005DC19','1799FE44-08BE-42CF-B130-E89921BC3E85','Code1','NUMBER','Q1','Text1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA');
INSERT INTO RefCusProfileQuestionAnswerList (XQ4_PK,XQ4_XQ2_Question,XQ4_Value,XQ4_Description)
VALUES ('5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','C673520E-9D5E-4388-AF33-03166005DC19','Value1','desc 1');
INSERT INTO RefCusProfileQuestionAnswerListLanguage(XAL_PK,XAL_XQ4_QuestionAnswer,XAL_Description,XAL_ZX6_NKLanguage)
VALUES (NEWID(),'5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','desc 1','EN');
INSERT INTO RefCusProfileQuestionAttribute(XQ3_PK,XQ3_XQ2_Question,XQ3_Name,XQ3_Value)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','Name1','Value1');
INSERT INTO RefCusProfileQuestionLanguage(XQL_PK,XQL_XQ2_Question,XQL_Name,XQL_Text,XQL_Note,XQL_ZX6_NKLanguage)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','Name1','Text1','','EN');
INSERT INTO RefCusProfileQuestionPathway(XQP_PK,XQP_XQ2_QuestionParent,XQP_XQ2_QuestionChild,XQP_Description,XQP_StartDate,XQP_EndDate)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','C673520E-9D5E-4388-AF33-03166005DC19','desc 1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000');
", trans);
					var info = new RefDataGroupingUpdaterInfo_11();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_Grouping, Deleted)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','ZA','South Africa',NULL, 1),
(NEWID(),'BB','XBBBB',NULL, 0),
(NEWID(),'CC','CCCCC',NULL, 0)
", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefDataGrouping WHERE ZZZ_DataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeCategory WHERE ZY3_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_RN_NKTradeGroupCountryCode = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffNationalCode WHERE ZZW_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAttribute WHERE ZZ3_Name = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCode WHERE ZY2_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionCode WHERE ZY7_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionCodeLanguage WHERE ZY8_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueType WHERE ZX4_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage WHERE ZXX_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_Value = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Source = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_StartDate = '1901-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusExcludedTradeGroup", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefHarbourRate WHERE ZXF_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselZZ WHERE ZZO_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCarrierCode WHERE ZZ4_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCarrierCodeAttribute WHERE ZZG_Name = 'SEA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCarrierVesselPivot", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeList WHERE ZZD_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListLanguage WHERE ZXA_Description = 'Desc1GRM'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttribute WHERE ZZE_Value = 'EXP'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_TransportMode = 'ROA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusMap WHERE ZZM_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroup WHERE ZZ5_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupNote WHERE ZZL_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureLanguage WHERE ZX8_Description = 'BBB'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreference WHERE ZZS_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusPreferenceLanguage WHERE ZX9_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureAttribute WHERE ZXB_Value = 'AA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureLanguage WHERE ZXV_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateType WHERE ZZR_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCode WHERE ZY1_Description = 'Flour Duty'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCodeLanguage WHERE ZXC_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateTypeLanguage WHERE ZXT_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeLanguage WHERE ZY4_Description = 'A1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusQuota WHERE ZXQ_OrderNumber = 'AA'", trans));

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfile WHERE XX0_TariffCode = '1001' AND XX0_QuestionCode = 'QC1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileAttribute WHERE XXY_Name = 'Name1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_Code = 'Code1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAnswerList WHERE XQ4_Value = 'Value1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAnswerListLanguage WHERE XAL_Description = 'desc 1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAttribute WHERE XQ3_Name = 'Name1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionLanguage WHERE XQL_Name = 'Name1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionPathway WHERE XQP_Description = 'desc 1'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefDataGrouping WHERE ZZZ_DataGrouping = 'BB' AND ZZZ_Description = 'XBBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefDataGrouping WHERE ZZZ_DataGrouping = 'CC'", trans));
				}
			}
		}
	}
}
