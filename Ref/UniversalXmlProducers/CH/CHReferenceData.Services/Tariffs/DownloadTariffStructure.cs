using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs
{
	public sealed class DownloadTariffStructure :  BaseDownload
	{
		public static DownloadResult Download(HttpClient client)
		{
			return Download(client, ApplicationConfig.Instance.TariffStructureUrl);
		}
	}
}
