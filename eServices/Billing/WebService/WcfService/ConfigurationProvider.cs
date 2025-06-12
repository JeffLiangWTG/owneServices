using System;
using System.Configuration;

namespace CargoWise.eServices.Billing.WcfService
{
	public interface IConfigurationProvider
	{
		string BillingKafkaTopic { get; }
		string UsageELKKafkaTopic { get; }
		string BilledELKKafkaTopic { get; }
		string[] NonBilledCodes { get; }
		string[] SendToELKBillingCategories { get; }
		string[] NotSendToELKBillingCodes { get; }
		int MaxRetries { get; }
		int RetryTimeoutInMilliseconds { get; }
		string BillingConnectionString { get; }
		int ELKResubmissionBatchSize { get; }
		int BillingKafkaBatchSize { get; }
		int PartitionLagThreshold { get; }
		string[] HostedServerNames { get; }
		int ThrottlingThresholdOfStaging { get; }
		int CacheTimeInMinutesOfStagingCounter { get; }
		int MaxBillingProcessingTimeInMinutes { get; }
		bool SkipHangfireJobReset { get; }
	}

	public class ConfigurationProvider : IConfigurationProvider
	{
		public string BillingKafkaTopic { get; } = ConfigurationManager.AppSettings["BillingKafkaTopic"];

		public string UsageELKKafkaTopic { get; } = ConfigurationManager.AppSettings["UsageELKKafkaTopic"];

		public string BilledELKKafkaTopic { get; } = ConfigurationManager.AppSettings["BilledELKKafkaTopic"];

		public string[] NonBilledCodes { get; } = (ConfigurationManager.AppSettings["NonBilledItemCodes"] ?? string.Empty).Split(',');
		public string[] SendToELKBillingCategories { get; } = (ConfigurationManager.AppSettings["SendToELKBillingCategories"] ?? string.Empty).Split(',');
		public string[] NotSendToELKBillingCodes { get; } = (ConfigurationManager.AppSettings["NotSendToELKBillingCodes"] ?? string.Empty).Split(',');

		public int MaxRetries { get; } =
			(int.TryParse(ConfigurationManager.AppSettings["MaxRetries"], out var maxRetries) ? maxRetries : 3);

		public int RetryTimeoutInMilliseconds { get; } =
			(int.TryParse(ConfigurationManager.AppSettings["RetryTimeoutInMilliseconds"], out var timeoutInMilliseconds)
				? timeoutInMilliseconds
				: 2000);

		public int ELKResubmissionBatchSize { get; } = (int.TryParse(ConfigurationManager.AppSettings["ELKResubmissionBatchSize"], out var batchSize) ? batchSize : 1000);
		public int BillingKafkaBatchSize{ get; } = (int.TryParse(ConfigurationManager.AppSettings["BillingKafkaConsumerBatchSize"], out var batchSize) ? batchSize : 1000);
		public int PartitionLagThreshold { get; } = (int.TryParse(ConfigurationManager.AppSettings["PartitionLagThreshold"], out var threshold) ? threshold : 100000);
		public string BillingConnectionString { get; } = ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString;
		public string[] HostedServerNames { get; } = (ConfigurationManager.AppSettings["HostedServerNames"] ?? string.Empty).Split(',');
		public int ThrottlingThresholdOfStaging { get; } = (int.TryParse(ConfigurationManager.AppSettings["ThrottlingThresholdOfStaging"], out var output) ? output : Int32.MaxValue);
		public int CacheTimeInMinutesOfStagingCounter { get; } = (int.TryParse(ConfigurationManager.AppSettings["CacheTimeInMinutesOfStagingCounter"], out var output) ? output : 5);
		public int MaxBillingProcessingTimeInMinutes { get; } = (int.TryParse(ConfigurationManager.AppSettings["MaxBillingProcessingTimeInMinutes"], out var output) ? output : 60);
		public bool SkipHangfireJobReset { get; } = (bool.TryParse(ConfigurationManager.AppSettings["SkipHangfireJobReset"], out var output) ? output : false);
	}
}
