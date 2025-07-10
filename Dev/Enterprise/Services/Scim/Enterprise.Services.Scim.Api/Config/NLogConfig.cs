using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using WTG.Logging.NLog.Kafka;

namespace Enterprise.Services.Scim.Api.Config
{
	public static class NLogConfig
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Json attributes")]
		public static void ConfigNLog()
		{
			var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();

			var jsonLayout = new JsonLayout
			{
				MaxRecursionLimit = 3,
				IncludeEventProperties = true
			};
			jsonLayout.Attributes.Add(new JsonAttribute("message", "${message}"));
			jsonLayout.Attributes.Add(new JsonAttribute("eventTime", "${date:universalTime=true:format=yyyy-MM-dd\\THH\\:mm\\:ss.fffK}"));

			try
			{
				var topic = Env.Registry.ScimLoggingKafkaTopic;
				var brokers = Env.Registry.ScimLoggingKafkaBrokers;
				var username = Env.Registry.ScimLoggingKafkaTopicUsername;
				var password = Env.Registry.ScimLoggingKafkaTopicPassword;
				if (!string.IsNullOrEmpty(topic) && !string.IsNullOrEmpty(brokers) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
				{
					var brokerAddresses = brokers.Split(',');
					var caFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Config/Kafka.pem");
					var kafkaTarget = KafkaLoggingTarget.GetSaslSslTarget("kafka", topic, brokerAddresses, username, password, jsonLayout, caFile);
					var asyncKafkaTarget = new AsyncTargetWrapper(kafkaTarget)
					{
						Name = kafkaTarget.Name,
						QueueLimit = 50,
						OverflowAction = AsyncTargetWrapperOverflowAction.Discard
					};
					config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncKafkaTarget, "*");
				}
				else
				{
					var eventLog = new EventLogTarget("scim");
					eventLog.EventId = "${event-properties:EventId:whenEmpty=0}";
					eventLog.Layout = "${longdate} | ${level} | ${logger}: ${message} ${exception:format=tostring}";
					eventLog.Source = "SCIM";
					var asyncTarget = new AsyncTargetWrapper(eventLog)
					{
						Name = eventLog.Name,
						QueueLimit = 50,
						OverflowAction = AsyncTargetWrapperOverflowAction.Discard
					};
					config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncTarget, "*");
				}

				LogManager.Configuration = config;
			}
			catch (DatabaseUpgradedException)
			{
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Enterprise.Services.Scim.Api.NLogConfig", ex.Message, ex);
			}
		}
	}
}
