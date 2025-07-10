using System;
using System.IO;
using System.Net.Http;

namespace CargoWise.RefDbRepo.NZReferenceData.Services
{
	public class TariffListFileDownloader
	{
		public void Download(HttpClient client, string url, string dir)
		{
			try
			{
				using (var response = client.GetAsync(new Uri(url)).GetAwaiter().GetResult())
				{
					var data = response.Content.ReadAsByteArrayAsync().Result;
					var fileName = GetDownloadFileName(response);
					Console.WriteLine($@"Downloaded List from the following URL: {url} in name: {fileName}.");

					var compressedFilePath = Path.Combine(dir, fileName);
					File.WriteAllBytes(compressedFilePath, data);
					GZipHelper.UnzipFolder(compressedFilePath, dir);
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($@"Unable to Load NZ Tariff List from the following URL: {url}{Environment.NewLine}{ex.Message}");
			}
		}

		protected virtual string GetDownloadFileName(HttpResponseMessage httpResponseMessage)
		{
			var contentDisposition = httpResponseMessage.Content.Headers.ContentDisposition;
			var fileName = contentDisposition.FileName;

			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new InvalidOperationException($"Can not get a valid file name from the content-disposition: {contentDisposition}");
			}

			return fileName;
		}
	}
}
