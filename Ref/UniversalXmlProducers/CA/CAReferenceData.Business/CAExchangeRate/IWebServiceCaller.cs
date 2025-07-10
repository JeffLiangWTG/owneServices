using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate
{
	public interface IWebServiceCaller
	{
		void QueryAndParseResponse(DateTime nowDate);
		List<ForeignExchangeRates> ExchangeRates { get; }
	}
}
