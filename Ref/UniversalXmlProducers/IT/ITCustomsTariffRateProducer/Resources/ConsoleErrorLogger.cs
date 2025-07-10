using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources
{
	public sealed class ConsoleErrorLogger : ILogger
	{
		void ILogger.Log(string logMessage)
		{
			Argument.NotNullOrEmpty(logMessage, nameof(logMessage));

			Console.Error.WriteLine(logMessage);
		}

		void ILogger.Log(Exception exception)
		{
			Argument.NotNull(exception, nameof(exception));

			Console.Error.WriteLine(exception.Message);
		}

		void ILogger.Log(string tariffCode, Exception exception)
		{
			Argument.NotNullOrEmpty(tariffCode, nameof(tariffCode));
			Argument.NotNull(exception, nameof(exception));

			Console.Error.WriteLine(tariffCode);
			Console.Error.WriteLine(exception.Message);
		}

		void ILogger.Log(ParsingResult parsingResult)
		{
			Argument.NotNull(parsingResult, nameof(parsingResult));

			LogIfNotEmpty(nameof(parsingResult.TariffCode), parsingResult.TariffCode);
			LogIfNotEmpty(nameof(parsingResult.ErrorMessage), parsingResult.ErrorMessage);
			LogIfNotEmpty(nameof(parsingResult.StackTrace), parsingResult.StackTrace);
			LogIfNotEmpty(nameof(parsingResult.ErrorDescription), parsingResult.ErrorDescription);

			void LogIfNotEmpty(string name, string value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					Console.Error.WriteLine($"{name}: {value}");
				}
			}
		}
	}
}
