using System;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class NaccsPublicationDateScraper
	{
		const string DateFormat = "yyyy.M.d";

		public static bool TryGetPublicationDate(IHttpClientHelper httpClientHelper, string pageUrl, string downloadUrl, out DateTime publicationDate)
		{
			publicationDate = default;

			if (TryGetHtmlDocument(httpClientHelper, pageUrl, out var htmlDocument))
			{
				if (TryGetPublicationDateFromHtmlDocument(htmlDocument, AppConfig.NACCS.BaseUrl, downloadUrl, out publicationDate))
				{
					return true;
				}
			}
			throw new InvalidOperationException("Publication date is not retrieved");
		}

		static bool TryGetPublicationDateFromHtmlDocument(HtmlDocument htmlDocument, string baseUrl, string downloadUrl, out DateTime publicationDate)
		{
			publicationDate = default;

			if (downloadUrl.Contains(baseUrl))
			{
				downloadUrl = downloadUrl.Replace(baseUrl, "");
			}

			var xPath = $"//tr[td[a[@href='{downloadUrl}']]]";
			var tr = htmlDocument?.DocumentNode?.SelectNodes(xPath)?.FirstOrDefault();

			if (tr != null)
			{
				var dateAsString = htmlDocument.DocumentNode.SelectNodes(tr.XPath + "/td[4]").FirstOrDefault()?.InnerHtml;

				if (!string.IsNullOrEmpty(dateAsString))
				{
					if (DateTime.TryParseExact(dateAsString, DateFormat, null, System.Globalization.DateTimeStyles.None, out var result))
					{
						publicationDate = result;
						return true;
					}
				}
			}

			throw new InvalidOperationException($"Failed to find a tr element that matches //tr[td[a[@href='{downloadUrl}']]]");
		}

		public static bool TryGetHtmlDocument(IHttpClientHelper httpClientHelper, string url, out HtmlDocument htmlDocument)
		{
			htmlDocument = new HtmlDocument();
			var html = httpClientHelper.GetWebPageAsync(url).GetAwaiter().GetResult();
			htmlDocument.LoadHtml(html);
			return true;
		}
	}
}
