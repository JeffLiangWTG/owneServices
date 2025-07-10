using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RenameDG_FPtoDG_FlashPointFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'UNDGSubstance' and  column_name = 'DG_FlashPoint' ";
				cmd.Transaction = Transaction;
				Assert.AreEqual(1, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RenameDG_FPtoDG_FlashPoint(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
EXEC sp_rename 'dbo.UNDGSubstance.DG_FlashPoint', 'DG_FP', 'COLUMN'
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
