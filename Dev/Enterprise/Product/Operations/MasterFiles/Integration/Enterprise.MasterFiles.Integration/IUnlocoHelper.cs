namespace Enterprise.MasterFiles.Integration
{
	public interface IUnlocoHelper
	{
		bool IsUnloco(string unloco);
		string GetCityCountry(string unloco);
	}
}