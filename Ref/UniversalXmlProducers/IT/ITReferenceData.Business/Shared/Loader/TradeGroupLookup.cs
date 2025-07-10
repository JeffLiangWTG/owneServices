using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.Business.Shared;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public class TradeGroupLookup : BaseDataLoader, IDataLookup
	{
		Dictionary<string, string> _lookupDictionary;

		public TradeGroupLookup(ILogger logger, HttpClient httpClient) : base(logger, httpClient)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public override async Task<bool> LoadAsync(Uri baseUri)
		{
			var loadSuccessful = false;
			var requestUri = Utils.BuildUri(baseUri, GetQueryString());

			string rawData = null;

			try
			{

				using var response = await HttpClient.GetWithRetryAsync(requestUri);
				rawData = await response.Content.ReadAsStringAsync();
			}
			catch (HttpRequestException ex)
			{
				Logger.Log(ex, Constants.ErrorMessages.TradeGroupLookupNoResponseFromDatabase);
				return false;
			}

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
				Logger.Log(Constants.ErrorMessages.TradeGroupLookupUnableToLoadFromDatabase);
			}

			return loadSuccessful;
		}

		static string GetQueryString()
		{
			var today = DateTime.Today;
			var formattedDate = $"{today:s}{today.ToString("zzz", CultureInfo.InvariantCulture).Replace("+", "%2B").Replace("-", "%2D")}";

			return "RefCusTradeGroupUpdate?$filter=ZZA_ZZZ_NKDataGrouping%20eq%20%27" + Constants.Measures.EuropeanUnionTradeCode + "%27" +
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

			if (_lookupDictionary.TryGetValue(key, out var value))
			{
				return value;
			}

			return _lookupDictionary.ContainsValue(key) ? key : null;
		}

		public string ReplacementLookup(string tariffCode, string key)
		{
			return null;
		}
	}
}
