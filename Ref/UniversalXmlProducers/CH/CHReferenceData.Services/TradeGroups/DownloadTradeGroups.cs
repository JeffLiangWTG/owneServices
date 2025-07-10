using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.TradeGroups
{
	public sealed class DownloadTradeGroups : BaseDownload
	{
		public static DownloadResult DownloadAndUnzip(HttpClient client) => DownloadAndUnzip(client, ApplicationConfig.Instance.TradeGroupsUrl);
	}
}
