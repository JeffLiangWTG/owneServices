using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public static class WebClientHelper
	{
		static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		static StringBuilder errorBuilder;

		public static string DownloadFile(string downloadUrl, string downloadFileName, int timeout = 60000)
		{
			string downloadFileAbsolutePath;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			var downloadDir = Path.Combine(ApplicationConfig.DownloadDir, "Download_BE");

			try
			{
				Directory.CreateDirectory(downloadDir);

				var downloadAbsolutePath = Path.GetFullPath(downloadDir);
				downloadFileAbsolutePath = Path.Combine(downloadAbsolutePath, downloadFileName);

				File.Delete(downloadFileAbsolutePath);
				DownloadUrlToFile(downloadUrl, downloadFileAbsolutePath);

				if (!File.Exists(downloadFileAbsolutePath))
				{
					throw new FileNotFoundException($"Can not find file {downloadFileName} in the website");
				}
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to download file: {ex.Message}");

				Console.Error.WriteLine(ErrorBuilder.ToString());
				throw;
			}
			return downloadFileAbsolutePath;
		}

		static void DownloadUrlToFile(string url, string fileName)
		{
			using (var webClient = new WebDownload(60000))
			{
				webClient.DownloadFile(url, fileName);
			}
		}
	}

	class WebDownload : WebClient
	{
		/// <summary>
		/// Time in milliseconds
		/// </summary>
		public int Timeout { get; set; }

		public WebDownload() : this(60000) { }

#pragma warning disable SYSLIB0014 // Type or member is obsolete
		public WebDownload(int timeout)
		{
			Timeout = timeout;
		}
#pragma warning restore SYSLIB0014 // Type or member is obsolete

		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address);
			if (request != null)
			{
				request.Timeout = Timeout;
			}
			return request;
		}
	}
}
