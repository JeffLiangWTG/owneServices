using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class CustomsPublicationDateScraper
	{
		const string DateFormat = "yyyy.M.d";

		public static bool TryGetPublicationDate(IHttpClientHelper httpClientHelper, string pageUrl, string downloadUrl, out DateTime publicationDate)
		{
			publicationDate = default;

			if (TryGetHtmlDocument(httpClientHelper, pageUrl, out var htmlDocument))
			{
				if (TryGetPublicationDateFromHtmlDocument(htmlDocument, AppConfig.Customs.BaseUrl, downloadUrl, out publicationDate))
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

			var xPath = $"//a[contains(@href, \"{downloadUrl}\")]";
			var anchorNode = htmlDocument?.DocumentNode?.SelectNodes(xPath)?.FirstOrDefault();

			if (anchorNode != null)
			{
				var fullText = anchorNode.InnerText;
				var match = Regex.Match(fullText, @"令和(\d+)年(\d+)月(\d+)日");
				if (match.Success)
				{
					var reiwaYear = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
					var month = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
					var day = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
					var gregorianYear = 2018 + reiwaYear;
					if (DateTime.TryParseExact($"{gregorianYear}.{month}.{day}", DateFormat, null, DateTimeStyles.None, out var result))
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
			var html = httpClientHelper.GetWebPageAsync(url, "shift_jis").GetAwaiter().GetResult();
			htmlDocument.LoadHtml(html);
			return true;
		}
	}
}
