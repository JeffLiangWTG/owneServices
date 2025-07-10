using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class SectionDetailsScrapper : ISectionDetailsScrapper
	{
		readonly IWebDriverHelper webDriverHelper;

		public SectionDetailsScrapper(IWebDriverHelper webDriverHelper)
		{
			Argument.NotNull(webDriverHelper, nameof(webDriverHelper));

			this.webDriverHelper = webDriverHelper;
		}

		public IDictionary<int, ISection> ExtractChapterToSectionMap(string url)
		{
			Argument.NotNullOrEmpty(url, nameof(url));
			var result = new Dictionary<int, ISection>();

			var sectionPageHtmlDocument = new HtmlDocument();
			sectionPageHtmlDocument.LoadHtml(webDriverHelper.GetWebPage(url, ApplicationConfig.WaitPageLoadingInSeconds));

			var sectionNodes = sectionPageHtmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'section_heading') and not(@id='section_header')]") ?? new HtmlNodeCollection(null);

			foreach (var sectionNode in sectionNodes)
			{
				var descendants = sectionNode.Descendants("td");

				var sectionLabel = descendants.FirstOrDefault(x => x.HasClass("sectlabel"));

				var rawSectionNumber = sectionLabel.InnerText.ToUpperInvariant().Replace("SECTION", string.Empty).Trim();

				var sectionNumber = EUNUtils.ConvertFromRomanNumeral(rawSectionNumber);

				var sectionDescription = sectionLabel.NextSibling.InnerText.Trim();
				if (string.IsNullOrEmpty(sectionDescription) && sectionLabel.NextSibling.NextSibling != null)
				{
					sectionDescription = sectionLabel.NextSibling.NextSibling.InnerText.Trim();
				}

				var section = new Section(sectionNumber, sectionDescription);
				var chapterPageSource = webDriverHelper.GetWebPageByLinkId(sectionNode.Id);
				var chapterPageHtmlDocument = new HtmlDocument();
				chapterPageHtmlDocument.LoadHtml(chapterPageSource);

				var chapterNodes = chapterPageHtmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'chapter_heading') and not(@id='chapter_header')]");
				foreach (var chapterNode in chapterNodes)
				{
					var chapterDescendants = chapterNode.Descendants("td");
					var chapterLabel = chapterDescendants.FirstOrDefault(x => x.HasClass("chaplabel"));
					var rawChapterNumber = chapterLabel.InnerText.ToUpperInvariant().Replace("CHAPTER", string.Empty).Replace("&NBSP;", string.Empty).Trim();
					var chapterNumber = int.Parse(rawChapterNumber, CultureInfo.InvariantCulture);

					result.Add(chapterNumber, section);
				}
			}

			return result;
		}
	}
}
