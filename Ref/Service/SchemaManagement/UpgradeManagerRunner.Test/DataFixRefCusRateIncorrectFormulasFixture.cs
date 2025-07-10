using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixRefCusRateIncorrectFormulasFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				Assert.AreEqual("MIN(VFD * 0.055 + #EAR(1)#, VFD * 0.155 +#ADSZR(1)#)", DbHelper.ExecuteScalar(Transaction, "SELECT ZZ2_RateFormula FROM RefCusRate WHERE ZZ2_PK='5A8429E4-0044-4E69-9FB0-FE17719784E4'"));
				Assert.AreEqual("MIN(VFD * 0.055 + #EAR(2)#, VFD * 0.155 +#ADFMR(1)#)", DbHelper.ExecuteScalar(Transaction, "SELECT ZZ2_RateFormula FROM RefCusRate WHERE ZZ2_PK='614DA671-1770-43DB-9FDB-9202FE444D0C'"));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixRefCusRateIncorrectFormulas(0);
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

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','4E70545B-4FE7-4EBC-91E4-A78108140A73','48114190',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('452D2DBE-F28F-47D7-B255-FE956CD6D60F','30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','2015-01-01 00:00:00','2016-01-01 00:00:00','0','STANDARD');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('04E8D6C9-8DCB-46E2-A602-1A43484F3C15','4E70545B-4FE7-4EBC-91E4-A78108140A73','03055990',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('9A4D516C-C230-4995-88D6-ADB40FD04EA3','4E70545B-4FE7-4EBC-91E4-A78108140A73','03055990',0,'Description','2010-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('5A8429E4-0044-4E69-9FB0-FE17719784E4','9A4D516C-C230-4995-88D6-ADB40FD04EA3','2010-01-01 00:00:00','2014-12-31 23:59:00','MIN(VFD * 0.055 + #EAR(1)#, 1VFD * 0.055 +#ADSZR(1)#)','STANDARD');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('614DA671-1770-43DB-9FDB-9202FE444D0C','9A4D516C-C230-4995-88D6-ADB40FD04EA3','2015-01-01 00:00:00','2079-06-06 23:59:00','MIN(VFD * 0.055 + #EAR(2)#, 1VFD * 0.055 +#ADFMR(2)#)','STANDARD');

";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
