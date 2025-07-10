using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.ExchangeRates
{
	public sealed class DownloadExchangeRates : BaseDownload
	{
		public static DownloadResult Download(HttpClient client) => Download(client, ApplicationConfig.Instance.ExchangeRatesUrl);
	}
}
