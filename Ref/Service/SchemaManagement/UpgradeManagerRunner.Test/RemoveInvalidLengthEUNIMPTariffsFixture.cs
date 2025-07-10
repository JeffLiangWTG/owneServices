using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class RemoveInvalidLengthEUNIMPTariffsFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(*) FROM RefCusTariff
JOIN RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
JOIN RefDbVersionControl ON RVC_ParentPK = ZZ1_PK
WHERE ZZI_TariffType = 'IMP' AND ZZI_ZZZ_NKDataGrouping = 'EUN' AND RVC_Deleted = 0 AND ZZ1_TariffCode = '9880110000'";
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RemoveInvalidLengthEUNIMPTariffs(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','EUN','EUN');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','IMP','IMP','EUN', NULL,1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','98801',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('389C7627-9378-47FE-AE8E-E477649EF2C3','4E70545B-4FE7-4EBC-91E4-A78108140A73','9880110000',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
