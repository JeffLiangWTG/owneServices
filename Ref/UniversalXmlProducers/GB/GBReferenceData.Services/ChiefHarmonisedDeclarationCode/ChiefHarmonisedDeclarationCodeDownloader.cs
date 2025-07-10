using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.ChiefHarmonisedDeclarationCode
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
	public static class ChiefHarmonisedDeclarationCodeDownloader
	{
		public static DateTime Download(IFileDownloaderWrapper fileDownloader, IWebDriverHelper webDriver, string downloadSourceURL, string tempOutputFileNameAndPath)
		{
			var page = webDriver.GetWebPage(downloadSourceURL);
			var url = GetSpecificFileURL(page, downloadSourceURL);
			var publicationDate = GetPublicationTime(page, downloadSourceURL);
			fileDownloader.DownloadFile(url.AbsoluteUri, tempOutputFileNameAndPath);
			return publicationDate;
		}


		static DateTime GetPublicationTime(string pageSource, string downloadSourceURL)
		{
			var result = DateTime.UtcNow;
			var match = Regex.Match(pageSource, @"Last\supdated\s(\d+\s\w+\s\d+)");
			if (match.Success)
			{
				result = DateTime.Parse(match.Groups[1].Value, CultureInfo.CurrentCulture);
			}
			else
			{
				throw new ApplicationException($"Unable to find Last Updated details on webpage: {downloadSourceURL}");
			}
			return result;
		}

		public static Uri GetSpecificFileURL(string pageSource, string downloadSourceURL)
		{
			var match = Regex.Match(pageSource, @"\<a.*href=""(.*\.ods)"">UK Trade Tariff: document, certificate and authorisation codes for harmonised declarations");
			if (!match.Success)
			{
				throw new ApplicationException($"Unable to find certificate_authorisation_codes_for_harmonised_declarations.ods on webpage: {downloadSourceURL}");
			}
			return new Uri(match.Groups[1].Value);
		}
	}
}
