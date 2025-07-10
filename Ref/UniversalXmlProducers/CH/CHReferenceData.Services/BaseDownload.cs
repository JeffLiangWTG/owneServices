using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CargoWise.RefDbRepo.CHReferenceData.Services
{
	public class BaseDownload
	{
		protected static DownloadResult Download(HttpClient client, string downloadUrl)
		{
			SetupHttpClient(client);
			using (var response = client.GetAsync(new Uri(downloadUrl)).Result)
			{
				return new DownloadResult
				{
					Content = response.Content.ReadAsByteArrayAsync().Result,
				};
			}
		}

		protected static DownloadResult DownloadAndUnzip(HttpClient client, string downloadUrl)
		{
			SetupHttpClient(client);
			using (var response = client.GetAsync(new Uri(downloadUrl)).Result)
			using (var zip = new ZipArchive(response.Content.ReadAsStreamAsync().Result))
			using (var packed = zip.Entries.First().Open())
			using (var unpacked = new MemoryStream())
			{
				packed.CopyTo(unpacked);

				return new DownloadResult()
				{
					Content = unpacked.ToArray(),
				};
			}
		}

		static void SetupHttpClient(HttpClient client)
		{
			client.DefaultRequestHeaders.Add(UserAgent, Constants.HttpUserAgent);
		}

		const string UserAgent = "User-Agent";
	}
}
