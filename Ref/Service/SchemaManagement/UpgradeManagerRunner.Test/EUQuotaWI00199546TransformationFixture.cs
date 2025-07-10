using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class EUQuotaWI00199546TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusRateRule Where ZZ2_ZZ1_Tariff = '4C0F8C9E-CDAF-4D66-A6A1-A4ECDEAC4629' and ZZ2_SelectorFormula = 'pp=''EUQUOTA''' And ZZ2_RateFormula = '0'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCusRate Where ZZ2_StartDate = '2018-01-01' And ZZ2_RateFormula = '0'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCusRate Where ZZ2_EndDate = '2017-12-31 23:59'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new EUQuotaWI00199546Transformation(19);
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
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','1P1','E9E87774-45AC-49CA-925B-6756A31C5E3A','1P1');

INSERT INTO RefCusPreference(ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES ('83E8CF30-E966-423D-AC4B-10BD027AA576','400','Preferential Quota','ZA');

INSERT INTO [dbo].[RefCusTradeGroup] ([ZZA_TradeGroup],[ZZA_Description],[ZZA_StartDate],[ZZA_EndDate],[ZZA_ZZZ_NKDataGrouping])
VALUES ('EUQUOTA','EUQUOTA','1900-01-01','2079-06-06 23:59','ZA');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','040610',0,'Description','2010-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO [dbo].[RefCusRate] ([ZZ2_ZZ1_Tariff],[ZZ2_StartDate],[ZZ2_EndDate],[ZZ2_ZY1_RateCode],[ZZ2_RateFormula],[ZZ2_ZZS_Preference],[ZZ2_SelectorFormula],[ZZ2_ZZZ_NKDataGrouping],[ZZ2_RateFormulaDerivedFrom])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','2010-01-01 00:00:00','2079-06-06 23:59:00','DAA86362-673C-469B-AB26-E6B55B921B34','Old Formula','83E8CF30-E966-423D-AC4B-10BD027AA576','pp=''EUQUOTA''','ZA','Some Rule');

INSERT INTO RefCusTariffRule(ZZ1_PK,ZZ1_TariffCode,ZZ1_ZZZ_NKDataGrouping)
VALUES ('4C0F8C9E-CDAF-4D66-A6A1-A4ECDEAC4629','040610','ZA');

INSERT INTO RefCusRateRule(ZZ2_ZZ1_Tariff,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('4C0F8C9E-CDAF-4D66-A6A1-A4ECDEAC4629','Old Formula','pp=''EUQUOTA''','ZA');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
