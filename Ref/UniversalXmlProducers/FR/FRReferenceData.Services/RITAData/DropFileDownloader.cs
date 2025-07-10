using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public static class DropFileDownloader
	{
		/// <summary>
		///  Download drop.zip file from french customs into ExtractionDirectory
		/// </summary>
		/// <param name="error">Return EventsErrors.DownloadErr if error</param>
		/// <returns>true/false</returns>
		public static void DownloadZipFromCustoms(IFileDownloader fileDownloader, string baseUrl, string downloadDirectory, string downloadFileName, ref Errors error)
		{
			error = Errors.No;

			Console.WriteLine("Downloading drop.zip file from French Customs");

			try
			{
				// Create download directory

				if (!Directory.Exists(downloadDirectory))
				{
					Directory.CreateDirectory(downloadDirectory);
				}

				string zipFileDownloaded = Path.Combine(downloadDirectory, downloadFileName);

				// Del downloaded zip file if exist

				if (File.Exists(zipFileDownloaded))
				{
					File.Delete(zipFileDownloaded);
				}

				// Download zip file by webClient

				fileDownloader.DownloadFile(baseUrl, zipFileDownloaded);

				// Control file downloaded

				if (!File.Exists(zipFileDownloaded))
				{
					error = Errors.DownloadErr;
					throw new DropFileException("Downloaded drop file not found.");
				}
				else if (new FileInfo(zipFileDownloaded).Length == 0)
				{
					error = Errors.DownloadErr;
					throw new DropFileException("Downloaded drop file is empty.");
				}
			}
			catch (IOException e)
			{
				error = Errors.IO;
				throw new DropFileException("Download of drop file triggered an I/O exception.", e);
			}
			catch (WebException e)
			{
				error = Errors.WebErr;
				throw new DropFileException("Download of drop file triggered a Web exception.", e);
			}
		}

		public static void ExtractZipFile(string downloadDirectory, string downloadFileName, string extractToDirectory, string[] filesToExtract, out DateTime publicationDate, ref Errors error)
		{
			publicationDate = DateTime.MinValue;

			string zipFileDownloaded = Path.Combine(downloadDirectory, downloadFileName);

			Console.WriteLine("Extracting file from :" + zipFileDownloaded);

			if (!Directory.Exists(downloadDirectory) || !File.Exists(zipFileDownloaded))
			{
				error = Errors.ExtractErr;
			}

			try
			{
				// Open archive file

				using (ZipArchive archive = ZipFile.Open(zipFileDownloaded, ZipArchiveMode.Read))
				{
					foreach (var file in filesToExtract)
					{
						var entry = archive.Entries.FirstOrDefault(e => string.Equals(e.Name, file, StringComparison.OrdinalIgnoreCase));
						string fileExtractedPath = Path.Combine(extractToDirectory, entry.Name);
						entry.ExtractToFile(fileExtractedPath, true);
						publicationDate = entry.LastWriteTime.DateTime;
					}
				}

				// Control files extraction

				if (!new DirectoryInfo(extractToDirectory).GetFiles().Any())
				{
					error = Errors.ExtractErr;
					throw new DropFileException("Extract of drop file didn't create any file.");
				}
			}
			catch (IOException e)
			{
				error = Errors.ExtractErr;
				throw new DropFileException("Extract of drop file triggered an I/O exception.", e);
			}
		}
	}
}
