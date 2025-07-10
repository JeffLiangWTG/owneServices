using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE
{
	public interface ISEMeasureDownloader
	{
		measure[] DownloadLatestIncrementalAndExtract(string fileRepositoryUrl, string temporaryDownloadPath);
	}
}
