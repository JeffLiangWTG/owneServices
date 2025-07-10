using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class AddingRVC_IsPublishedTranformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_ParentCode = 'XXX'";
				cmd.Transaction = Transaction;
				Assert.AreEqual(3, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new AddingRVC_IsPublishedTranformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDbVersionControl (RVC_ParentPK, RVC_ParentCode)
VALUES (newid(), 'XXX'),
	(newid(), 'XXX'),
	(newid(), 'XXX')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
