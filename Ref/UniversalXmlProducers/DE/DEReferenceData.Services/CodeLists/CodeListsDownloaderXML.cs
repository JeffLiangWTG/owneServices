using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists
{
	public static class CodeListsDownloaderXML
	{
		public static async Task<DownloadResult> Download(HttpClient client, string downloadLink)
		{
			using (var xmlResponse = RetryHelper.RetryWithDelayAsync(async () => await client.GetAsync(new Uri(downloadLink))).GetAwaiter().GetResult())
			{
				var xmlCodelistAsString = await xmlResponse.Content.ReadAsStringAsync();
				return new DownloadResult
				{
					LastModified = xmlResponse.Content.Headers.LastModified ?? DateTimeOffset.Now,
					Content = xmlCodelistAsString
				};
			}
		}

		public static string[] GetDownloadLinks(HttpClient client, string codeListsDownloadLinksUrl)
		{
			using (var responseMessage = RetryHelper.RetryWithDelayAsync(async () => await client.GetAsync(new Uri(codeListsDownloadLinksUrl))).GetAwaiter().GetResult())
			{
				var content = responseMessage.Content.ReadAsStringAsync().Result;
				return content?.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.StartsWith("#", StringComparison.InvariantCulture)).ToArray();
			}
		}

		public static string[] GetDownloadLinks(HttpClient client, string codeListsDownloadLinksUrlPrimary, string codeListsDownloadLinksUrlSecondary)
		{
			var primaryLinks = GetDownloadLinks(client, codeListsDownloadLinksUrlPrimary);
			var secondaryLinks = GetDownloadLinks(client, codeListsDownloadLinksUrlSecondary);
			return primaryLinks.Union(secondaryLinks, new URIComparer()).ToArray();
		}
	}

	public class URIComparer : IEqualityComparer<string>
	{
		public bool Equals(string x, string y) => GetCodelistfromURI(x) == GetCodelistfromURI(y);

		public int GetHashCode(string obj) => GetCodelistfromURI(obj).GetHashCode();

		static string GetCodelistfromURI(string s) => new Uri(s).Segments.LastOrDefault();
	}
}
