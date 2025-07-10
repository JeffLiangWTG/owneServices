using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace FsisEstNumbersCrawler.Services
{
	public class Downloader : IDownloader
	{
		public void Download(string url, string localPath)
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var client = new WebClient())
			{
				client.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
				client.Headers.Add(HttpRequestHeader.Cookie, "Cookies");
				try
				{
					var downloadedData = client.DownloadData(new Uri(url));
					var contentEncoding = client.ResponseHeaders["Content-Encoding"];
					if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.ToLower(CultureInfo.CurrentCulture).Contains("gzip"))
					{
						using (var compressedStream = new MemoryStream(downloadedData))
						using (var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
						using (var fileStream = File.Create(localPath))
						{
							decompressionStream.CopyTo(fileStream);
						}
					}
					else
					{
						File.WriteAllBytes(localPath, downloadedData);
					}
				}
				catch (Exception e)
				{
					throw new WebException("Error occured when downloading file, see the inner exception for details.", e);
				}
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}
	}
}
