using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class Qrtz_Job_DetailsFixture
{
	[Test]
	public void Triggers()
	{
		var connectionString = TestConnectionString.GetAdmin(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging));

		// Insert
		using (var entities = new StagingRepository(connectionString))
		{
			var job_Details = new QRTZ_JOB_DETAILS
			{
				JOB_PK = Guid.NewGuid(),
				JOB_NAME = "ZZ Tariff Parser Test",
				SCHED_NAME = "RefDbRepoQuartzServer",
				JOB_GROUP = "DEFAULT",
				QRTZ_TRIGGERS = null,
				JOB_CLASS_NAME = "CargoWise.RefDbRepo.Staging.Schedulers.Common.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common"
			};
			entities.Add(job_Details);
			entities.SaveChanges();
			var job_DetailsSample = entities.Get<QRTZ_JOB_DETAILS>().FirstOrDefault();
			Assert.AreEqual(job_Details.JOB_NAME, job_DetailsSample.JOB_NAME);
			Assert.AreEqual(job_Details.SCHED_NAME, job_DetailsSample.SCHED_NAME);
			Assert.AreEqual(job_Details.JOB_GROUP, job_DetailsSample.JOB_GROUP);
			Assert.AreEqual(job_Details.JOB_CLASS_NAME, job_DetailsSample.JOB_CLASS_NAME);

			// Update
			job_Details.JOB_CLASS_NAME = "modified";
			entities.Update(job_Details);
			entities.SaveChanges();
		}
		using (var entities = new StagingRepository(connectionString))
		{
			var job_DetailsSample = entities.Get<QRTZ_JOB_DETAILS>().FirstOrDefault();
			Assert.AreEqual("modified", job_DetailsSample.JOB_CLASS_NAME);
		}

		// Delete
		using (var entities = new StagingRepository(connectionString))
		{
			var job_DetailsSample = entities.Get<QRTZ_JOB_DETAILS>().FirstOrDefault();
			entities.Remove(job_DetailsSample);
			entities.SaveChanges();
			Assert.AreEqual(0, entities.Get<QRTZ_JOB_DETAILS>().Count());
		}
	}
}
