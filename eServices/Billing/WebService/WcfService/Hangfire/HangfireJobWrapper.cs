using System;
using System.Linq.Expressions;
using Hangfire;

namespace CargoWise.eServices.Billing.WcfService.Hangfire
{
	public interface IHangfireJobWrapper
	{
		void AddOrUpdateRecurringJob(string recurringJobId, Expression<Action> action, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");
		string EnqueueBackgroundJob<T>(Expression<Action<T>> action);
		string EnqueueBackgroundJob(Expression<Action> action);
		bool DeleteBackgroundJob(string jobId);
		void TriggerRecurringJob(string recurringJobId);
	}

	public class HangfireJobWrapper : IHangfireJobWrapper
	{
		public void AddOrUpdateRecurringJob(string recurringJobId, Expression<Action> action, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
		{
			RecurringJob.AddOrUpdate(recurringJobId, queue, action, cronExpression, new RecurringJobOptions()
			{
				TimeZone = timeZone ?? TimeZoneInfo.Utc
			});
		}

		public string EnqueueBackgroundJob<T>(Expression<Action<T>> action) => BackgroundJob.Enqueue(action);

		public string EnqueueBackgroundJob(Expression<Action> action) => BackgroundJob.Enqueue(action);

		public bool DeleteBackgroundJob(string jobId) => BackgroundJob.Delete(jobId);

		public void TriggerRecurringJob(string recurringJobId)
		{
			RecurringJob.TriggerJob(recurringJobId);
		}
	}
}
