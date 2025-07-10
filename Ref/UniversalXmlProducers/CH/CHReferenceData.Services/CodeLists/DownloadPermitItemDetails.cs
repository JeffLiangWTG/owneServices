using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists
{
	public sealed class DownloadPermitItemDetails : BaseDownload
	{
		public static DownloadResult DownloadAndUnzip(HttpClient client)
		{
			return DownloadAndUnzip(client, ApplicationConfig.Instance.PermitItemDetailsUrl);
		}
	}
}
