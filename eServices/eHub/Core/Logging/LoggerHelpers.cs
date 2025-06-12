using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Common.Logging;
using Common.Logging.Simple;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.Logging
{
	public enum LogType
	{
		Host,
		Pipeline,
		Orchestration
	}

	public static class LoggerHelpers
	{
		public static ILog GetHostLogger()
		{
			return GetHostLogger(null);
		}

		public static ILog GetHostLogger(string componentName)
		{
			try
			{
				return GetLogger(Environment.GetHostName(), componentName, LogType.Host);
			}
			catch (Exception e)
			{
				try
				{
					LogManager.GetLogger(typeof(LoggerHelpers).FullName).Error("Error retrieving BizTalk host name", e);
				}
				catch { }
				return new NoOpLogger();
			}
		}

		public static ILog GetPipelineLogger(IBaseMessage message)
		{
			return GetLogger(GetPipelineName(message), null, LogType.Pipeline);
		}

		public static ILog GetPipelineLogger(IBaseMessage message, string componentName)
		{
			return GetLogger(GetPipelineName(message), componentName, LogType.Pipeline);
		}

		public static string GetPipelineName(IBaseMessage message)
		{
			string outboundLocn = (string)message.Context.Read("OutboundTransportLocation", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			bool isRequestResponse = Convert.ToBoolean(message.Context.Read("IsRequestResponse", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			bool wasSolicitResponse = Convert.ToBoolean(message.Context.Read("WasSolicitResponse", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));

			string rcvLocnName = (string)message.Context.Read("ReceiveLocationName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			string spName = (string)message.Context.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");

			if (!wasSolicitResponse && (outboundLocn == null || isRequestResponse))
				return rcvLocnName + "+Receive";
			else
				return spName + "+Send";
		}

		public static void LogComponentStart(ILog logger, IBaseComponent component)
		{
			logger.InfoFormat("======== START: {0} {1}", component.Name, new String('=', Math.Max(63 - component.Name.Length, 4)));
		}

		public static void LogComponentEnd(ILog logger, IBaseComponent component)
		{
			logger.InfoFormat("========== END: {0} {1}", component.Name, new String('=', Math.Max(63 - component.Name.Length, 4)));
		}

		public static void LogProperties(ILog logger, object target)
		{
			var props = target.GetType().GetProperties().Where(p => !p.GetCustomAttributes(typeof(BrowsableAttribute), false).Any(a => !((BrowsableAttribute)a).Browsable)).ToDictionary(k => k.Name, e => e.GetValue(target, null));
			if (props.Count > 0)
			{
				var leftAlign = props.Keys.Select(k => k.Length).Max();
				foreach (var prop in props)
					logger.InfoFormat("{0} = {1}", prop.Key.PadLeft(leftAlign), prop.Value);
			}
		}

		public static void LogMessageContextProperties(IBaseMessage inmsg, ILog logger)
		{
			string name, ns;
			for (int i = 0; i < inmsg.Context.CountProperties; i++)
			{
				object value = inmsg.Context.ReadAt(i, out name, out ns);
				logger.DebugFormat("{0}#{1} = {2}", ns, name, value);
			}
		}

		internal static ILog GetLogger(string primaryLogName, string componentName, LogType logType)
		{
			ILog logger = new NoOpLogger();
			string logName = null;

			try
			{
				if (String.IsNullOrWhiteSpace(componentName))
					logName = primaryLogName;
				else
					logName = primaryLogName + "#" + componentName;

				logger = logRepository.GetOrAdd(logName, l => CreateLogger(logName, primaryLogName, logType));
			}
			catch (Exception ex)
			{
				try
				{
					LogManager.GetLogger(typeof(LoggerHelpers).FullName).ErrorFormat("Error creating logger '{0}'.", ex, logName ?? "unknown");
				}
				catch { }
				return new NoOpLogger();
			}

			return logger;
		}

		internal static ILog CreateLogger(string logName, string folderName, LogType logType)
		{
			string sectionName = null;
			string defaultLogDir = null;
			switch (logType)
			{
				case LogType.Host:
					sectionName = "hostLogging";
					defaultLogDir = @"C:\Logs\BizTalk\Hosts";
					break;
				case LogType.Pipeline:
					sectionName = "pipelineLogging";
					defaultLogDir = @"C:\Logs\BizTalk\Pipelines";
					break;
				case LogType.Orchestration:
					sectionName = "orchestrationLogging";
					defaultLogDir = @"C:\Logs\BizTalk\Orchestrations";
					break;
			}
			var eHubGroup = Environment.GetExeConfig().GetSectionGroup("eHub");
			var config = (eHubGroup != null ? eHubGroup.Sections[sectionName] as LoggerConfigSection : null) ??
						 new LoggerConfigSection();
			var settings = config.Loggers[logName] ?? new LoggerConfigItem
			{
				Level = config.DefaultLevel,
				MaxSizeRollBackups = config.DefaultMaxSizeRollBackups,
				MaximumFileSize = config.DefaultMaximumFileSize
			};

			Logger log4NetLogger = log4net.LogManager.GetLogger(logName).Logger as Logger;
			if (log4NetLogger.Appenders.Count == 0)
			{
				string logFolder = Path.Combine(config.LogDir ?? defaultLogDir, folderName);
				string logFile = Path.Combine(logFolder, logName) + ".log";
				string level = settings.Level.Remove(1).ToUpper() + settings.Level.Substring(1).ToLower();
				var logLevel = (Level)typeof(log4net.Core.Level).GetField(level).GetValue(null);
				var layout = new PatternLayout("%date{ddd, dd MMM yyyy HH:mm:ss.fff %K} (%6thread) [%-6level] %message%newline");
				var appender = Environment.GetRollingFileAppender(logName, logFile, layout, settings, logLevel);
				layout.ActivateOptions();
				appender.ActivateOptions();

				log4NetLogger.Additivity = true;
				log4NetLogger.AddAppender(appender);
				log4NetLogger.Level = logLevel;
			}

			return LogManager.GetLogger(logName);
		}

		internal static LoggerEnvironment Environment = new LoggerEnvironment();
		internal static readonly ConcurrentDictionary<string, ILog> logRepository = new ConcurrentDictionary<string, ILog>();
	}
}
