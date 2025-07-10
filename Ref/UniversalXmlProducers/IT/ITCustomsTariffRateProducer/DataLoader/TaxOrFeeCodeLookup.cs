using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class TaxOrFeeCodeLookup : BaseDataLoader, IDataLookup
	{
		Dictionary<string, string> _lookupDictionary;
		Dictionary<Tuple<string, string>, string> _replacementLookup;

		public TaxOrFeeCodeLookup(ILogger logger, IHttpHandler handler) : base(logger, handler)
		{
		}

		public override bool Load(string baseUrl)
		{
			var loadSuccessful = false;
			var requestUrl = baseUrl + GetQueryString();

			var response = Handler.Get(requestUrl);
			if (response == null || !response.IsSuccessStatusCode || response.Content == null)
			{
				Logger.Log(ErrorMessagesConstant.TaxOrFeeCodeLookupNoResponseFromDatabase);
				return false;
			}

			var rawData = response.Content.ReadAsStringAsync()?.Result;

			try
			{
				var taxOrFeeCodeData = JsonConvert.DeserializeObject<TaxOrFeeCodeDataWrapper>(rawData);
				_lookupDictionary = taxOrFeeCodeData?.Value?.ToDictionary(x => (x.Value * 100).ToString(CultureInfo.InvariantCulture), x => x.Code);
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
				Logger.Log(ErrorMessagesConstant.TaxOrFeeCodeLookupUnableToLoadFromDatabase);
			}

			LoadReplacementLookup();

			return loadSuccessful;
		}

		static string GetQueryString()
		{
			var today = DateTime.Today;
			var formattedDate = $"{today:s}{today.ToString("zzz", CultureInfo.InvariantCulture).Replace("+", "%2B").Replace("-", "%2D")}";

			return "RefCusTaxOrFeeUpdate?$filter=ZZF_ZZZ_NKDataGrouping%20eq%20'IT'" +
								"%20and%20ZZF_StartDate%20le%20" + formattedDate +
								"%20and%20ZZF_EndDate%20ge%20" + formattedDate;
		}

		void LoadReplacementLookup()
		{
			var item1 = Tuple.Create("1211908620", "5");
			var item2 = Tuple.Create("1211908690", "5");
			_replacementLookup = new Dictionary<Tuple<string, string>, string>
			{
				{ item1, "ESE" },
				{ item2, "ESE" }
			};
		}

		public Dictionary<string, string> LookupDictionary => _lookupDictionary;

		public string Lookup(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			return _lookupDictionary.ContainsKey(key) ? _lookupDictionary[key] : null;
		}

		public string ReplacementLookup(string tariffCode, string key)
		{
			if (string.IsNullOrEmpty(tariffCode) || string.IsNullOrEmpty(key))
			{
				return null;
			}

			var compositeKey = Tuple.Create(tariffCode, key);
			return _replacementLookup.ContainsKey(compositeKey) ? _replacementLookup[compositeKey] : null;
		}
	}
}
