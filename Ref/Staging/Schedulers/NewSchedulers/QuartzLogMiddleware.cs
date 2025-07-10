using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl.Triggers;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	public class QuartzLogMiddleware
	{
		readonly RequestDelegate next;
		readonly ILogHelper logHelper;
		readonly IScheduler scheduler;

		public QuartzLogMiddleware(RequestDelegate next, ILogHelper logHelper, IScheduler scheduler)
		{
			Argument.NotNull(next, nameof(next));
			Argument.NotNull(logHelper, nameof(logHelper));

			this.next = next;
			this.logHelper = logHelper;
			this.scheduler = scheduler;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var userName = context.User?.Identity?.Name;
			if (!string.IsNullOrEmpty(userName) && context.Request.HasFormContentType)
			{
				var formCollectionService = context.RequestServices.GetRequiredService<IFormCollectionService>();
				var command = formCollectionService.Command;
				if (!formCollectionService.IsGetCommand)
				{
					var jobName = formCollectionService.Job;
					var jobGroup = formCollectionService.Group;

					var triggerInfo = string.Empty;
					if (formCollectionService.IsTriggerRelatedCommand)
					{
						string triggerName;
						string triggerType;
						double repeatInterval = 0;
						var cronExpression = string.Empty;
						if (formCollectionService.IsAddTriggerCommand)
						{
							triggerName = formCollectionService.Name;
							triggerType = formCollectionService.TriggerType;
							_ = double.TryParse(formCollectionService.RepeatInterval, out repeatInterval);
							cronExpression = formCollectionService.CronExpression;
						}
						else
						{
							triggerName = formCollectionService.Trigger;
							var triggerGroup = formCollectionService.Group;
							var trigger = await scheduler.GetTrigger(new TriggerKey(triggerName, triggerGroup));
							jobName = trigger?.JobKey.Name;
							jobGroup = trigger?.JobKey.Group;
							triggerType = trigger.CalendarName;
							if (trigger.GetType() == typeof(SimpleTriggerImpl))
							{
								triggerType = "Simple";
								repeatInterval = ((SimpleTriggerImpl)trigger).RepeatInterval.TotalMilliseconds;
							}
							else if (trigger.GetType() == typeof(CronTriggerImpl))
							{
								triggerType = "Cron";
								cronExpression = ((CronTriggerImpl)trigger).CronExpressionString;
							}
						}
						var repeatIntervalMinutes = repeatInterval / 1000 / 60;
						triggerInfo = $", Trigger Name: {triggerName}, Trigger Type: {triggerType}, Repeat Interval: {repeatIntervalMinutes} min, Cron Expression: {cronExpression}";
					}

					var message = $"User: {userName}, Command: {command}, Job Name: {jobName}, Job Group: {jobGroup}{triggerInfo}";
					logHelper.LogInfo(message);
				}
			}

			await next(context);
		}
	}
}
