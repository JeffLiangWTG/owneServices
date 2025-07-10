using System;
using System.Globalization;
using System.Reflection;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test;

[Property("DAT:CapabilityRequirements", "SQL2019+")]
class PopulateDatasetPKInDPRTransformationFixture : TransformationFixture
{
	readonly string dbSafeName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
	protected override void AssertTransformationResults()
	{
		using var cmd = Connection.CreateCommand();
		cmd.Transaction = Transaction;
		cmd.CommandText = @"select count(*) from DataProcessingResult where DPR_DatasetPK is null";
		Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
	}

	protected override IDataTransformationTask GetTask()
	{
		var transformation = new PopulateDatasetPKInDPRTransformation(0);
		var typeDbName = transformation.GetType().GetField("safeDbName", BindingFlags.Instance | BindingFlags.NonPublic);
		typeDbName.SetValue(transformation, dbSafeName);
		return transformation;
	}

	protected override void PrepareTestData()
	{
		using var cmd = Connection.CreateCommand();
		cmd.Transaction = Transaction;
		cmd.CommandText = GetTestDataSql_DbStaging();
		cmd.ExecuteNonQuery();

		cmd.CommandText = GetTestDataSql_DbSafe();
		cmd.ExecuteNonQuery();
	}

	string GetTestDataSql_DbStaging()
	{
		var sql = $@"
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'Cocos (Keeling) Islands', '2022-03-05 12:25:00.0000000', 'ZZZ', 'C49B2C7E-D816-4446-90D4-0059C7C3317B', NULL, NULL, 'QUE', NULL);

insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'BR RefCusCodeType', '2022-03-05 12:25:00.0000000', 'ZZK', '44B01507-D7D4-4DAA-8543-332D8C4045DA', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'Status Aduaneiro na Interface', '2022-03-05 12:25:00.0000000', 'ZXI', 'FD54B0C7-80F1-4655-ACDD-0160FF78B7B6', NULL, NULL, 'QUE', NULL);

insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZZ1', '907D737C-FFF6-450D-B49B-0C98D0DA2522', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZZ2', 'B9176E1E-7892-417B-8DD0-004B38A7A2BA', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZXG', 'CA8772C1-B8F0-4E2C-BAE8-00000F2E7DB8', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZX1', 'BFF18D05-6FA7-47FD-8072-000021AEA69B', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZX3', 'A2A85143-A98F-489B-B9D7-000000B3C13F', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZZT', 'AD8E7D9E-2703-40B9-9CF8-000001DCA706', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZZC', '3B824988-D305-4057-BEC7-00001A6D0F2D', NULL, NULL, 'QUE', NULL);
insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'FR Tariff chapter 04', '2023-04-03 09:53:42.0000000', 'ZX5', 'E79DBA82-2AD9-402A-8447-00001F43CAB4', NULL, NULL, 'QUE', NULL);

