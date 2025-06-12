using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.WiseCloudRefFileValidator
{
	public class StringLogger : ILogger
	{
		const char errorMarker = '\u0014';
		const char warnMarker = '\u0013';
		const char infoMarker = '\u0012';

		StringBuilder builder = new();

		public string Logged => builder.ToString();

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			char severityMarker;
			switch (logLevel)
			{
				case LogLevel.Information:
					severityMarker = infoMarker;
					break;
				case LogLevel.Warning:
					severityMarker = warnMarker;
					break;
				case LogLevel.Error:
					severityMarker = errorMarker;
					break;
				default:
					severityMarker = infoMarker;
					break;
			}

			builder.Append(severityMarker).Append($"{state}{(exception == null ? string.Empty : $" {exception}")}").Append("\u0011");
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			throw new NotImplementedException();
		}

		public IDisposable BeginScope<TState>(TState state) where TState : notnull
		{
			throw new NotImplementedException();
		}
	}
}
