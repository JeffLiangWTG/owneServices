using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public interface IDownloadEntity
	{
		DateTime PublishDate { get; }
		string DownloadLink { get; }
	}
}
