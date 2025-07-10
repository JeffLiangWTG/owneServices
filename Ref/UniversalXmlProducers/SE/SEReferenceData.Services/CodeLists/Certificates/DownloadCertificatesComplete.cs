using System;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.Certificates.Services
{
	public class DownloadCertificatesComplete : IDownloadCertificatesComplete
	{
		private HttpClient Client { get; set; }

		public DownloadCertificatesComplete() : this(new HttpClient()) { }
		public DownloadCertificatesComplete(HttpClient client)
		{
			Client = client;
		}

		public certificate[] DownloadLatestTotAndExtract(string url)
		{
			var records = Download(Client, url);
			return records.items.Where(x => x is certificate period).ToArray();
		}

		static export Download(HttpClient client, string url)
		{
			try
			{
				return Helper.DownloadKryptFile<export>(client, url);
			}
			catch (Exception ex)
			{
				throw new CertificatesException($"Unable to Load Certificates XML from the following URL: {url} /r/n {ex.Message}");
			}
		}
	}
}
