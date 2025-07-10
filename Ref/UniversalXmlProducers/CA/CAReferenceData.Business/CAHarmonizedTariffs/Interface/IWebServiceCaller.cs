using System;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public interface IWebServiceCaller
	{
		DateTime? GetLatestUpdateOnDate(string queryType);
		bool QueryAndDownloadXmlFiles(string workingFolder, string queryType, string[] orderByColumns, string[] selectColumns, string acceptLanguage = "EN", string filter = "");
	}
}
