using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00227004TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '020910' and convert(varchar(19),ZZ2_EndDate,120) = '2079-06-06 23:59:00'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '020322' and convert(varchar(19),ZZ2_EndDate,120) = '2018-12-31 23:59:00'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '020322' and convert(varchar(19),ZZ2_StartDate,120) = '2019-01-01 00:00:00' and r.ZZ2_RateFormula = 'MAX(0.075 * VFD, 0.65 * [KG])'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '020322' and convert(varchar(19),ZZ2_StartDate,120) = '2020-01-01 00:00:00' and r.ZZ2_RateFormula = 'MAX(0.05625 * VFD, 0.4875* [KG])'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '020322' and convert(varchar(19),ZZ2_StartDate,120) = '2021-01-01 00:00:00' and r.ZZ2_RateFormula = 'MAX(0.0375 * VFD, 0. 325* [KG])'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '04051010' and convert(varchar(19),ZZ2_StartDate,120) = '2019-01-01 00:00:00' and r.ZZ2_RateFormula = 'MIN(2.5 * [KG], 0.395 * VFD)'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '04051010' and convert(varchar(19),ZZ2_StartDate,120) = '2020-01-01 00:00:00' and r.ZZ2_RateFormula = 'MIN(1.875 * [KG], 0.29625 * VFD)'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK Where ZZ1_TariffCode = '04051010' and convert(varchar(19),ZZ2_StartDate,120) = '2021-01-01 00:00:00' and r.ZZ2_RateFormula = 'MIN(1.25 * [KG], 0.1975 * VFD)'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t
Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
Join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
Where ZZ1_TariffCode = '020322'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 4);
				cmd.CommandText = @"Select Count(1) from RefCusTariff t
Join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
Where ZZ1_TariffCode = '020322'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 4);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00227004Transformation(41);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','1P1','1P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','1P1','E9E87774-45AC-49CA-925B-6756A31C5E3A','2P1');

declare @tariffPK uniqueidentifier
,@ratePK uniqueidentifier

Select @tariffPK = newid()
,@ratePK = newid()

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','020910',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffAttribute ([ZZ3_PK],[ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (newid(),@tariffPK,'CheckDigit','80');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES (@ratePK,@tariffPK,'2015-01-01 00:00:00','2019-01-01 00:00:00','0','pp=''EUQUOTA''','ZA');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (newid(),@ratePK,'2015-01-01 00:00:00','2019-01-01 00:00:00','Additional','OrderNO');

Select @tariffPK = newid()
,@ratePK = newid()

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','020322',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffAttribute ([ZZ3_PK],[ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (newid(),@tariffPK,'CheckDigit','80');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES (@ratePK,@tariffPK,'2015-01-01 00:00:00','2019-01-01 00:00:00','0','pp=''EUQUOTA''','ZA');


INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (newid(),@ratePK,'2015-01-01 00:00:00','2019-01-01 00:00:00','Additional','OrderNO');

Select @tariffPK = newid()
,@ratePK = newid()

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@tariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','04051010',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffAttribute ([ZZ3_PK],[ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (newid(),@tariffPK,'CheckDigit','80');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES (@ratePK,@tariffPK,'2015-01-01 00:00:00','2019-01-01 00:00:00','0','pp=''EUQUOTA''','ZA');


INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (newid(),@ratePK,'2015-01-01 00:00:00','2019-01-01 00:00:00','Additional','OrderNO');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
