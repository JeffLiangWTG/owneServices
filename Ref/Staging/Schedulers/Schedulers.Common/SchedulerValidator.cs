using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class SchedulerValidator : ISchedulerValidator
	{
		const string responseTriggerMessage = "{ \"_err\":\"Triggers with JobDataMap are not allowed.\"}";
		const string responseScheduleMessage = "{ \"_err\":\"New Schedule jobs are not allowed.\"}";
		const string responseTriggerNameMessage = "{ \"_err\":\"Invalid Trigger Name.\"}";
		const string responseInvalidCornExpression = "{ \"_err\":\"Invalid Cron Expression.\"}";
		const string responseInvalidDataExpression = "{ \"_err\":\"Invalid RepeatCount or RepeatInterval.\"}";
		const string responseNotAllowedEndPointMessage = "{ \"_err\":\"Action is not allowed.\"}";
		const string triggerNamePattern = @"^[\w\s\.,;:!?'\-\*]*$";

		readonly IStagingRepository stagingRepo;

		public SchedulerValidator(IStagingRepository stagingRepo)
		{
			this.stagingRepo = stagingRepo;
		}

		public bool IsNewScheduleJob(string jobName, string groupName)
		{
			return !stagingRepo.Get<QRTZ_JOB_DETAILS>().Any(x => x.JOB_NAME == jobName && x.JOB_GROUP == groupName);
		}

		public bool Validate(IFormCollectionService formCollectionService, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (formCollectionService.IsAllowedCommand)
			{
				return true;
			}
			if (!formCollectionService.IsAddTriggerCommand)
			{
				errorMessage = responseNotAllowedEndPointMessage;
				return false;
			}

			if (IsNewScheduleJob(formCollectionService.Job, formCollectionService.Group))
			{
				errorMessage = responseScheduleMessage;
				return false;
			}
			if (HasJobDataMap(formCollectionService.Keys))
			{
				errorMessage = responseTriggerMessage;
				return false;
			}
			if (!IsValidTriggerName(formCollectionService.Name))
			{
				errorMessage = responseTriggerNameMessage;
				return false;
			}

			var inputTriggerType = formCollectionService.TriggerType;
			_ = Enum.TryParse(typeof(TriggerType), inputTriggerType, true, out var triggerType);

			var repeatCount = formCollectionService.RepeatCount;
			var repeatInterval = formCollectionService.RepeatInterval;
			var repeatForeverStr = formCollectionService.RepeatForever;
			var cronExpression = formCollectionService.CronExpression;

			if ((TriggerType)triggerType == TriggerType.Cron &&
				(string.IsNullOrWhiteSpace(cronExpression) || !CronExpression.IsValidExpression(cronExpression)))
			{
				errorMessage = responseInvalidCornExpression;
				return false;
			}
			var repeatForever = !string.IsNullOrEmpty(repeatForeverStr) && bool.Parse(repeatForeverStr);
			if ((TriggerType)triggerType == TriggerType.Simple &&
				!((repeatForever || uint.TryParse(repeatCount, out _)) && ulong.TryParse(repeatInterval, out _)))
			{
				errorMessage = responseInvalidDataExpression;
				return false;
			}

			return true;
		}

		static bool HasJobDataMap(ICollection<string> keys)
		{
			return keys.Any(key => key.StartsWith("jobDataMap", StringComparison.OrdinalIgnoreCase));
		}

		static bool IsValidTriggerName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return true;
			}
			return Regex.IsMatch(name, triggerNamePattern);
		}
	}

	public enum TriggerType
	{
		Cron,
		Simple
	}
}
