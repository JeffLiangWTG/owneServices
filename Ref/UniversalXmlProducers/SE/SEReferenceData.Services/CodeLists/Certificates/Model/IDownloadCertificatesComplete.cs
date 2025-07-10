namespace CargoWise.RefDbRepo.SEReferenceData.Certificates.Services
{
	public interface IDownloadCertificatesComplete
	{
		certificate[] DownloadLatestTotAndExtract(string url);
	}
}
