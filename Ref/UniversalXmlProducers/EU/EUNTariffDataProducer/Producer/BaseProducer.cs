using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public abstract class BaseProducer : IProducer
	{
		public abstract IEnumerable<string> FilesToLocate { get; }
		public abstract string EUNLibraryBasePage { get; }
		public DateTime PublishTime { get; protected set; }

		public virtual IEnumerable<IWebFileInfo> LocateWebFiles()
		{
			IEnumerable<IWebFileInfo> webFileInfos;

			using (var pageNavigator = GetNewWebDriverHelper())
			{
				var webFileLocator = new WebFileLocator(pageNavigator, FilesToLocate);
				var basePage = EUNLibraryBasePage;
				webFileInfos = webFileLocator.GetLocationOfLatestFiles(basePage);
			}

			return webFileInfos;
		}

		public virtual bool DownloadFiles(IEnumerable<IWebFileInfo> webFileInfos)
		{
			var result = true;

			var webFileDownloader = GetFileDownloaderWrapper();
			var downloadsFolder = ApplicationConfig.DownloadsFolder;
			if (!Directory.Exists(downloadsFolder))
			{
				Directory.CreateDirectory(downloadsFolder);
			}
			PublishTime = DateTime.MinValue;

			foreach (var webFileInfo in webFileInfos)
			{
				var localFilePath = string.Empty;
				if (new Uri(webFileInfo.DownloadPath).IsFile)
				{
					localFilePath = webFileInfo.DownloadPath;
				}
				else
				{
					var downloadAbsolutePath = Path.GetFullPath(downloadsFolder);
					localFilePath = Path.Combine(downloadAbsolutePath, webFileInfo.FileName);
					result &= webFileDownloader.DownloadFile(webFileInfo.DownloadPath, localFilePath, webFileInfo.LastModificationTime);
				}

				var fileInfo = new FileInfo(localFilePath);
				if (PublishTime < fileInfo.LastWriteTime)
				{
					PublishTime = fileInfo.LastWriteTime;
				}
			}

			ApplicationConfig.SetPublishTime(PublishTime);

			return result;
		}

		protected virtual IFileDownloaderWrapper GetFileDownloaderWrapper()
		{
			return new FileDownloaderWrapper();
		}

		protected virtual IWebDriverHelper GetNewWebDriverHelper()
		{
			return new WebDriverHelper();
		}

		protected virtual IWebFileLocator GetNewWebFileLocator(IWebDriverHelper webDriverHelper, IEnumerable<string> filesToLocate)
		{
			return new WebFileLocator(webDriverHelper, filesToLocate);
		}
	}
}
