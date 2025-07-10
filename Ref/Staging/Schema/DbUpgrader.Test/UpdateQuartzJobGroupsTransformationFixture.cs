using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class UpdateQuartzJobGroupsTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(1) FROM QRTZ_JOB_DETAILS WHERE JOB_GROUP = 'DEFAULT'";
			Assert.AreEqual(0, (int)DbHelper.ExecuteScalar(Transaction, sql));
			sql = @"SELECT COUNT(1) FROM QRTZ_TRIGGERS WHERE JOB_GROUP = 'DEFAULT'";
			Assert.AreEqual(0, (int)DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new UpdateQuartzJobGroupsTransformation(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"INSERT INTO QRTZ_JOB_DETAILS (JOB_PK, SCHED_NAME, JOB_NAME, JOB_GROUP, JOB_CLASS_NAME, IS_DURABLE, IS_NONCONCURRENT, IS_UPDATE_DATA, REQUESTS_RECOVERY)
VALUES (NEWID(), 'RefDbRepoQuartzServer', 'UXML Parser', 'DEFAULT', 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzAppRunner', 1, 0, 0, 0),
(NEWID(), 'RefDbRepoQuartzServer', 'UXML Merger', 'DEFAULT', 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzAppRunner', 1, 0, 0, 0),
(NEWID(), 'RefDbRepoQuartzServer', 'RefAirline UXML', 'DEFAULT', 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzAppRunner', 1, 0, 0, 0);

INSERT INTO QRTZ_TRIGGERS (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, JOB_NAME, JOB_GROUP, NEXT_FIRE_TIME, PREV_FIRE_TIME, PRIORITY, TRIGGER_STATE, TRIGGER_TYPE, START_TIME)
VALUES ('RefDbRepoQuartzServer', NEWID(), 'DEFAULT', 'UXML Parser', 'DEFAULT', 638168629180572444, 638168628580572444, 5, 'WAITING', 'SIMPLE', 637081074580572444),
('RefDbRepoQuartzServer', NEWID(), 'DEFAULT', 'UXML Merger', 'DEFAULT', 638168629180572444, 638168628580572444, 5, 'WAITING', 'SIMPLE', 637081074580572444),
('RefDbRepoQuartzServer', NEWID(), 'DEFAULT', 'RefAirline UXML', 'DEFAULT', 638168629180572444, 638168628580572444, 5, 'PAUSED', 'SIMPLE', 637081074580572444);
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
