using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader
{
	public class CountryMatcher : ICountryMatcher
	{
		public string[] GetCountryCodes(string[] countryNames)
		{
			var clientHelper = new HttpClientHelper();
			return clientHelper.GetMatchedEntityCodesAsync(ConfigurationProvider.CountryMatchingURI, countryNames).Result;
		}
	}
}
