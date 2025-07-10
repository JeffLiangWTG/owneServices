using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixHLTConditionalRateFormulaEUNTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(*) FROM RefCusRate WHERE zz2_rateformula like 'if(VFD/[[]DTN]%* [[]HLT]%'"));
				Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(*) FROM RefCusRate WHERE zz2_rateformula = 'If(VFD/[HLT] >= 42.500, 20.600 * [DTN], If(VFD/[HLT] >= 41.700, 0.800 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 40.800, 1.700 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 40.000, 2.500 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 39.100, 3.400 * [HLT] + 20.600 * [DTN], 27.000 * [HLT] + 20.600 * [DTN])))))'"));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixHLTConditionalRateFormulaEUNTransformation(67);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','European','EUN');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'EUN','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','IMP','IMP','EUN','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','0201100010',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

INSERT INTO RefCusTariffRelationship (ZZH_PK,ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES ('537D8090-0057-4AA4-835F-8C3EBB773507','C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','70052925');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('D987A1F5-AB3C-47AE-A6FA-4A6AABB8269A','C0CB362C-26C2-40CF-9B99-846503F705B8','2017-11-17 00:00:00','2079-06-06 23:59:00','If(VFD/[DTN] >= 42.500, 20.600 * [DTN], If(VFD/[DTN] >= 41.700, 0.800 * [HLT] + 20.600 * [DTN], If(VFD/[DTN] >= 40.800, 1.700 * [HLT] + 20.600 * [DTN], If(VFD/[DTN] >= 40.000, 2.500 * [HLT] + 20.600 * [DTN], If(VFD/[DTN] >= 39.100, 3.400 * [HLT] + 20.600 * [DTN], 27.000 * [HLT] + 20.600 * [DTN])))))','','EUN');
";

			using (var cmd = Transaction?.Connection?.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
