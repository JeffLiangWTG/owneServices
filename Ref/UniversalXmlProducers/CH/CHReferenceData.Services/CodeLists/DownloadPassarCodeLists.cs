using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists
{
	public sealed class DownloadPassarCodeLists : BaseDownload
	{
		public static DownloadResult DownloadAndUnzip(HttpClient client)
		{
			return DownloadAndUnzip(client, ApplicationConfig.Instance.PassarCodeListsUrl);
		}
	}
}
