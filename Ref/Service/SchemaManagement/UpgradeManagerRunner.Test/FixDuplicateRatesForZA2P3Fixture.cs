using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class FixDuplicateRatesForZA2P3Fixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_ZZ1_Tariff = 'C0CB362C-26C2-40CF-9B99-846503F705B8'";
			Assert.AreEqual(2, DbHelper.ExecuteScalar(Transaction, sql));
			sql = @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZA_TradeGroup IS NULL";
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, sql));
			sql = @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZA_TradeGroup = '6DEF775F-7FA2-4C72-9308-436DBEB9F5E4'";
			Assert.AreEqual(2, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new FixDuplicateRatesForZA2P3(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','ZA','ZA', NULL)

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','2P3','2P3','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','2P3','E9E87774-45AC-49CA-925B-6756A31C5E3A','1P1');

INSERT INTO [dbo].[RefCusTradeGroup] (ZZA_PK, [ZZA_TradeGroup],[ZZA_Description],[ZZA_StartDate],[ZZA_EndDate],[ZZA_ZZZ_NKDataGrouping])
VALUES ('6DEF775F-7FA2-4C72-9308-436DBEB9F5E4', 'STANDARD','STANDARD','1900-01-01','2079-06-06 23:59','ZA');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','260030104',0,'Description','2010-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO [dbo].[RefCusRate] (ZZ2_PK, [ZZ2_ZZ1_Tariff],[ZZ2_StartDate],[ZZ2_EndDate],[ZZ2_ZY1_RateCode],[ZZ2_RateFormula],[ZZ2_ZZZ_NKDataGrouping],[ZZ2_RateFormulaDerivedFrom])
VALUES
	('95A87ABD-B88F-45E6-A9E0-0171260B5A28','C0CB362C-26C2-40CF-9B99-846503F705B8','2017-08-11','2018-08-10 23:59:00','DAA86362-673C-469B-AB26-E6B55B921B34', '0.1 * VFD','ZA',''),
	('8B5F5670-7BD0-4847-8021-B7694E2B755B','C0CB362C-26C2-40CF-9B99-846503F705B8', '2017-08-11','2019-08-10 23:59:00','DAA86362-673C-469B-AB26-E6B55B921B34', '0.1 * VFD','ZA',''),
	('62B771F5-0A8F-42D1-9211-BBAB5C33B674','C0CB362C-26C2-40CF-9B99-846503F705B8', '2017-08-11','2019-08-10 23:59:00','DAA86362-673C-469B-AB26-E6B55B921B34', '0.1 * VFD','ZA','');

INSERT INTO [dbo].[RefCusApplicability] (ZZT_PK, ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber)
VALUES
	(newid(), '95A87ABD-B88F-45E6-A9E0-0171260B5A28', '1900-01-01', '2079-06-06', NULL, '', ''),
	(newid(), '8B5F5670-7BD0-4847-8021-B7694E2B755B', '1900-01-01', '2079-06-06', '6DEF775F-7FA2-4C72-9308-436DBEB9F5E4', '', ''),
	(newid(), '62B771F5-0A8F-42D1-9211-BBAB5C33B674', '1900-01-01', '2079-06-06', NULL, '', '')
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
