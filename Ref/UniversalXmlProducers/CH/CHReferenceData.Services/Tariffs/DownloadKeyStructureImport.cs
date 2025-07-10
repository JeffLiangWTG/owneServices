using System.Net.Http;

namespace CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs
{
	public sealed class DownloadKeyStructureImport :  BaseDownload
	{
		public static DownloadResult Download(HttpClient client)
		{
			return Download(client, ApplicationConfig.Instance.KeyStructureImportUrl);
		}
	}
}
