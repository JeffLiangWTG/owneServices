using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser
{
	public class WebPageScrape
	{
		readonly IHttpClientHelper _httpClientHelper;
		readonly string _url;
		readonly string pageContents;
		const string FileUrlPattern = @"(/sites/default/files/assets/documents/.+?\.pdf)""";
		const string urlPrefix = @"https://www.cbp.gov";
		const string ModifiedDatePattern = @"(Last modified[^<.]+)(<[^>]*>)+([A-Za-z ]+[0-9]{1,2}[, ]+[0-9]{4}){1}";

		public WebPageScrape(IHttpClientHelper httpClientHelper, string url)
		{
			Argument.NotNull(httpClientHelper, nameof(httpClientHelper));
			Argument.NotNullOrEmpty(url, nameof(url));

			_httpClientHelper = httpClientHelper;
			_url = url;
			pageContents = _httpClientHelper.GetWebPageAsync(_url)?.GetAwaiter().GetResult();
		}

#pragma warning disable CA1055 // URI return values should not be strings
		public string GetFileUrl()
		{
			return FetchFileUrl(pageContents);
		}
#pragma warning restore CA1055 // URI return values should not be strings

		public DateTime? GetPublishDate()
		{
			var match = Regex.Match(pageContents, ModifiedDatePattern);
			if (match.Groups.Count == 4)
			{
				if (DateTime.TryParseExact(match.Groups[3].Value, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
				{
					return parsedDate;
				}
			}
			return null;
		}

		static string FetchFileUrl(string pageContent)
		{
			Argument.NotNull(pageContent, nameof(pageContent));

			string url = "";
			var match = Regex.Match(pageContent, FileUrlPattern);
			if (match.Groups.Count == 2)
			{
				url = match.Groups[1].Value;
				url = urlPrefix + url;
			}
			return url;
		}
	}
}
