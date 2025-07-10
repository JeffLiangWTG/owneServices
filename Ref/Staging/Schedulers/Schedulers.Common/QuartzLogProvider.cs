using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using Quartz.Logging;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class QuartzLogProvider : ILogProvider
	{
		public QuartzLogProvider(ILogHelper logHelper)
		{
			Argument.NotNull(logHelper, nameof(logHelper));
			_logHelper = logHelper;
			_logger = new Logger(WriteLog);
		}

		public bool WriteLog(LogLevel level, Func<string> func, Exception exception, params object[] parameters)
		{
			if (func != null)
			{
				switch (level)
				{
					case LogLevel.Info:
						{
							_logHelper.LogInfoFormat(CultureInfo.InvariantCulture, func(), exception, parameters);
							break;
						}
					case LogLevel.Warn:
						{
							_logHelper.LogWarnFormat(CultureInfo.InvariantCulture, func(), exception, parameters);
							break;
						}
					case LogLevel.Error:
						{
							_logHelper.LogErrorFormat(CultureInfo.InvariantCulture, func(), exception, parameters);
							break;
						}
					case LogLevel.Fatal:
						{
							_logHelper.LogFatalFormat(CultureInfo.InvariantCulture, func(), exception, parameters);
							break;
						}
					default:
						break;
				}
			}
			return true;
		}

		public Logger GetLogger(string name)
		{
			Argument.NotNull(_logger, nameof(_logger));
			return _logger;
		}

		public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
		{
			throw new NotImplementedException();
		}

		public IDisposable OpenNestedContext(string message)
		{
			throw new NotImplementedException();
		}

		readonly ILogHelper _logHelper;
		readonly Logger _logger;
	}
}
