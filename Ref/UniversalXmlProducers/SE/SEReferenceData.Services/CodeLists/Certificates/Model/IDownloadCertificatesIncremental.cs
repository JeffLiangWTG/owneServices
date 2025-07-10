namespace CargoWise.RefDbRepo.SEReferenceData.Certificates.Services
{
	public interface IDownloadCertificatesIncremental
	{
		SEReferenceData.Services.certificate[] DownloadLatestIncrementalAndExtract(string url);
	}
}
