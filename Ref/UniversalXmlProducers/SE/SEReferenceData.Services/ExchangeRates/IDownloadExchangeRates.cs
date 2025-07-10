using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Services
{
	public interface IDownloadExchangeRates
	{
		monetaryExchangePeriod[] DownloadLatestIncrementalAndExtract(string url);

		string FindLatestIncrementFile(string fileRepositoryUrl);
	}
}
