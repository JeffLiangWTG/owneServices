using System;
using System.Globalization;
using CargoWise.eHub.Common;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor
{
	public static class ApplicationConfig
	{
		public static string EHubGatewayServerAddress => Config["eHubGatewayServerAddress"];
		public static string EHubGatewayClientId => Config["eHubGatewayClientId"];
		public static string EHubGatewayClientPassword => Config["eHubGatewayClientPassword"];
		public static int MessageBatchSize => Convert.ToInt32(Config["messageBatchSize"], CultureInfo.InvariantCulture);
		public static string SafeConnectionString => DbConnectionStringManager.SafeConnectionString;
		public static int CommandTimeoutInSeconds => Convert.ToInt32(Config["CommandTimeoutInSeconds"], CultureInfo.InvariantCulture);
		public static int RetryCount => Convert.ToInt32(Config["RetryCount"], CultureInfo.InvariantCulture);
		public static int RetryIntervalInSeconds => Convert.ToInt32(Config["RetryIntervalInSeconds"], CultureInfo.InvariantCulture);

		public static class ProgramArgs
		{
			public const string PUSH = "PUSH";
			public const string AUTO = "AUTO";
		}

		public static class Messaging
		{
			public const string SchemaName = @"http://cargowise.com/ehub/core/genericmessagedelivery#GenericMessageInterchange";
			public const string From = "RefDbRepoDataPublishing";
			public const MessageSchemaType SchemaType = MessageSchemaType.Xml;
			public const string ApplicationCode = "GMD";
			public const string InterchangeType = "REN";
		}

		public const string TempDataSetIdTableName = "#DataSetIds";

		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.Staging.DataPublishingProcessor.config.json").Build();
	}
}
