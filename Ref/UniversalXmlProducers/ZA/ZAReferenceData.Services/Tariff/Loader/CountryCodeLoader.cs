using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader
{
	public class CountryCodeLoader : ICountryCodeLoader
	{
		readonly ICountryMatcher CountryMatcher;
		readonly ILogger Logger;
		public CountryCodeLoader(ICountryMatcher countryMatcher, ILogger logger)
		{
			CountryMatcher = countryMatcher;
			Logger = logger;
		}

		public Dictionary<string, string> GetCountryData(IEnumerable<string> uniqueCountries)
		{
			var result = new Dictionary<string, string>();
			var countries = uniqueCountries.ToList();

			SetValueAndRemoveIfRequired(result, countries, CountryGroupings.AllCountries, TradeGroups.Standard);
			SetValueAndRemoveIfRequired(result, countries, CountryGroupings.EU, TradeGroups.EU);

			AddRemainingCountries(result, countries);

			return result;
		}

		static void SetValueAndRemoveIfRequired(Dictionary<string, string> dict, List<string> countries, string key, string value)
		{
			if (countries.Contains(key))
			{
				dict.Add(key, value);
				countries.Remove(key);
			}
		}

		void AddRemainingCountries(Dictionary<string, string> dict, List<string> countries)
		{
			if (countries.Any())
			{
				try
				{
					var codes = CountryMatcher.GetCountryCodes(countries.ToArray());

					if (codes.Length == countries.Count)
					{
						for (int i = 0; i < codes.Length; i++)
						{
							dict.Add(countries[i], codes[i]);
						}
					}
				}
#pragma warning disable CA1031
				catch (Exception ex)
				{
					Logger.LogError($"Failed to retrieve countries: {ex.Message}");
				}
#pragma warning restore CA1031
			}
		}
	}
}
