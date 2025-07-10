using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public class Logger : ILoggerWithErrors
	{
		public Logger()
		{
			log = new StringBuilder();
			errorLog = new StringBuilder();
		}
		StringBuilder log { get; }
		StringBuilder errorLog { get; }

		public IDisposable BeginScope<TState>(TState state) => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			log.AppendLine(CultureInfo.InvariantCulture, $"[{logLevel}]: {formatter(state, exception)}");
			if(logLevel == LogLevel.Error)
			{
				errorLog.AppendLine(CultureInfo.InvariantCulture, $"[{logLevel}]: {formatter(state, exception)}");
			}
		}

		public override string ToString() => log.ToString();

		public bool HasErrors => errorLog.Length > 0;

		public string GetErrors() => errorLog.ToString();
	}

	public interface ILoggerWithErrors : ILogger
	{
		bool HasErrors { get; }
		string GetErrors();
	}
}
