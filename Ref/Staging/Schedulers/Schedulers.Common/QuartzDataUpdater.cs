using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Quartz;
using Quartz.Impl.Matchers;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class QuartzDataUpdater : IJob
	{
		IStagingRepository stagingRepository;

		public QuartzDataUpdater()
		{
		}

		public QuartzDataUpdater(IStagingRepository stagingRepository)
		{
			this.stagingRepository = stagingRepository;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var repo = stagingRepository ?? new StagingRepository(ConfigurationManager.ConnectionStrings["StagingConnectionString"].ConnectionString);
			try
			{
				var previousRunDate = context.PreviousFireTimeUtc?.DateTime ?? DateTime.MinValue;
				var results = from sourceData in repo.Get<SourceData>()
							  join dataChange in repo.Get<DataChangeCapture>()
							  on sourceData.SDA_PK equals dataChange.DCC_ParentPK
							  where dataChange.DCC_ParentCode == "SDA"
							  && dataChange.DCC_NewValue == StatusProvider.GetMERStatus()
							  && dataChange.DCC_EventTimeUTC > previousRunDate
							  group sourceData by sourceData.SDA_SubSource into result
							  select result.FirstOrDefault().SDA_SubSource;

				if (results.Any())
				{
					var jobsToTrigger = new List<JobKey>();
					var allJobKeys = await context.Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
					foreach (var jobKey in allJobKeys)
					{
						var jobDetail = await context.Scheduler.GetJobDetail(jobKey);
						var datasetNames = jobDetail.JobDataMap.Where(x => x.Key == nameof(QuartzAppRunner.DataSetNames)).FirstOrDefault().Value?.ToString().Split(',') ?? new string[] { };
						if (results.Intersect(datasetNames).Any())
						{
							jobsToTrigger.Add(jobKey);
						}
					}
					foreach (var jobkey in jobsToTrigger)
					{
						await context.Scheduler.TriggerJob(jobkey);
					}
				}
			}
			finally
			{
				if (repo != null)
				{
					repo.Dispose();
				}
			}
		}
	}
}
