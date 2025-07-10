using System;
using System.IO;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public static class FileHelper
	{
		#region File Exists
		public static bool FileExists(string localPath, string searchPattern) => Directory.GetFiles(localPath, searchPattern).Length > 0;
		#endregion

		#region Download File
		public static string[] DownloadFile(IWebDriverHelperWrapper webDriver, string downloadURL, string localPath, string xpath, bool isMonthly)
		{
			if (webDriver == null)
			{
				throw new ArgumentNullException(nameof(webDriver));
			}

			if (string.IsNullOrEmpty(localPath))
			{
				throw new ArgumentException($"Invalid argument {nameof(localPath)} '{localPath}'");
			}

			try
			{
				Directory.CreateDirectory(localPath);

				webDriver.DownloadFiles(downloadURL, xpath, isMonthly);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Could not download file from '{downloadURL}'", ex);
			}

			return Directory.GetFiles(localPath, "*");
		}
		#endregion

		#region UnzipFile
		public static string[] UnzipFile(string compressedFile, string outputPath)
		{
			if (!File.Exists(compressedFile))
			{
				throw new FileNotFoundException($"Zip file '{compressedFile}' does not exist");
			}

			if (!Directory.Exists(outputPath))
			{
				throw new DirectoryNotFoundException($"Output path '{outputPath}' does not exist");
			}

			try
			{
				var zipHelper = ZipFileHelper.GetZipHelper(compressedFile);

				zipHelper.UnzipFile(compressedFile, outputPath);
			}
			catch (Exception ex)
			{
				throw new FileNotFoundException($"Failed to extract {compressedFile}", ex);
			}
			return Directory.GetFiles(outputPath, "*", new EnumerationOptions { RecurseSubdirectories = true });
		}
		#endregion
	}
}
