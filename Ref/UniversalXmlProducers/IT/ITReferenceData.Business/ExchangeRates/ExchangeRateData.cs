using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public class ExchangeRateData
	{
		public ExchangeRateData(IList<string> unProcessedData, IList<RefExchangeRateZZ> processedData)
		{
			ProcessedData = processedData;
			UnProcessedData = unProcessedData;
		}

		public IList<RefExchangeRateZZ> ProcessedData { get; private set; }

		public IList<string> UnProcessedData { get; private set; }
	}
}
