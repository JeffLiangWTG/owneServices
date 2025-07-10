using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public abstract class TariffProcessManager
	{
		const string DateTimeFormat = "yyyy_MM_dd_HH_mm_ss_fff";

		public static List<TariffDownloadElement> ReadTariffDownloadElements(XmlDocument downloadedXml)
		{
			var tariffDownloadElements = new List<TariffDownloadElement>();
			var tariffDownloads = downloadedXml.SelectSingleNode("//downloads");

			if (tariffDownloads != null)
			{
				foreach (XmlNode tariffDownload in tariffDownloads.ChildNodes)
				{
					var element = new TariffDownloadElement
					{
						FileName = tariffDownload.SelectSingleNode("bestand").InnerText,
						Url = tariffDownload.SelectSingleNode("url").InnerText
					};
					tariffDownloadElements.Add(element);
				}
			}

			return tariffDownloadElements;
		}

		public static List<FileInfo> GetTariffZipFilesToBeProcessed(string workingFolder)
		{
			var dirInfo = new DirectoryInfo(workingFolder);
			var allFiles = dirInfo.GetFiles().OrderByDescending(x => x.Name).ToArray();
			var filesToBeProcessed = new List<FileInfo>();

			foreach (var file in allFiles)
			{
				var fileNameSplit = file.Name.Split(new char[] { '-', '.' });
				var fileNameDate = fileNameSplit[1];
				if (DateTime.TryParseExact(fileNameDate, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fileDate))
				{
					filesToBeProcessed.Add(file);
					if (fileDate.DayOfWeek == DayOfWeek.Saturday || fileDate.DayOfWeek == DayOfWeek.Sunday)
					{
						break;
					}
				};
			}

			return filesToBeProcessed;
		}

		public static List<FileInfo> ExtractTariffZipFiles(List<FileInfo> filesToBeProcessed, string contentFolder)
		{
			DownloadManagerHelper.PrepareEnvironment(contentFolder);
			var fileDownloadWrapper = new FileDownloaderWrapper();
			foreach (var file in filesToBeProcessed)
			{
				fileDownloadWrapper.ExtractLocalZipFile(file.FullName, contentFolder);
			}
			return new DirectoryInfo(contentFolder).GetFiles().OrderBy(x => x.LastWriteTime).ToList();
		}

		public void Cleanup()
		{
			if (Directory.Exists(TariffOutputDirectory))
			{
				Directory.Delete(TariffOutputDirectory, true);
			}

			if (Directory.Exists(DownloadDir))
			{
				Directory.Delete(DownloadDir, true);
			}
		}

		protected virtual string DownloadDir => Path.GetFullPath(Path.Combine(Path.GetTempPath(), "TariffData"));

		protected virtual string TariffOutputDirectory => Path.GetFullPath(ApplicationConfig.TariffOutputDirectory);
	}
}
