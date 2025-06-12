using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using Common.Logging;
using Common.Logging.Simple;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	[ExcludeFromCodeCoverage]
	public class Logging
	{
		public static ILog CreateLogger(string name, XmlDocument configXml)
		{
			string logDir = TransferrerHelpers.GetValue<string>(configXml, "/Config/LogDir");
			string logLevel = TransferrerHelpers.GetValue<string>(configXml, "/Config/LogLevel");
			int logMaxSize = TransferrerHelpers.GetValue<int>(configXml, "/Config/LogMaxSize");
			int logMaxCount = TransferrerHelpers.GetValue<int>(configXml, "/Config/LogMaxCount");
			return CreateLogger(name, logDir, logMaxSize, logMaxCount, logLevel);
		}

		public static ILog CreateLogger(string name, string folder, int maxSizeMB, int maxCount, string level)
		{
			try
			{
				var logger = log4net.LogManager.GetLogger(name).Logger as Logger;
				foreach (var app in logger.Appenders)
					app.Close();
				logger.RemoveAllAppenders();

				var logLevel = (Level)typeof(Level).GetField(level).GetValue(null);
				var layout = new PatternLayout("%date{ddd, dd MMM yyyy HH:mm:ss %K} (%6thread) [%-6level] %message%newline");
				var appender = new RollingFileAppender()
				{
					Name = name,
					File = String.Format("{0}.log", Path.Combine(folder, name)),
					Layout = layout,
					RollingStyle = RollingFileAppender.RollingMode.Size,
					MaximumFileSize = String.Format("{0}MB", maxSizeMB),
					MaxSizeRollBackups = maxCount,
					Threshold = logLevel,
					Encoding = Encoding.UTF8
				};

				layout.ActivateOptions();
				appender.ActivateOptions();

				logger.Additivity = false;
				logger.AddAppender(appender);
				logger.Level = appender.Threshold;

				return LogManager.GetLogger(name);
			}
			catch (Exception ex)
			{
				var logger = new TraceLoggerFactoryAdapter().GetLogger(typeof(Logging).FullName);
				logger.ErrorFormat("Error creating logger. Name='{0}' Folder='{1}' Level='{2}'", ex, name, folder, level);
				return new NoOpLogger();
			}
		}
	}
}
