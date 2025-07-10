using System;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.UKOfficeCodes
{
	public class UKOfficeCodesDownloader
	{
		public UKOfficeCodesDownloader(IWebClientWrapper webClient)
		{
			this.webClient = webClient;
		}

		public byte[] GetSpreadsheetData(string landingPageUrl)
		{
			var url = GetDataUrlFromPage(landingPageUrl);
			return webClient.GetContentAsByteArray(url);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		internal string GetDataUrlFromPage(string landingPageUrl)
		{
			try
			{
				var html = webClient.GetContent(landingPageUrl);
				var regex = ConfigurationProvider.UKOfficeCodesDataAnchorRegex;
				var link = GetUrlFromAnchorRegex(html, regex);

				if (string.IsNullOrEmpty(link))
				{
					throw new ApplicationException($"Download link could not be found using the regex '{regex}'.");
				}
				return link;
			}
			catch (Exception ex) when (!(ex is ApplicationException))
			{
				throw new ApplicationException($"Processing of {landingPageUrl} failed. ", ex);
			}
		}

		static string GetUrlFromAnchorRegex(string html, string regex)
		{
			var match = Regex.Match(html, regex);
			return match.Success ? match.Groups[1].Value : null;
		}

		readonly IWebClientWrapper webClient;
	}
}
