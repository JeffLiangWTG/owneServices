using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public abstract class DownloadManager : IDownloadManager
	{
		protected DownloadManager(string workingFolder)
		{
			this.workingFolder = workingFolder;
		}
		readonly string workingFolder;

		public void RunDownloadProcess(string contentFolder, DateTime downloadDate)
		{
			using (var webDriverHelper = GetWebDriverHelper())
			{
				PrepareEnvironment(workingFolder, contentFolder);
				try
				{
					var downloadLink = ApplicationConfig.TariffUrl;
					webDriverHelper.GetWebPage(ApplicationConfig.TariffUrl, ApplicationConfig.LoadInterval);

					for (int day = 1; day <= downloadDate.Day; day++)
					{
						var currentDownloadDate = new DateTime(downloadDate.Year, downloadDate.Month, day);
						DownloadData(webDriverHelper, workingFolder, contentFolder, currentDownloadDate, isMonthly: day == 1);
					}
				}
				catch (ApplicationException ex)
				{
					throw new InvalidOperationException($"Processing of files in {workingFolder} failed", ex);
				}
			}
		}

		static void DownloadData(IWebDriverHelperWrapper webDriverHelper, string workingFolder, string contentFolder, DateTime downloadDate, bool isMonthly)
		{
			string searchPattern;
			string xPath;
			if (isMonthly)
			{
				var dateString = $"{downloadDate:yyyyMMdd}T000000";
				searchPattern = $"export-{dateString}-*.zip";
				xPath = $"//a[starts-with(@id,'xmlExtractionsControllerForm')] [contains(text(),'export-{dateString}-')]";
			}
			else
			{
				downloadDate = downloadDate.AddDays(-1);
				var dateString = $"{downloadDate:yyyyMMdd}T235959";
				searchPattern = $"export*_{dateString}*.zip";
				xPath = $"//a[starts-with(@id,'xmlExtractionsControllerForm')] [contains(text(),'_{dateString}')]";
			}

			if (!FileHelper.FileExists(workingFolder, searchPattern))
			{
				DownloadAndUnzipFile(webDriverHelper, workingFolder, contentFolder, xPath, downloadDate, isMonthly);
			}
		}

		static void DownloadAndUnzipFile(IWebDriverHelperWrapper webDriverHelper, string workingFolder, string contentFolder, string xpath, DateTime downloadDate, bool isMonthly)
		{
			var downloadedFiles = FileHelper.DownloadFile(webDriverHelper, ApplicationConfig.TariffUrl, workingFolder, xpath, isMonthly);

			if (downloadedFiles.Any())
			{
				if (isMonthly)
				{
					var dateString = $"{downloadDate:yyyyMMdd}T000000";
					var zipFile = downloadedFiles.FirstOrDefault(x => x.Contains($"export-{dateString}-"));
					if (zipFile != null)
					{
						var unzippedFiles = FileHelper.UnzipFile(zipFile, contentFolder);
						if (unzippedFiles.Any(x => x.Contains("Measures")))
						{
							RemoveUnexpectedFiles(workingFolder, zipFile, false);
							downloadedFiles = null;
						}
						else
						{
							RemoveUnexpectedFiles(workingFolder, zipFile, true);
							downloadedFiles = downloadedFiles.Where(x => x != zipFile).ToArray();
						}
					}
				}
				if (downloadedFiles != null)
				{
					foreach (var downloadedFile in downloadedFiles)
					{
						FileHelper.UnzipFile(downloadedFile, contentFolder);
					}
				}
			}
		}

		static void RemoveUnexpectedFiles(string workingFolder, string file, bool keepFile)
		{
			var files = Directory.GetFiles(workingFolder);
			foreach (var filepath in files)
			{
				if ((keepFile && filepath.Equals(file, StringComparison.Ordinal)) || (!keepFile && !filepath.Equals(file, StringComparison.Ordinal)))
				{
					File.Delete(filepath);
				}
			}
		}

		static void PrepareEnvironment(string workingFolder, string contentFolder)
		{
			try
			{
				Directory.CreateDirectory(workingFolder);
			}
			catch
			{
				throw new InvalidOperationException($"Could not create working folder '{workingFolder}'");
			}

			try
			{
				RecreateContentFolder(contentFolder, TimeSpan.FromMilliseconds(100));
			}
			catch
			{
				throw new InvalidOperationException($"Could not create content folder '{contentFolder}'");
			}
		}
		static void RecreateContentFolder(string contentFolder, TimeSpan timeout)
		{
			if (Directory.Exists(contentFolder))
			{
				Directory.Delete(contentFolder, true); 
				using (var cancelToken = new CancellationTokenSource(timeout))
				{
					while (Directory.Exists(contentFolder) && !cancelToken.Token.IsCancellationRequested)
					{
						Thread.Sleep(10);
					}
				}
			}

			Directory.CreateDirectory(contentFolder);
		}

		public abstract IWebDriverHelperWrapper GetWebDriverHelper();
	}

	public class WebDriverDownloadManager : DownloadManager
	{
		public WebDriverDownloadManager() : base(GetWorkingFolder())
		{
		}

		const string TariffData = "TariffData";

		public static string GetWorkingFolder() => Path.GetFullPath(Path.Combine(ApplicationConfig.ServiceDir, TariffData));

		public override IWebDriverHelperWrapper GetWebDriverHelper() => new WebDriverHelperWrapper(GetWorkingFolder());
	}
}
