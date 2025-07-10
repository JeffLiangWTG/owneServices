using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class QuartzAppRunner : IJob
	{
		public string ProgramExePath { get; set; }
		public string ProgramArgs { get; set; }
		public string DataSetNames { get; set; }

		public Task Execute(IJobExecutionContext context)
		{
			var programExePath = Path.IsPathRooted(ProgramExePath) ? ProgramExePath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProgramExePath);
			var jobName = context.JobDetail.Key.Name;

			var monitor = ProcessMonitor.Instance;
			using (var logWrapper = new LogWrapper(jobName))
			{
				var logHelper = new LogHelper(logWrapper, jobName);
				var exitCode = ProcessorRunnerHelper.RunApp(logHelper, jobName, programExePath, ProgramArgs, SchedulerConstants.UXMLProducerConfigPaths,
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
