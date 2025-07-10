using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public sealed class Logger : ILogger
	{
		public Logger(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		public void Log(LogType logType, string message, object value = null)
		{
			var logMessage = string.Format(CultureInfo.InvariantCulture, "{0} [{1}]: {2}: {3}", dateTimeProvider.GetIndiaTime(), logType, message, value);
			if (logType == LogType.ReviewRequired)
			{
				Console.Error.WriteLine(logMessage);
			}
			else
			{
				Console.WriteLine(logMessage);
			}
		}

		readonly IDateTimeProvider dateTimeProvider;
	}
}
