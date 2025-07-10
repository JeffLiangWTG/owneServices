using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources
{
	public interface ILogger
	{
		void Log(string logMessage);

		void Log(Exception exception);

		void Log(string tariffCode, Exception exception);

		void Log(ParsingResult parsingResult);
	}
}
