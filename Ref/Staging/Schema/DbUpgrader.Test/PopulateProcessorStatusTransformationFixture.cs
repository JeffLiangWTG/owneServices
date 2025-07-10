using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class PopulateProcessorStatusTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"
SELECT COUNT(*) FROM ProcessorStatus
WHERE PRC_Processor = 'CDS Tariff Data' AND PRC_SchedName = 'RefDbRepoQuartzServer' AND PRC_JobName = 'CDS Tariff Data' AND PRC_JobGroup = 'GB Customs';
";
			Assert.AreEqual((int)DbHelper.ExecuteScalar(Transaction, sql), 1);
			sql = @"
SELECT COUNT(*) FROM ProcessorStatus
WHERE PRC_Processor = 'AU Tariff' AND PRC_SchedName = 'RefDbRepoQuartzServer' AND PRC_JobName = 'AU Tariff' AND PRC_JobGroup = 'AU Customs';
";
			Assert.AreEqual((int)DbHelper.ExecuteScalar(Transaction, sql), 1);
			sql = @"
SELECT COUNT(*) FROM ProcessorStatus
WHERE PRC_Processor = 'Legacy Job' AND PRC_JobName = 'Legacy Job';
";
			Assert.AreEqual((int)DbHelper.ExecuteScalar(Transaction, sql), 0);
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateProcessorStatusTransformation(0);
		}

		protected override void PrepareTestData()
		{
			var preSql = @"
ALTER TABLE [ProcessorStatus] ADD PRC_Processor VARCHAR(200), PRC_Country CHAR(3);
ALTER TABLE [ProcessorStatus] NOCHECK CONSTRAINT FK_ProcessorStatus_QrtzJobDetails;
DROP INDEX IX_ProcessorStatus_PRC_SchedName_PRC_JobGroup_PRC_JobName ON [ProcessorStatus];
";
			DbHelper.ExecuteNonQuery(Transaction, preSql);

			var insertSql = @"
INSERT INTO QRTZ_JOB_DETAILS([JOB_PK],[SCHED_NAME],[JOB_NAME],[JOB_GROUP],[DESCRIPTION],[JOB_CLASS_NAME],[IS_DURABLE],[IS_NONCONCURRENT],[IS_UPDATE_DATA],[REQUESTS_RECOVERY])
	VALUES(NEWID(), 'RefDbRepoQuartzServer', 'CDS Tariff Data', 'GB Customs', null, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 1, 1, 0, 0),
		(NEWID(), 'RefDbRepoQuartzServer', 'AU Tariff', 'AU Customs', null, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 1, 1, 0, 0);

INSERT INTO ProcessorStatus([PRC_PK],[PRC_Processor],[PRC_Country],[PRC_SchedName],[PRC_JobName],[PRC_JobGroup],[PRC_LastRunTime],[PRC_LastSuccessRunTime],[PRC_LastSuccessRecordUpdatedCount],[PRC_LastDataSetUpdatedTime],[PRC_Status])
	VALUES(NEWID(), 'CDS Tariff Data', 'GB', '', '', '', '2024-1-1', '2024-1-1', null, null, 'PRS'),
		(NEWID(), 'AU Tariff', 'AU', '', '', '', '2024-1-2', '2024-1-1', null, null, 'ERR'),
		(NEWID(), 'Legacy Job', 'AU', '', '', '', '2020-1-2', '2020-1-2', null, null, 'PRS');
";
			DbHelper.ExecuteNonQuery(Transaction, insertSql);
		}
	}
}
