using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataSetPKTransformationOnRefCusTaxOrFeeLanguageTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusTaxOrFeeLanguage WHERE ZXU_DataSetPK = '1BBB5305-E596-4DBE-A1E3-6360B2C2F063' AND ZXU_DataSetCode = 'ZX0'";
				Assert.AreEqual(1, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataSetPKTransformationOnRefCusTaxOrFeeLanguage(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var taxOrFeeType1 = Guid.NewGuid();
				var taxOrFeeType2 = new Guid("1BBB5305-E596-4DBE-A1E3-6360B2C2F063");
				var taxOrFee1 = Guid.NewGuid();
				var taxOrFee2 = Guid.NewGuid();

				cmd.Transaction = Transaction;
				cmd.CommandText = $@"
INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(newid(), 'EN', 'English')

INSERT INTO RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(newid(), 'AU', 'Australia')

INSERT INTO RefCusTaxOrFeeType(ZX0_PK, ZX0_Description, ZX0_TaxOrFeeType)
VALUES ('{taxOrFeeType1}', 'Desc1', 'TP1'),
('{taxOrFeeType2}', 'Desc2', 'TP2');

INSERT INTO RefCusTaxOrFee(ZZF_PK, ZZF_Code, ZZF_Description, ZZF_EndDate, ZZF_StartDate, ZZF_Maximum, ZZF_Minimum, ZZF_Threshold, ZZF_Value, ZZF_ZX0_NKTaxOrFeeType, ZZF_ZZZ_NKDataGrouping) 
VALUES ('{taxOrFee1}','CD1','Desc1', sysutcdatetime(), sysutcdatetime(), 1, 1, 1, 1, 'TP1', 'AU'),
('{taxOrFee2}','CD2','Desc2', sysutcdatetime(), sysutcdatetime(), 1, 1, 1, 1, 'TP2', 'AU');

INSERT INTO RefCusTaxOrFeeLanguage(ZXU_PK, ZXU_Description, ZXU_ZZF_TaxOrFee, ZXU_ZX6_NKLanguage)
VALUES (newid(), 'Desc1', '{taxOrFee2}', 'EN')";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
