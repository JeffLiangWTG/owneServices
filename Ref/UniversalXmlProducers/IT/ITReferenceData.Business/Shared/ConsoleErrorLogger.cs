using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public sealed class ConsoleErrorLogger : ILogger
	{
		void ILogger.Log(string logMessage)
		{
			Argument.NotNullOrEmpty(logMessage, nameof(logMessage));

			Console.Error.WriteLine(logMessage);
		}

		public void Log(Exception exception, string logMessage)
		{
			Argument.NotNull(exception, nameof(exception));
			Argument.NotNullOrEmpty(logMessage, nameof(logMessage));

			Console.Error.WriteLine(logMessage);
			LogException(exception);
		}

		void ILogger.Log(Exception exception)
		{
			Argument.NotNull(exception, nameof(exception));

			LogException(exception);
		}

		static void LogException(Exception exception)
		{
			Console.Error.WriteLine($"{exception.GetType().Name}: {exception.Message}");
		}
	}
}
