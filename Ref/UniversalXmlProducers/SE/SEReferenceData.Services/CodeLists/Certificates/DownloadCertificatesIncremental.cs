using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.Certificates.Services
{
	public class DownloadCertificatesIncremental : IDownloadCertificatesIncremental
	{
		private HttpClient Client { get; set; }

		public DownloadCertificatesIncremental() : this(new HttpClient()) { }
		public DownloadCertificatesIncremental(HttpClient client)
		{
			Client = client;
		}

		public SEReferenceData.Services.certificate[] DownloadLatestIncrementalAndExtract(string url)
		{
			var records = DownloadTraderObjectExport.Download<SEReferenceData.Services.IncrementalExport.export>(Client, url);
			return records.items.Where(x => x.Item is SEReferenceData.Services.certificate period && period.national == 1L).Select(x => x.Item).Cast<SEReferenceData.Services.certificate>().ToArray();
		}
	}
}
