using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	internal class DataFixRemoveEUNConditionWhenConditionTypeIsFFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusConditionValue Where ZX3_ZX1_Condition = 'DBA7EBEA-0D8D-4A16-B48D-0E6C56EA3943'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				cmd.CommandText = @"Select Count(1) from RefCusCondition Where ZX1_PK = 'DBA7EBEA-0D8D-4A16-B48D-0E6C56EA3943'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				cmd.CommandText = @"Select Count(1) from RefCusApplicability Where ZZT_ZX1_Conditions = 'DBA7EBEA-0D8D-4A16-B48D-0E6C56EA3943'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));

				cmd.CommandText = @"Select Count(1) from RefCusConditionValue Where ZX3_ZX1_Condition = '6DB67723-95B5-4412-878A-024F5141A56B'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
				cmd.CommandText = @"Select Count(1) from RefCusCondition Where ZX1_PK = '6DB67723-95B5-4412-878A-024F5141A56B'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
				cmd.CommandText = @"Select Count(1) from RefCusApplicability Where ZZT_ZX1_Conditions = '6DB67723-95B5-4412-878A-024F5141A56B'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixRemoveEUNConditionWhenConditionTypeIsF(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','Europe','EUN');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'EUN','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','2P1','2P1','EUN','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusConditionValueType] ([ZX4_PK],[ZX4_ValueType],[ZX4_Description],[ZX4_IsFormula],[ZX4_ZZZ_NKDataGrouping])
VALUES ('779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4','FRM','Formula','1','EUN');

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('4ECE8DC2-8E84-493E-83BD-6C3195F2956E', 'RATE', '745', 'Testing', 'EUN')

--valid condition
declare @tariffPK uniqueidentifier
select @tariffPK = newid()
INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','0201100013',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping,ZX1_Comment)
VALUES('DBA7EBEA-0D8D-4A16-B48D-0E6C56EA3943', '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', @tariffPK, '2017-11-17 00:00:00', '2079-06-06 23:59:00', NULL, 'EUN','Condition Y: test text')
INSERT INTO [dbo].[RefCusConditionValue]([ZX3_PK],[ZX3_ZX4_ValueType],[ZX3_ZX1_Condition],[ZX3_Value],[ZX3_LogicalORWithinGroup])
VALUES (newid(),'779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4','DBA7EBEA-0D8D-4A16-B48D-0E6C56EA3943','L136',1)
INSERT INTO RefCusApplicability(ZZT_PK, ZZT_AdditionalCode, ZZT_StartDate, ZZT_EndDate, ZZT_OrderNumber, ZZT_ZX1_Conditions)
VALUES(newid(), 'A1', '2017-11-18 00:00:00', '2079-06-06 23:59:00', '1', 'DBA7EBEA-0D8D-4A16-B48D-0E6C56EA3943')

--condition F, to delete
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping,ZX1_Comment)
VALUES('6DB67723-95B5-4412-878A-024F5141A56B', '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', @tariffPK, '2017-11-17 00:00:00', '2079-06-06 23:59:00', NULL, 'EUN','Condition F: test text')
INSERT INTO [dbo].[RefCusConditionValue]([ZX3_PK],[ZX3_ZX4_ValueType],[ZX3_ZX1_Condition],[ZX3_Value],[ZX3_LogicalORWithinGroup])
VALUES (newid(),'779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4','6DB67723-95B5-4412-878A-024F5141A56B','L136',1)
INSERT INTO RefCusApplicability(ZZT_PK, ZZT_AdditionalCode, ZZT_StartDate, ZZT_EndDate, ZZT_OrderNumber, ZZT_ZX1_Conditions)
VALUES(newid(), 'A1', '2017-11-17 00:00:00', '2079-06-06 23:59:00', '1', '6DB67723-95B5-4412-878A-024F5141A56B')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
