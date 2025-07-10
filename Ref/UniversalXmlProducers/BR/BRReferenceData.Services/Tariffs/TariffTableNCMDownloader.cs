using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffTableNCMDownloader
	{
		const string title = "tabela de ncm e respectiva utrib";

		string BaseUrl = ConfigurationProvider.Configuration.GetSection("URL_BASE_TABELAS_NOTA_FISCAL_ELETRONICA").Value;

		public byte[] DownloadTableNCM(HttpClient client)
		{
			return ReadPageAndGetFile(client);
		}
		byte[] ReadPageAndGetFile(HttpClient client)
		{
			using (var xmlFilePageResponse = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TABELAS_NOTA_FISCAL_ELETRONICA").Value))
			{
				var htmlFileDownloadPage = xmlFilePageResponse?.Result;

				Contract.Assume(htmlFileDownloadPage != null);

				var downloadPage = new HtmlDocument();

				downloadPage.LoadHtml(htmlFileDownloadPage.Content?.ReadAsStringAsync()?.Result);

				Contract.Assume(downloadPage != null);

				var elementDownloadPage = ElementDownloadPage(downloadPage);

				var urlToDownload = BaseUrl + GetCleanUrlToDownload(elementDownloadPage.Attributes["href"].Value);
				var downloadResponse = DoPostDownloadTableNcm(client, urlToDownload);

				Contract.Assume(downloadResponse != null);

				var tabelaNcm = downloadResponse.Content.ReadAsByteArrayAsync()?.Result;

				Contract.Assume(tabelaNcm != null);

				return tabelaNcm;
			}
		}

		static string GetCleanUrlToDownload(string originalUrl)
		{
			return !string.IsNullOrEmpty(originalUrl)
				? originalUrl.Substring(originalUrl.IndexOf("exibirArquivo", StringComparison.Ordinal))
				: string.Empty;
		}

		static HttpResponseMessage DoPostDownloadTableNcm(HttpClient client, string urlToDownload)
		{
			var info = new Dictionary<string, string> { { "href", urlToDownload } };

			using (var postBody = new FormUrlEncodedContent(info))
			{
				var downloadResponse = client.PostAsyncEx(urlToDownload, postBody).Result;

				Contract.Assume(downloadResponse != null);
				Contract.Assume(downloadResponse.IsSuccessStatusCode);

				return downloadResponse;
			}
		}

		static HtmlNode ElementDownloadPage(HtmlDocument document)
		{
			var nodes = document.DocumentNode.SelectNodes("//span").Where(a => a.InnerHtml.ToLower(CultureInfo.CurrentCulture).Contains(title)).ToList();

			nodes.ForEach(n => n.InnerHtml = Regex.Match(n.InnerHtml, @"\d{2}\/\d{2}\/\d{4}").Value);

			var node = nodes.Where(x => !string.IsNullOrWhiteSpace(x.InnerHtml) && DateTime.ParseExact(x.InnerHtml, "dd/MM/yyyy", CultureInfo.InvariantCulture) <= DateTime.Now).OrderByDescending(n => DateTime.ParseExact(n.InnerHtml, "dd/MM/yyyy", CultureInfo.InvariantCulture)).FirstOrDefault();

			Contract.Assume(node != null);

			return node.ParentNode;
		}
	}
}
