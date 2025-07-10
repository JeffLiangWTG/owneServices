using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public sealed class XTMessagingConfig
	{
		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFileName)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static XTMessagingConfig Instance => instance.Value;
		static readonly Lazy<XTMessagingConfig> instance = new Lazy<XTMessagingConfig>(() => new XTMessagingConfig());


		public static void ReLoad()
		{
			Instance.XTIdleConnectionKeepAliveInSecondsValue = Convert.ToDouble(Configuration["XTIdleConnectionKeepAliveInSecondsValue"], ilCultureInfo);
			Instance.XTIdleConnectionRetryPauseInSecondsValue = Convert.ToDouble(Configuration["XTIdleConnectionRetryPauseInSecondsValue"], ilCultureInfo);
			Instance.InterchangeCountPerBatchOnReceivingValue = Convert.ToInt32(Configuration["InterchangeCountPerBatchOnReceivingValue"], ilCultureInfo);
			Instance.XTServerMessageChunkSizeWhenSendingValue = Convert.ToInt32(Configuration["XTServerMessageChunkSizeWhenSendingValue"], ilCultureInfo);
		}

		XTMessagingConfig()
		{
			XTIdleConnectionKeepAliveInSecondsValue = Convert.ToDouble(Configuration["XTIdleConnectionKeepAliveInSecondsValue"], ilCultureInfo);
			XTIdleConnectionRetryPauseInSecondsValue = Convert.ToDouble(Configuration["XTIdleConnectionRetryPauseInSecondsValue"], ilCultureInfo);
			InterchangeCountPerBatchOnReceivingValue = Convert.ToInt32(Configuration["InterchangeCountPerBatchOnReceivingValue"], ilCultureInfo);
			XTServerMessageChunkSizeWhenSendingValue = Convert.ToInt32(Configuration["XTServerMessageChunkSizeWhenSendingValue"], ilCultureInfo);
		}

		public double XTIdleConnectionKeepAliveInSecondsValue { get; set; }
		public double XTIdleConnectionRetryPauseInSecondsValue { get; set; }
		public int InterchangeCountPerBatchOnReceivingValue { get; set; }
		public int XTServerMessageChunkSizeWhenSendingValue { get; set; }

		static CultureInfo ilCultureInfo => CultureInfo.GetCultureInfo("he-IL");

		const string JsonConfigFileName = "XTMessagingConfig.json";
	}
}
