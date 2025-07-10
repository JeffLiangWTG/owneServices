namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces
{
	public interface ICountryMatcher
	{
		string[] GetCountryCodes(string[] countryNames);
	}
}
