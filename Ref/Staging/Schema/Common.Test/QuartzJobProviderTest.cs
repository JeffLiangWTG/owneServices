using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Common.Test
{
	[TestFixture]
	public class QuartzJobProviderTest
	{
		[Test]
		[TransactionedTestCase]
		public void GetTriggeredTimesPerDay()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var repo = new StagingRepository(connectionString))
			{
				var sql = @"insert into [dbo].[QRTZ_JOB_DETAILS]([JOB_PK],[SCHED_NAME],[JOB_NAME],[JOB_GROUP],[DESCRIPTION],[JOB_CLASS_NAME],[IS_DURABLE],[IS_NONCONCURRENT],[IS_UPDATE_DATA],[REQUESTS_RECOVERY],[JOB_DATA])
values
('E5272860-D596-4BFA-8D1A-00F42777026A','RefDbRepoQuartzServer','AF Exchange Rate','DEFAULT',NULL,'Cargowise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, Cargowise.RefDbRepo.Staging.Schedulers.Common',1,1,0,0,NULL),
('E5272860-D596-4BFA-8D1A-00F42777026B','RefDbRepoQuartzServer','BF Exchange Rate','DEFAULT',NULL,'Cargowise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, Cargowise.RefDbRepo.Staging.Schedulers.Common',1,1,0,0,NULL),
('E5272860-D596-4BFA-8D1A-00F42777026C','RefDbRepoQuartzServer','CF Exchange Rate','DEFAULT',NULL,'Cargowise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, Cargowise.RefDbRepo.Staging.Schedulers.Common',1,1,0,0,NULL),
('E5272860-D596-4BFA-8D1A-00F42777026E','RefDbRepoQuartzServer','DF Exchange Rate','DEFAULT',NULL,'Cargowise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, Cargowise.RefDbRepo.Staging.Schedulers.Common',1,1,0,0,NULL)

insert into [dbo].[QRTZ_TRIGGERS] ([SCHED_NAME],[TRIGGER_NAME],[TRIGGER_GROUP],[JOB_NAME],[JOB_GROUP],[DESCRIPTION],[NEXT_FIRE_TIME],[PREV_FIRE_TIME],[PRIORITY],[TRIGGER_STATE],[TRIGGER_TYPE],[START_TIME],[END_TIME],[CALENDAR_NAME],[MISFIRE_INSTR],[JOB_DATA])
values
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51a','DEFAULT','AF Exchange Rate','DEFAULT',NULL,637929252000000000,637928388000000000,5,'WAITING','CRON',637467126500000000,NULL,NULL,0,NULL),
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51b','DEFAULT','BF Exchange Rate','DEFAULT',NULL,637929252000000000,637928388000000000,5,'WAITING','SIMPLE',637467126500000000,NULL,NULL,0,NULL),
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51c','DEFAULT','CF Exchange Rate','DEFAULT',NULL,637929252000000000,637928388000000000,5,'WAITING','CRON',637467126500000000,NULL,NULL,0,NULL),
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51d','DEFAULT','DF Exchange Rate','DEFAULT',NULL,637929252000000000,637928388000000000,5,'WAITING','SIMPLE',637467126500000000,NULL,NULL,0,NULL)

insert into [dbo].[QRTZ_CRON_TRIGGERS] ([SCHED_NAME],[TRIGGER_NAME],[TRIGGER_GROUP],[CRON_EXPRESSION],[TIME_ZONE_ID])
values
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51a','DEFAULT','0 0 11 * * ?','AUS Eastern Standard Time'),
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51c','DEFAULT','0 0 9 * * MON','AUS Eastern Standard Time')

insert into [dbo].[QRTZ_SIMPLE_TRIGGERS] ([SCHED_NAME],[TRIGGER_NAME],[TRIGGER_GROUP],[REPEAT_COUNT],[REPEAT_INTERVAL],[TIMES_TRIGGERED])
values
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51b','DEFAULT',-1,86400000,189),
('RefDbRepoQuartzServer','0a01111c-5903-48cf-9c46-31a36245a51d','DEFAULT',-1,604800000,189)";
				_ = repo.ExecuteSqlCommand(sql);
				var provider = new QuartzJobProvider(repo);

				var jobName = "AD Exchange Rate";
				Assert.AreEqual(1, provider.GetTriggeredTimesPerDay(jobName));

				jobName = "BD Exchange Rate";
				Assert.AreEqual(1, provider.GetTriggeredTimesPerDay(jobName));

				jobName = "CD Exchange Rate";
				Assert.AreEqual(1, provider.GetTriggeredTimesPerDay(jobName));

				jobName = "DD Exchange Rate";
				Assert.AreEqual(1, provider.GetTriggeredTimesPerDay(jobName));
			}
		}

		[Test]
		public void GetTriggeredTimesPerDay_CronTriggerType()
		{
			var repo = PrepareTriggerData();
			var provider = new QuartzJobProvider(repo.Object);

			var jobName = "BE Exchange Rate";
			Assert.That(provider.GetTriggeredTimesPerDay(jobName), Is.EqualTo(1));

			jobName = "SE Exchange Rate";
			Assert.That(provider.GetTriggeredTimesPerDay(jobName), Is.EqualTo(1));

			jobName = "AU Exchange Rate Parser";
			Assert.That(provider.GetTriggeredTimesPerDay(jobName), Is.EqualTo(19));
		}

		[Test]
		public void GetTriggeredTimesPerDay_SimpleTriggerType()
		{
			var repo = PrepareTriggerData();
			var provider = new QuartzJobProvider(repo.Object);

			var jobName = "UNLOCO Updater UXML";
			Assert.AreEqual(1, provider.GetTriggeredTimesPerDay(jobName));

			jobName = "CH Customs Office";
			Assert.AreEqual(1, provider.GetTriggeredTimesPerDay(jobName));

			jobName = "E-Hub Messages Downloader";
			Assert.AreEqual(288, provider.GetTriggeredTimesPerDay(jobName));
		}

		[Test]
		public void GetTriggeredTimesPerDay_MultipleTriggerTypes()
		{
			var repo = PrepareTriggerData();
			var provider = new QuartzJobProvider(repo.Object);

			var jobName = "NZ Exchange Rate Parser";
			Assert.AreEqual(2, provider.GetTriggeredTimesPerDay(jobName));
		}

		[Test]
		public void GetTriggeredTimesPerDay_OtherType()
		{
			var repo = PrepareTriggerData();
			var provider = new QuartzJobProvider(repo.Object);

			var jobName = "NZ Exchange Rate Parser Fake";
			Assert.Throws<NotImplementedException>(() => provider.GetTriggeredTimesPerDay(jobName));
		}

		[TestCase("CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification.exe", "", ExpectedResult = "Data Change Capture Email Notification")]
		[TestCase("..\\UniversalXMLProducers\\CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.exe", "NOMENCLATURE", ExpectedResult = "EUN Tariff Data Producer UXML")]
		[TestCase("..\\UniversalXMLProducers\\CargoWise.RefDbRepo.TaiwanReferenceData.exe", "-I=MODULE_EXCHANGE_RATE -F=..\\UxmlFiles\\TWExchangeRates.xml", ExpectedResult = "W Exchange Rates UXML")]
		[TestCase("..\\UniversalXMLProducers\\CargoWise.RefDbRepo.UniversalXMLProducers.CASIMAProducer.exe", "", ExpectedResult = "")]
		public string GetQuartzJobName(string programExePath, string programArgs)
		{
			var mockStagingRepository = new Mock<IStagingRepository>();
			mockStagingRepository.Setup(o => o.Get<QRTZ_JOB_DETAILS>()).Returns(new[]
			{
				new QRTZ_JOB_DETAILS{ JOB_NAME="Data Change Capture Email Notification", ProgramExePath="CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification.exe", ProgramArgs=""},
				new QRTZ_JOB_DETAILS{ JOB_NAME="EUN Tariff Data Producer UXML", ProgramExePath="..\\UniversalXMLProducers\\CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.exe", ProgramArgs="NOMENCLATURE"},
				new QRTZ_JOB_DETAILS{ JOB_NAME="W Exchange Rates UXML", ProgramExePath="..\\UniversalXMLProducers\\CargoWise.RefDbRepo.TaiwanReferenceData.exe", ProgramArgs="-I=MODULE_EXCHANGE_RATE -F=..\\UxmlFiles\\TWExchangeRates.xml"},
			}.AsQueryable());
			var quartzJobProvider = new QuartzJobProvider(mockStagingRepository.Object);
			return quartzJobProvider.GetQuartzJobName(programExePath, programArgs);
		}

		[Test]
		[TransactionedTestCase]
		public void GetQuartzJobName_WithDb()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using var repo = new StagingRepository(connectionString);
			_ = repo.ExecuteSqlCommand(dbSql);
			var jobDetails = repo.Get<QRTZ_JOB_DETAILS>().ToList();
			jobDetails.ForEach(QRTZ_JOB_DETAILSHelper.SetCalculatedProperties);
			Assert.NotNull(jobDetails);
			Assert.AreEqual(4, jobDetails.Count);

			var quartzJobProvider = new QuartzJobProvider(repo);
			Assert.Multiple(() =>
			{
				foreach (var item in jobDetails)
				{
					Assert.AreEqual(item.JOB_NAME, quartzJobProvider.GetQuartzJobName(item.ProgramExePath, item.ProgramArgs));
				}
			});
		}

		string dbSql = @"INSERT [dbo].[QRTZ_JOB_DETAILS] ([JOB_PK], [SCHED_NAME], [JOB_NAME], [JOB_GROUP], [DESCRIPTION], [JOB_CLASS_NAME], [IS_DURABLE], [IS_NONCONCURRENT], [IS_UPDATE_DATA], [REQUESTS_RECOVERY], [JOB_DATA]) VALUES (N'ff52a498-bd2a-4011-97e4-a1942dbedefa', N'RefDbRepoQuartzServer', N'Data Change Capture Email Notification', N'Reference Data', NULL, N'CarWise.RefDbRepo.Staging.Schedulers.QuartzAppRunner, CarWise.RefDbRepo.Staging.Schedulers', 1, 0, 0, 0, 0x7B22436F756E747279436F6465223A224252222C2250726F6772616D45786550617468223A222E2E5C5C2E2E5C5C556E6976657273616C584D4C50726F6475636572735C5C6E6574362E305C5C436172676F576973652E52656644625265706F2E42525265666572656E6365446174612E436D644C696E652E657865222C2250726F6772616D41726773223A22544152494646535F43484152414354455249535449435F4E434D20455850227D)
