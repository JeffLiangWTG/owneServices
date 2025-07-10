namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IDailyPackageDownloader
	{
		void DownloadAndExtract(string url, string destinationPath, string extractionFolderPath, bool deletePackage);
	}
}
