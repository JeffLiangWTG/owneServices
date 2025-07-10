using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Util;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	class QuartzJobDetailsSynchronizer
	{
		readonly ILogHelper logHelper;

		internal QuartzJobDetailsSynchronizer(ILogHelper logHelper)
		{
			this.logHelper = logHelper;
		}

		internal async Task Synchronize(IScheduler scheduler)
		{
			logHelper.LogInfo("Start to synchronize job details between database and xml");
			var jobKeySet = await ConstructJobKeysFromXmlFiles();
			await DeleteJobsFromDb(scheduler, jobKeySet);
			logHelper.LogInfo("Finish to synchronize job details between database and xml");
		}

		async Task DeleteJobsFromDb(IScheduler scheduler, IReadOnlySet<JobKey> jobKeySet)
		{
			var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
			foreach (var jobKey in jobKeys)
			{
				if (!jobKeySet.Contains(jobKey))
				{
					await scheduler.DeleteJob(jobKey);
					logHelper.LogInfo($"Delete job in database with jobName {{{jobKey.Name}}} and groupName {{{jobKey.Group}}}");
				}
			}
		}

		static async Task<HashSet<JobKey>> ConstructJobKeysFromXmlFiles()
		{
			var jobKeySet = new HashSet<JobKey>();
			var fileNames = ConfigurationProvider.QuartzProps["quartz.plugin.jobInitializer.fileNames"];
			if (string.IsNullOrEmpty(fileNames))
			{
				throw new SchedulerConfigException("quartz.plugin.jobInitializer.fileNames should not be null");
			}

			var jobDetails = await QRTZ_JOB_DETAILSHelper.GetJobDetailsFromXmlFiles(fileNames.Split(','));
			foreach (var jobDetail in jobDetails)
			{
				var jobName = jobDetail.name.TrimEmptyToNull();
				var jobGroup = jobDetail.group.TrimEmptyToNull() ?? Key<string>.DefaultGroup;
				jobKeySet.Add(JobKey.Create(jobName, jobGroup));
			}

			return jobKeySet;
		}
	}
}
