using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.Business.Shared;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public class AdditionalCodeLookup : BaseDataLoader, IDataLookup
	{
		public AdditionalCodeLookup(ILogger logger, HttpClient httpClient) : base(logger, httpClient)
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
				Logger.Log(ex, Constants.ErrorMessages.AdditionalCodeLookupNoResponseFromDatabase);
				return false;
			}

			try
			{
				var additionalCodeData = JsonConvert.DeserializeObject<AdditionalCodeDataWrapper>(rawData);
				lookupDictionary = additionalCodeData?.Value?.GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Description, StringComparer.OrdinalIgnoreCase);
			}
			catch (Exception exception)
			{
				Logger.Log(exception);
			}

			if (lookupDictionary != null)
			{
				loadSuccessful = true;
			}
			else
			{
				Logger.Log(Constants.ErrorMessages.AdditionalCodeLookupUnableToLoadFromDatabase);
			}

			return loadSuccessful;
		}

		static string GetQueryString()
		{
			return "RefCusCodeListUpdate?$filter=ZZD_ZZK_NKCodeType%20eq%20%27" + Constants.Measures.AdditionalCodeType + "%27" +
							"%20and%20ZZD_ZZZ_NKDataGrouping%20eq%20%27" + Constants.RefDataGroupings.Italy + "%27";
		}

		public Dictionary<string, string> LookupDictionary => lookupDictionary;

		public string Lookup(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			if (lookupDictionary.TryGetValue(key, out var value))
			{
				return value;
			}

			return null;
		}

		public string ReplacementLookup(string tariffCode, string key)
		{
			return null;
		}

		Dictionary<string, string> lookupDictionary;
	}
}
