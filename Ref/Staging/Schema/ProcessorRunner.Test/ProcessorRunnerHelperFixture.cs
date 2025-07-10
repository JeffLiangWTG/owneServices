using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner.Test
{
	[TestFixture]
	class ProcessorRunnerHelperFixture
	{
		[Test]
		public void PostCrashReportRunApp_WhenThereIsFailure()
		{
			var failProgramExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ref\FailProgram.exe");
			var jobName = "A job";
			using (errorReportingWrapper.Object)
			{
				ProcessorRunnerHelper.RunApp(logHelper.Object, jobName, failProgramExePath, string.Empty,
					errorReportingWrapper.Object, new[] { AppDomain.CurrentDomain.BaseDirectory });

				errorReportingWrapper.Verify(x => x.PostCrashReport());
				errorReportingWrapper.Verify(x => x.AppendDescription(It.IsAny<string>()));
				logHelper.Verify(x => x.LogMonitorInfo(It.IsAny<DateTime>(), It.IsAny<double>(), false));
				logHelper.Verify(x => x.LogInfo("Process executed failed."));
			}
		}

		[Test]
		public void PostCrashReportRunApp_WhenConfigured()
		{
			var parseFailureAppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ref\ParseFailureApp.exe");
			var jobName = "A job";
			using (errorReportingWrapper.Object)
			{
				ProcessorRunnerHelper.RunApp(logHelper.Object, jobName, parseFailureAppPath, string.Empty,
					errorReportingWrapper.Object, new[] { AppDomain.CurrentDomain.BaseDirectory });

				errorReportingWrapper.Verify(x => x.PostCrashReport());
				errorReportingWrapper.Verify(x => x.AppendDescription(It.IsAny<string>()));
				logHelper.Verify(x => x.LogMonitorInfo(It.IsAny<DateTime>(), It.IsAny<double>(), false));
				logHelper.Verify(x => x.LogInfo("Process executed failed."));
			}
		}

		[Test]
		public void AppendJobNameToKeyBase()
		{
			var parseFailureAppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ref\ParseFailureApp.exe");
			var jobName = "job A";
			using (errorReportingWrapper.Object)
			{
				ProcessorRunnerHelper.RunApp(logHelper.Object, jobName, parseFailureAppPath, string.Empty,
					errorReportingWrapper.Object, new[] { AppDomain.CurrentDomain.BaseDirectory });
				errorReportingWrapper.Verify(x => x.AppendBaseKey(jobName));
			}
		}

		[Test]
		public void ShouldReportIssue()
		{
			var failProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ref\ParseFailureApp.exe");

			Assert.AreEqual(true, ProcessorRunnerHelper.ShouldReportIssue(failProgramPath, 1));
			Assert.AreEqual(true, ProcessorRunnerHelper.ShouldReportIssue(failProgramPath, 2));
			Assert.AreEqual(false, ProcessorRunnerHelper.ShouldReportIssue(failProgramPath, 4));
		}

		Mock<ILogHelper> logHelper;
		Mock<IErrorReportingWrapper> errorReportingWrapper;

		[SetUp]
		public void SetUp()
		{
			logHelper = new Mock<ILogHelper>();
			errorReportingWrapper = new Mock<IErrorReportingWrapper>();
			errorReportingWrapper.Setup(x => x.HasErrorToReport).Returns(true);
		}
	}
}
