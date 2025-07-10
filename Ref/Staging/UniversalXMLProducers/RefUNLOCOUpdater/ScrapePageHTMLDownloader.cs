using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public class ScrapePageHTMLDownloader : IScrapePageHTMLDownloader
	{
		public string DownloadFile(ScrapePageHTML scrapePageHTML)
		{
			var fileDownloaderUnece = new FileDownloader(new Uri(scrapePageHTML.UNLOCOHyperLink));
			return DownloadFile(fileDownloaderUnece);
		}

		static string DownloadFile(IFileDownloader fileDownloader)
		{
			Argument.NotNull(fileDownloader, nameof(fileDownloader));
			var downloadMdbPath = ConfigurationProvider.DownloadMDBTempPath;
			if (!Directory.Exists(downloadMdbPath))
			{
				Directory.CreateDirectory(downloadMdbPath);
			}
			using var file = fileDownloader.GetFileStream();
			using var stream = new ZipArchive(file.GetResponseStream(), ZipArchiveMode.Read);
			var mdbFile = stream.Entries.FirstOrDefault(o => o.FullName.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase));
			downloadMdbPath = Path.Combine(downloadMdbPath, mdbFile.FullName);
			using var archive = mdbFile.Open();
			using var fileStream = File.Create(downloadMdbPath);
			archive.CopyTo(fileStream);
			return downloadMdbPath;
		}
	}
}
