namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	public interface ILogHelper
	{
		bool TrySetAppContext(string value, out string message);
		bool TrySetSubSource(string value, out string message);
		void SetProcessId(int value);
		void LogInfo(string message);
		void LogWarn(string message);
		void LogError(string message);
		void LogMonitorInfo(DateTime startTime, double duration, bool status);
		void LogMemoryInfo(double memoryUsage);
		void LogSqlDurationInfo(string id, double duration);
		void LogError(string message, Exception ex);
		void LogFatal(string message, Exception ex);
		void LogInfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
		void LogWarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
		void LogErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
		void LogFatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
	}
}
