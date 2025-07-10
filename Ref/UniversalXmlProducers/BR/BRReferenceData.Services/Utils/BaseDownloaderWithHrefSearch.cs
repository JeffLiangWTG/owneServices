using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public abstract class BaseDownloaderWithHrefSearch
	{
		protected abstract string Url { get; }

		protected abstract string InnerHtml { get; }

		public virtual byte[] Download(HttpClient client)
		{
			using (var getAsync = client.GetAsync(new Uri(Url)))
			{
				var httpResponseMessage = getAsync?.Result;
				Contract.Assume(httpResponseMessage != null);

				var href = GetHrefValue(httpResponseMessage.Content.ReadAsStringAsync()?.Result, InnerHtml);

				var bXml = DownloadFile(href, client);

				Contract.Assume(bXml != null);

				return bXml;
			}
		}

		protected static string GetHrefValue(string html, string innerHtml)
		{
			var page = new HtmlDocument();
			page.LoadHtml(html);
			var hrefNode = page.DocumentNode?.SelectNodes("//a[@href]")?.Where(x => x.InnerHtml.Contains(innerHtml))?.FirstOrDefault();
			Contract.Assume(hrefNode != null);
			var hrefValue = hrefNode.GetAttributeValue("href", string.Empty);
			return hrefValue;
		}

		protected virtual byte[] DownloadFile(string href, HttpClient client)
		{
			byte[] result = null;
			using (var response = client.GetAsync(new Uri(href))?.Result)
			{
				Contract.Assume(response != null);
				if (response.IsSuccessStatusCode)
				{
					result = response.Content.ReadAsByteArrayAsync()?.Result;
				}
			}
			return result;
		}
	}
}
