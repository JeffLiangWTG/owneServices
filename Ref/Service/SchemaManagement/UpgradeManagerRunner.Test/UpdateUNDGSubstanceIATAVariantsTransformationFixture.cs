using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class UpdateUNDGSubstanceIATAVariantsTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "SELECT DG_Variant FROM UNDGSubstance where DG_UniqueRecordId = '9998'";
				Assert.AreEqual("a", cmd.ExecuteScalar());
				cmd.CommandText = "SELECT DG_Variant FROM UNDGSubstance where DG_UniqueRecordId = '9999'";
				Assert.AreEqual("b", cmd.ExecuteScalar());
				cmd.CommandText = "SELECT DG_Variant FROM UNDGSubstance where DG_UniqueRecordId = '10000'";
				Assert.AreEqual("c", cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new UpdateUNDGSubstanceIATAVariantsTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
IF EXISTS(select * from
sys.indexes
where name = 'IX_UNDGSubstance_DG_UNNO_DG_Variant_DG_Standard')
BEGIN
	DROP INDEX IX_UNDGSubstance_DG_UNNO_DG_Variant_DG_Standard ON UNDGSubstance;
END

INSERT INTO UNDGSubstance (DG_PK, DG_UNNO, DG_Variant, DG_Standard, DG_UniqueRecordId)
VALUES (newid(), '2222', '', 'IAT', '9998'),
(newid(), '2222', '', 'IAT', '9999'),
(newid(), '2222', '', 'IAT', '10000')";

				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
