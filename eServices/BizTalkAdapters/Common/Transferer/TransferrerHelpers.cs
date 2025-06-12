using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using WTG.ErrorReporting;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public static class TransferrerHelpers
	{
		public static void Log(object source, ILog logger, LogLevel level, string messageFormat, params object[] args)
		{
			try
			{
				switch (level)
				{
					case LogLevel.Debug:
						if (logger.IsDebugEnabled)
							logger.Debug(ReformatMessage(source, messageFormat, args));
						break;
					case LogLevel.Info:
						if (logger.IsInfoEnabled)
							logger.Info(ReformatMessage(source, messageFormat, args));
						break;
					case LogLevel.Warn:
						if (logger.IsWarnEnabled)
							logger.Warn(ReformatMessage(source, messageFormat, args));
						break;
					case LogLevel.Error:
						if (logger.IsErrorEnabled)
							logger.Error(ReformatMessage(source, messageFormat, args));
						break;
					case LogLevel.Trace:
						if (logger.IsTraceEnabled)
							logger.Trace(ReformatMessage(source, messageFormat, args));
						break;
					default:
						break;
				}
			}
			catch (Exception) { }
		}

		static string ReformatMessage(object source, string messageFormat, params object[] args)
		{
			var message = new StringBuilder();
			if (source != null)
				message.AppendFormat("({0,-30}) ", source.GetType().Name);
			if (args.Length > 0)
				message.AppendFormat(messageFormat, args);
			else
				message.Append(messageFormat);
			return message.ToString();
		}

		public static void LogException(object source, ILog logger, Exception ex, LogLevel eventLogLevel = LogLevel.Off, string eventLogMessage = null, params object[] args)
		{
			try
			{
				if (logger.IsDebugEnabled)
					foreach (var line in (ex.ToString()).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
						TransferrerHelpers.Log(source, logger, LogLevel.Error, line);
				else
					if (ex is TransferrerException && ex.InnerException != null)
						TransferrerHelpers.Log(source, logger, LogLevel.Error, "{0}: {1} ---> {2}", ex.GetType().FullName, ex.Message, ex.InnerException.Message);
					else
						TransferrerHelpers.Log(source, logger, LogLevel.Error, "{0}: {1}", ex.GetType().FullName, ex.Message);

				if (eventLogLevel != LogLevel.Off)
				{
					var eventLogger = LogManager.GetLogger(source.GetType().FullName);
					if ((eventLogLevel == LogLevel.Warn && eventLogger.IsWarnEnabled) || eventLogger.IsErrorEnabled)
					{
						string message = String.Empty;
						if (!String.IsNullOrWhiteSpace(eventLogMessage))
							message = String.Format(eventLogMessage, args) + Environment.NewLine + ex.ToString();
						else
							message = "Exception raised from '" + source.GetType().FullName + "'." + Environment.NewLine + ex.ToString();
						if (eventLogLevel == LogLevel.Warn)
							eventLogger.Warn(message);
						else
							eventLogger.Error(message);
					}
				}
			}
			catch (Exception) { }
		}

		public static ILog CreateLogger(string name, string folder, int maxSizeMB, int maxCount, Level level)
		{
			try
			{
				var logger = log4net.LogManager.GetLogger(name).Logger as Logger;
				foreach (var app in logger.Appenders)
					app.Close();
				logger.RemoveAllAppenders();

				var layout = new PatternLayout("%date{ddd, dd MMM yyyy HH:mm:ss %K} (%6thread) [%-6level] %message%newline");
				var appender = new RollingFileAppender()
				{
					Name = name,
					File = String.Format("{0}.log", Path.Combine(folder, name)),
					Layout = layout,
					RollingStyle = RollingFileAppender.RollingMode.Size,
					MaximumFileSize = String.Format("{0}MB", maxSizeMB),
					MaxSizeRollBackups = maxCount,
					Threshold = level,
					Encoding = Encoding.UTF8
				};

				layout.ActivateOptions();
				appender.ActivateOptions();

				logger.Additivity = false;
				logger.AddAppender(appender);
				logger.Level = appender.Threshold;

				return LogManager.GetLogger(name);
			}
			catch (Exception)
			{
				return new NoOpLogger();
			}
		}

		public static ILog CreatePortLogger(string name, string folder, string level, int maxSize, int maxCount)
		{
			var prefixMatch = Regex.Match(folder, @"\{prefix,(?<len>[\d])\}");
			if (prefixMatch.Success)
			{
				string prefix = name.Remove(int.Parse(prefixMatch.Groups["len"].Value));
				folder = folder.Remove(prefixMatch.Index, prefixMatch.Length).Insert(prefixMatch.Index, prefix);
			}
			var logLevel = (Level)typeof(log4net.Core.Level).GetField(level).GetValue(null);
			return TransferrerHelpers.CreateLogger(name, folder, maxSize, maxCount, logLevel);
		}

		public static ILog CreateAdapterLogger(object source, string transportType, string hostInstance)
		{
			string logDir = ConfigurationManager.AppSettings["AdapterLoggingDirectory"];
			if (logDir != null)
			{
				string logName = String.Join("_", transportType, source.GetType().Name, hostInstance);
				return TransferrerHelpers.CreateLogger(logName, logDir, 2, 10, Level.Debug);
			}
			else
			{
				return new NoOpLogger();
			}
		}

		public static void ReportToIssueManager(object source, ILog logger,  string subject, string message, TransferrerProperties.Receive properties, string locationUri, Exception ex, CancellationToken cancelToken = default)
		{
			var url = ConfigurationManager.AppSettings["IssueManagerUri"];

			try
			{
				if (string.IsNullOrWhiteSpace(url))
				{
                    Log(source, logger, LogLevel.Warn, "Did not log Issue Manager issue due to un-set URI of appsetting: 'IssueManagerUri'");
					return;
				}
				if (properties == null)
				{
					Log(source, logger, LogLevel.Warn, "Did not log Issue Manager issue because properties are null.");
					return;
				}

				var reportBuilder = new EnterpriseErrorReportBuilder();
				var key = $"{subject} {properties.PortName} {Environment.MachineName} {locationUri}";
				reportBuilder = reportBuilder
					.SetKey(key)
					.SetRandomErrorReportID()
					.SetTimeOfException(DateTime.Now)
					.SetSubject(subject)
					.SetExceptionDescription(message)
					.SetRootException(ex)
					.SetExeCreationTime(DateTime.Now)
					.SetRequestURL(locationUri);

				if (cancelToken.IsCancellationRequested)
					return;

				Uri serviceUri = new Uri(url);
				using (var errorReportingClient = new ErrorReportingClient(serviceUri))
				{
					if (cancelToken.IsCancellationRequested)
						return;

					var task = errorReportingClient.PostCrashReportAsync(reportBuilder);
					task.ConfigureAwait(false);
					if (cancelToken.IsCancellationRequested)
						return;

					var timedOut = !task.Wait(properties.Timeout, cancelToken);

					if (timedOut)
					{
						Log(source, logger, LogLevel.Error, "Timed out when submitting report to the IssueManager using the URI {0}", serviceUri.AbsoluteUri);
						if (task.Exception != null)
							LogException(source, logger, task.Exception, LogLevel.Error, "Timed out when submitting report to the IssueManager using the URI {0} ", serviceUri.AbsoluteUri);
					}
					else if (task.IsFaulted)
					{
						Log(source, logger, LogLevel.Error, "Unable to submit report to the IssueManager using the URI {0}", serviceUri.AbsoluteUri);
						if (task.Exception != null)
							LogException(source, logger, task.Exception, LogLevel.Error, "Unable to submit report to the IssueManager using the URI {0}", serviceUri.AbsoluteUri);
					}

					Log(source, logger, LogLevel.Debug, "Logged Issue Manager issue to URL: " + url);
				}
			}
			catch (Exception e)
			{
				Log(source, logger, LogLevel.Error, "Failed to raise an issue manager issuse. See EventViewer for more information. Subject: {0}, Uri: {1} ", subject, url);
				LogException(source, logger, e, LogLevel.Error, "Failed to raise an issue manager issuse. Subject: {0}, Message: {1}", subject, message);
			}
		}

		public static async Task ReportToIssueManagerAsync(object source, ILog logger, string subject, string message, TransferrerProperties.Receive properties, string locationUri, Exception ex, CancellationToken cancelToken = default)
		{
			var url = ConfigurationManager.AppSettings["IssueManagerUri"];

			try
			{
				if (string.IsNullOrWhiteSpace(url))
				{
					Log(source, logger, LogLevel.Warn, "Did not log Issue Manager issue due to un-set URI of appsetting: 'IssueManagerUri'");
					return;
				}
				if (properties == null)
				{
					Log(source, logger, LogLevel.Warn, "Did not log Issue Manager issue because properties are null.");
					return;
				}

				var reportBuilder = new EnterpriseErrorReportBuilder();
				var key = $"{subject} {properties.PortName} {Environment.MachineName} {locationUri}";
				reportBuilder = reportBuilder
					.SetKey(key)
					.SetRandomErrorReportID()
					.SetTimeOfException(DateTime.Now)
					.SetSubject(subject)
					.SetExceptionDescription(message)
					.SetRootException(ex)
					.SetExeCreationTime(DateTime.Now)
					.SetRequestURL(locationUri);

				if (cancelToken.IsCancellationRequested)
					return;

				Uri serviceUri = new Uri(url);
				using (var errorReportingClient = new ErrorReportingClient(serviceUri))
				{
					if (cancelToken.IsCancellationRequested)
						return;

					await errorReportingClient.PostCrashReportAsync(reportBuilder, cancelToken).ConfigureAwait(false); ;
				}
			}
			catch (Exception e)
			{
				Log(source, logger, LogLevel.Error, "Failed to raise an issue manager issuse. See EventViewer for more information. Subject: {0}, Uri: {1} ", subject, url);
				LogException(source, logger, e, LogLevel.Error, "Failed to raise an issue manager issuse. Subject: {0}, Message: {1}", subject, message);
			}
		}

		public static bool HasValidCharacters(string uriString, string additionalChars = null)
		{
			var validChars = string.IsNullOrWhiteSpace(additionalChars) ? validUriChars : validUriChars + additionalChars;
			return Regex.IsMatch(uriString, @"^([" + validChars + "]|" + escapedChar + ")*$");
		}


		const string validUriChars = @"!'()=\-._~a-zA-Z0-9";
		const string escapedChar = @"%(?=[0-9a-fA-F]{2})";
		public static Random Rng = new Random();
	}
}
