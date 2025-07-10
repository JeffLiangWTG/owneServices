using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists
{
	public static class CodeListsDownloaderTSV
	{
		public static async Task Download(HttpClient client, string downloadUrl, string destinationFilePath)
		{
			using (var response = await RetryHelper.RetryWithDelayAsync(async () => await client.GetAsync(new Uri(downloadUrl))))
			using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
			{
				response.Content.ReadAsStreamAsync().Result.CopyTo(fileStream);
			}
		}

		public static Dictionary<string, string> GetEMCSDownloadLinks(HttpClient httpClient, string downloadPageUrl)
		{
			using (var content = RetryHelper.RetryWithDelayAsync(async () => await httpClient.GetAsync(new Uri(downloadPageUrl))).GetAwaiter().GetResult())
			{
				var site = new HtmlDocument();
				site.LoadHtml(content.Content.ReadAsStringAsync().Result);
				var baseUri = new Uri(site.DocumentNode.SelectSingleNode("//base").GetAttributeValue("href", string.Empty));
				var xpath = "//a[@class='c-link is-download-link']";
				var dynamicCodeListDownloadDetails = site.DocumentNode.SelectNodes(xpath);
				return dynamicCodeListDownloadDetails.ToDictionary(key => key.FirstChild.InnerText.Trim(), value => (new Uri(baseUri, value.GetAttributeValue("href", string.Empty))).ToString());
			}
		}

		public static Dictionary<string, string> GetImportDownloadLinks(HttpClient httpClient, string downloadPageUrl)
		{
			using (var responseMessage = RetryHelper.RetryWithDelayAsync(async () => await httpClient.GetAsync(new Uri(downloadPageUrl))).GetAwaiter().GetResult())
			{
				var content = responseMessage.Content.ReadAsStringAsync().Result;
				var downloadLinks = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
				return downloadLinks.ToDictionary(key => Path.GetFileNameWithoutExtension(key), value => value);
			}
		}
	}
}
