using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00307638TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;

				cmd.CommandText = @"SELECT RDS_DataSetTableCode FROM RefDataSetInformation WHERE RDS_DataSetId = 22";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("ZZO"));

				cmd.CommandText = @"SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentCode = 'ZZO'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(22));

				cmd.CommandText = "SELECT RVC_LastUpdatedUTC FROM RefDbVersionControl WHERE RVC_ParentCode = 'ZZO'";
				var lastUpdatedUtcExpected = cmd.ExecuteScalar();

				cmd.CommandText = "SELECT RDS_LastUpdatedUTC FROM RefDataSetInformation WHERE RDS_DataSetTableCode = 'ZZO'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(lastUpdatedUtcExpected));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00307638Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
VALUES (newid(), 22, 'RefVesselZZ', 'RefVesselZZ', 'ZZ0', 0);

INSERT INTO RefDbVersionControl (RVC_ParentPK, RVC_ParentCode, RVC_LastUpdatedUTC, RVC_IsPublished)
VALUES(newid(), 'ZZO', sysutcdatetime(), 1)
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
