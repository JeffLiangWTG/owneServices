using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using FluentFTP;

namespace CargoWise.RefDbRepo.IHSReferenceData.Services.Vessel
{
	public static class VesselListFileDownloader
	{
		public static string DownloadAndExtractVesselFile(string ftpHost, string ftpUserName, string ftpPassword, string targetFileName, string flagCodesFileName, out DateTime publicationDateTime, out string flagCodeCsv)
		{
			string result = null;
			flagCodeCsv = null;
			var localTempFileName = Path.GetTempFileName();
			publicationDateTime = new DateTime();

			try
			{
				using (var ftp = new FtpClient(ftpHost, ftpUserName, ftpPassword))
				{
					ftp.Connect();
					var filesList = new List<string>();
					foreach (var fileName in ftp.GetNameListing())
					{
						if (fileName.StartsWith("ShipData_", StringComparison.OrdinalIgnoreCase))
						{
							if (!ftp.DirectoryExists(fileName))
							{
								filesList.Add(fileName);
							}
						}
					}

					if (filesList.Count == 0)
					{
						throw new DownloadException($"No IHS Vessel List found from the following host: {ftpHost}");
					}

					filesList.Sort();
					var lastFileName = filesList.Last();

					Console.WriteLine("Last file is : " + lastFileName);
					Console.WriteLine("Downloading ...");

					publicationDateTime = ftp.GetModifiedTime("/" + lastFileName);
					ftp.DownloadFile(localTempFileName, "/" + lastFileName, FtpLocalExists.Overwrite, FtpVerify.Retry);

					Console.WriteLine("Downloaded to : " + localTempFileName);

					using (ZipArchive archive = ZipFile.OpenRead(localTempFileName))
					{
						var vesselEntry = archive.Entries.Where(x => x.Name.Equals(targetFileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
						if (vesselEntry != null)
						{
							using (var sr = new StreamReader(vesselEntry.Open(), Encoding.UTF8)
							)
							{
								result = sr.ReadToEnd();
							}
						}

						var flagCodeEntry = archive.Entries.Where(x => x.Name.Equals(flagCodesFileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
						if (flagCodeEntry != null)
						{
							using (var sr = new StreamReader(flagCodeEntry.Open(), Encoding.UTF8)
							)
							{
								flagCodeCsv = sr.ReadToEnd();
							}
						}
					}
				}

				Console.WriteLine(string.IsNullOrWhiteSpace(result) ? "Nothing was extracted!" : "Data extracted.");

				return result;
			}
			catch (Exception ex)
			{
				throw new DownloadException($"Unable to Load IHS Vessel List from the following host: {ftpHost} /r/n {ex.Message}");
			}
			finally
			{
				if (File.Exists(localTempFileName))
				{
					File.Delete(localTempFileName);
					Console.WriteLine("TEMP file deleted.");
				}
			}
		}
	}
}
