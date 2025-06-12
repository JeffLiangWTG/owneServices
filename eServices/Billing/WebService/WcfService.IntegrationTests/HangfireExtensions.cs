using Hangfire;
using Hangfire.Storage;

namespace CargoWise.eServices.Billing.WcfService.IntegrationTests
{
	public static class HangfireExtensions
	{
		public static void DeleteProcessingJobs(this IBackgroundJobClient backgroundJobClient, IMonitoringApi monitoringApi)
		{
			var processingJobs = monitoringApi.ProcessingJobs(0, int.MaxValue);
			foreach (var processingJob in processingJobs)
			{
				backgroundJobClient.Delete(processingJob.Key);
			}
		}
	}
}
