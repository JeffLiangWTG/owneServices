namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public interface IFileDownloader
	{
		void DownloadFile(string address, string fileName);
	}
}
