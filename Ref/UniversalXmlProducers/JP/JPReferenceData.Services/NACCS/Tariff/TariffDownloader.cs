using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class TariffDownloader
	{
		public TariffDownloader(IWebSourceProvider sourceProvider)
		{
			downloadFileFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AppConfig.NACCS.CodeLists.JPNACCSTariffPath);
			Directory.CreateDirectory(downloadFileFolderPath);

			this.sourceProvider = sourceProvider;
		}

		readonly IWebSourceProvider sourceProvider;
		readonly string downloadFileFolderPath;

		public string DownloadImportTariffData()
		{
			var site = new Site(sourceProvider, AppConfig.NACCS.CodeLists.JPNACCSTariffHomePageDownloadUrl) { FullPath = Path.Combine(downloadFileFolderPath, Constants.FileNames.ImportTariffFileName) };
			return DownloadCore(site);
		}

		public string DownloadExportTariffData()
		{
			var site = new Site(sourceProvider, AppConfig.NACCS.CodeLists.JPNACCSExportTariffHomePageDownloadUrl) { FullPath = Path.Combine(downloadFileFolderPath, Constants.FileNames.ExportTariffFileName) };
			return DownloadCore(site);
		}

		string DownloadCore(Site site)
		{
			if (site.AlreadyDownloaded)
			{
				return site.PublishDate;
			}

			var sw = Stopwatch.StartNew();

			try
			{
				Console.WriteLine($"Download JP NACCS Tariff Start. IsParallelMode: {sourceProvider.IsParallelMode}");

				site.Translate();
				site.Export();
			}
			finally
			{
				sw.Stop();
				Console.WriteLine($"Download JP NACCS Tariff Complete. Total Time: {sw.Elapsed.TotalMinutes} minutes");
			}

			return site.PublishDate;
		}
	}
}
