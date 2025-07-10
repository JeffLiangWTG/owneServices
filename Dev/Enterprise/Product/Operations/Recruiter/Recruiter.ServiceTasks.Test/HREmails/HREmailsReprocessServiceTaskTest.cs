using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Recruiter.ServiceTasks.Testing.HREmailsServiceTaskTest;

namespace Enterprise.Recruiter.ServiceTasks.Testing
{
	[TestedType(typeof(HREmailsReprocessServiceTask))]
	sealed class HREmailsReprocessServiceTaskTest : ServiceTaskTestCase<HREmailsReprocessServiceTask>
	{
		IDisposable userContext;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			userContext = Env.Instance.TemporaryServiceTaskContext("HRR", canRunInAnyBranch: true);
		}

		protected override void TearDownCore()
		{
			userContext.Dispose();
			base.TearDownCore();

			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(HREmailsReprocessServiceTask).GetMethod(nameof(HREmailsReprocessServiceTask.CheckDaxtraEnabled));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("This service requires Daxtra to be enabled", HREmailsReprocessServiceTask.CheckDaxtraEnabled());
			}

			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("", HREmailsReprocessServiceTask.CheckDaxtraEnabled());
			}
		}

		public void TestLogs()
		{
			RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var serviceTask = new HREmailsReprocessServiceTaskForTest();

			TestLogger.Logs.Clear();
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.RunTask();

			AssertLogs(
				new LogForTest(LogType.Information, "Processing applications 3 of 100. Processing batches 1 of 10. Batches sent 1.", null),
				new LogForTest(LogType.Information, "Process completed.", null),
				new LogForTest(LogType.Information,
				@"Processed applications: 100
Batches processed: 1
Documents saved: 2", null));
		}

		public void TestLogsWithErrors()
		{
			RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var serviceTask = new HREmailsReprocessServiceTaskForTest();

			TestLogger.Logs.Clear();
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.AddError("error");

			serviceTask.RunTask();

			AssertLogs(
				new LogForTest(LogType.Information, "Processing applications 3 of 100. Processing batches 1 of 10. Batches sent 1.", null),
				new LogForTest(LogType.Error, @"Process completed with errors.
error", null),
				new LogForTest(LogType.Information,
				@"Processed applications: 100
Batches processed: 1
Documents saved: 2", null));
		}

		void AssertLogs(params LogForTest[] logs)
		{
			var expected = string.Join("\r\n", Array.ConvertAll(logs, l => l.ToString()));
			var actual = string.Join("\r\n", TestLogger.Logs.ConvertAll(l => l.ToString()).ToArray());
			var message = string.Format(CultureInfo.InvariantCulture, "\r\nEXPECTED:\r\n{0}\r\n\r\nACTUAL:\r\n{1}\r\n", expected, actual);

			AssertEquals(message, logs.Length, TestLogger.Logs.Count);
			for (int i = 0; i < logs.Length; i++)
			{
				var lineErrorMessage = string.Format(CultureInfo.InvariantCulture, "Line differs:{0}\r\n{1}", i + 1, message);
				AssertEquals(lineErrorMessage, logs[i], TestLogger.Logs[i]);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		LoggerForTest TestLogger
		{
			get { return testLogger ?? (testLogger = new LoggerForTest()); }
		}
		LoggerForTest testLogger;

		class HREmailsReprocessServiceTaskForTest : HREmailsReprocessServiceTask
		{
			readonly ApplicationDocumentsUpdaterForTest updater = new ApplicationDocumentsUpdaterForTest();
			protected override ApplicationDocumentsUpdater GetUpdater(DaxtraResumeParser daxtraResumeParser)
			{
				return updater;
			}

			public void AddError(string error)
			{
				updater.AddError(error);
			}
		}

		class ApplicationDocumentsUpdaterForTest : ApplicationDocumentsUpdater
		{
			public ApplicationDocumentsUpdaterForTest() : base(new DaxtraResumeParser())
			{
				errors = new List<string>();
			}

			public void AddError(string error)
			{
				errors.Add(error);
			}

			public override void ProcessEmailsBacklog(CancellationToken cancellationToken)
			{
				Stats = new ApplicationDocumentsProgressStats(100, 10);
				Stats.BatchesProcessed++;
				Stats.BatchesSent++;
				Stats.DocumentsSaved++;
				Stats.DocumentsSaved++;
				Stats.ItemsProcessed++;
				Stats.ItemsProcessed++;
				Stats.ItemsProcessed++;

				InvokeProgress();
				InvokeFinish(false);
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(Business.Testing.EmailParsingRuleTest).Assembly));

		public static void SetupTestDataForTestRunTask()
		{
			RecruiterDataRegistry.Instance.DaxtraEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RecruiterDataRegistry.Instance.HREmailMailServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestMailServer");
			RecruiterDataRegistry.Instance.HREmailMailboxUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HREmail");
		}

		[UseSnapshotProtection]
		public void TestCallResumeConverter_ApplicationCreated()
		{
			var logger = new LoggerForTest();

			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			using (RecruitmentDataRegistry.Instance.ConvertApiSecretKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestKey"))
			using (ObjectFactory.Substitute<IResumeConverter>(new ResumeConverterForTest(logger)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTest();
				serviceTask.ServiceLogger = logger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From("Test FullName", "test@cargowise.com");
					emailForTest.Subject("attach this to CS00000101");
					var emptyPdfPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.pdf", "empty.pdf");
					emailForTest.WithAttachment(emptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};
				serviceTask.RunTask();
				Assert(logger.Logs.Contains(new LogForTest(LogType.Information, "Called resume conversion", null)));
			}
		}
	}
}
