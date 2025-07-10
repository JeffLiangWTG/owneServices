using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.NOReferenceData.Services
{
	public static class DownloadContent
	{
		public static (string Errors, T XmlData, ResourceData ResourceData) Download<T>(HttpClient httpClient, Uri resourceUrl, string schemalocation)
			where T : class
		{
			var (resourceData, errors) = DownloadResourceData.Download(httpClient, resourceUrl);
			if (!string.IsNullOrEmpty(errors))
			{
				return (errors, null, null);
			}

			errors = XmlHelper.ValidateXml(resourceData.FileContents, schemalocation);
			if (!string.IsNullOrEmpty(errors))
			{
				return ("Error validating XML downloaded from " + resourceUrl + ": " + errors, null, null);
			}

			var xmlData = XmlHelper.DeserializeFromString<T>(resourceData.FileContents);
			return (string.Empty, xmlData, resourceData);
		}
	}
}
