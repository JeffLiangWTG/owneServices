using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataSetPKTransformationOnRefCusRateCodeLanguageTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRateCodeLanguage WHERE ZXC_DataSetPK = '1BBB5305-E596-4DBE-A1E3-6360B2C2F063' AND ZXC_DataSetCode = 'ZZR'";
				Assert.AreEqual(1, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataSetPKTransformationOnRefCusRateCodeLanguage(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var rateType = new Guid("1BBB5305-E596-4DBE-A1E3-6360B2C2F063");
				var rateCode = Guid.NewGuid();

				cmd.Transaction = Transaction;
				cmd.CommandText = $@"
INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(newid(), 'EN', 'English')

INSERT INTO RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(newid(), 'AU', 'Australia')

INSERT INTO RefCusRateType(ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
VALUES ('{rateType}', 'TP1', 'Desc1', 0, 'AU', '')

INSERT INTO RefCusRateCode(ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description, ZY1_InternalUse)
VALUES ('{rateCode}', 'RC1', '{rateType}', 'Desc1', 1)

INSERT INTO RefCusRateCodeLanguage(ZXC_PK, ZXC_Description, ZXC_ZY1_RateCode, ZXC_ZX6_NKLanguage)
VALUES (newid(), 'Desc1', '{rateCode}', 'EN')";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
