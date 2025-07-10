using System.Collections.Generic;
using System.Threading;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Registry;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Recruitment.ServiceTasks.ResumeBacklogConversionTask.Code, 
	"Resume Backlog Conversion Service Task", 
	"HRM", 
	typeof(Enterprise.Recruitment.ServiceTasks.ResumeBacklogConversionTask), 
	MinimumPeriod = "15minutes", 
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true
	)]

namespace Enterprise.Recruitment.ServiceTasks
{
	public class ResumeBacklogConversionTask : ServiceProviderImpl
	{
		public const string Code = "RBC";

		public override void RunTask(CancellationToken token)
		{
			using (Env.Instance.SuspendBranchAccessError())
			{
				if (!RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.Value)
				{
					ServiceLogger.Log(LogType.Warning, "[ResumeBacklogConversionTask] Recruitment module is disabled. This task will not run.");
					return;
				}

				var applications = new Queue<HRJobApplication>(ApplicationQueryHelper.WithinDates(ZDateTime.Now, ZDateTime.Now.AddDays(-2)));
				ServiceLogger.Log(LogType.Information, $"[ResumeBacklogConversionTask] Running task on {applications.Count} resumes");

				var result = ApplicationQueryHelper.BatchConvertResumes(ServiceLogger, applications, null, token);
				ServiceLogger.Log(LogType.Information, $"[ResumeBacklogConversionTask] Finished, result={result}");
			}
		}
	}
}
