namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public interface IDownloadManager
	{
		void RunDownloadProcess(string contentFolder, System.DateTime downloadDate);
	}
}
