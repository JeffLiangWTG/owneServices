using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.SEReferenceData.Services
{
	public class DownloadExportXml
	{
		protected HttpClient Client { get; set; }

		public DownloadExportXml() : this(new HttpClient()) { }
		public DownloadExportXml(HttpClient client)
		{
			Client = client;
		}

		public (TXmlItem[], string) CombinedDownloadFile<TXmlItem>(string filePrefix) where TXmlItem : class
		{
			var filename = DownloadDataHelper.FindLatestTotalFilename(Client, filePrefix, ApplicationConfig.CompleteMonthlyRepositoryUrl);
			var xmlData = DownloadLatestFullAndExtract<TXmlItem>(filename, Path.GetTempPath());

			var modified = DownloadDataHelper.FindModifiedDateFromFilename(filename);
			return (xmlData, modified);
		}

		public TXmlItem[] DownloadLatestFullAndExtract<TXmlItem>(string url, string temporaryDownloadPath) where TXmlItem : class
		{
			var records = DownloadTraderObjectExport.Download<FullExport.export>(Client, url, temporaryDownloadPath);
			return records.items.Where(x => x is TXmlItem).Cast<TXmlItem>().ToArray();
		}

		public virtual string FindLatestFile(string fileRepositoryUrl, string xmlFileObjectName)
		{
			var task = FindLatestFileAsync(fileRepositoryUrl, xmlFileObjectName);
			task.Wait();
			return task.Result;
		}

		public static async Task<string> FindLatestFileAsync(string fileRepositoryUrl, string xmlFileObjectName)
		{
			string html;
			using (var httpClient = new HttpClient())
			{
				var uri = new Uri(fileRepositoryUrl);
				html = await httpClient.GetStringAsync(uri);
			}

			var result = string.Empty;
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var dateToCheck = DateTime.Today;
			var dateLimit = DateTime.Today.AddMonths(-2);
			do
			{
				var latestIncrementalFileSuffix = "_" + dateToCheck.ToString("yyMMdd", CultureInfo.InvariantCulture) + ".xml.gz.pgp";
				var node = doc.DocumentNode.Descendants()
					.FirstOrDefault(x =>
					{
						var href = x.GetAttributeValue("href", string.Empty);
						return href.StartsWith(xmlFileObjectName, StringComparison.InvariantCulture) && href.EndsWith(latestIncrementalFileSuffix, StringComparison.InvariantCulture);
					});
				result = node?.GetAttributeValue("href", string.Empty);
				dateToCheck = dateToCheck.AddDays(-1);
			}
			while (string.IsNullOrEmpty(result) && dateToCheck > dateLimit);
			return result;
		}
	}
}
