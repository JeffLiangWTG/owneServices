using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	sealed class DailyPackageDownloader : IDailyPackageDownloader
	{
		public void DownloadAndExtract(string url, string destinationPath, string extractionFolderPath, bool deletePackage)
		{
			using (HttpClient client = FileDownloaderHttpClientFactory.CreateHttpClient())
			{
				using (HttpResponseMessage response = client.GetAsync(new Uri(url)).Result)
				{
					response.EnsureSuccessStatusCode();
					using (Stream contentStream = response.Content.ReadAsStreamAsync().Result,
						fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						contentStream.CopyTo(fileStream);
					}
				}
			}

			ZipFile.ExtractToDirectory(destinationPath, extractionFolderPath);
			File.Delete(destinationPath);
		}
	}
}
