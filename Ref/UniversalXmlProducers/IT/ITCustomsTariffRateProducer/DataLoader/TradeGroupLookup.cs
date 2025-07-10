using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class TradeGroupLookup : BaseDataLoader, IDataLookup
	{
		Dictionary<string, string> _lookupDictionary;

		public TradeGroupLookup(ILogger logger, IHttpHandler handler) : base(logger, handler)
		{
		}

		public override bool Load(string baseUrl)
		{
			var loadSuccessful = false;
			var requestUrl = baseUrl + GetQueryString();

			var response = Handler.Get(requestUrl);
			if (response == null || !response.IsSuccessStatusCode || response.Content == null)
			{
				Logger.Log(ErrorMessagesConstant.TradeGroupLookupNoResponseFromDatabase);
				return false;
			}

			var rawData = response.Content.ReadAsStringAsync()?.Result;

			try
			{
				var tradeGroupData = JsonConvert.DeserializeObject<TradeGroupDataWrapper>(rawData);
				_lookupDictionary = tradeGroupData?.Value?.GroupBy(x => x.Description, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Code, StringComparer.OrdinalIgnoreCase);
			}
			catch (Exception exception)
			{
				Logger.Log(exception);
			}

			if (_lookupDictionary != null)
			{
				loadSuccessful = true;
			}
			else
			{
				Logger.Log(ErrorMessagesConstant.TradeGroupLookupUnableToLoadFromDatabase);
			}

			return loadSuccessful;
		}

		static string GetQueryString()
		{
			var today = DateTime.Today;
			var formattedDate = $"{today:s}{today.ToString("zzz", CultureInfo.InvariantCulture).Replace("+", "%2B").Replace("-", "%2D")}";

			return "RefCusTradeGroupUpdate?$filter=ZZA_ZZZ_NKDataGrouping%20eq%20%27" + MeasuresConstant.EuropeanUnionTradeCode + "%27" +
								"%20and%20ZZA_StartDate%20le%20" + formattedDate +
								"%20and%20ZZA_EndDate%20ge%20" + formattedDate;
		}

		public Dictionary<string, string> LookupDictionary => _lookupDictionary;

		public string Lookup(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			if (_lookupDictionary.ContainsKey(key))
			{
				return _lookupDictionary[key];
			}

			return _lookupDictionary.ContainsValue(key) ? key : null;
		}

		public string ReplacementLookup(string tariffCode, string key)
		{
			return null;
		}
	}
}
