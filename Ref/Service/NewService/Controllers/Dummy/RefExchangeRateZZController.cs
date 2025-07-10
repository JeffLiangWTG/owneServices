using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefExchangeRateZZController
{
	protected override IEnumerable<Models.RefExchangeRateZZ> GetData(DataBlockKey<Models.RefExchangeRateZZ> key, string version)
	{
		var exchangeRates = base.GetData(key, version);
		if (exchangeRates.Any())
		{
			// Intentionally yield the dummy record first.
			// The client is expected to retrieve this dummy record and immediately throw an exception if rounding issue exists.
			yield return DummyRefExchangeRateZZ();
		}

		foreach (var exchangeRate in exchangeRates)
		{
			yield return exchangeRate;
		}
	}

	static Models.RefExchangeRateZZ DummyRefExchangeRateZZ()
	{
		return new Models.RefExchangeRateZZ
		{
			ZZN_Rate = RoundingIssueConstants.Value,
			ZZN_ExRateType = "XXX",
			ZZN_StartDate = DateTime.UtcNow,
			ZZN_EndDate = DateTime.UtcNow.AddMonths(1),
			ZZN_AsPublished = string.Empty,
			ZZN_RX_NKExCurrency = "USD",
			ZZN_RN_NKCountry = RoundingIssueConstants.Flag
		};
	}
}
