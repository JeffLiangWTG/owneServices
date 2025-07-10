using System.IO;
using System.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services.IncrementalExport;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE
{
	public class SEMeasureDownloader : ISEMeasureDownloader
	{
		public measure[] DownloadLatestIncrementalAndExtract(string fileRepositoryUrl, string temporaryDownloadPath)
		{
			var url = FindLatestIncrementFile(fileRepositoryUrl);
			if (url == null)
			{
				return null;
			}
			var records = DownloadTraderObjectExport.Download<export>(url, temporaryDownloadPath);
			return records.items.Where(x => x.Item is measure measureItem && measureItem.national == 0L && !string.IsNullOrEmpty(measureItem.goodsNomenclatureCode)).Select(x => x.Item).Cast<measure>().ToArray();
		}

		static string FindLatestIncrementFile(string fileRepositoryUrl)
		{
			if (!string.IsNullOrEmpty(ApplicationConfig.SEDailyFileUrlForTest))
			{
				return ApplicationConfig.SEDailyFileUrlForTest;
			}
			string html;
			using (var client = FileDownloaderHttpClientFactory.CreateHttpClient())
			{
				html = client.GetStringAsync(new Uri(fileRepositoryUrl)).Result;
			}
			var result = string.Empty;
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var dateToCheck = ApplicationConfig.PublishDate;
			var latestIncrementalFileSuffix = "_" + dateToCheck.ToString("yyMMdd", CultureInfo.InvariantCulture) + ".xml.gz.pgp";
			var node = doc.DocumentNode.Descendants().FirstOrDefault(x => !x.GetAttributeValue("href", string.Empty).Contains("DeclarableGoodsNomenclature") && x.GetAttributeValue("href", string.Empty).EndsWith(latestIncrementalFileSuffix, StringComparison.Ordinal));
			result = node?.GetAttributeValue("href", string.Empty);

			if (string.IsNullOrEmpty(result))
			{
				return null;
			}

			return Path.Combine(fileRepositoryUrl, result);
		}
	}
}
