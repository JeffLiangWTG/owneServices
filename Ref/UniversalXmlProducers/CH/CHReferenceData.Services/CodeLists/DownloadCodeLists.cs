using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists
{
	public sealed class DownloadCodeLists : BaseDownload
	{
		public static DownloadResult DownloadAndUnzip(HttpClient client) => DownloadAndUnzip(client, ApplicationConfig.Instance.CodeListsUrl);
	}
}
