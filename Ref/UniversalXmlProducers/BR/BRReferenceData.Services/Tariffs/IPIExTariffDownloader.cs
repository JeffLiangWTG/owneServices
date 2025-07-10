using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class IPIExTariffDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			using (var clientResponse = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TIPI_DOCS").Value))
			{
				var response = clientResponse?.Result;

				Contract.Assume(response != null);

				var html = new HtmlDocument();
				html.Load(response.Content.ReadAsStreamAsync()?.Result);

				string downloadUrl = html.DocumentNode.SelectSingleNode("//*[@id='content-core']").ChildNodes["p"].ChildNodes["a"].GetAttributeValue("href", "");
				var downloadResponse = DoPostDownloadXlsx(client, downloadUrl);

				Contract.Assume(downloadResponse != null);

				var bXml = downloadResponse.Content.ReadAsByteArrayAsync()?.Result;

				Contract.Assume(bXml != null);

				return bXml;
			}
		}

		public static string DownloadAttributesHtml(HttpClient client)
		{
			using (var httpResponseMessage = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TIPI_DOCS").Value)?.Result)
			{
				Contract.Assume(httpResponseMessage != null);

				return httpResponseMessage.IsSuccessStatusCode ? httpResponseMessage.Content.ReadAsStringAsync()?.Result : null;
			}
		}

		static HttpResponseMessage DoPostDownloadXlsx(HttpClient client, string urlToDownload)
		{
			var info = new Dictionary<string, string> { { "href", urlToDownload } };

			using (var postBody = new FormUrlEncodedContent(info))
			{

				var downloadResponse = client.PostAsyncEx(urlToDownload, postBody).Result;

				Contract.Assume(downloadResponse != null);

				return downloadResponse;
			}
		}
	}
}
