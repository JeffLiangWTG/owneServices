using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RuleRemoveWI00205083TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT Count(*)
FROM dbo.refcustariff t
JOIN dbo.refcustariffuom uom on t.ZZ1_PK = uom.ZZ8_ZZ1_Tariff
JOIN dbo.refcustariffattribute attr on attr.ZZ3_ZZ1_Tariff=t.ZZ1_PK
WHERE t.ZZ1_TariffCode like '6205%'
AND t.zz1_zzz_nkdatagrouping = 'ZA'
AND uom.ZZ8_Type = 'RU1'
AND uom.ZZ8_UOM = 'KG'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RuleRemoveWI00205083Transformation(22);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefCusTariffRule (ZZ1_PK, ZZ1_TariffCode, ZZ1_ZZZ_NKDataGrouping) VALUES ('69E542D5-8CB3-4CF8-93E3-6D8195303FA9', '620520', 'ZA'); 
INSERT INTO RefCusTariffUOMRule (ZZ8_ZZ1_Tariff, ZZ8_Type, ZZ8_UOM) VALUES ('69E542D5-8CB3-4CF8-93E3-6D8195303FA9', 'RU1','KG'); 
INSERT INTO RefCusTariffAttributeRule (ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value) VALUES ('69E542D5-8CB3-4CF8-93E3-6D8195303FA9', 'CheckDigit', '0')

INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','REF','Refund',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','5P1','5P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','4E70545B-4FE7-4EBC-91E4-A78108140A73','620520',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','CheckDigit','0');

INSERT INTO [dbo].[RefCusTariffUOM] ([ZZ8_ZZ1_Tariff],[ZZ8_Type],[ZZ8_UOM])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','RU1','ZA')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
