using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.CustomsOffices
{
	public sealed class DownloadCustomsOffices : BaseDownload
	{
		public static DownloadResult DownloadAndUnzip(HttpClient client) => DownloadAndUnzip(client, ApplicationConfig.Instance.CustomsOfficesUrl);
	}
}
