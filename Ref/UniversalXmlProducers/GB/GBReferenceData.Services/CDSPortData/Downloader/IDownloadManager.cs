using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader
{
	public interface IDownloadManager
	{
		string GetCSVContent(ICDSPortSource cdsPortSource);
		byte[] GetBinaryData(ICDSPortSource cdsPortSource);
	}
}
