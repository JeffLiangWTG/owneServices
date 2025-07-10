using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class FixEUNTariffPreferenceRateMappingTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusApplicability a INNER JOIN
RefCusRate ON ZZ2_PK = ZZT_ZZ2_Rate INNER JOIN
RefCusTariff ON ZZ1_PK = ZZ2_ZZ1_Tariff INNER JOIN
RefCusTradeGroup ON ZZT_ZZA_TradeGroup = ZZA_PK INNER JOIN
RefCusPreference ON ZZ2_ZZS_Preference =ZZS_PK
WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZS_Preference LIKE '2%' AND ZZA_TradeGroup NOT IN ('2005', '2020','2027')";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusApplicability a INNER JOIN
RefCusRate ON ZZ2_PK = ZZT_ZZ2_Rate INNER JOIN
RefCusTariff ON ZZ1_PK = ZZ2_ZZ1_Tariff INNER JOIN
RefCusTradeGroup ON ZZT_ZZA_TradeGroup = ZZA_PK INNER JOIN
RefCusPreference ON ZZ2_ZZS_Preference =ZZS_PK
WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZS_Preference LIKE '3%' AND ZZA_TradeGroup IN ('2005', '2020','2027')";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusRate";
				Assert.AreEqual(3, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusApplicability a INNER JOIN
RefCusCondition ON ZX1_PK = ZZT_ZX1_Conditions INNER JOIN
RefCusTariff ON ZZ1_PK = ZX1_ZZ1_Tariff INNER JOIN
RefCusTradeGroup ON ZZT_ZZA_TradeGroup = ZZA_PK INNER JOIN
RefCusPreference ON ZX1_ZZS_Preference =ZZS_PK
WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZS_Preference LIKE '2%' AND ZZA_TradeGroup NOT IN ('2005', '2020','2027')";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusApplicability a INNER JOIN
RefCusCondition ON ZX1_PK = ZZT_ZX1_Conditions INNER JOIN
RefCusTariff ON ZZ1_PK = ZX1_ZZ1_Tariff INNER JOIN
RefCusTradeGroup ON ZZT_ZZA_TradeGroup = ZZA_PK INNER JOIN
RefCusPreference ON ZX1_ZZS_Preference =ZZS_PK
WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZS_Preference LIKE '3%' AND ZZA_TradeGroup IN ('2005', '2020','2027')";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT COUNT(1) FROM RefCusCondition";
				Assert.AreEqual(3, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new FixEUNTariffPreferenceRateMappingTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var tariff = Guid.NewGuid();
				var tarifftype = Guid.NewGuid();
				var incorrectrate1 = Guid.NewGuid();
				var incorrectrate2 = Guid.NewGuid();
				var correctrate = Guid.NewGuid();
				var incorrectratewithotherapplicability = Guid.NewGuid();
				var tradegroup1 = Guid.NewGuid();
				var tradegroup2 = Guid.NewGuid();
				var preference1 = Guid.NewGuid();
				var preference2 = Guid.NewGuid();
				var conditiontype = Guid.NewGuid();
				var incorrectcondition1 = Guid.NewGuid();
				var incorrectcondition2 = Guid.NewGuid();
				var correctcondition = Guid.NewGuid();
				var incorrectconditionwithotherapplicability = Guid.NewGuid();

				cmd.CommandText = $@"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping) VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','EUN','EUN', NULL)

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping) VALUES ('{tarifftype}', '1P1', '1P1','EUN');

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode) VALUES ('{tariff}', '{tarifftype}','EUN','Test','T','');

INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES ('{preference1}','300','3000','EUN')

INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES ('{preference2}','200','2000','EUN')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('{conditiontype}', 'CLASS', 'ZADOC', 'Testing', 'EUN')

INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula) 
VALUES ('{incorrectrate1}','{preference1}','{tariff}','pp=''MERCOSUR1''','1900-01-01','2079-06-06','{tariff}','ZZ1','EUN','T1');

INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula) 
VALUES ('{incorrectrate2}','{preference2}','{tariff}','pp=''MERCOSUR2''','1900-01-01','2079-06-06','{tariff}','ZZ1','EUN','T2');

--Correct Rate
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula) 
VALUES ('{correctrate}','{preference1}','{tariff}','pp=''MERCOSUR3''','1900-01-01','2079-06-06','{tariff}','ZZ1','EUN','T2');

--Incorrect Rate with other applicability
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula) 
VALUES ('{incorrectratewithotherapplicability}','{preference2}','{tariff}','pp=''MERCOSUR4''','1900-01-01','2079-06-06','{tariff}','ZZ1','EUN','T2');

--Rate has no applicability
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula) 
VALUES (NEWID(),NULL,'{tariff}','pp=''MERCOSUR5''','1900-01-01','2079-06-06','{tariff}','ZZ1','EUN','T2');

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description) 
VALUES ('{tradegroup1}','2005','EUN', 'GSP (R 12/978) - Annex IV');

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description) 
VALUES ('{tradegroup2}','CH','EUN', 'Switzerland');

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping)
VALUES('{incorrectcondition1}', '{conditiontype}', '{tariff}', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '{preference1}', 'EUN')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping)
VALUES('{incorrectcondition2}', '{conditiontype}', '{tariff}', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '{preference2}', 'EUN')

--Correct Condition
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping)
VALUES('{correctcondition}', '{conditiontype}', '{tariff}', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '{preference1}', 'EUN')

--Incorrect Condition with other applicability
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping)
VALUES('{incorrectconditionwithotherapplicability}', '{conditiontype}', '{tariff}', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '{preference2}', 'EUN')

--Condition has no applicability
INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping)
VALUES(NEWID(), '{conditiontype}', '{tariff}', '2019-12-14 00:00:00', '2079-06-06 23:59:00', NULL, 'EUN')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectrate1}','1900-01-01','2079-06-06','{tradegroup1}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectrate2}','1900-01-01','2079-06-06','{tradegroup2}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectratewithotherapplicability}','1900-01-01','2079-06-06','{tradegroup1}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectratewithotherapplicability}','1900-01-01','2079-06-06','{tradegroup2}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{correctrate}','1900-01-01','2079-06-06','{tradegroup2}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectcondition1}','1900-01-01','2079-06-06','{tradegroup1}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectcondition2}','1900-01-01','2079-06-06','{tradegroup2}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectconditionwithotherapplicability}','1900-01-01','2079-06-06','{tradegroup1}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{incorrectconditionwithotherapplicability}','1900-01-01','2079-06-06','{tradegroup2}','','');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'{correctcondition}','1900-01-01','2079-06-06','{tradegroup2}','','');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
