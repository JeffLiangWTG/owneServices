using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs
{
	public sealed class DownloadPassarMasterData :  BaseDownload
	{
		public static DownloadResult DownloadAndUnzip(HttpClient client)
		{
			return DownloadAndUnzip(client, ApplicationConfig.Instance.PassarMasterDataUrl);
		}
	}
}
