using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;

public static class Taric4FileDownloaderFactory
{
	public static ITaric4FileDownloader CreateFileDownloader() => new Taric4FileDownloader();
}
