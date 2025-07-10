using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class ParserHelper
	{
		public static string DateFormat => "yyyy.M.d";

		public static string[] SplitComma(string row)
		{
			string[] columns;
			using (var parser = new TextFieldParser(new StringReader(row)))
			{
				parser.HasFieldsEnclosedInQuotes = true;
				parser.SetDelimiters(",");
				columns = parser.ReadFields();
			}
			return columns;
		}

		public static bool GetPublicationDateByDownloadUrl(IHttpClientHelper httpClientHelper, string downloadUrl, out DateTime publicationDate)
		{
			var baseUrl = AppConfig.NACCS.BaseUrl;
			if (NaccsPublicationDateScraper.TryGetHtmlDocument(httpClientHelper, AppConfig.NACCS.CodeLists.CarrierBaseUrl, out var htmlDocument))
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
					var dateAsString = htmlDocument.DocumentNode.SelectNodes(tr.XPath + "/td[3]").FirstOrDefault()?.InnerHtml;
					dateAsString = dateAsString.Split("\r\n").FirstOrDefault().Replace("<br>", "");

					if (!string.IsNullOrEmpty(dateAsString))
					{
						if (DateTime.TryParseExact(dateAsString, DateFormat, null, System.Globalization.DateTimeStyles.None, out var result))
						{
							publicationDate = result;
							return true;
						}
					}
				}
			}
			throw new InvalidOperationException("Publication date is not retrieved");
		}
	}
}
