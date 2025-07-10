using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces
{
	public interface ICountryCodeLoader
	{
		Dictionary<string, string> GetCountryData(IEnumerable<string> uniqueCountries);
	}
}
