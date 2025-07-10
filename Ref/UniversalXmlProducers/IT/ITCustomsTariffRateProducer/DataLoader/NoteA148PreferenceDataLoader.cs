using System;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public sealed class NoteA148PreferenceDataLoader : BaseDataLoader, IPreferenceDataLookup
	{
		public NoteA148PreferenceDataLoader(ILogger logger, IHttpHandler handler) : base(logger, handler)
		{
		}

		public override bool Load(string baseUrl)
		{
			var requestUrl = baseUrl + QueryString;

			var response = Handler.Get(requestUrl);
			if (response == null || !response.IsSuccessStatusCode || response.Content == null)
			{
				Logger.Log(ErrorMessagesConstant.PreferenceDataLookupNoResponseFromDatabase);
				return false;
			}

			var rawData = response.Content.ReadAsStringAsync()?.Result;

			try
			{
				var preferenceData = JsonConvert.DeserializeObject<PreferenceDataWrapper>(rawData);
				_preferences = preferenceData?.Value;
			}
			catch (Exception exception)
			{
				Logger.Log(exception);
				return false;
			}

			if (_preferences is null || _preferences.Length == 0)
			{
				Logger.Log(ErrorMessagesConstant.PreferenceDataLookupUnableToLoadFromDatabase);
				return false;
			}

			return true;
		}

		static string QueryString => "RefCusPreferenceUpdate?" +
			"$select=ZZS_Preference,ZZS_ZZZ_NKDataGrouping&" +
			"$filter=ZZS_ZZZ_NKDataGrouping%20eq%20%27" + MeasuresConstant.EuropeanUnionTradeCode + "%27%20" +
			"and%20not(startswith(ZZS_Preference,%27" + PreferenceCode3 + "%27))%20" +
			"and%20not(startswith(ZZS_Preference,%27" + PreferenceCode4 + "%27))&" +
			"$orderby=ZZS_Preference";

		PreferenceData[] IPreferenceDataLookup.Preferences => _preferences;

		PreferenceData[] _preferences;

		const string PreferenceCode3 = "3";
		const string PreferenceCode4 = "4";
	}
}
