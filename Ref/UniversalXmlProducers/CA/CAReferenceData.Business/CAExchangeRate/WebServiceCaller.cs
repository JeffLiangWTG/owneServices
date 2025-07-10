using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.CAReferenceData.Services;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate
{
	public class WebServiceCaller : IWebServiceCaller
	{
		public WebServiceCaller()
		{
		}

		static string WebServiceUrlBase => ApplicationConfig.ExchangeRateAPI;

		public void QueryAndParseResponse(DateTime nowDate)
		{
			int extractedRecordCount = 0;
			var skip = 0;
			do
			{
				var param = new Hashtable();
				param.Add("order", "ExchangeRateId");
				param.Add("limit", 100);
				param.Add("skip", skip);
				param.Add("startDate", nowDate.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				param.Add("endDate", nowDate.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				var response = WebScraper.GetResponse(WebServiceUrlBase, param);
				CAExchangeRates exchangeRateJson = JsonConvert.DeserializeObject<CAExchangeRates>(response);
				if (exchangeRateJson != null)
				{
					ExchangeRates.AddRange(exchangeRateJson.ForeignExchangeRates);
					extractedRecordCount = exchangeRateJson.ForeignExchangeRates.Count;
				}
				Console.WriteLine($"{extractedRecordCount + skip} exchange rates have been got");
				skip += 100;
			} while (extractedRecordCount == 100);
		}

		public List<ForeignExchangeRates> ExchangeRates
		{
			get
			{
				if (_exchangeRates == null)
				{
					_exchangeRates = new List<ForeignExchangeRates>();
				}
				return _exchangeRates;
			}
		}
		List<ForeignExchangeRates> _exchangeRates;
	}
}
