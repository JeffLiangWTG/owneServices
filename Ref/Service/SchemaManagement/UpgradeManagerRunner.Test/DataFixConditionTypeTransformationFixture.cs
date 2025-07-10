using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixConditionTypeTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				var query = @"select count(1) 
from RefCusConditionValueType cvt
join RefCusConditionValue cv on cvt.ZX4_PK = cv.ZX3_ZX4_ValueType
join RefCusCondition c on c.ZX1_PK = cv.ZX3_ZX1_Condition
Where ZX4_ValueType = 'FRM'";
				Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, query));
				query = @"select count(1) 
from RefCusConditionValueType cvt
join RefCusConditionValue cv on cvt.ZX4_PK = cv.ZX3_ZX4_ValueType
join RefCusCondition c on c.ZX1_PK = cv.ZX3_ZX1_Condition
Where ZX4_ValueType = 'SUP'";
				Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, query));

				query = @"select count(1)  
from RefCusConditionValueType cvt
join RefCusConditionValue cv on cvt.ZX4_PK = cv.ZX3_ZX4_ValueType
join RefCusCondition c on c.ZX1_PK = cv.ZX3_ZX1_Condition
Where ZX4_ValueType = 'SNR'";
				Assert.AreEqual(2, DbHelper.ExecuteScalar(Transaction, query));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixConditionTypeTransformation(68);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','European','EUN');
INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'EUN','','0');
INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','IMP','IMP','EUN','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);
INSERT INTO RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
VALUES('3E82B265-4D78-412E-B428-4E4AE00A4B99', 'ANY', 'Testing', 'EUN')

--Setup conditionsvalue types for SUP,FRM,Z

INSERT INTO [dbo].[RefCusConditionValueType] ([ZX4_PK],[ZX4_ValueType],[ZX4_Description],[ZX4_IsFormula],[ZX4_ZZZ_NKDataGrouping])
VALUES ('779C9E24-A62A-7D3B-BEF1-08D7DC44DDB3','SUP','Supporting document','1','EUN');
INSERT INTO [dbo].[RefCusConditionValueType] ([ZX4_PK],[ZX4_ValueType],[ZX4_Description],[ZX4_IsFormula],[ZX4_ZZZ_NKDataGrouping])
VALUES ('779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4','FRM','Formula','1','EUN');
INSERT INTO [dbo].[RefCusConditionValueType] ([ZX4_PK],[ZX4_ValueType],[ZX4_Description],[ZX4_IsFormula],[ZX4_ZZZ_NKDataGrouping])
VALUES ('74A52793-9301-46BF-A555-1967694A61DA','SNR','Presentation of more than one certificate','1','EUN');
INSERT INTO [dbo].[RefCusConditionValueType] ([ZX4_PK],[ZX4_ValueType],[ZX4_Description],[ZX4_IsFormula],[ZX4_ZZZ_NKDataGrouping])
VALUES ('74A52793-9301-46BF-A555-1967694A61DB','DUM','Dummy','1','EUN');

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('4ECE8DC2-8E84-493E-83BD-6C3195F2956E', 'RATE', '745', 'Testing', 'EUN')

--create tariffs

declare @tariffPK uniqueidentifier
,@conditionPK uniqueidentifier
select @tariffPK = newid(),@conditionPK = newid()

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','0201100010',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping,ZX1_Comment)
VALUES(@conditionPK, '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', @tariffPK, '2017-11-17 00:00:00', '2079-06-06 23:59:00', '3E82B265-4D78-412E-B428-4E4AE00A4B99', 'EUN','[ABC]>=100')
INSERT INTO [dbo].[RefCusConditionValue]([ZX3_PK],[ZX3_ZX4_ValueType],[ZX3_ZX1_Condition],[ZX3_Value],[ZX3_LogicalORWithinGroup])
VALUES (newid(),'779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4',@conditionPK,'Y022',1)

select @tariffPK = newid(),@conditionPK = newid()

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','0201100011',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping,ZX1_Comment)
VALUES(@conditionPK, '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', @tariffPK, '2017-11-17 00:00:00', '2079-06-06 23:59:00', '3E82B265-4D78-412E-B428-4E4AE00A4B99', 'EUN','Condition Y: test text')
INSERT INTO [dbo].[RefCusConditionValue]([ZX3_PK],[ZX3_ZX4_ValueType],[ZX3_ZX1_Condition],[ZX3_Value],[ZX3_LogicalORWithinGroup])
VALUES (newid(),'74A52793-9301-46BF-A555-1967694A61DB',@conditionPK,'Y022',1)

select @tariffPK = newid(),@conditionPK = newid()

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','0201100012',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping,ZX1_Comment)
VALUES(@conditionPK, '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', @tariffPK, '2017-11-17 00:00:00', '2079-06-06 23:59:00', '3E82B265-4D78-412E-B428-4E4AE00A4B99', 'EUN','Condition Y: test text')
INSERT INTO [dbo].[RefCusConditionValue]([ZX3_PK],[ZX3_ZX4_ValueType],[ZX3_ZX1_Condition],[ZX3_Value],[ZX3_LogicalORWithinGroup])
VALUES (newid(),'74A52793-9301-46BF-A555-1967694A61DB',@conditionPK,'Y032',1)

select @tariffPK = newid(),@conditionPK = newid()

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','0201100013',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping,ZX1_Comment)
VALUES(@conditionPK, '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', @tariffPK, '2017-11-17 00:00:00', '2079-06-06 23:59:00', '3E82B265-4D78-412E-B428-4E4AE00A4B99', 'EUN','Condition Y: test text')
INSERT INTO [dbo].[RefCusConditionValue]([ZX3_PK],[ZX3_ZX4_ValueType],[ZX3_ZX1_Condition],[ZX3_Value],[ZX3_LogicalORWithinGroup])
VALUES (newid(),'74A52793-9301-46BF-A555-1967694A61DB',@conditionPK,'L136',1)
";

			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
