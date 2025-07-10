using System;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Services.Tariff;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public class FolderLocationProvider : IFolderLocationProvider
	{
		public string GetPdfDownloadFolder(DateTime? tariffDate)
		{
			if (tariffDate == null)
			{
				return null;
			}

			var downloadFolder = Path.Combine(pdfDownloadDirectory, tariffDate.Value.GetDateInCbicString());
			Directory.CreateDirectory(downloadFolder);
			return downloadFolder;
		}

		public string GetJsonCreationFolder(DateTime? tariffDate)
		{
			if (tariffDate == null)
			{
				return null;
			}

			var downloadFolder = Path.Combine(jsonCreationDirectory, tariffDate.Value.GetDateInCbicString());
			Directory.CreateDirectory(downloadFolder);
			return downloadFolder;
		}

		public string GetOutputTariffXmlFilePath(string chapter, DateTime date)
		{
			var fileName = $"RefCusTariff_IN_CTH_Chapter{chapter}_{date:yyyyMMdd}.xml";
			var outputPath = AppConfig.Shared.OutputDirectory;
			Directory.CreateDirectory(outputPath);
			var filePath = Path.Combine(outputPath, fileName);
			return filePath;
		}

		readonly string pdfDownloadDirectory = Path.Combine(AppConfig.Tariff.IntermediateOutputDirectory, "Pdfs");
		readonly string jsonCreationDirectory = Path.Combine(AppConfig.Tariff.IntermediateOutputDirectory, "Jsons");
	}
}
