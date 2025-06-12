using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.eServices.Billing.WcfService.Hangfire;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using Cronos;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace CargoWise.eServices.Billing.WcfService.HealthCheck
{
	public abstract class ProcessingBillingDbHealthCheckItemProviderBase : IHealthCheckItemProvider
	{
		public Task<HealthCheckItem> CheckHealthAsync() => Task.FromResult(CheckJobHealth());

		protected HealthCheckItem CheckJobHealth()
		{
			try
			{

				using (var connection = GetStorageConnection())
				{
					var job = connection.GetRecurringJobs().FirstOrDefault(x => x.Id == JobId);
					if (job == null)
					{
						return HealthCheckItem.Error("Recurring job has not been added");
					}

					if (job.LastExecution == null || string.IsNullOrEmpty(job.LastJobId))
					{
						return HealthCheckItem.Info($"Healthy. The job will run first time at {job.NextExecution?.ToLocalTime():yyyy-MM-dd HH:mm:sszzz}");
					}
				}

				var recurringJob = GetStorageConnection().GetRecurringJobs(new[] { JobId }).First();
				var cron = CronExpression.Parse(recurringJob.Cron);
				var utcNow = DateTimeProvider.UtcNow;
				var occurrences = cron.GetOccurrences(utcNow.AddMonths(-12), utcNow, TimeZoneInfo.FindSystemTimeZoneById(recurringJob.TimeZoneId));
				DateTime? startUtc = occurrences.Last();
				DateTime? endUtc = utcNow;

				var succeededJobs = BillingJobManager.GetBillingJobs<SucceededJobDto>(MonitoringApi.Value, typeof(BillingDatabaseProcessor), JobMethodName, startUtc, endUtc).ToArray();
				if (succeededJobs.Any())
				{
					return HealthCheckItem.Info($"Healthy. Succeeded at {DateTime.SpecifyKind(succeededJobs.First().Value.SucceededAt.Value, DateTimeKind.Utc).ToLocalTime():yyyy-MM-dd HH:mm:sszzz}");
				}

				var failedJobs = BillingJobManager.GetBillingJobs<FailedJobDto>(MonitoringApi.Value, typeof(BillingDatabaseProcessor), JobMethodName, startUtc, endUtc).ToArray();
				if (failedJobs.Any())
				{
					return HealthCheckItem.Error($"The job failed at {DateTime.SpecifyKind(failedJobs.First().Value.FailedAt.Value, DateTimeKind.Utc).ToLocalTime():yyyy-MM-dd HH:mm:sszzz}. See https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/10748/Processing-Billing-database");
				}

				var processingJob = BillingJobManager.GetBillingJobs<ProcessingJobDto>(MonitoringApi.Value, typeof(BillingDatabaseProcessor), JobMethodName, startUtc, endUtc).ToArray();
				if (processingJob.Any())
				{
					return DateTimeProvider.UtcNow - DateTime.SpecifyKind(processingJob.First().Value.StartedAt.Value, DateTimeKind.Utc) > TimeSpan.FromMinutes(Configuration.Value.MaxBillingProcessingTimeInMinutes)
						? HealthCheckItem.Error($"Processing billing exceeded {Configuration.Value.MaxBillingProcessingTimeInMinutes} minutes. Job Id: {processingJob.First().Key}")
						: HealthCheckItem.Info($"Processing billing job is running. Job Id: {processingJob.First().Key}");
				}

				return HealthCheckItem.Error("No succeeded job. See https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/10748/Processing-Billing-database");
			}
			catch (Exception ex)
			{
				var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check. Message: {1}", ex.GetType().FullName, ex.Message);
				return HealthCheckItem.Error(errorDescription);
			}
		}

		protected internal virtual IStorageConnection GetStorageConnection() => JobStorage.Current.GetConnection();
		protected internal virtual IDateTimeProvider DateTimeProvider { get; } = new DateTimeProvider();
		protected abstract string JobMethodName { get; }
		public string Name => JobId;
		public abstract string JobId { get; }

		readonly Lazy<IConfigurationProvider> Configuration = new Lazy<IConfigurationProvider>(() => Global.WindsorContainer.Resolve<IConfigurationProvider>());
		readonly Lazy<IMonitoringApi> MonitoringApi = new Lazy<IMonitoringApi>(() => Global.WindsorContainer.Resolve<IMonitoringApi>());
	}
}
