using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	internal class DownloadEntity : IDownloadEntity
	{
		public DownloadEntity(DateTime publishDate, string downloadLink)
		{
			PublishDate = publishDate;
			DownloadLink = downloadLink;
		}

		public DateTime PublishDate { get; }
		public string DownloadLink { get; }
	}
}
