using System;
using System.IO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web
{
	public interface IFileDownloaderWrapper
	{
		bool DownloadFile(string remoteUrl, string localFilePath);
		bool DownloadFile(string remoteUrl, string localFilePath, DateTime? lastModifiedDate);
		byte[] DownloadFile(string remoteUrl);
		string[] DownloadAndExtract(string remoteUrl, string localFilePath);
		string[] DownloadAndExtract(string remoteUrl, string localFilePath, DateTime? lastModifiedDate);
		FileInfo GetFileInfo(string localFilePath);
		string[] ExtractLocalZipFile(string filePath, string destinationPath = "");
	}
}
