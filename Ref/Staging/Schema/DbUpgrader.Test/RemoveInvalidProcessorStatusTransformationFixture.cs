using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class RemoveInvalidProcessorStatusTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"
SELECT count(*) FROM ProcessorStatus;
";
			Assert.AreEqual(2, (int)DbHelper.ExecuteScalar(Transaction, sql));

			sql = @"
SELECT count(*) FROM ProcessorStatus WHERE PRC_PK NOT IN 
(
	SELECT PRC_PK FROM ProcessorStatus JOIN QRTZ_JOB_DETAILS 
		ON PRC_JobName=JOB_NAME AND PRC_JobGroup=JOB_GROUP AND PRC_SchedName=SCHED_NAME
);
";
			Assert.AreEqual(0, (int)DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RemoveInvalidProcessorStatusTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var insertSql = @"
ALTER TABLE [ProcessorStatus] NOCHECK CONSTRAINT [FK_ProcessorStatus_QrtzJobDetails];

insert into QRTZ_JOB_DETAILS([JOB_PK],[SCHED_NAME],[JOB_NAME],[JOB_GROUP],[DESCRIPTION],[JOB_CLASS_NAME],[IS_DURABLE],[IS_NONCONCURRENT],[IS_UPDATE_DATA],[REQUESTS_RECOVERY])
	values(NEWID(), 'RefDbRepoQuartzServer', 'CDS Tariff Data', 'GB Customs', null, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 1, 1, 0, 0),
		(NEWID(), 'RefDbRepoQuartzServer', 'AU Tariff', 'AU Customs', null, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 1, 1, 0, 0);

insert into ProcessorStatus([PRC_PK],[PRC_SchedName],[PRC_JobName],[PRC_JobGroup],[PRC_LastRunTime],[PRC_LastSuccessRunTime],[PRC_LastSuccessRecordUpdatedCount],[PRC_LastDataSetUpdatedTime],[PRC_Status])
	values(NEWID(), 'RefDbRepoQuartzServer', 'CDS Tariff Data', 'GB Customs', '2024-1-1', '2024-1-1', null, null, 'PRS'),
		(NEWID(), 'RefDbRepoQuartzServer', 'AU Tariff', 'AU Customs', '2024-1-2', '2024-1-1', null, null, 'ERR'), 
		(NEWID(), '', '', '', '2020-1-2', '2020-1-2', null, null, 'PRS');
";
				DbHelper.ExecuteNonQuery(Transaction, insertSql);
			}
		}
	}
}
