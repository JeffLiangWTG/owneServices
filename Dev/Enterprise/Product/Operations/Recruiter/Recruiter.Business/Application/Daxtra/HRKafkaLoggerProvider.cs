using System;
using System.IO;
using System.Linq;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using WTG.Logging.NLog.Kafka;

namespace Enterprise.Recruiter.Business
{
	public class HRKafkaLoggerProvider : IHRLoggerProvider, IDisposable
	{
		public HRKafkaLoggerProvider(string name)
		{
			this.name = string.Join(" ", name, Guid.NewGuid());
		}

		public HRKafkaLoggerProvider(string name, Layout layout) : this(name)
		{
			this.layout = layout;
		}

		public ILogger GetLogger()
		{
			if (logger != null)
			{
				return logger;
			}

			var logLevel = LogLevel.AllLevels.First(l => l.Name == RecruiterDataRegistry.Instance.LoggingMinLevel.Value);
			logger = LogManager.Setup().LoadConfiguration(builder =>
			{
				builder.ForLogger(ruleName: RuleName).FilterMinLevel(logLevel).WriteTo(KafkaLog);
			}).GetLogger(name);

			return logger;
		}

		readonly string name;
		string RuleName => $"{name}_recruiter_logger_rule";

		Target kafkaLog;

		ILogger logger;

		readonly Layout layout;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Layout")]
		Target KafkaLog
		{
			get
			{
				if (kafkaLog == null)
				{
					var topic = RecruiterDataRegistry.Instance.LoggingKafkaTopic.Value;
					var brokers = RecruiterDataRegistry.Instance.LoggingKafkaBrokers.Value;
					var username = RecruiterDataRegistry.Instance.LoggingKafkaTopicUsername.Value;
					var password = RecruiterDataRegistry.Instance.LoggingKafkaTopicPassword.Value;
					var layout = this.layout ?? "${message}";
					var caFile = Path.GetFullPath(".\\Recruiter.Business\\Certificate\\Kafka.pem");
					kafkaLog = KafkaLoggingTarget.GetSaslSslTarget(name, topic, brokers.Split(','), username, password, layout: layout, caFileLocation: caFile);
				}

				return kafkaLog;
			}
		}

		bool isDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					LogManager.Configuration?.RemoveTarget(name);
					_ = LogManager.Configuration?.RemoveRuleByName(RuleName);
					kafkaLog?.Dispose();
				}

				isDisposed = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
