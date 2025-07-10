using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class RateCodeDataLookup : BaseDataLoader, IRateCodeDataLookup
	{
		Dictionary<string, RateCodeDataLookupResult> _lookupDictionary;
		Dictionary<string, RateCodeDataLookupResult> _tariffCodeLookupDictionary;

		public RateCodeDataLookup(ILogger logger, IHttpHandler handler) : base(logger, handler)
		{
		}

		public override bool Load(string baseUrl)
		{
			Argument.NotNullOrEmpty(baseUrl, nameof(baseUrl));
			var loadSuccessful = false;
			var requestUrl = baseUrl + GetQueryString();

			var response = Handler.Get(requestUrl);
			if (response == null || !response.IsSuccessStatusCode || response.Content == null)
			{
				Logger.Log(ErrorMessagesConstant.RateCodeDataLookupNoResponseFromDatabase);
				return false;
			}

			var rawData = response.Content.ReadAsStringAsync()?.Result;

			try
			{
				var rateCodeData = JsonConvert.DeserializeObject<RateCodeDataWrapper>(rawData);
				_lookupDictionary = rateCodeData?.Value?
									.ToDictionary(x => x.Description,
									x => new RateCodeDataLookupResult
									{
										Code = x.RateCode,
										RateType = x.RefCusRateType.RateType,
										RateTypeDataGrouping = x.RefCusRateType.RateTypeDataGrouping
									});
			}
			catch (Exception exception)
			{
				Logger.Log(exception);
			}

			LoadTariffCodeLookupDictionary();
			if (_lookupDictionary != null)
			{
				LoadAdditionalMapping();
				loadSuccessful = true;
			}
			else
			{
				Logger.Log(ErrorMessagesConstant.RateCodeDataLookupUnableToLoadFromDatabase);
			}

			return loadSuccessful;
		}

		static string GetQueryString()
		{
			return "RefCusRateCodeUpdate?$filter=RefCusRateType/ZZR_ZZZ_NKDataGrouping%20eq%20%27" + MeasuresConstant.ItalianTradeCode + "%27"
							+ "&$expand=RefCusRateType($select=ZZR_ZZZ_NKDataGrouping,ZZR_RateType)"
							+ "&$select=ZY1_Description,ZY1_RateCode";
		}

		public RateCodeDataLookupResult Lookup(string key, string tariffCode)
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			if (!string.IsNullOrEmpty(tariffCode) && _tariffCodeLookupDictionary != null)
			{
				var result = _tariffCodeLookupDictionary.Keys.Where(x => tariffCode.StartsWith(x, StringComparison.InvariantCulture)).ToList();
				if (result.Count != 0)
				{
					return _tariffCodeLookupDictionary[result[0]];
				}
			}

			return _lookupDictionary.ContainsKey(key) ? _lookupDictionary[key] : null;
		}

		public Dictionary<string, RateCodeDataLookupResult> GetLookupDictionary()
		{
			return _lookupDictionary;
		}

		void LoadAdditionalMapping()
		{
			AddToLookupDictionary("Accise", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Imposta di consumo", new RateCodeDataLookupResult { Code = "125", RateType = "MSC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Conserve", new RateCodeDataLookupResult { Code = "909", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Pelli", new RateCodeDataLookupResult { Code = "910", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Essenze", new RateCodeDataLookupResult { Code = "911", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Combustibili", new RateCodeDataLookupResult { Code = "912", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Carta, Cartoni e Pasta per Carta", new RateCodeDataLookupResult { Code = "913", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Seta", new RateCodeDataLookupResult { Code = "914", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Olii e Grassi", new RateCodeDataLookupResult { Code = "915", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo Stazione Sperimentale Vetro", new RateCodeDataLookupResult { Code = "916", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
			AddToLookupDictionary("Contributo obbligatorio consorzio oli usati", new RateCodeDataLookupResult { Code = "931", RateType = "LEV", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _lookupDictionary);
		}

		void LoadTariffCodeLookupDictionary()
		{
			_tariffCodeLookupDictionary = new Dictionary<string, RateCodeDataLookupResult>();
			AddToLookupDictionary("1302", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2103", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2203", new RateCodeDataLookupResult { Code = "110", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2204", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2205", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2206", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2207", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2208", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2710", new RateCodeDataLookupResult { Code = "933", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("2711", new RateCodeDataLookupResult { Code = "933", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("3302", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
			AddToLookupDictionary("3303", new RateCodeDataLookupResult { Code = "116", RateType = "EXC", RateTypeDataGrouping = MeasuresConstant.ItalianTradeCode }, _tariffCodeLookupDictionary);
		}

		static void AddToLookupDictionary(string key, RateCodeDataLookupResult value, Dictionary<string, RateCodeDataLookupResult> lookupDictionary)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			Argument.NotNull(value, nameof(value));

			if (!lookupDictionary.ContainsKey(key))
			{
				lookupDictionary.Add(key, value);
			}
		}
	}

	public interface IRateCodeDataLookup : IDataLoader
	{
		RateCodeDataLookupResult Lookup(string key, string tariffCode);

		Dictionary<string, RateCodeDataLookupResult> GetLookupDictionary();
	}
}
