using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.Common.ErrorReporting
{
	public class QuartzJobProvider : IQuartzJobProvider
	{
		readonly IStagingRepository repo;
		public const string SimpleType = "SIMPLE";
		public const string CronType = "CRON";

		public QuartzJobProvider(IStagingRepository repo)
		{
			Argument.NotNull(repo, nameof(repo));
			this.repo = repo;
		}

		List<QRTZ_JOB_DETAILS> jobDetails;
		List<QRTZ_JOB_DETAILS> JobDetails
		{
			get
			{
				if (jobDetails == null)
				{
					jobDetails = repo.Get<QRTZ_JOB_DETAILS>()?.ToList();
					Argument.NotNull(jobDetails, nameof(jobDetails));
					jobDetails = jobDetails.Select(o =>
						{
							QRTZ_JOB_DETAILSHelper.SetCalculatedProperties(o);
							return o;
						})
						.Where(o => !(string.IsNullOrEmpty(o.ProgramExePath) && string.IsNullOrEmpty(o.ProgramArgs)))
						.ToList();
				}
				return jobDetails;
			}
		}
		public string GetQuartzJobName(string programExePath, string programArgs)
		{
			if (string.IsNullOrEmpty(programExePath))
			{
				return string.Empty;
			}

			var jobDetail = JobDetails.SingleOrDefault(o => o.ProgramExePath == programExePath && (o.ProgramArgs == programArgs || (string.IsNullOrEmpty(programArgs) && o.ProgramArgs is null)));
			return jobDetail?.JOB_NAME ?? string.Empty;
		}

		public int GetTriggeredTimesPerDay(string jobName)
		{
			Argument.NotNullOrEmpty(jobName, nameof(jobName));

			int triggeredTimesOneDay = 0;
			var triggers = repo.Get<QRTZ_TRIGGERS>().Where(x => x.JOB_NAME == jobName);
			var currentTime = DateTimeOffset.Now;

			foreach (var trigger in triggers)
			{
				var triggerType = trigger.TRIGGER_TYPE;
				switch (triggerType)
				{
					case SimpleType:
						triggeredTimesOneDay += GetTriggeredTimesPerDayFromSimpleTrigger(trigger);
						break;
					case CronType:
						triggeredTimesOneDay += GetTriggeredTimesPerDayFromCronTrigger(currentTime, trigger);
						break;
					default: throw new System.NotImplementedException();
				}
			}

			return triggeredTimesOneDay < 1 ? 1 : triggeredTimesOneDay;
		}

		int GetTriggeredTimesPerDayFromSimpleTrigger(QRTZ_TRIGGERS trigger)
		{
			var triggerInfo = repo.Get<QRTZ_SIMPLE_TRIGGERS>().Where(x => x.TRIGGER_NAME == trigger.TRIGGER_NAME);
			if (!triggerInfo.Any())
			{
				throw new ArgumentException($"Can not find trigger {trigger.TRIGGER_NAME} in QRTZ_SIMPLE_TRIGGER");
			}

			var interval = triggerInfo.Single().REPEAT_INTERVAL;
			return (int)(24*60*60 / TimeSpan.FromMilliseconds(interval).TotalSeconds);
		}

		int GetTriggeredTimesPerDayFromCronTrigger(DateTimeOffset currentTime, QRTZ_TRIGGERS trigger)
		{
			var triggerInfo = repo.Get<QRTZ_CRON_TRIGGERS>().Where(x => x.TRIGGER_NAME == trigger.TRIGGER_NAME);
			if (!triggerInfo.Any())
			{
				throw new ArgumentException($"Can not find trigger {trigger.TRIGGER_NAME} in QRTZ_CRON_TRIGGERS");
			}

			var cronExpression = new CronExpression(triggerInfo.Single().CRON_EXPRESSION);
			var nextTriggerTime = cronExpression.GetNextValidTimeAfter(currentTime);
			var count = 0;

			while(nextTriggerTime != null && nextTriggerTime <= currentTime.AddDays(1)){
				count++;
				nextTriggerTime = cronExpression.GetNextValidTimeAfter(nextTriggerTime.Value);
			}
				
			return count;

		}
	}
}
