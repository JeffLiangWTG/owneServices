using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner.Test
{
	[TestFixture]
	class ProcessorRunnerFixture
	{
		[Test]
		public void LogException()
		{
			var ex = new Exception();
			try
			{
				new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => { throw ex; });
			}
			catch (Exception) { }
			logHelper.Verify(x => x.LogError(It.Is<string>(s => s.StartsWith("System.Exception"))));
			errorReportingWrapper.Verify(x => x.PostCrashReport(ex, null, null));
		}

		[Test]
		public void PostErrorReportEvenIfLogHasException()
		{
			var ex = new Exception();
			logHelper.Setup(x => x.LogError(It.IsAny<string>())).Throws(new Exception());
			try
			{
				new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => { throw ex; });
			}
			catch (Exception) { }
			errorReportingWrapper.Verify(x => x.PostCrashReport(ex, null, null));
		}

		[Test]

		public void Log()
		{
			new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => Tuple.Create(13, true));
			logHelper.Verify(x => x.LogInfo(It.Is<string>(s => s.StartsWith("ProcessStart Id:"))));
			logHelper.Verify(x => x.LogInfo(It.Is<string>(s => s.EndsWith("EffectedRecords: 13."))));
		}

		[Test]
		public void RecordLastRunStatus()
		{
			var now = DateTime.UtcNow;
			var status = new ProcessorStatus
			{
				PRC_JobName = "A Process",
				PRC_JobGroup = "XX Group",
				PRC_SchedName = "QuartzServer",
				PRC_LastRunTime = now.AddDays(-1)
			};
			staging.Setup(x => x.Get<ProcessorStatus>()).Returns(new[] { status }.AsQueryable());
			try
			{
				new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => { throw new Exception(); });
			}
			catch (Exception) { }
			Assert.That(status.PRC_LastRunTime, Is.Not.EqualTo(now.AddDays(-1)));
			Assert.True(status.IsProcessorStatusErrorStatus());
			var lastRunTime = status.PRC_LastRunTime;
			Thread.Sleep(100);
			new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => Tuple.Create(0, true));
			Assert.That(status.PRC_LastRunTime, Is.Not.EqualTo(lastRunTime));
			Assert.True(status.IsProcessorStatusProcessedStatus());
		}

		[Test]
		public void RecordLastSuccessStatus()
		{
			var now = DateTime.UtcNow;
			var status = new ProcessorStatus
			{
				PRC_JobName = "A Process",
				PRC_JobGroup = "XX Group",
				PRC_SchedName = "QuartzServer",
				PRC_LastSuccessRunTime = now.AddDays(-1)
			};
			staging.Setup(x => x.Get<ProcessorStatus>()).Returns(new[] { status }.AsQueryable());
			try
			{
				new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => { throw new Exception(); });
			}
			catch (Exception) { }
			Assert.That(status.PRC_LastSuccessRunTime, Is.EqualTo(now.AddDays(-1)));
			new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => Tuple.Create(0, true));
			Assert.That(status.PRC_LastSuccessRunTime, Is.Not.EqualTo(now.AddDays(-1)));
		}

		[Test]
		public void RecordLastDataSetUpdatedCount()
		{
			var status = new ProcessorStatus
			{
				PRC_JobName = "A Process",
				PRC_JobGroup = "XX Group",
				PRC_SchedName = "QuartzServer",
				PRC_LastSuccessRecordUpdatedCount = 1
			};
			staging.Setup(x => x.Get<ProcessorStatus>()).Returns(new[] { status }.AsQueryable());
			new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => Tuple.Create(0, true));
			Assert.That(status.PRC_LastSuccessRecordUpdatedCount, Is.EqualTo(1));
			new ProcessorRunner(logHelper.Object, staging.Object, errorReportingWrapper.Object).Run(processor, () => Tuple.Create(4, true));
			Assert.That(status.PRC_LastSuccessRecordUpdatedCount, Is.EqualTo(4));
		}

		ProcessorInfo processor = new ProcessorInfo("A Process", "XX Group", "QuartzServer");
		Mock<IStagingRepository> staging;
		Mock<IErrorReportingWrapper> errorReportingWrapper;
		Mock<ILogHelper> logHelper;

		[SetUp]
		public void SetUp()
		{
			staging = new Mock<IStagingRepository>();
			logHelper = new Mock<ILogHelper>();
			errorReportingWrapper = new Mock<IErrorReportingWrapper>();
		}
	}
}
