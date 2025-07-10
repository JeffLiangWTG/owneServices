namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

public interface ITaric4FileDownloader
{
	string DownloadTaric4BaseFile(string destinationDirectoryPath);
}
