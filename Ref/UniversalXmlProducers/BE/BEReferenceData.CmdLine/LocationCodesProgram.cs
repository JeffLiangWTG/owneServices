using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	public static class LocationCodesProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();
			var downloadFileAbsolutePath = DownloadFilePath(Path.Combine(ApplicationConfig.ServiceDir, ApplicationConfig.LocationCodesFolder), "Locatiecodes_", errorCollector);
			if (!string.IsNullOrEmpty(downloadFileAbsolutePath))
			{
				var locationCodes = ExcelParser.ReadLocationCodesXlsIntoResults(downloadFileAbsolutePath, null);
				XMLGeneration.GenerateLocXml("BE Customs Locations", "RefCusCodeListZZ_BE_FAC.xml", locationCodes);
				DeleteProcessedLocationCodesFile(downloadFileAbsolutePath);
			}

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred: \r\n{errorCollector}";
				Console.WriteLine(errorMessage);
			}
		}

		public static string DownloadFilePath(string locationCodesFilePath, string fileName, StringBuilder errorCollector)
		{
			string downloadFilePath = null;

			if (Directory.Exists(locationCodesFilePath))
			{
				var filenames = Directory.GetFiles(locationCodesFilePath, fileName + "*.xls*");
				if (filenames.Length == 0)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"No files matching the pattern '{fileName}'*.xls*' were found in the directory '{locationCodesFilePath}'.");
				}
				downloadFilePath = filenames.FirstOrDefault();
			}
			else
			{
				errorCollector.AppendLine(CultureInfo.InvariantCulture, $"The directory '{locationCodesFilePath}' does not exist.");
			}
			return downloadFilePath;
		}

		static void DeleteProcessedLocationCodesFile(string fileName)
		{
			File.Delete(fileName);
		}
	}
}
