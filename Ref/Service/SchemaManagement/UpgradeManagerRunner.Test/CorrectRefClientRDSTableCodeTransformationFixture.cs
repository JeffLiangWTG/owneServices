using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class CorrectRefClientRDSTableCodeTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefDataSetInformation WHERE RDS_DataSetTableCode = 'ZCT' AND RDS_TableName = 'RefClient' ";
				Assert.AreEqual(cmd.ExecuteScalar(), 0);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new CorrectRefClientRDSTableCodeTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
VALUES (newid(), 57, 'RefClient', 'RefClient', 'ZCT', 0);
				";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
