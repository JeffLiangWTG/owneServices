using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.NOReferenceData.Services
{
	public static class DownloadResourceData
	{
		// Common methods to get the correct URL for various data from Norway Customs (Exchange-rates, tariffs, etc).
		// See https://data.toll.no/api/3/action/resource_search?query=name:xml for available data-sets

		public static (ResourceData ResourceData, string Errors) Download(HttpClient httpClient, Uri uri)
		{
			ResourceData resourceData = null;
			var errors = new StringBuilder();
			var jsonQueryResults = DownloadResourceContentFromUrl(httpClient, uri, errors);
			if (!string.IsNullOrEmpty(jsonQueryResults))
			{
				var fileContent = string.Empty;
				var (url, fileName, lastModified) = ExtractResourceDetails(jsonQueryResults, errors);
				if (errors.Length == 0)
				{
					fileContent = DownloadResourceContentFromUrl(httpClient, new Uri(url), errors);
				}
				resourceData = new ResourceData(url, fileName, fileContent, lastModified);
			}
			return (resourceData, errors.ToString());
		}

		static (string Url, string FileName, string LastModified) ExtractResourceDetails(string resourceData, StringBuilder errors)
		{
			var jsonObject = JObject.Parse(resourceData);
			var url = (string)jsonObject.SelectToken("result.results[0].url");
			var fileName = (string)jsonObject.SelectToken("result.results[0].name");
			var lastModified = (string)jsonObject.SelectToken("result.results[0].last_modified");

			if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(lastModified))
			{
				errors.AppendFormat(CultureInfo.InvariantCulture, "We have incomplete Resource Data information to download the content. The query response was: {0}", resourceData).AppendLine();
			}
			return (url, fileName, lastModified);
		}

		static string DownloadResourceContentFromUrl(HttpClient httpClient, Uri uri, StringBuilder errors)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			var resourceDataContent = string.Empty;
			var response = httpClient.GetAsync(uri).Result;
			if (response.IsSuccessStatusCode)
			{
				resourceDataContent = response.Content.ReadAsStringAsync().Result;
				if (resourceDataContent.Length == 0)
				{
					errors.AppendFormat(CultureInfo.InvariantCulture, "Downloaded content from '{0}', was zero bytes.", uri).AppendLine();
				}
			}
			else
			{
				errors.AppendFormat(CultureInfo.InvariantCulture, "Error requesting resource content for, '{0}' the response status code was: {1}.", uri, response.StatusCode).AppendLine();
			}
			return resourceDataContent;
		}
	}
}
