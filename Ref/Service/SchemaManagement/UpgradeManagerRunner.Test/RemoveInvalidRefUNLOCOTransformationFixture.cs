using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RemoveInvalidRefUNLOCOTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM RefUNLOCO WHERE RL_Code NOT LIKE '[A-Z][A-Z][A-Z0-9][A-Z0-9][A-Z0-9]'";
				Assert.AreEqual(0, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RemoveInvalidRefUNLOCOTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
IF OBJECT_ID('dbo.[Constraint_RL_Code]', 'C') IS NOT NULL 
ALTER TABLE dbo.[RefUNLOCO] DROP CONSTRAINT Constraint_RL_Code

INSERT INTO RefUNLOCO (RL_PK,RL_Code,RL_GeoLocation,RL_RN_NKCountryCode) VALUES (NEWID(), 'XXXX1', CONVERT(geography, 'POINT (14.3 29.1)'),'AU')
INSERT INTO RefUNLOCO (RL_PK,RL_Code,RL_GeoLocation,RL_RN_NKCountryCode) VALUES (NEWID(), 'XXXX ', CONVERT(geography, 'POINT (12.3 29.1)'),'AU')";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
