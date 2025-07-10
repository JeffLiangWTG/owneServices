using System.IO;
using System;
using FluentFTP;
using FluentFTP.Exceptions;

namespace CargoWise.RefDbRepo.NZReferenceData.Services
{
	public static class FTPDownloader
	{
		public static void Download(IFtpClient client, string localFilePath, string serverFilePath)
		{
			var downloadStatus = client.DownloadFile(localFilePath, serverFilePath, existsMode: FtpLocalExists.Overwrite);
			if (downloadStatus != FtpStatus.Success)
			{
				throw new FtpException($"Unable to download file {serverFilePath} to {localFilePath}, status: {downloadStatus}");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static string DownloadWithFallback(IFtpClient ftpClient, string fileName, string localPath, string serverPath, string portalPath, Action<Exception> onFtpError)
		{
			var filePath = string.Empty;
			var localFilePath = Path.Combine(localPath, fileName);
			try
			{
				var localTempFilePath = localFilePath + ".temp";
				ftpClient.Connect();
				Download(ftpClient, localTempFilePath, Path.Combine(serverPath, fileName));

				if (File.Exists(localFilePath))
				{
					File.Delete(localFilePath);
				}
				File.Move(localTempFilePath, localFilePath);

				filePath = localFilePath;
			}
			catch (Exception exception)
			{
				onFtpError(exception);
			}

			if (string.IsNullOrEmpty(filePath))
			{
				var portalFilePath = Path.Combine(portalPath, fileName);
				var localFileInfo = new FileInfo(localFilePath);
				var portalFileInfo = new FileInfo(portalFilePath);
				var localFileModifiedTime = DateTime.MinValue;
				var portalFileModifiedTime = DateTime.MinValue;

				if (localFileInfo.Exists)
				{
					localFileModifiedTime = localFileInfo.LastWriteTimeUtc;
				}
				if (portalFileInfo.Exists)
				{
					portalFileModifiedTime = portalFileInfo.LastWriteTimeUtc;
				}

				if (localFileModifiedTime > portalFileModifiedTime)
				{
					filePath = localFilePath;
				}
				else
				{
					filePath = portalFilePath;
				}
			}

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException($"Cannot find file {filePath}");
			}

			return filePath;
		}
	}
}
