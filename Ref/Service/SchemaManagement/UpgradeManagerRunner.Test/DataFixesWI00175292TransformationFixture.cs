using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixesWI00175292TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTariffRule t Join RefCusTariffUOMRule u on u.ZZ8_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '27121010' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And u.ZZ8_Type = 'RU1' And u.ZZ8_UOM = 'NO'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariffRule t Join RefCusTariffUOMRule u on u.ZZ8_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '16025040' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And u.ZZ8_Type = 'RU1' And u.ZZ8_UOM = 'NO'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '48114190' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And t.ZZ1_StartDate = '2015-01-01 00:00:00' And t.ZZ1_EndDate = '2079-06-06 23:59' And r.ZZ2_StartDate = '2015-01-01 00:00:00' And r.ZZ2_EndDate = '2079-06-06 23:59'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0, true);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefDbVersionControl vc on vc.RVC_ParentPK = t.ZZ1_PK Where t.ZZ1_TariffCode = '03055990' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And vc.RVC_Deleted = 1";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Where t.ZZ1_TariffCode = '03055990' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And t.ZZ1_StartDate = '2010-01-01 00:00:00' And t.ZZ1_EndDate = '2016-12-31 23:59'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '03055990' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And r.ZZ2_EndDate = '2016-12-31 23:59'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '03055990' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And r.ZZ2_EndDate = '2014-12-31 23:59'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '1041605' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And r.ZZ2_StartDate = '2017-02-22 00:00:00' And r.ZZ2_EndDate = '2079-06-06 23:59:00' And r.ZZ2_RateFormula = '6.17 * [LI]'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '1041605' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And r.ZZ2_StartDate = '2017-02-22 00:00:00' And r.ZZ2_EndDate = '2079-06-06 23:59:00' And r.ZZ2_RateFormula = '6.71 * [LI]'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixesWI00175292Transformation(3);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefCusTariffRule (ZZ1_PK, ZZ1_TariffCode, ZZ1_ZZZ_NKDataGrouping) VALUES ('69E542D5-8CB3-4CF8-93E3-6D8195303FA9', '27121010', 'ZA'); 
INSERT INTO RefCusTariffUOMRule (ZZ8_ZZ1_Tariff, ZZ8_Type, ZZ8_UOM) VALUES ('69E542D5-8CB3-4CF8-93E3-6D8195303FA9', 'RU1','KG'); 
INSERT INTO RefCusTariffAttributeRule (ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value) VALUES ('69E542D5-8CB3-4CF8-93E3-6D8195303FA9', 'CheckDigit', '0')

INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','1P1','1P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','4E70545B-4FE7-4EBC-91E4-A78108140A73','48114190',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('452D2DBE-F28F-47D7-B255-FE956CD6D60F','30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','2015-01-01 00:00:00','2016-01-01 00:00:00','0','STANDARD');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('04E8D6C9-8DCB-46E2-A602-1A43484F3C15','4E70545B-4FE7-4EBC-91E4-A78108140A73','03055990',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('9A4D516C-C230-4995-88D6-ADB40FD04EA3','4E70545B-4FE7-4EBC-91E4-A78108140A73','03055990',0,'Description','2010-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('5A8429E4-0044-4E69-9FB0-FE17719784E4','9A4D516C-C230-4995-88D6-ADB40FD04EA3','2010-01-01 00:00:00','2014-12-31 23:59:00','0','STANDARD');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('614DA671-1770-43DB-9FDB-9202FE444D0C','9A4D516C-C230-4995-88D6-ADB40FD04EA3','2015-01-01 00:00:00','2079-06-06 23:59:00','0','STANDARD');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','1041605',0,'Description','2010-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('D987A1F5-AB3C-47AE-A6FA-4A6AABB8269A','C0CB362C-26C2-40CF-9B99-846503F705B8','2017-02-22 00:00:00','2079-06-06 23:59:00','6.71 * [LI]','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('F06D342F-01F6-4CB3-B85E-AFAF4A33E706','C0CB362C-26C2-40CF-9B99-846503F705B8','2017-02-22 00:00:00','2017-02-22 23:59:00','6.17 * [LI]','');

";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
