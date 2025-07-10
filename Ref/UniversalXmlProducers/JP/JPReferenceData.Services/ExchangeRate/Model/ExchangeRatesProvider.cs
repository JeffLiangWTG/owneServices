using System.Collections.Generic;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExchangeRatesProvider : IExchangeRates
	{
		public ExchangeRatesProvider(string startDate, string endDate, List<IExchangeRateDetails> exchangeRateDetails)
		{
			StartDate = startDate;
			EndDate = endDate;
			ExchangeRateDetails = exchangeRateDetails;
		}

		public string StartDate { get; private set; }

		public string EndDate { get; private set; }

		public IEnumerable<IExchangeRateDetails> ExchangeRateDetails { get; private set; }
	}
}
