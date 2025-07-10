using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00879039TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "SELECT COUNT(*) FROM RefApplicationAttributeType WHERE RAT_Type = 'SecretFile'";
				Assert.That(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), Is.EqualTo(1));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00879039Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
SELECT 1;
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
