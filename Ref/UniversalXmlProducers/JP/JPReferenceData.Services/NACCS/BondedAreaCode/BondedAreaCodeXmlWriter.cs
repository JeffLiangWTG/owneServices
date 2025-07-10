using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class BondedAreaCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Bonded Location Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_BondedAreaCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.BondedAreaCodeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new BondedAreaCodeParser();

		protected override bool GetPublicationDate(IHttpClientHelper httpClientHelper, out DateTime publicationDate)
		{
			var htmlDocument = new HtmlDocument();

			var url = AppConfig.NACCS.CodeLists.BondedAreaCodeHomePageUrl;
			var html = httpClientHelper.GetWebPageAsync(url).GetAwaiter().GetResult();
			htmlDocument.LoadHtml(html);

			var webTextLines = htmlDocument.Text.Split('\n');

			for (var i = 0; i < webTextLines.Length; i++)
			{
				var line = webTextLines[i];
				if (line.TrimStart().StartsWith("<td>保税地域コード", StringComparison.InvariantCulture))
				{
					var lineContainPublishDate = webTextLines[i + 2].Trim();
					var matches = Regex.Matches(lineContainPublishDate, @"[0-9]+\.[0-9]+\.[0-9]+");
					if (matches.Count == 1)
					{
						var dateString = matches[0].ToString();
						publicationDate = DateTime.Parse(dateString, DateTimeFormatInfo.InvariantInfo);
						return true;
					}
				}
			}

			throw new UnhandledApplicationException($"Can't find a valid Publication Date from {url}");
		}
	}
}
