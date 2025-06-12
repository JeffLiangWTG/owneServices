using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Common.Logging;
using Common.Logging.Simple;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public interface ILoggerFactory
	{
		ILog CreateLogger(string name, XmlDocument configXml);
		ILog CreateLogger(string name, string type, XmlDocument configXml);
		ILog CreateLogger(string name, string type, ILoggerConfiguration config);
	}

	[ExcludeFromCodeCoverage]
	public class LoggerFactory : ILoggerFactory
	{
		private LoggerFactory() { }

		public static ILoggerFactory Instance { get; } = new LoggerFactory();

		public ILog CreateLogger(string name, XmlDocument configXml) => CreateLogger(name, null, new LoggerConfiguration(configXml));
		public ILog CreateLogger(string name, string type, XmlDocument configXml) => CreateLogger(name, type, new LoggerConfiguration(configXml));
		private readonly ConcurrentDictionary<string, SemaphoreSlim> modifyLoggerSemaphoreSlims = new();

		public ILog CreateLogger(string name, string type, ILoggerConfiguration config)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(config.LogDir))
					throw new InvalidOperationException("Log configuration is invalid.");

				var logName = string.IsNullOrWhiteSpace(type) ? name : $"{name}.{type}";
				if (log4net.LogManager.GetLogger(logName).Logger is Logger logger)
				{
					var semaphore = modifyLoggerSemaphoreSlims.GetOrAdd(logName, key => new SemaphoreSlim(1, 1));
					semaphore.Wait();
					try
					{
						ConfigAppenders(logger, config, name);
					}
					finally
					{
						semaphore.Release();
					}
				}

				return LogManager.GetLogger(logName);
			}
			catch (Exception ex)
			{
				var logger = new TraceLoggerFactoryAdapter().GetLogger(typeof(LoggerFactory).FullName);
				logger.ErrorFormat("Error creating logger. Name='{0}'", ex, name);
				return new NoOpLogger();
			}
		}

		private void ConfigAppenders(Logger logger, ILoggerConfiguration config, string resource)
		{
			logger.Additivity = false;

			switch (config.LogFormat)
			{
				case TransferrerLogFormat.Flat:
					AddOrUpdateAppender(logger, config, resource, TransferrerLogFormat.Flat);
					RemoveAppender(logger, TransferrerLogFormat.Structured);
					break;
				case TransferrerLogFormat.Structured:
					AddOrUpdateAppender(logger, config, resource, TransferrerLogFormat.Structured);
					RemoveAppender(logger, TransferrerLogFormat.Flat);
					break;
				case TransferrerLogFormat.Both:
					AddOrUpdateAppender(logger, config, resource, TransferrerLogFormat.Flat);
					AddOrUpdateAppender(logger, config, resource, TransferrerLogFormat.Structured);
					break;
				default:
					throw new InvalidOperationException($"{nameof(TransferrerLogFormat)}: {config.LogFormat} is not implemented.");
			}
		}

		private void AddOrUpdateAppender(Logger logger, ILoggerConfiguration config, string resource, TransferrerLogFormat format)
		{
			var appenderName = GetAppenderName(logger.Name, format);
			var created = false;
			if (logger.GetAppender(appenderName) is not RollingFileAppender appender)
			{
				created = true;
				appender = CreateAppender(appenderName, resource, format);
			}

			ConfigAppender(appender, config, logger.Name, format);

			if (created)
			{
				logger.AddAppender(appender);
				logger.Level = appender.Threshold;
			}
		}

		private void RemoveAppender(Logger logger, TransferrerLogFormat format)
		{
			var appender = logger.GetAppender(GetAppenderName(logger.Name, format));
			if (appender != null)
			{
				appender.Close();
				logger.RemoveAppender(appender);
			}
		}

		private static string GetAppenderName(string logName, TransferrerLogFormat format)
		{
			return $"{logName}_{format}";
		}

		private void ConfigAppender(RollingFileAppender appender, ILoggerConfiguration config, string logName, TransferrerLogFormat format)
		{
			appender.Threshold = (Level)typeof(Level).GetField(config.LogLevel.ToString()).GetValue(null);
			appender.MaximumFileSize = $"{config.LogMaxSize ?? LoggerConfiguration.DefaultLogMaxSize}MB";
			appender.MaxSizeRollBackups = config.LogMaxCount ?? LoggerConfiguration.DefaultLogMaxCount;

			switch (format)
			{
				case TransferrerLogFormat.Flat:
					appender.File = Path.Combine(config.LogDir, $"{logName}.log");
					appender.RollingStyle = RollingFileAppender.RollingMode.Size;
					appender.CountDirection = 1;
					break;
				case TransferrerLogFormat.Structured:
					appender.File = Path.Combine(config.StructuredLogDir, $"{logName}.log");
					appender.RollingStyle = RollingFileAppender.RollingMode.Date;
					appender.CountDirection = -1;
					appender.DatePattern = ".yyyyMMdd";
					appender.StaticLogFileName = false;
					break;
				case TransferrerLogFormat.Both:
				default:
					throw new InvalidOperationException($"{nameof(TransferrerLogFormat)}: {format} is not implemented.");
			}

			appender.ActivateOptions();
		}

		private RollingFileAppender CreateAppender(string name, string resource, TransferrerLogFormat format)
		{
			var appender = new RollingFileAppender
			{
				Name = name,
				PreserveLogFileNameExtension = true,
				Encoding = Encoding.UTF8
			};

			switch (format)
			{
				case TransferrerLogFormat.Flat:
					appender.Layout = CreateFlatLayout();
					break;
				case TransferrerLogFormat.Structured:
					appender.Layout = CreateStructLayout(resource);
					break;
				case TransferrerLogFormat.Both:
				default:
					throw new InvalidOperationException($"{nameof(TransferrerLogFormat)}: {format} is not implemented.");
			}

			return appender;
		}

		private PatternLayout CreateFlatLayout()
		{
			var flatLayout = new PatternLayout("%date{ddd, dd MMM yyyy HH:mm:ss %K} [%-6level] %message%newline");
			flatLayout.ActivateOptions();
			return flatLayout;
		}

		private PatternLayout CreateStructLayout(string resource)
		{
			var structLayout = new PatternLayout("%JsonPatternLayoutConverterMessage%newline")
			{
				IgnoresException = false
			};

			structLayout.AddConverter(new ConverterInfo()
			{
				Name = "JsonPatternLayoutConverterMessage",
				Type = typeof(JsonPatternLayoutConverter)
			});
			structLayout.ActivateOptions();
			return structLayout;
		}
	}
}
