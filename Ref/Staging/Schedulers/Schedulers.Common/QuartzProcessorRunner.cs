using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	[DisallowConcurrentExecution]
	public class QuartzProcessorRunner : IJob
	{
		public string ProgramArgs { get; set; }
		public string ProgramExePath { get; set; }

		public Task Execute(IJobExecutionContext context)
		{
			var jobName = context.JobDetail.Key.Name;
			var jobGroup = ((dynamic)context.JobDetail).Group;
			var schedName = ConfigurationProvider.SchedulerName;
			var programExePath = Path.IsPathRooted(ProgramExePath) ? ProgramExePath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProgramExePath);

			var monitor = ProcessMonitor.Instance;
			using (var logWrapper = new LogWrapper(jobName))
			{
				var logHelper = new LogHelper(logWrapper, jobName);
				var exitCode = ProcessorRunnerHelper.RunAppAsProcessor(new ProcessorInfo(jobName, jobGroup, schedName), logHelper, jobName,
					programExePath, ProgramArgs, SchedulerConstants.UXMLProducerConfigPaths,
					[x =>
						{
							monitor.AddProcessToJobManager(x, logHelper);
							monitor.AddProcessToMemoryMonitor(x, logHelper);
						}
					]);
				if (exitCode != 0)
				{
					throw new JobExecutionException();
				}
			}
			return Task.CompletedTask;
		}
	}
}
