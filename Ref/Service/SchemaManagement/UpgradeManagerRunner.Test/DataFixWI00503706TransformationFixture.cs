using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	internal class DataFixWI00503706TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_AdditionalCode IS NULL OR  ZZT_OrderNumber IS NULL";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusApplicability";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));


				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage WHERE ZXX_Description IS NULL";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariffUOM";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(2));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_Type NOT IN ('CU1','RU1','AD1','CU2','CU3','CU4','CU5')";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00503706Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
ALTER TABLE RefCusApplicability ALTER COLUMN ZZT_AdditionalCode NVARCHAR(15) NULL;
ALTER TABLE RefCusApplicability ALTER COLUMN ZZT_OrderNumber NVARCHAR(15) NULL;
ALTER TABLE RefCusConditionValueTypeLanguage DROP CONSTRAINT CK_RefCusConditionValueTypeLanguage_ZXX_Description;
ALTER TABLE RefCusConditionValueTypeLanguage ALTER COLUMN ZXX_Description NVARCHAR(500) NULL;

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
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','ZA', 'GSP (R 12/978) - Annex IV')

INSERT INTO RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
Values ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','ZA','2018-07-19','2079-06-06 23:59:00')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','ZA','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01')

INSERT INTO RefCusTariffNationalCode (ZZW_PK,ZZW_ZZ1_Tariff,ZZW_NationalCode,ZZW_Description,ZZW_ZZF_NKTaxOrFeeCode,ZZW_StartDate,ZZW_EndDate,ZZW_ZZZ_NKDataGrouping,ZZW_PublishedDate)
VALUES ('8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'AAA', 'AAAA','', '1900-01-01','2079-06-06', 'ZA', '1900-01-01')

INSERT INTO RefCusTariffAttribute (ZZ3_PK,ZZ3_ZZ1_Tariff,ZZ3_Name,ZZ3_Value,ZZ3_ZZW_TariffNationalCode)
VALUES (NEWID(), NULL, 'AAA', 'AAA', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8')

INSERT INTO RefCusTariffUOM (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_ZZW_TariffNationalCode,ZZ8_ZZZ_NKDataGrouping)
VALUES (NEWID(), NULL, 'CU2', 'A2', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'ZA')

INSERT INTO RefCusTariffAdditionalCode (ZY2_PK,ZY2_ZZ1_Tariff,ZY2_ZZW_NationalCode,ZY2_AdditionalCode,ZY2_Description,ZY2_ZY3_NKCategory,ZY2_IsMandatory,ZY2_ZZZ_NKDataGrouping)
VALUES ('7558A41B-F5AE-46A0-AB6E-510117207DFF', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, 'AAA', 'A1', 'A', 1, 'ZA')

INSERT INTO RefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_ZZA_TradeGroup,ZX5_VATCategory)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, '', '1900-01-01','2079-06-06', 'A', 'A1', 'ZA', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'CLASS', 'ZADOC', 'Testing', 'ZA')

INSERT RefCusConditionTypeLanguage (ZXW_PK, ZXW_ZX2_ConditionType, ZXW_ZX6_NKLanguage, ZXW_Description)
VALUES(NEWID(),'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1','EN', 'AAA')

INSERT INTO RefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_Description,ZX4_IsFormula,ZX4_ZZZ_NKDataGrouping) 
VALUES ('CB81B42C-3B0E-4149-804A-DC38A81CB7BA','TEST','Test Description',1,'ZA')

INSERT RefCusConditionValueTypeLanguage (ZXX_PK, ZXX_ZX4_ValueType, ZXX_ZX6_NKLanguage, ZXX_Description)
VALUES(NEWID(),'CB81B42C-3B0E-4149-804A-DC38A81CB7BA','EN', NULL)

INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_ZZW_TariffNationalCode) 
VALUES ('D83003BA-A3B6-437B-8312-9079C190BB3D',NULL,'1900-01-01','2079-06-06','ZA','A2', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'A5B026EA-ECF3-4844-9856-A0C11A0CBCE1', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'ZA', 'A', '', 1, 0, 1, 1)

INSERT INTO RefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES(NEWID(), 'CB81B42C-3B0E-4149-804A-DC38A81CB7BA', 'C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'AAA', 1)

INSERT RefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition)
VALUES(NEWID(), 'EN', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_ZZ2_Rate,ZZT_ZY2_AdditionalCode,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber )
VALUES ('236353D1-77F3-4F1A-A262-68548903FBC1','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', NULL, NULL,'1901-01-01','2079-06-06','879DD842-4D80-46AB-9EFA-73DC72C5D5A8',NULL,'')

ALTER TABLE RefCusTariffUOM DROP CONSTRAINT CK_RefCusTariffUOM_ZZ8_Type;
INSERT INTO RefCusTariffUOM (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZW_TariffNationalCode,ZZ8_ZZZ_NKDataGrouping)
VALUES 
(NEWID(), NULL, 'CU2', 'RU1', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'EUN'),
(NEWID(), NULL, 'FF', 'RU1', '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'EUN');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
