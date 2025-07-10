using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Tariff
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
	public static class FileHelper
	{
		#region File List
		public static List<FileDetails> GetFileList(ITariffWebClientWrapper webClient, string fileListURL)
		{
			if (webClient == null)
			{
				throw new ArgumentNullException(nameof(webClient));
			}

			string fileListContent;

			try
			{
				fileListContent = webClient.GetContent(fileListURL);
			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Could not get file list for '{fileListURL}'", ex);
			}

			return ConvertFileListContent(fileListContent);
		}

		static List<FileDetails> ConvertFileListContent(string content)
		{
			var list = new List<FileDetails>();

			try
			{
				if (!string.IsNullOrEmpty(content))
				{
					list.AddRange(JsonConvert.DeserializeObject<List<FileDetails>>(content));
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Could not convert content to file list", ex);
			}

			return list;
		}
		#endregion

		#region Download File
		public static string DownloadFile(ITariffWebClientWrapper webClient, FileDetails fileDetails, string localPath)
		{
			if (webClient == null)
			{
				throw new ArgumentNullException(nameof(webClient));
			}

			if (fileDetails == null)
			{
				throw new ArgumentNullException(nameof(fileDetails));
			}

			if (string.IsNullOrEmpty(localPath))
			{
				throw new ArgumentException($"Invalid argument {nameof(localPath)} '{localPath}'");
			}

			var localFile = Path.Combine(localPath, fileDetails.Filename);

			try
			{
				Directory.CreateDirectory(localPath);

				if (!IsFileDownloaded(localFile, fileDetails.FileSize))
				{
					webClient.DownloadFile(fileDetails.DownloadURL, localFile);
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Could not download file '{fileDetails.Filename}' from '{fileDetails.DownloadURL}'", ex);
			}

			return localFile;
		}

#if DEBUG
		public
#endif
		static bool IsFileDownloaded(string localFile, long expectedSize)
		{
			var fileInfo = new FileInfo(localFile);
			return fileInfo.Exists && fileInfo.Length == expectedSize;
		}
		#endregion

		#region UnzipFile
		public static void UnzipFile(string compressedFile, string outputPath)
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
		}
		#endregion
	}
}
