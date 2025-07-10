using System;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public interface IFileDownloader
	{
		(DateTime, string) GetTradeGroupEffectiveDateAndUrl(string html = null, string detailHtml = null);

		(DateTime, string) GetLastEditDateAndAccessDbUrl(string html = null);

		(DateTime, string) GetConditionExcelModifiedDateAndUrl(string html = null, string detailHtml = null);

		bool DownloadFile(string fileURL, string fileName);
	}
}
