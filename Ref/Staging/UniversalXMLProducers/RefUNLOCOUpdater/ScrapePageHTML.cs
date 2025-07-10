using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public class ScrapePageHTML
	{
		public DateTime PublicationTime { get; set; }
		public string UNLOCOHyperLink { get; set; }

		public ScrapePageHTML(IHttpClientHelper helper, string url)
		{
			Argument.NotNull(helper, nameof(helper));
			Argument.NotNullOrEmpty(url, nameof(url));
			helper.CloseConnection();   // we should close the connection to trigger the smartproxy work.
			var pageHtml = helper.GetWebPageAsync(url).Result;
			if (!string.IsNullOrEmpty(pageHtml))
			{
				SetPublicationTime(pageHtml);
				SetUNLOCOHyperLink(pageHtml);
			}
		}

		void SetPublicationTime(string pageHtml)
		{
			Argument.NotNullOrEmpty(pageHtml, nameof(pageHtml));

			var match = Regex.Match(pageHtml, ConfigurationProvider.UNECELastUpdatedTimeRegex);
			if (match.Success)
			{
				var matchedValue = match.Groups[0].Value;
				PublicationTime = DateTime.ParseExact(matchedValue.Substring(matchedValue.LastIndexOf(">", StringComparison.Ordinal) + 1), ConfigurationProvider.UNECELastUpdatedTimeFormat, CultureInfo.InvariantCulture);
			}
		}

		void SetUNLOCOHyperLink(string pageHtml)
		{
			Argument.NotNullOrEmpty(pageHtml, nameof(pageHtml));

			var match = Regex.Match(pageHtml, ConfigurationProvider.UNECEMdbZipFileRegex);
			if (match.Success)
			{
				var uri = match.Groups[0].Value;
				UNLOCOHyperLink = uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? uri : ConfigurationProvider.BaseAddress + uri;
			}
		}
	}
}
