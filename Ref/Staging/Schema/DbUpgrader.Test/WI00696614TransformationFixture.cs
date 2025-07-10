using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00696614TransformationFixture : TransformationFixture
	{
		protected override void PrepareTestData()
		{
		}

		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = GetTemporalTableCheckQuery("QRTZ_FIRED_TRIGGERS");
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = GetTemporalTableCheckQuery("QRTZ_CRON_TRIGGERS");
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = GetTemporalTableCheckQuery("QRTZ_SIMPLE_TRIGGERS");
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = GetTemporalTableCheckQuery("QRTZ_TRIGGERS");
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = GetTemporalTableCheckQuery("QRTZ_JOB_DETAILS");
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}


		string GetTemporalTableCheckQuery(string tableName)
		{
			return $"SELECT OBJECTPROPERTY(OBJECT_ID('{tableName}'), 'TableTemporalType')";
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00696614Transformation(0);
		}
	}
}
