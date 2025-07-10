using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	public class HealthCheckerFixture
	{
		[Test, TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task CheckBlockedTriggersWithoutFiredTrigger()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var repo = new StagingRepository(connectionString))
			{
				var job1 = new QRTZ_JOB_DETAILS
				{
					JOB_PK = Guid.NewGuid(),
					SCHED_NAME = "RefDbRepoQuartzServer",
					JOB_NAME = "PL Code Lists",
					JOB_GROUP = "DEFAULT",
					JOB_CLASS_NAME = "ABC",
				};
				var job2 = new QRTZ_JOB_DETAILS
				{
					JOB_PK = Guid.NewGuid(),
					SCHED_NAME = "RefDbRepoQuartzServer",
					JOB_NAME = "CA Facility Data Parser",
					JOB_GROUP = "DEFAULT",
					JOB_CLASS_NAME = "ABC",
				};
				var job3 = new QRTZ_JOB_DETAILS
				{
					JOB_PK = Guid.NewGuid(),
					SCHED_NAME = "RefDbRepoQuartzServer",
					JOB_NAME = "BE Additional Info",
					JOB_GROUP = "DEFAULT",
					JOB_CLASS_NAME = "ABC",
				};
				repo.Add(job1);
				repo.Add(job2);
				repo.Add(job3); 
				await repo.SaveChangesAsync();
				var trigger1 = new QRTZ_TRIGGERS
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "7edcad33-af68-4122-8c73-d8cc304a6ab0",
					TRIGGER_GROUP = "DEFAULT",
					JOB_NAME = "PL Code Lists",
					JOB_GROUP = "DEFAULT",
					TRIGGER_STATE = "BLOCKED",
					TRIGGER_TYPE = "SIMPLE",
				};
				var trigger12 = new QRTZ_TRIGGERS
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "7f9ba1384-f8ea-41d4-9f0c-70e3bb2c35e2",
					TRIGGER_GROUP = "DEFAULT",
					JOB_NAME = "PL Code Lists",
					JOB_GROUP = "DEFAULT",
					TRIGGER_STATE = "BLOCKED",
					TRIGGER_TYPE = "SIMPLE",
				};
				var trigger2 = new QRTZ_TRIGGERS
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "108432c3-b329-4ebb-8213-7eff664245c7",
					TRIGGER_GROUP = "DEFAULT",
					JOB_NAME = "CA Facility Data Parser",
					JOB_GROUP = "DEFAULT",
					TRIGGER_STATE = "WAITING",
					TRIGGER_TYPE = "SIMPLE",
				};
				var trigger3 = new QRTZ_TRIGGERS
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "14486496-f922-4044-bf65-02a6a638e28e",
					TRIGGER_GROUP = "DEFAULT",
					JOB_NAME = "BE Additional Info",
					JOB_GROUP = "DEFAULT",
					TRIGGER_STATE = "BLOCKED",
					TRIGGER_TYPE = "SIMPLE",
				};
				var firedTrigger = new QRTZ_FIRED_TRIGGERS
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					JOB_NAME = "PL Code Lists",
					TRIGGER_NAME = "f9ba1384-f8ea-41d4-9f0c-70e3bb2c35e2",
					TRIGGER_GROUP = "DEFAULT",
					INSTANCE_NAME = "auto",
					STATE = "ACQUIRED",
					ENTRY_ID = "auto637897496735836832",
				};
				repo.Add(trigger1);
				repo.Add(trigger12);
				repo.Add(trigger2);
				repo.Add(trigger3);
				repo.Add(firedTrigger);
				await repo.SaveChangesAsync();
			}
			using (var repo = new StagingRepository(connectionString))
			{
				var errorReporter = new Mock<IErrorReportingWrapper>();
				HealthChecker.CheckBlockedTriggersWithoutFiredTrigger(repo, errorReporter.Object);
				errorReporter.Verify(x => x.AppendDescription(It.Is<string>(t => t.Contains("BE Additional Info"))), Times.Once());
				errorReporter.Verify(x => x.AppendDescription(It.Is<string>(t => t.Contains("PL Code Lists"))), Times.Never());
			}
		}
	}
}
