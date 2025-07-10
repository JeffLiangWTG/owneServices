using System;
using Common.Logging;
using Common.Logging.Serilog;
using Serilog;
using Serilog.Core;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class LogWrapper : ILogWrapper, IDisposable
	{
		public LogWrapper(string logFileName)
		{
			var settings = SerilogConfigProvider.GetSerilogSettings(logFileName);
			logger = new LoggerConfiguration().ReadFrom?.KeyValuePairs(settings)?.CreateLogger();
			serilogFactoryAdapter = new SerilogFactoryAdapter(logger);
		}

		public ILog GetLog<T>()
		{
			var result = GetLog(typeof(T));
			return result;
		}

		public ILog GetLog(Type type)
		{
			var result = serilogFactoryAdapter.GetLogger(type);
			return result;
		}

		public ILog GetLog(string key)
		{
			var result = serilogFactoryAdapter.GetLogger(key);
			return result;
		}

		public void Dispose()
		{
			if (serilogFactoryAdapter != null)
			{
				serilogFactoryAdapter = null;
			}
			logger?.Dispose();
		}

		Logger logger;
		SerilogFactoryAdapter serilogFactoryAdapter;
	}
}