insert into DataProcessingResult (DPR_PK,DPR_SubSource,DPR_PublicationTime,DPR_ParentTableCode,DPR_ParentPK,DPR_ExpirableAncestorPK,DPR_DatasetPK,DPR_Status,DPR_ExpirationTime)
values (newID(), 'AU Customs LCT Rates', '2022-09-01 00:00:00.0000000', 'ZZF', '00780C83-4F2F-4248-8603-528AC94CFFDB', NULL, NULL, 'QUE', NULL);
";
		return sql;
	}

	string GetTestDataSql_DbSafe()
	{
		var sql = $@"insert into [{dbSafeName}].dbo.RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values ('C49B2C7E-D816-4446-90D4-0059C7C3317B','CC','Cocos (Keeling) Islands',NULL);

insert into [{dbSafeName}].dbo.RefCusCodeType (ZZK_PK,ZZK_CodeType,ZZK_Description,ZZK_IsReadonly,ZZK_MaxLength,ZZK_ZZZ_NKDataGrouping)
values ('44B01507-D7D4-4DAA-8543-332D8C4045DA','CSTI','Customs Status for Interface','1','0','BR');
insert into [{dbSafeName}].dbo.RefLanguageType (ZX6_PK,ZX6_Language,ZX6_Description)
values ('CAFE923D-5358-43AF-B8FB-F8FB6253EA14','PT','Portuguese');

insert into [{dbSafeName}].dbo.RefCusCodeTypeLanguage (ZXI_PK,ZXI_ZX6_NKLanguage,ZXI_ZZK_CodeType,ZXI_Description)
values ('FD54B0C7-80F1-4655-ACDD-0160FF78B7B6','PT','44B01507-D7D4-4DAA-8543-332D8C4045DA','Status Aduaneiro na Interface');

insert into [{dbSafeName}].dbo.RefCusTariffType (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping,ZZI_ZZR_RateType,ZZI_HasFormulaSpecificQuestions)
values ('719AD9E6-B889-49D0-BEA7-06B6FF47FBC8','2P1','Schedule 2 Part 1','','CC',NULL,'0');
insert into [{dbSafeName}].dbo.RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_PublishedDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
values ('907D737C-FFF6-450D-B49B-0C98D0DA2522','719AD9E6-B889-49D0-BEA7-06B6FF47FBC8','201020707','0','FROZEN MEAT OF FOWLS OF THE SPECIES GALLUS DOMESTICUS','2015-02-27 00:00:00.000','2022-03-30 07:00:03.767',NULL,'VAT','CC','');
insert into [{dbSafeName}].dbo.RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_ZZW_TariffNationalCode,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZY1_RateCode,ZZ2_RateFormula,ZZ2_ZZS_Preference,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_RateFormulaDerivedFrom,ZZ2_RX_NKCurrencyOverride)
values ('B9176E1E-7892-417B-8DD0-004B38A7A2BA','907D737C-FFF6-450D-B49B-0C98D0DA2522',NULL,'2015-02-01 00:00:00.000','2079-06-06 23:59:00.000',NULL,'9.4 * [KG]',NULL,'CofO=''US''','ZA','907D737C-FFF6-450D-B49B-0C98D0DA2522','ZZ1','940C/KG','');
insert into [{dbSafeName}].dbo.RefCusRateUOM (ZXG_PK,ZXG_ZZ2_Rate,ZXG_UOM,ZXG_DataSetPK,ZXG_DataSetCode)
values ('CA8772C1-B8F0-4E2C-BAE8-00000F2E7DB8','B9176E1E-7892-417B-8DD0-004B38A7A2BA','KGM','907D737C-FFF6-450D-B49B-0C98D0DA2522','ZZ1');
insert into [{dbSafeName}].dbo.RefCusConditionType (ZX2_PK,ZX2_ConditionClass,ZX2_ConditionType,ZX2_Description,ZX2_ZZZ_NKDataGrouping)
values ('CBB0AC42-51A6-424C-813A-003D91D0D7E7','CTRL','728','Import control on luxury goods','CC');
insert into [{dbSafeName}].dbo.RefCusCondition (ZX1_PK,ZX1_ZX2_ConditionType,ZX1_ZZ1_Tariff,ZX1_ZZ5_Nomenclature,ZX1_StartDate,ZX1_EndDate,ZX1_Source,ZX1_Comment,ZX1_IsImport,ZX1_IsExport,ZX1_ConditionValueTrueMeansStop,ZX1_ZZS_Preference,ZX1_ZZZ_NKDataGrouping,ZX1_LogicalANDWithinGroup,ZX1_DataSetPK,ZX1_DataSetCode)
values ('BFF18D05-6FA7-47FD-8072-000021AEA69B','CBB0AC42-51A6-424C-813A-003D91D0D7E7','907D737C-FFF6-450D-B49B-0C98D0DA2522',NULL,'2021-02-01 00:00:00','2021-04-30 23:59:00','CH Tares','Mean Value Check','1','0','0',NULL,'CC','0','907D737C-FFF6-450D-B49B-0C98D0DA2522','ZZ1');
insert into [{dbSafeName}].dbo.RefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_Description,ZX4_IsFormula,ZX4_ZZZ_NKDataGrouping)
values ('EC50B888-733B-42D9-8826-0530963B4030','SUP','Supporting Document','0','CC');
insert into [{dbSafeName}].dbo.RefCusConditionValue (ZX3_PK,ZX3_ZX4_ValueType,ZX3_ZX1_Condition,ZX3_Value,ZX3_LogicalORWithinGroup,ZX3_DataSetPK,ZX3_DataSetCode)
values ('A2A85143-A98F-489B-B9D7-000000B3C13F','EC50B888-733B-42D9-8826-0530963B4030','BFF18D05-6FA7-47FD-8072-000021AEA69B','Y087','0','907D737C-FFF6-450D-B49B-0C98D0DA2522','ZZ1');
insert into [{dbSafeName}].dbo.RefCusTradeGroup (ZZA_PK,ZZA_TradeGroup,ZZA_Description,ZZA_StartDate,ZZA_EndDate,ZZA_ZZZ_NKDataGrouping)
values ('5665FFEB-6765-4560-83D5-00001F533EA9','1080','Channel Islands','2021-01-01 00:00:00','2079-06-06 23:59:00','CC');
insert into [{dbSafeName}].dbo.RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_ZX1_Conditions,ZZT_ZY2_AdditionalCode,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_ZZA_SecondTradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_DataSetPK,ZZT_DataSetCode)
values ('AD8E7D9E-2703-40B9-9CF8-000001DCA706',NULL,'BFF18D05-6FA7-47FD-8072-000021AEA69B',NULL,'1996-12-04 00:00:00','2079-06-06 23:59:00',NULL,NULL,'abc','113000F','907D737C-FFF6-450D-B49B-0C98D0DA2522','ZZ1');
insert into [{dbSafeName}].dbo.RefCusExcludedTradeGroup (ZZC_PK,ZZC_ZZT_Applicability,ZZC_ZZA_TradeGroup,ZZC_DataSetPK,ZZC_DataSetCode)
values ('3B824988-D305-4057-BEC7-00001A6D0F2D','AD8E7D9E-2703-40B9-9CF8-000001DCA706','5665FFEB-6765-4560-83D5-00001F533EA9','907D737C-FFF6-450D-B49B-0C98D0DA2522','ZZ1');
insert into [{dbSafeName}].dbo.RefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_DataSetPK,ZX5_DataSetCode,ZX5_ZZA_TradeGroup,ZX5_VATCategory)
values ('E79DBA82-2AD9-402A-8447-00001F43CAB4','907D737C-FFF6-450D-B49B-0C98D0DA2522',NULL,'STD','2014-01-01 00:00:00','2079-06-06 23:59:00','','','CC','907D737C-FFF6-450D-B49B-0C98D0DA2522','ZZ1','5665FFEB-6765-4560-83D5-00001F533EA9','');

insert into [{dbSafeName}].dbo.RefCusTaxOrFeeType (ZX0_PK,ZX0_TaxOrFeeType,ZX0_Description)
values (newID(),'VAT','Value Added Tax');
insert into [{dbSafeName}].dbo.RefCusTaxOrFee (ZZF_PK,ZZF_Code,ZZF_Description,ZZF_Value,ZZF_StartDate,ZZF_EndDate,ZZF_ZZZ_NKDataGrouping,ZZF_Minimum,ZZF_Maximum,ZZF_Threshold,ZZF_ZX0_NKTaxOrFeeType)
values ('00780C83-4F2F-4248-8603-528AC94CFFDB','Q1F','N10 FCL','30.00000000','2013-01-01 00:00:00.000','2015-11-30 23:59:00.000','CC','0.00000000','0.00000000','0.00000000','VAT');
";
		return sql;
	}
}
