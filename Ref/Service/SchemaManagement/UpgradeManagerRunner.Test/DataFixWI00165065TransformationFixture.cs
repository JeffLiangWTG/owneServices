using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00165065TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusRate r Where ZZ2_ZZ1_Tariff = 'C0CB362C-26C2-40CF-9B99-846503F705B8' and ZZ2_RateFormula = '0'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusRate r Where ZZ2_ZZ1_Tariff = 'C0CB362C-26C2-40CF-9B99-846503F705B8' and ZZ2_RateFormula = '0.0025 * VFD'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusTariffUOM uom Where ZZ8_ZZ1_Tariff = 'C0CB362C-26C2-40CF-9B99-846503F705B8' and ZZ8_UOM = 'KLT'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusRate r Join RefCusRateCode rc on r.ZZ2_ZY1_RateCode = rc.ZY1_PK Where ZZ2_ZZ1_Tariff = 'C0CB362C-26C2-40CF-9B99-846503F705B8' and ZY1_RateCode = '933'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 4);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00165065Transformation(44);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','IT','Italy','IT');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'IT','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','2P1','2P1','IT','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','116','E9E87774-45AC-49CA-925B-6756A31C5E3A','116');

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES (newid(),'933','E9E87774-45AC-49CA-925B-6756A31C5E3A','933');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','2710194729',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','IT','');

INSERT INTO RefCusTariffAttribute ([ZZ3_PK],[ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (newid(),'C0CB362C-26C2-40CF-9B99-846503F705B8','CheckDigit','80');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZY1_RateCode)
VALUES (newid(),'C0CB362C-26C2-40CF-9B99-846503F705B8','2015-01-01 00:00:00','2079-06-06 23:59:00','0 * [ASVX]','','IT','DAA86362-673C-469B-AB26-E6B55B921B34');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZY1_RateCode)
VALUES (newid(),'C0CB362C-26C2-40CF-9B99-846503F705B8','2015-01-01 00:00:00','2079-06-06 23:59:00','617.4 * [KLT]','','IT','DAA86362-673C-469B-AB26-E6B55B921B34');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZY1_RateCode)
VALUES (newid(),'C0CB362C-26C2-40CF-9B99-846503F705B8','2014-01-01 00:00:00','2014-12-31 23:59:00','717.4 * [KLT]','','IT','DAA86362-673C-469B-AB26-E6B55B921B34');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZY1_RateCode)
VALUES (newid(),'C0CB362C-26C2-40CF-9B99-846503F705B8','2015-01-01 00:00:00','2079-06-06 23:59:00','0.25','','IT','DAA86362-673C-469B-AB26-E6B55B921B34');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
