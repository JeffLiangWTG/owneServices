using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00707281TransformationFixture : TransformationFixture
	{
		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO QRTZ_JOB_DETAILS (JOB_PK, SCHED_NAME, JOB_NAME, JOB_GROUP, DESCRIPTION, JOB_CLASS_NAME, IS_DURABLE, IS_NONCONCURRENT, IS_UPDATE_DATA, REQUESTS_RECOVERY, JOB_DATA) VALUES
('943B0F93-426D-44B9-9203-63BF3107DE5C', 'RefDbRepoQuartzServer', 'ZA Vessels and Carriers', 'ZA Customs', NULL, 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers', '1', '1', '0', '0', NULL),
('D18960AF-4FEF-471E-9F3A-9660CEBB6A16', 'RefDbRepoQuartzServer', 'TR Code Lists UXML', 'TR Customs', NULL, 'CargoWise.RefDbRepo.Staging.Schedulers.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers', '1', '1', '0', '0', NULL),
('A4C44789-320F-45F0-811D-032E243928F5', 'RefDbRepoQuartzServer', 'AU Exchange Rate Parser', 'AU Customs', NULL, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', '1', '1', '0', '0', NULL);
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM QRTZ_JOB_DETAILS WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers' OR JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"SELECT COUNT(1) FROM QRTZ_JOB_DETAILS";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00707281Transformation(0);
		}
	}
}