INSERT [dbo].[QRTZ_JOB_DETAILS] ([JOB_PK], [SCHED_NAME], [JOB_NAME], [JOB_GROUP], [DESCRIPTION], [JOB_CLASS_NAME], [IS_DURABLE], [IS_NONCONCURRENT], [IS_UPDATE_DATA], [REQUESTS_RECOVERY], [JOB_DATA]) VALUES (N'7c54024f-94b8-4862-8be8-69f1f84b0403', N'RefDbRepoQuartzServer', N'EUN Tariff Data Producer UXML', N'EU Customs', NULL, N'CarWise.RefDbRepo.Staging.Schedulers.QuartzProcessorRunner, CarWise.RefDbRepo.Staging.Schedulers', 1, 1, 0, 0, 0x7B22436F756E747279436F6465223A224555222C2250726F6772616D45786550617468223A222E2E5C5C2E2E5C5C556E6976657273616C584D4C50726F6475636572735C5C6E6574362E305C5C436172676F576973652E52656644625265706F2E45555265666572656E6365446174612E436D644C696E652E657865222C2250726F6772616D41726773223A22494353324852434D53435245454E494E474D4554484F44227D)
INSERT [dbo].[QRTZ_JOB_DETAILS] ([JOB_PK], [SCHED_NAME], [JOB_NAME], [JOB_GROUP], [DESCRIPTION], [JOB_CLASS_NAME], [IS_DURABLE], [IS_NONCONCURRENT], [IS_UPDATE_DATA], [REQUESTS_RECOVERY], [JOB_DATA]) VALUES (N'156ada7e-3d16-4e89-b8cc-f2dfa8718c5d', N'RefDbRepoQuartzServer', N'TW Exchange Rates UXML', N'TW Customs', NULL, N'CarWise.RefDbRepo.Staging.Schedulers.QuartzProcessorRunner, CarWise.RefDbRepo.Staging.Schedulers', 1, 1, 0, 0, 0x7B22436F756E747279436F6465223A22504C222C2250726F6772616D45786550617468223A222E2E5C5C2E2E5C5C556E6976657273616C584D4C50726F6475636572735C5C6E6574362E305C5C436172676F576973652E52656644625265706F2E504C5265666572656E6365446174612E436D644C696E652E657865222C2250726F6772616D41726773223A22435544227D)
INSERT [dbo].[QRTZ_JOB_DETAILS] ([JOB_PK], [SCHED_NAME], [JOB_NAME], [JOB_GROUP], [DESCRIPTION], [JOB_CLASS_NAME], [IS_DURABLE], [IS_NONCONCURRENT], [IS_UPDATE_DATA], [REQUESTS_RECOVERY], [JOB_DATA]) VALUES (N'a1d2aa2b-edba-4364-b400-27b56458cd25', N'RefDbRepoQuartzServer', N'Tariff Rule Runner', N'Reference Data', NULL, N'CarWise.RefDbRepo.Staging.Schedulers.SingleInstanceQuartzAppRunner, CarWise.RefDbRepo.Staging.Schedulers', 1, 1, 0, 0, 0x7B22436F756E747279436F6465223A224555222C2250726F6772616D45786550617468223A222E2E5C5C2E2E5C5C556E6976657273616C584D4C50726F6475636572735C5C6E6574362E305C5C436172676F576973652E52656644625265706F2E45555265666572656E6365446174612E436D644C696E652E657865222C2250726F6772616D41726773223A224145534E4154494F4E414C495459227D)

