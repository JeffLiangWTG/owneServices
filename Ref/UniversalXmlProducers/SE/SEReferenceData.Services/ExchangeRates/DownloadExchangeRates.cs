using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services.IncrementalExport;

namespace CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Services
{
	public class DownloadExchangeRates : DownloadExportXml, IDownloadExchangeRates
	{
		public DownloadExchangeRates() : base() { }
		public DownloadExchangeRates(HttpClient client) : base(client) { }

		public monetaryExchangePeriod[] DownloadLatestIncrementalAndExtract(string url)
		{
			var records = DownloadTraderObjectExport.Download<export>(Client, url);
			return records.items.Where(x => x.Item is monetaryExchangePeriod period && period.national == 1L).Select(x => x.Item).Cast<monetaryExchangePeriod>().ToArray();
		}

		public string FindLatestIncrementFile(string fileRepositoryUrl) => FindLatestFile(fileRepositoryUrl, string.Empty);

		static async public Task<string> FindLatestIncrementFileAsync(string fileRepositoryUrl) => await FindLatestFileAsync(fileRepositoryUrl, string.Empty);
	}
}
