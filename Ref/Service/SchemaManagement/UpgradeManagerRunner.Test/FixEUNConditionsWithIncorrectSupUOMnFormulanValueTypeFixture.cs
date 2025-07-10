using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class FixEUNConditionsWithIncorrectSupUOMnFormulanValueTypeFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			Assert.AreEqual("[KGM]/[LPA] < 0.201 & [KGM]/[LPA] >= 0.130", DbHelper.ExecuteScalar(Transaction, "SELECT ZX3_Value FROM RefCusConditionValue WHERE ZX3_PK = 'A6288DAE-6A5D-4BF6-BBF2-F1B7125F08D6'"));
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_PK = 'AE460AF9-9304-4FDF-9BF4-8E377FE6F733'"));
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_PK = '2D09942E-1C6E-41A8-AD59-B4B0013197F4'"));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new FixEUNConditionsWithIncorrectSupUOMnFormulanValueType(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','European','EUN');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','IMP','IMP','EUN', NULL,1);

INSERT INTO [dbo].[RefCusConditionValueType] ([ZX4_PK],[ZX4_ValueType],[ZX4_Description],[ZX4_IsFormula],[ZX4_ZZZ_NKDataGrouping])
VALUES ('779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4','FRM','Formula','1','EUN'),
('8C51AAB2-7FAE-4C55-97E0-D1F009663D26','SNR','Supporting Document','1','EUN')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('4ECE8DC2-8E84-493E-83BD-6C3195F2956E', 'RATE', '745', 'Testing', 'EUN')

INSERT INTO [dbo].[RefCusTradeGroup] (ZZA_PK, [ZZA_TradeGroup],[ZZA_Description],[ZZA_StartDate],[ZZA_EndDate],[ZZA_ZZZ_NKDataGrouping])
VALUES ('6DEF775F-7FA2-4C72-9308-436DBEB9F5E4', 'EUN','EUN','1900-01-01','2079-06-06 23:59','EUN');

-- 5208129620 has incorrect sup uom
INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','5208129620', 0,'With:  - a width of not more than 145 cm','2021-01-01 00:00:00.000','2021-12-31 00:00:00.000','','EUN','');

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping, ZX1_Comment)
VALUES('32E2DAE7-F3C8-4D74-8184-C253D8F1C687', '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', 'C0CB362C-26C2-40CF-9B99-846503F705B8', '2017-11-17 00:00:00', '2079-06-06 23:59:00', NULL, 'EUN','')

INSERT INTO [dbo].[RefCusConditionValue]([ZX3_PK],[ZX3_ZX4_ValueType],[ZX3_ZX1_Condition],[ZX3_Value],[ZX3_LogicalORWithinGroup])
VALUES
('A6288DAE-6A5D-4BF6-BBF2-F1B7125F08D6', '779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4', '32E2DAE7-F3C8-4D74-8184-C253D8F1C687','[KGM]/[NAR] < 0.201 & [KGM]/[NAR] >= 0.130', 1),
('AE460AF9-9304-4FDF-9BF4-8E377FE6F733', '779C9E24-A62A-7D3B-BEF1-08D7DC44DDB4', '32E2DAE7-F3C8-4D74-8184-C253D8F1C687','[] <=', 1),
('2D09942E-1C6E-41A8-AD59-B4B0013197F4', '8C51AAB2-7FAE-4C55-97E0-D1F009663D26', '32E2DAE7-F3C8-4D74-8184-C253D8F1C687','[KGM] <= 20.000', 1)

INSERT INTO RefCusApplicability (ZZT_PK, ZZT_ZX1_Conditions, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode,ZZT_OrderNumber, ZZT_ZZA_TradeGroup)
VALUES ('D5E2EA9D-4121-4C97-934E-E619B0610072', '32E2DAE7-F3C8-4D74-8184-C253D8F1C687','2017-11-17 00:00:00','2079-06-06 23:59:00','','', '6DEF775F-7FA2-4C72-9308-436DBEB9F5E4')

INSERT INTO [dbo].[RefCusTariffUOM] (ZZ8_PK, ZZ8_ZZ1_Tariff, ZZ8_Type, ZZ8_UOM, ZZ8_ZZA_TradeGroup, ZZ8_ZZZ_NKDataGrouping)
VALUES (newid(), 'C0CB362C-26C2-40CF-9B99-846503F705B8', 'CU2', 'LPA', '6DEF775F-7FA2-4C72-9308-436DBEB9F5E4', 'EUN')
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
