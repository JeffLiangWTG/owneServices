using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.NZReferenceData.Services
{
	public class SupplierListFileDownloder
	{
		const string DateFormat = @"yyyyMMdd";
		const string Pattern = @"\d{8}";

		const string ServerUnavailableError = @"The remote server returned an error: (503) Server Unavailable.";
		const string MaintenanceContent = @"We are currently performing planned maintenance. Please check back shortly. Thank you for your patience.";

		public (bool IsSuccess, DateTime PublishDate) Download(HttpClient client, string url, string localPath)
		{
			var publishDate = DateTime.MinValue;
			string fileName;
			var isSuccess = false;

			try
			{
				using (var response = client.GetAsync(new Uri(url)).GetAwaiter().GetResult())
				{
					var data = response.Content.ReadAsByteArrayAsync().Result;
					fileName = GetDownloadFileName(response);
					Console.WriteLine($@"Downloaded NZ Supplier List from the following URL: {url} in name: {fileName}.");

					var matchDate = Regex.Match(fileName, Pattern);

					if (matchDate.Success)
					{
						DateTime.TryParseExact(matchDate.Value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out publishDate);
					}

					if (publishDate.Equals(DateTime.MinValue))
					{
						throw new InvalidOperationException($"Can not get a valid publish date from the file name: {fileName}");
					}

					var dir = Path.GetDirectoryName(localPath);
					var compressedFilePath = Path.Combine(dir, fileName);

					File.WriteAllBytes(compressedFilePath, data);
					GZipHelper.UnzipFile(compressedFilePath, localPath);
					isSuccess = true;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Equals(ServerUnavailableError, StringComparison.OrdinalIgnoreCase) && IsSiteInMaintenance())
				{
					Console.WriteLine($@"Unable to Load NZ Supplier List from the following URL: {url} as the website is in maintenance.");
				}
				else
				{
					throw new InvalidOperationException($@"Unable to Load NZ Supplier List from the following URL: {url}{Environment.NewLine}{ex.Message}");
				}
			}

			return (isSuccess, publishDate);
		}

		static bool IsSiteInMaintenance()
		{
			try
			{
				var url = ApplicationConfig.SupplierPageUrl;
				var clientHelper = new HttpClientHelper();
				var content = clientHelper.GetWebPageAsync(url).Result;
				return content.Contains(MaintenanceContent);
			}
			catch (HttpRequestException)
			{
				return false;
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
