using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public class ExchangeRates
{
	public ExchangeRates(DateTime startDate, DateTime endDate, List<ExchangeRate> exchangeRateDetails)
	{
		StartDate = startDate;
		EndDate = endDate;
		ExchangeRateDetails = exchangeRateDetails;
	}

	public DateTime StartDate { get; private set; }

	public DateTime EndDate { get; private set; }

	public IEnumerable<ExchangeRate> ExchangeRateDetails { get; private set; }
}
