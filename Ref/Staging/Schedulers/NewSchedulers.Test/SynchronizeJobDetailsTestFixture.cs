using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.NewSchedulers.Test.IntegrationTest;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[TransactionedTestCase]
	class SynchronizeJobDetailsTestFixture
	{
		[Test]
		public void TestJobsAreSynchronizedBetweenXmlAndDatabase()
		{
			PrepareData();
			ConfigurationProvider.QuartzDefaultConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			ConfigurationProvider.StagingConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			ConfigurationProvider.QuartzProps["quartz.plugin.jobInitializer.fileNames"] = "~/TestConfiguration/TestJobs.xml";
			using (IntegrationTestHelper.WebAppFactory.CreateClient())
			{
				AssertProcessingResult();
			}
		}

		void AssertProcessingResult()
		{
			var jobDetailsFromDb = new List<QRTZ_JOB_DETAILS>();
			using (var stagingDbConnection = new SqlConnection(TestConnectionString.GetAdmin(stagingDbName)))
			{
				stagingDbConnection.Open();
				var transaction = stagingDbConnection.BeginTransaction();
				using (var command = stagingDbConnection.CreateCommand())
				{
					command.Transaction = transaction;
					command.CommandText = "SELECT JOB_NAME, JOB_GROUP FROM dbo.QRTZ_JOB_DETAILS";
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							jobDetailsFromDb.Add(new QRTZ_JOB_DETAILS
							{
								JOB_NAME = reader[nameof(QRTZ_JOB_DETAILS.JOB_NAME)].ToString(),
								JOB_GROUP = reader[nameof(QRTZ_JOB_DETAILS.JOB_GROUP)].ToString()
							});
						}
					}
				}
			}

			Assert.That(jobDetailsFromDb, Has.Count.EqualTo(4));
			Assert.IsFalse(jobDetailsFromDb.Any(jobDetail => jobDetail.JOB_NAME.Equals("TR Customs Job 2", StringComparison.OrdinalIgnoreCase) && jobDetail.JOB_GROUP.Equals("TR Customs", StringComparison.OrdinalIgnoreCase)));
		}

		void PrepareData()
		{
			const string jobData = @"{""ProgramExePath"":""..\\..\\UniversalXMLProducers\\net8.0\\CargoWise.RefDbRepo.UniversalXMLProducers.DummyNet6Proj.exe""}";
			using (var stagingDbConnection = new SqlConnection(TestConnectionString.GetAdmin(stagingDbName)))
			{
				stagingDbConnection.Open();
				var transaction = stagingDbConnection.BeginTransaction();
				using (var command = stagingDbConnection.CreateCommand())
				{
					command.Transaction = transaction;
					command.CommandText = $@"
INSERT INTO dbo.QRTZ_JOB_DETAILS (JOB_PK, SCHED_NAME, JOB_NAME, JOB_GROUP, DESCRIPTION, JOB_CLASS_NAME, IS_DURABLE, IS_NONCONCURRENT, IS_UPDATE_DATA, REQUESTS_RECOVERY, JOB_DATA) VALUES
(NEWID(), 'RefDbRepoQuartzServer', 'TR Customs Job 1', 'TR Customs', NULL, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 1, 1, 0, 0, CONVERT(VARBINARY(MAX), '{jobData}')),
(NEWID(), 'RefDbRepoQuartzServer', 'TR Customs Job 2', 'TR Customs', NULL, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 1, 1, 0, 0, CONVERT(VARBINARY(MAX), '{jobData}')),
(NEWID(), 'RefDbRepoQuartzServer', 'NZ Customs Job 1', 'NZ Customs', NULL, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 1, 1, 0, 0, CONVERT(VARBINARY(MAX), '{jobData}')),
(NEWID(), 'RefDbRepoQuartzServer', 'BR Customs Job 1', 'BR Customs', NULL, 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common', 0, 1, 0, 0, CONVERT(VARBINARY(MAX), '{jobData}'))
";
					command.ExecuteNonQuery();
				}
				transaction.Commit();
			}
		}

		readonly string stagingDbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
	}
}
