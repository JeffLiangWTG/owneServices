using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class WebFileInfo : IWebFileInfo
	{
		public string FileName { get; }
		public string DownloadPath { get; }
		public DateTime LastModificationTime { get; }
		public Exception Exception { get; }

		public WebFileInfo(string fileName, string downloadPath, DateTime lastModificationTime)
		{
			Argument.NotNullOrEmpty(fileName, nameof(fileName));
			Argument.NotNullOrEmpty(downloadPath, nameof(downloadPath));

			FileName = fileName;
			DownloadPath = downloadPath;
			LastModificationTime = lastModificationTime;
		}

		public WebFileInfo(string fileName, Exception exception)
		{
			Argument.NotNullOrEmpty(fileName, nameof(fileName));

			this.FileName = fileName;
			this.Exception = exception;
		}
	}
}
