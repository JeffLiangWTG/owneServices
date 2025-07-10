using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate
{
	public interface ICAExchangeRateParser
	{
		bool ParseExchangeRateIntoXML(List<ForeignExchangeRates> exchangeRates, string exportFilePath, DateTime latestUpdateOnDate);
	}
}
