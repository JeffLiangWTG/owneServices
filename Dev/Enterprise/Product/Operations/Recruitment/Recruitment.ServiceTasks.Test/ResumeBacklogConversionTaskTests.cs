using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.Recruitment.ServiceTasks;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	[TestedType(typeof(ResumeBacklogConversionTask))]
	sealed class ResumeBacklogConversionTaskTests : ServiceTaskTestCase<ResumeBacklogConversionTask>
	{
		IDisposable userContext;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			userContext = EnvProxy.Instance.TemporaryServiceTaskContext("RBC", canRunInAnyBranch: true);
			RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDownCore()
		{
			userContext.Dispose();
			base.TearDownCore();
		}

		public void TestResumeBacklogConversionTask_RecModuleDisabled()
		{
			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = new ResumeBacklogConversionTask { ServiceLogger = DummyLogger(out var logs) };

				serviceTask.RunTask(CancellationToken.None);
				AssertContains("Recruitment module is disabled. This task will not run.", logs.ToString());
			}
		}

		public void TestRun()
		{
			var converter = new MockResumeConverter();

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute("IResumeConverter", converter))
			{
				var factory = new BusinessObjectFactory();
				var app1 = factory.NewWithValidTestData<HRJobApplication>();
				var app2 = factory.NewWithValidTestData<HRJobApplication>();
				factory.Save();

				var serviceTask = new ResumeBacklogConversionTask { ServiceLogger = DummyLogger(out var logs) };
				serviceTask.RunTask(CancellationToken.None);

				AssertEquals("___", app1.HP_CurrentStatus);
				AssertEquals("___", app2.HP_CurrentStatus);

				// assert that resumes were converted
				AssertContains("[ResumeBacklogConversionTask] Running task on 2 resumes", logs.ToString());
				AssertContains("[ResumeBacklogConversionTask] Finished, result=True", logs.ToString());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Setup

		ILogger DummyLogger(out StringBuilder logs)
		{
			var sb = logs = new StringBuilder();

			var logger = new DummyLogger();
			logger.OnLog += (o, e) => sb.AppendLine(e.Type + ": " + e.Message);
			return logger;
		}

		#endregion
	}
}
