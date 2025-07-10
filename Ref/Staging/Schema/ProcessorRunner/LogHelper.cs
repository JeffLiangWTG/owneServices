using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	public class LogHelper : ILogHelper
	{
		public LogHelper(ILogWrapper logWrapper, string key)
		{
			Argument.NotNull(logWrapper, nameof(logWrapper));
			Argument.NotNull(key, nameof(key));
			log = logWrapper.GetLog(key);
			sourceContext = key;
		}

		public bool TrySetAppContext(string value, out string message)
		{
			if (string.IsNullOrEmpty(value))
			{
				message = $"AppContext cannot be set to NullOrEmpty explicitly in this application [{sourceContext}].";
				return false;
			}
			if (!string.IsNullOrEmpty(appContext))
			{
				message = $"AppContext is already set for this application [{sourceContext}].";
				return false;
			}

			appContext = value;
			message = string.Empty;
			return true;
		}

		public bool TrySetSubSource(string value, out string message)
		{
			if (string.IsNullOrEmpty(value))
			{
				message = $"SubSource cannot be set to NullOrEmpty explicitly in this application [{sourceContext}].";
				return false;
			}
			if (!string.IsNullOrEmpty(subSource))
			{
				message = $"SubSource is already set for this application [{sourceContext}].";
				return false;
			}

			subSource = value;
			message = string.Empty;
			return true;
		}

		public void SetProcessId(int value) => processId = value;

		public void LogInfo(string message) => log.Info(NewAppLog(message));
		public void LogWarn(string message) => log.Warn(NewAppLog(message));
		public void LogError(string message) => log.Error(NewAppLog(message));
		public void LogMonitorInfo(DateTime startTime, double duration, bool status) => log.Info(NewAppMonitorLog(startTime, duration, status));
		public void LogMemoryInfo(double memoryUsage) => log.Info(NewAppMemoryUsageLog(memoryUsage));
		public void LogSqlDurationInfo(string id, double duration) => log.Info(NewSqlDurationLog(id, duration));
		public void LogError(string message, Exception ex) => log.Error(NewAppLog(message), ex);
		public void LogFatal(string message, Exception ex) => log.Fatal(NewAppLog(message), ex);
		public void LogInfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
			=> log.InfoFormat(formatProvider, format, exception, args);
		public void LogWarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
			=> log.WarnFormat(formatProvider, format, exception, args);
		public void LogErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
			=> log.ErrorFormat(formatProvider, format, exception, args);
		public void LogFatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
			=> log.FatalFormat(formatProvider, format, exception, args);

		AppLog NewAppLog(string message) => new() { ProcessId = processId, AppContext = appContext, SubSource = subSource, Message = message };
		AppMonitorLog NewAppMonitorLog(DateTime startTime, double duration, bool status)
			=> new() { ProcessId = processId, AppContext = appContext, SubSource = subSource, StartTime = startTime, Duration = duration, Status = status ? "success" : "fail", Message = "Monitoring information." };
		AppMemoryUsageLog NewAppMemoryUsageLog(double memoryUsage) => new() { ProcessId = processId, MemoryUsage = memoryUsage };
		SqlDurationLog NewSqlDurationLog(string id, double duration) => new() { ProcessId = processId, Id = id, Duration = duration };

		readonly string sourceContext;
		readonly ILog log;
		int processId;
		string appContext;
		string subSource;
	}
}
