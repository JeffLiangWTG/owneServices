using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Common
{
	public class WebClientWrapper : IWebClientWrapper
	{
		public string GetContent(string url)
		{
			return GetDatedContent(url).Content;
		}

		public (DateTime LastModified, string Content) GetDatedContent(string url)
		{
			var lastModified = DateTime.Now;
			var content = string.Empty;

			using (var response = GetHttpResponse(new Uri(url)))
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					lastModified = GetLastModifiedDate(response.Content.Headers);
					content = response.Content.ReadAsStringAsync().Result;
				}
			}

			return (lastModified, content);
		}

		public byte[] GetContentAsByteArray(string url)
		{
			return GetDatedContentAsByteArray(url).Content;
		}
		public (DateTime LastModified, byte[] Content) GetDatedContentAsByteArray(string url)
		{
			var lastModified = DateTime.Now;
			var content = new byte[0];

			using (var response = GetHttpResponse(new Uri(url)))
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					lastModified = GetLastModifiedDate(response.Content.Headers);
					content = response.Content.ReadAsByteArrayAsync().Result;
				}
			}

			return (lastModified, content);
		}

		static HttpResponseMessage GetHttpResponse(Uri url)
		{
			using (var client = new HttpClient())
			{
				return client.GetAsync(url).Result;
			}
		}

		static DateTime GetLastModifiedDate(HttpContentHeaders headers)
		{
			var lastModifiedDate = DateTime.MinValue;

			var header = headers.FirstOrDefault(x => x.Key.ToLower(CultureInfo.CurrentCulture) == "last-modified");
			if (!string.IsNullOrEmpty(header.Key))
			{
				var lastModifiedStr = header.Value.FirstOrDefault() ?? string.Empty;
				if (DateTime.TryParse(lastModifiedStr, out var lmDate))
				{
					lastModifiedDate = lmDate;
				}
			}

			return lastModifiedDate;
		}
	}
}
