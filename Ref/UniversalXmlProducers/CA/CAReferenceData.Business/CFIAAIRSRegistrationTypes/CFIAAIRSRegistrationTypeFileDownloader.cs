using System;
using System.Globalization;
using CargoWise.RefDbRepo.CAReferenceData.Services;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSRegistrationTypes
{
	public class CFIAAIRSRegistrationTypeFileDownloader : FileDownloader
	{
		public CFIAAIRSRegistrationTypeFileDownloader(PreProcessChecker checker)
			: base(checker)
		{
		}

		public bool ReadPages()
		{
			var branchURL = GetSpecifiedLink(RootURL, "English");
			var ifpaURL = GetSpecifiedLink(branchURL, "Importing food, plants or animals");
			var importRulesURL = GetSpecifiedLink(ifpaURL, "Automated Import Reference System (AIRS)");
			EnglishDownloadPageURL = GetSpecifiedLink(importRulesURL, "CFIA AIRS registration types");
			FrenchDownloadPageURL = GetSpecifiedLink(EnglishDownloadPageURL, "Français");
			if (string.IsNullOrEmpty(EnglishDownloadPageURL) || string.IsNullOrEmpty(FrenchDownloadPageURL))
			{
				return false;
			}
			return true;
		}

		public string GetSpecifiedLink(string url, string linkText, string html = "")
		{
			var page = new HtmlAgilityPack.HtmlDocument();
			page.LoadHtml(string.IsNullOrEmpty(html) ? WebScraper.GetHtml(url) : html);
			var links = page.DocumentNode.SelectNodes($"//a");

			var result = string.Empty;
			foreach (var link in links)
			{
				if (link.InnerText.Contains(linkText))
				{
					result = link.GetAttributeValue("href", "");
					break;
				}
			}

			result = AppendRootLink(result);
			return result;
		}

		protected override string GetCountryVersion(string url) => url.Equals(EnglishDownloadPageURL, StringComparison.Ordinal) ? "English Version: " : "French Version: ";
		protected override string LinkNodeXpath => $"//a[@class='gc-dwnld-lnk']";

		public string EnglishDownloadPageURL { get; set; }
		public string FrenchDownloadPageURL { get; set; }

		protected override bool CheckIsDownloadFileNode(string href)
		{
			href = href.ToUpper(CultureInfo.InvariantCulture);
			return href.EndsWith(".PDF", StringComparison.OrdinalIgnoreCase) && href.Contains("DOCUMENTS") && href.Contains("REGISTRATION");
		}

		protected override string RootURL => ApplicationConfig.InspectionCanadaCA;
	}
}
