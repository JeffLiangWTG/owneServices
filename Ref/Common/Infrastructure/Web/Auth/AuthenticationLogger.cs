using System;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public class AuthenticationLogger : ILogger
	{
		public AuthenticationLogger(ILogWrapper logWrapper)
		{
			_log = logWrapper.GetLog(nameof(AuthenticationLogger));
		}
		readonly ILog _log;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var message = formatter(state, exception);

			if (logLevel == LogLevel.Information)
			{
				_log.Info(message);
			}
			else if (logLevel == LogLevel.Warning)
			{
				LogWarning(message, exception);
			}
			else if (logLevel >= LogLevel.Error)
			{
				LogError(message, exception);
			}
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			// Previous implementation logged info and above.
			return logLevel >= LogLevel.Information;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return NullDisposable.Instance;
		}

		void LogWarning(string message, Exception exception)
		{
			if (exception != null)
			{
				_log.Warn(message, exception);
			}
			else
			{
				_log.Warn(message);
			}
		}

		void LogError(string message, Exception exception)
		{
			if (exception != null)
			{
				_log.Error(message, exception);
			}
			else
			{
				_log.Error(message);
			}
		}

		class NullDisposable : IDisposable
		{
			public void Dispose()
			{
			}

			// Used when we need an IDisposable for an interface but don't require any actual Dispose() logic.
			public static IDisposable Instance { get; } = new NullDisposable();
		}
	}
}