";

		Mock<IStagingRepository> PrepareTriggerData()
		{
			var repo = new Mock<IStagingRepository>();

			repo.Setup(x => x.Get<QRTZ_TRIGGERS>()).Returns(PrepareTotalTriggerAttributes().AsQueryable());
			repo.Setup(x => x.Get<QRTZ_CRON_TRIGGERS>()).Returns(PrepareCronTriggerAttributes().AsQueryable());
			repo.Setup(x => x.Get<QRTZ_SIMPLE_TRIGGERS>()).Returns(PrepareSimpleTriggerAttributes().AsQueryable());
			repo.Setup(x => x.DatabaseExists).Returns(true);

			return repo;
		}

		List<QRTZ_TRIGGERS> PrepareTotalTriggerAttributes()
		{
			var triggerAttributeList = new List<QRTZ_TRIGGERS>()
			{
				new()
				{
					TRIGGER_NAME = "016a9382-4b26-4ea4-a73a-7f46aad0f5cc",
					JOB_NAME = "UNLOCO Updater UXML",
					TRIGGER_TYPE = "SIMPLE"
				},
				new()
				{
					TRIGGER_NAME = "01fe7801-ff99-4cc6-a992-82007bbe8d9b",
					JOB_NAME = "CH Customs Office",
					TRIGGER_TYPE = "SIMPLE"
				},
				new()
				{
					TRIGGER_NAME = "1ff03f28-bf70-4c9c-94da-14b99dfd8b0a",
					JOB_NAME = "E-Hub Messages Downloader",
					TRIGGER_TYPE = "SIMPLE"
				},
				new()
				{
					TRIGGER_NAME = "0a01111c-5903-48cf-9c46-31a36245c53b",
					JOB_NAME = "BE Exchange Rate",
					TRIGGER_TYPE = "CRON"
				},
				new()
				{
					TRIGGER_NAME = "0a74d548-1611-43f1-aee8-14ce3e2168a1",
					JOB_NAME = "SE Exchange Rate",
					TRIGGER_TYPE = "CRON"
				},
				new()
				{
					TRIGGER_NAME = "6d9f90ae-cacc-4efd-bba3-5d6a7344e28e",
					JOB_NAME = "AU Exchange Rate Parser",
					TRIGGER_TYPE = "CRON"
				},
				new()
				{
					TRIGGER_NAME = "daily, early morning",
					JOB_NAME = "NZ Exchange Rate Parser",
					TRIGGER_TYPE = "CRON"
				},
				new()
				{
					TRIGGER_NAME = "e9d20238-e4da-4e49-aa3c-de372f0a5be5",
					JOB_NAME = "NZ Exchange Rate Parser",
					TRIGGER_TYPE = "SIMPLE"
				},
				new()
				{
					TRIGGER_NAME = "e9d20238-e4da-4e49-aa3c-de372f0a5ba4",
					JOB_NAME = "NZ Exchange Rate Parser Fake",
					TRIGGER_TYPE = "OTHER"
				}
			};

			return triggerAttributeList;
		}

		List<QRTZ_CRON_TRIGGERS> PrepareCronTriggerAttributes()
		{
			var cronTriggerAttributeList = new List<QRTZ_CRON_TRIGGERS>()
			{
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "0a01111c-5903-48cf-9c46-31a36245c53b",
					TRIGGER_GROUP = "DEFAULT",
					CRON_EXPRESSION = "0 0 11 * * ?",
					TIME_ZONE_ID = "AUS Eastern Standard Time"
				},
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "0a74d548-1611-43f1-aee8-14ce3e2168a1",
					TRIGGER_GROUP = "DEFAULT",
					CRON_EXPRESSION = "0 0 9 * * ?",
					TIME_ZONE_ID = "AUS Eastern Standard Time"
				},
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "6d9f90ae-cacc-4efd-bba3-5d6a7344e28e",
					TRIGGER_GROUP = "DEFAULT",
					CRON_EXPRESSION = "0 0 5-23 * * ?",
					TIME_ZONE_ID = "AUS Eastern Standard Time"
				},
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "daily, early morning",
					TRIGGER_GROUP = "DEFAULT",
					CRON_EXPRESSION = "0 0 5 * * ?",
					TIME_ZONE_ID = "AUS Eastern Standard Time"
				}
			};

			return cronTriggerAttributeList;
		}

		List<QRTZ_SIMPLE_TRIGGERS> PrepareSimpleTriggerAttributes()
		{
			var simpleTriggerAttributeList = new List<QRTZ_SIMPLE_TRIGGERS>()
			{
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "016a9382-4b26-4ea4-a73a-7f46aad0f5cc",
					TRIGGER_GROUP = "DEFAULT",
					REPEAT_INTERVAL= 604800000,
					REPEAT_COUNT = -1
				},
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "01fe7801-ff99-4cc6-a992-82007bbe8d9b",
					TRIGGER_GROUP = "DEFAULT",
					REPEAT_INTERVAL = 86400000,
					REPEAT_COUNT = -1
				},
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "1ff03f28-bf70-4c9c-94da-14b99dfd8b0a",
					TRIGGER_GROUP = "DEFAULT",
					REPEAT_INTERVAL = 300000,
					REPEAT_COUNT = -1
				},
				new()
				{
					SCHED_NAME = "RefDbRepoQuartzServer",
					TRIGGER_NAME = "e9d20238-e4da-4e49-aa3c-de372f0a5be5",
					TRIGGER_GROUP = "DEFAULT",
					REPEAT_INTERVAL = 86400000,
					REPEAT_COUNT = -1
				},
			};

			return simpleTriggerAttributeList;
		}
	}
}
