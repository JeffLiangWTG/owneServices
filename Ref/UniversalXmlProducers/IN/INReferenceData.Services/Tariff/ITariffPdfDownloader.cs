using System;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public interface ITariffPdfDownloader
	{
		string DownloadPdfFiles(DateTime date);
		DateTime? GetLatestTariffDate(DateTime? lastProcessedDate);
	}
}