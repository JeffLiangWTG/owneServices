using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingTransaction;
using CargoWise.eServices.Billing.WcfService.Hangfire.ELKMessage;
using Common.Logging;
using Confluent.Kafka;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace CargoWise.eServices.Billing.WcfService.Hangfire
{
	public interface IBillingJobManager
	{
		void ResetJobs();
		void EnqueueOrDeleteJobs();
	}

	public class BillingJobManager : IBillingJobManager
	{
		public void ResetJobs()
		{
			if (!Configuration.Value.SkipHangfireJobReset)
			{
				HangfireJobWrapper.Value.AddOrUpdateRecurringJob("ProcessBillingDatabase", () => BillingDatabaseProcessor.RecurringProcess(null, CancellationToken.None), Cron.Hourly(), queue: "billing-processing-hourly");
				HangfireJobWrapper.Value.AddOrUpdateRecurringJob("ProcessBillingDatabaseMonthlyAggregation", () => BillingDatabaseProcessor.RecurringProcessMonthlyAggregation(null, CancellationToken.None), "0 6 1 * *", TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"), queue: "billing-processing-alpha");
				HangfireJobWrapper.Value.AddOrUpdateRecurringJob("ProcessBillingDatabaseUpdateBillingCubePartial", () => BillingDatabaseProcessor.RecurringProcessUpdateBillingCubePartial(null, CancellationToken.None), "30 9 * * *", TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"), "billing-processing-daily");
				HangfireJobWrapper.Value.AddOrUpdateRecurringJob("BillingJobManager", () => EnqueueOrDeleteJobs(), Cron.Minutely());
				HangfireJobWrapper.Value.TriggerRecurringJob("BillingJobManager");
				HangfireJobWrapper.Value.AddOrUpdateRecurringJob("ProcessFailedELKMessages", () => ELKFailedMessagesProcessor.RecurringProcess(null, CancellationToken.None), "*/5 * * * *"); // Every 5 minute
				HangfireJobWrapper.Value.TriggerRecurringJob("ProcessFailedELKMessages");
			}
		}

		[DisableConcurrentExecution(0)]
		[AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete, OnlyOn = new[] { typeof(DistributedLockTimeoutException) })]
		public void EnqueueOrDeleteJobs()
		{
			var topicMetadata = AdminClient.Value.GetMetadata(Configuration.Value.BillingKafkaTopic, TimeSpan.FromSeconds(5));
			var partitionCount = topicMetadata.Topics[0].Partitions.Count;

			if (partitionCount == 0)
			{
				throw new InvalidOperationException($"Topic {Configuration.Value.BillingKafkaTopic} does not have any partition or does not exist");
			}

			var processingJobs = GetBillingJobs<ProcessingJobDto>(MonitoringApi.Value, typeof(BillingServiceProcessor), "StartProcess").ToList();

			if (processingJobs.Count < partitionCount)
			{
				for (int i = 0; i < partitionCount - processingJobs.Count; i++)
				{
					Logger.Value.InfoFormat("Create new billing job");
					HangfireJobWrapper.Value.EnqueueBackgroundJob<BillingServiceProcessor>(x => x.StartProcess(CancellationToken.None, null));
				}
			}
			else
			{
				for (int i = partitionCount; i < processingJobs.Count; i++)
				{
					HangfireJobWrapper.Value.DeleteBackgroundJob(processingJobs[i].Key);
				}
			}
		}

		internal static IEnumerable<KeyValuePair<string, TDto>> GetBillingJobs<TDto>(IMonitoringApi monitoringApi, Type jobType, string methodName, DateTime? startDateTimeUtc = null, DateTime? endDateTimeUtc = null)
		{
			var methodInfo = jobType.GetMethod(methodName);
			long count;

			bool IsMatched(Type type, MethodInfo method, DateTime? jobDatetime)
			{
				var isTypeMatched = type == jobType && method == methodInfo;

				var isWithinTimeRange = (startDateTimeUtc == null && endDateTimeUtc == null) || (jobDatetime != null && (startDateTimeUtc ?? DateTime.MinValue) <= DateTime.SpecifyKind(jobDatetime.Value, DateTimeKind.Utc) && DateTime.SpecifyKind(jobDatetime.Value, DateTimeKind.Utc) <= (endDateTimeUtc ?? DateTime.MaxValue));

				return isTypeMatched && isWithinTimeRange;
			};

			if (typeof(TDto) == typeof(ProcessingJobDto))
			{
				count = monitoringApi.ProcessingCount();
				return count == 0 ? Array.Empty<KeyValuePair<string, TDto>>() : (IEnumerable<KeyValuePair<string, TDto>>)monitoringApi.ProcessingJobs(0, Convert.ToInt32(count))
					.Where(x => x.Value.Job != null && IsMatched(x.Value.Job.Type, x.Value.Job.Method, x.Value.StartedAt)).OrderByDescending(x => x.Value.StartedAt);
			}

			if (typeof(TDto) == typeof(SucceededJobDto))
			{
				count = monitoringApi.SucceededListCount();
				return count == 0 ? Array.Empty<KeyValuePair<string, TDto>>() : (IEnumerable<KeyValuePair<string, TDto>>)monitoringApi.SucceededJobs(0, Convert.ToInt32(count))
					.Where(x => x.Value.Job != null && IsMatched(x.Value.Job.Type, x.Value.Job.Method, x.Value.SucceededAt)).OrderByDescending(x => x.Value.SucceededAt);
			}

			if (typeof(TDto) == typeof(FailedJobDto))
			{
				count = monitoringApi.FailedCount();
				return count == 0 ? Array.Empty<KeyValuePair<string, TDto>>() : (IEnumerable<KeyValuePair<string, TDto>>)monitoringApi.FailedJobs(0, Convert.ToInt32(count))
					.Where(x => x.Value.Job != null && IsMatched(x.Value.Job.Type, x.Value.Job.Method, x.Value.FailedAt)).OrderByDescending(x => x.Value.FailedAt);
			}

			throw new NotImplementedException($"Unknown type {typeof(TDto).FullName}");
		}

		internal static bool IsBackgroundJobServerRunning(IMonitoringApi monitoringApi) => monitoringApi.Servers().Any(x => x.Name.IndexOf(Dns.GetHostName(), StringComparison.CurrentCultureIgnoreCase) >= 0);

		readonly Lazy<ILog> Logger = new Lazy<ILog>(() => LogManager.GetLogger(typeof(BillingJobManager)));
		readonly Lazy<IHangfireJobWrapper> HangfireJobWrapper = new Lazy<IHangfireJobWrapper>(() => Global.WindsorContainer.Resolve<IHangfireJobWrapper>());
		readonly Lazy<IConfigurationProvider> Configuration = new Lazy<IConfigurationProvider>(() => Global.WindsorContainer.Resolve<IConfigurationProvider>());
		readonly Lazy<IAdminClient> AdminClient = new Lazy<IAdminClient>(() => Global.WindsorContainer.Resolve<IAdminClient>());
		readonly Lazy<IMonitoringApi> MonitoringApi = new Lazy<IMonitoringApi>(() => Global.WindsorContainer.Resolve<IMonitoringApi>());
	}
}
