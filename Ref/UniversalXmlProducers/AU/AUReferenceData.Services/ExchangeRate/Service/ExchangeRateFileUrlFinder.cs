using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public static class ExchangeRateFileUrlFinder
	{
		public static string Find(IHttpClientHelper clientHelper, string webPageUrl, string fileNamePrefix)
		{
			var html = clientHelper.GetWebPageAsync(webPageUrl).Result;
			if (string.IsNullOrEmpty(html))
			{
				throw new HttpRequestException($"Could not retrieve any content.");
			}

			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var node = doc.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("href", string.Empty).StartsWith(fileNamePrefix,StringComparison.InvariantCulture));
			var href = node?.GetAttributeValue("href", string.Empty);
			if (string.IsNullOrEmpty(href))
			{
				throw new InvalidOperationException($"No link to {fileNamePrefix} defined on node {node?.OuterHtml}.");
			}

			href = Path.Combine(webPageUrl, href);
			return href;
		}
	}
}
