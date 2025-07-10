using System.Collections.Generic;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public interface IExchangeRates
	{
		string StartDate { get; }
		string EndDate { get; }
		IEnumerable<IExchangeRateDetails> ExchangeRateDetails { get; }
	}
}
