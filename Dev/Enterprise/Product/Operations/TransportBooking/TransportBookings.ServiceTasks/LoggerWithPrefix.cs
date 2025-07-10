using System;
using Enterprise.Integration;

namespace Enterprise.TransportBookings.ServiceTasks
{
	class LoggerWithPrefix : ILogger
	{
		public LoggerWithPrefix(ILogger serviceTasklogger)
		{
			Prefix = string.Empty;
			ServiceTasklogger = serviceTasklogger;
		}

		public ILogger ServiceTasklogger { get; }

		public string Prefix { get; set; }

		public void Log(LogType type, string message)
		{
			ServiceTasklogger.Log(type, FormattableString.Invariant($"{GetPrefixAsString()}{message}"));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			ServiceTasklogger.Log(type, FormattableString.Invariant($"{GetPrefixAsString()}{message}"));
		}

		string GetPrefixAsString() => string.IsNullOrEmpty(Prefix) ? string.Empty : FormattableString.Invariant($"[{Prefix}]: ");
	}
}
